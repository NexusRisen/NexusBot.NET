using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using PKHeX.Core;
using SysBot.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using static Discord.GatewayIntents;
using static SysBot.Pokemon.DiscordSettings;
using SysBot.Pokemon.Discord.Helpers;
using SysBot.Pokemon.Helpers;
using Discord.Net;

namespace SysBot.Pokemon.Discord;

public static class SysCordSettings
{
    public static PokeTradeHubConfig HubConfig { get; internal set; } = default!;

    public static DiscordManager Manager { get; internal set; } = default!;

    public static DiscordSettings Settings => Manager.Config;

    public static AI.HuggingFaceService? AIService { get; internal set; }
}

public sealed class SysCord<T> : IDisposable where T : PKM, new()
{
    public readonly PokeTradeHub<T> Hub;
    private readonly PokeBotRunner<T> _runner;
    private readonly ProgramConfig _config;
    private readonly Dictionary<ulong, ulong> _announcementMessageIds = [];
    private readonly DiscordSocketClient _client;
    private readonly CommandService _commands;

    private readonly IServiceProvider _services;
    private readonly AI.HuggingFaceService? _aiService;
    private readonly System.Collections.Concurrent.ConcurrentDictionary<ulong, string> _pendingAIRequests = new();

    private readonly HashSet<string> _validCommands = new HashSet<string>
    {
        "trade", "t", "clone", "fixOT", "fix", "f", "dittoTrade", "ditto", "dt", "itemTrade", "item", "it",
        "egg", "Egg", "hidetrade", "ht", "batchTrade", "bt", "listevents", "le",
        "eventrequest", "er", "battlereadylist", "brl", "battlereadyrequest", "brr", "pokepaste", "pp",
        "PokePaste", "PP", "randomteam", "rt", "RandomTeam", "Rt", "specialrequestpokemon", "srp",
        "queueStatus", "qs", "queueClear", "qc", "ts", "tc", "deleteTradeCode", "dtc", "mysteryegg", "me"
    };

    private readonly DiscordManager Manager;
    private readonly CancellationTokenSource _cts = new();
    private readonly DMRelayService? _dmRelay;

    public SysCord(PokeBotRunner<T> runner, ProgramConfig config)
    {
        _runner = runner;
        Runner = runner;
        Hub = runner.Hub;
        Manager = new DiscordManager(Hub.Config.Discord);
        _config = config;

        foreach (var bot in runner.Hub.Bots.ToArray())
        {
            if (bot is ITradeBot tradeBot)
            {
                tradeBot.ConnectionError += OnBotConnectionError;
                tradeBot.ConnectionSuccess += OnBotConnectionSuccess;
            }
        }
        
        runner.BotAdded += OnBotAdded;
        runner.BotRemoved += OnBotRemoved;

        SysCordSettings.Manager = Manager;
        SysCordSettings.HubConfig = Hub.Config;

        _client = new DiscordSocketClient(new DiscordSocketConfig
        {
            LogLevel = LogSeverity.Info,
            GatewayIntents = Guilds | GuildMessages | DirectMessages | GuildMembers | GuildPresences | MessageContent,
        });

        // ===== DM Relay Setup =====
        ulong forwardTargetId = 0;
        if (!string.IsNullOrWhiteSpace(Hub.Config.Discord.UserDMsToBotForwarder))
        {
            if (!ulong.TryParse(Hub.Config.Discord.UserDMsToBotForwarder, out forwardTargetId))
            {
                LogUtil.LogInfo("SysCord", $"Invalid UserDMsToBotForwarder ID: {Hub.Config.Discord.UserDMsToBotForwarder}");
            }
        }

        if (forwardTargetId != 0)
        {
            _dmRelay = new DMRelayService(_client, forwardTargetId);
            LogUtil.LogInfo("SysCord", $"DM relay active -> forwarding bot DMs to {forwardTargetId}");
        }

        _commands = new CommandService(new CommandServiceConfig
        {
            // Again, log level:
            LogLevel = LogSeverity.Info,

            // This makes commands get run on the task thread pool instead on the websocket read thread.
            // This ensures long-running logic can't block the websocket connection.
            DefaultRunMode = RunMode.Async,

            // There's a few more properties you can set,
            // for example, case-insensitive commands.
            CaseSensitiveCommands = false,
        });

        // Subscribe the logging handler to both the client and the CommandService.
        _client.Log += Log;
        _commands.Log += Log;

        _services = ConfigureServices();

        if (Hub.Config.Discord.AISettings.EnableAIChatbot && !string.IsNullOrWhiteSpace(Hub.Config.Discord.AISettings.HuggingFaceApiKey))
        {
            var aiSettings = Hub.Config.Discord.AISettings;
            _aiService = new AI.HuggingFaceService(
                aiSettings.HuggingFaceApiKey, 
                aiSettings.HuggingFaceModel, 
                aiSettings.MaxTokens, 
                aiSettings.Temperature, 
                aiSettings.TopP);
            SysCordSettings.AIService = _aiService;
        }

        _client.PresenceUpdated += Client_PresenceUpdated;
        _client.Disconnected += OnDisconnected;

        QueueMonitor<T>.OnQueueStatusChanged = (full, count, max) => Task.Run(() => EchoModule.SendQueueStatusEmbedAsync(full, count, max));
    }

    private Task OnDisconnected(Exception exception)
    {
        LogUtil.LogText($"Discord connection lost. Reason: {exception?.Message ?? "Unknown"}");
        Task.Run(() => ReconnectAsync(_cts.Token), _cts.Token);
        return Task.CompletedTask;
    }

    private void OnBotConnectionError(object? sender, Exception ex) => Task.Run(HandleBotStop, _cts.Token);
    private void OnBotConnectionSuccess(object? sender, EventArgs e) => Task.Run(HandleBotStart, _cts.Token);

    private void OnBotAdded(object? sender, PokeRoutineExecutorBase b)
    {
        if (b is ITradeBot tradeBot)
        {
            tradeBot.ConnectionError += OnBotConnectionError;
            tradeBot.ConnectionSuccess += OnBotConnectionSuccess;
        }
    }

    private void OnBotRemoved(object? sender, PokeRoutineExecutorBase b)
    {
        if (b is ITradeBot tradeBot)
        {
            tradeBot.ConnectionError -= OnBotConnectionError;
            tradeBot.ConnectionSuccess -= OnBotConnectionSuccess;
        }
    }

    public void Dispose()
    {
        _cts.Cancel();
        _cts.Dispose();
        _dmRelay?.Dispose();

        _client.Log -= Log;
        _commands.Log -= Log;
        _client.PresenceUpdated -= Client_PresenceUpdated;
        _client.Disconnected -= OnDisconnected;
        _client.Ready -= LoadLoggingAndEcho;
        _client.MessageReceived -= HandleMessageAsync;

        QueueMonitor<T>.OnQueueStatusChanged = null;
        _announcementMessageIds.Clear();

        LogModule.ClearAll();
        EchoModule.ClearAll();
        TradeStartModule<T>.ClearAll();
        DiscordManager.ClearAllCaches();

        if (Runner != null)
        {
            Runner.BotAdded -= OnBotAdded;
            Runner.BotRemoved -= OnBotRemoved;
            foreach (var bot in Runner.Hub.Bots.ToArray())
            {
                if (bot is ITradeBot tradeBot)
                {
                    tradeBot.ConnectionError -= OnBotConnectionError;
                    tradeBot.ConnectionSuccess -= OnBotConnectionSuccess;
                }
            }
        }
        
        // Clear static reference to prevent memory leaks when reloading environment
        if (ReferenceEquals(Runner, _runner))
            Runner = null!;

        try
        {
            _client.StopAsync().GetAwaiter().GetResult();
            _client.Dispose();
        }
        catch { }
    }

    public static PokeBotRunner<T> Runner { get; private set; } = default!;

    // Track loading of Echo/Logging channels, so they aren't loaded multiple times.
    private bool MessageChannelsLoaded { get; set; }

    private async Task ReconnectAsync(CancellationToken token)
    {
        const int maxRetries = 5;
        const int delayBetweenRetries = 5000; // 5 seconds
        const int initialDelay = 10000; // 10 seconds

        // Initial delay to allow Discord's automatic reconnection
        try { await Task.Delay(initialDelay, token).ConfigureAwait(false); } catch (OperationCanceledException) { return; }

        for (int i = 0; i < maxRetries && !token.IsCancellationRequested; i++)
        {
            try
            {
                if (_client.ConnectionState == ConnectionState.Connected)
                {
                    LogUtil.LogText("Client reconnected automatically.");
                    return; // Already reconnected
                }

                // Check if the client is in the process of reconnecting
                if (_client.ConnectionState == ConnectionState.Connecting)
                {
                    LogUtil.LogText("Client is already attempting to reconnect.");
                    try { await Task.Delay(delayBetweenRetries, token).ConfigureAwait(false); } catch (OperationCanceledException) { return; }
                    continue;
                }

                await _client.LoginAsync(TokenType.Bot, Hub.Config.Discord.Token).ConfigureAwait(false);
                await _client.StartAsync().ConfigureAwait(false);
                LogUtil.LogText("Reconnected successfully.");
                return;
            }
            catch (Exception ex)
            {
                LogUtil.LogText($"Reconnection attempt {i + 1} failed: {ex.Message}");
                if (i < maxRetries - 1)
                {
                    try { await Task.Delay(delayBetweenRetries, token).ConfigureAwait(false); } catch (OperationCanceledException) { return; }
                }
            }
        }

        if (token.IsCancellationRequested) return;

        // If all attempts to reconnect fail, stop and restart the bot
        LogUtil.LogText("Failed to reconnect after maximum attempts. Restarting the bot...");

        // Stop the bot
        await _client.StopAsync().ConfigureAwait(false);

        if (token.IsCancellationRequested) return;

        // Restart the bot
        await _client.LoginAsync(TokenType.Bot, Hub.Config.Discord.Token).ConfigureAwait(false);
        await _client.StartAsync().ConfigureAwait(false);

        LogUtil.LogText("Bot restarted successfully.");
    }

    public async Task AnnounceBotStatus(string status, EmbedColorOption color)
    {
        if (!SysCordSettings.Settings.BotEmbedStatus)
            return;

        var botName = string.IsNullOrEmpty(SysCordSettings.HubConfig.BotName) ? DudeBot.Name : SysCordSettings.HubConfig.BotName;
        var fullStatusMessage = $"**Status**: {botName} is {status}!";
        var thumbnailUrl = status == "Online"
            ? "https://raw.githubusercontent.com/NexusRisen/Nexus-Risen-Edition-Sprite-Images/main/Assets/Bot/Status/botgo.png"
            : "https://raw.githubusercontent.com/NexusRisen/Nexus-Risen-Edition-Sprite-Images/main/Assets/Bot/Status/botstop.png";

        var embed = new EmbedBuilder()
            .WithTitle("Bot Status Report")
            .WithDescription(fullStatusMessage)
            .WithColor(EmbedColorConverter.ToDiscordColor(color))
            .WithThumbnailUrl(thumbnailUrl)
            .WithTimestamp(DateTimeOffset.Now)
            .Build();

        foreach (var channelId in SysCordSettings.Manager.WhitelistedChannels.List.Select(channel => channel.ID))
        {
            try
            {
                IMessageChannel? channel = _client.GetChannel(channelId) as IMessageChannel;
                if (channel == null)
                {
                    channel = await _client.Rest.GetChannelAsync(channelId) as IMessageChannel;
                    if (channel == null)
                    {
                        LogUtil.LogInfo("SysCord", $"AnnounceBotStatus: Failed to find channel with ID {channelId} even after direct fetch.");
                        continue;
                    }
                }

                if (_announcementMessageIds.TryGetValue(channelId, out ulong messageId))
                {
                    try
                    {
                        await channel.DeleteMessageAsync(messageId);
                    }
                    catch
                    {
                        // Ignore exception when deleting previous message
                    }
                }

                var message = await channel.SendMessageAsync(embed: embed);
                _announcementMessageIds[channelId] = message.Id;
                LogUtil.LogInfo("SysCord", $"AnnounceBotStatus: {fullStatusMessage} announced in channel {channelId}.");

                if (SysCordSettings.Settings.ChannelStatus && channel is ITextChannel textChannel)
                {
                    var emoji = status == "Online" ? SysCordSettings.Settings.OnlineEmoji : SysCordSettings.Settings.OfflineEmoji;
                    var updatedChannelName = $"{emoji}{SysCord<T>.TrimStatusEmoji(textChannel.Name)}";
                    await textChannel.ModifyAsync(x => x.Name = updatedChannelName);
                }
            }
            catch (Exception ex)
            {
                LogUtil.LogInfo("SysCord", $"AnnounceBotStatus: Exception in channel {channelId}: {ex.Message}");
                // Continue to the next channel despite the exception
            }
        }
    }

    public async Task HandleBotStart()
    {
        try
        {
            await AnnounceBotStatus("Online", EmbedColorOption.Green);
        }
        catch (Exception ex)
        {
            LogUtil.LogText($"HandleBotStart: Exception when announcing bot start: {ex.Message}");
        }
    }

    public async Task HandleBotStop()
    {
        try
        {
            await AnnounceBotStatus("Offline", EmbedColorOption.Red);
        }
        catch (Exception ex)
        {
            LogUtil.LogText($"HandleBotStop: Exception when announcing bot stop: {ex.Message}");
        }
    }

    public async Task InitCommands()
    {
        var assembly = Assembly.GetExecutingAssembly();

        _client.Ready += () =>
        {
            DudeBot.Name = _client.CurrentUser.Username;
            return Task.CompletedTask;
        };

        await _commands.AddModulesAsync(assembly, _services).ConfigureAwait(false);
        foreach (var t in assembly.DefinedTypes.Where(z => z.IsSubclassOf(typeof(ModuleBase<SocketCommandContext>)) && z.IsGenericType))
        {
            var genModule = t.MakeGenericType(typeof(T));
            await _commands.AddModuleAsync(genModule, _services).ConfigureAwait(false);
        }
        var modules = _commands.Modules.ToList();

        var blacklist = Hub.Config.Discord.ModuleBlacklist
            .Replace("Module", "").Split(',', StringSplitOptions.RemoveEmptyEntries)
            .Select(z => z.Trim()).ToList();

        foreach (var module in modules)
        {
            var name = module.Name;
            name = name.Replace("Module", "");
            var gen = name.IndexOf('`');
            if (gen != -1)
                name = name[..gen];
            if (blacklist.Any(z => z.Equals(name, StringComparison.OrdinalIgnoreCase)))
                await _commands.RemoveModuleAsync(module).ConfigureAwait(false);
        }

        // Subscribe a handler to see if a message invokes a command.
        _client.Ready += LoadLoggingAndEcho;
        _client.MessageReceived += HandleMessageAsync;
    }

    public async Task MainAsync(string apiToken, CancellationToken token)
    {
        // Centralize the logic for commands into a separate method.
        await InitCommands().ConfigureAwait(false);

        // Login and connect.
        await _client.LoginAsync(TokenType.Bot, apiToken).ConfigureAwait(false);
        await _client.StartAsync().ConfigureAwait(false);

        var app = await _client.GetApplicationInfoAsync().ConfigureAwait(false);
        Manager.Owner = app.Owner.Id;
        try
        {
            // Wait infinitely so your bot actually stays connected.
            await MonitorStatusAsync(token).ConfigureAwait(false);
        }
        catch (OperationCanceledException)
        {
            // Handle the cancellation and perform cleanup tasks
            LogUtil.LogText("MainAsync: Bot is disconnecting due to cancellation...");
            await AnnounceBotStatus("Offline", EmbedColorOption.Red);
            LogUtil.LogText("MainAsync: Cleanup tasks completed.");
        }
        finally
        {
            // Disconnect the bot
            await _client.StopAsync();
        }
    }
    // If any services require the client, or the CommandService, or something else you keep on hand,
    // pass them as parameters into this method as needed.
    // If this method is getting pretty long, you can separate it out into another file using partials.
    private static ServiceProvider ConfigureServices()
    {
        var map = new ServiceCollection();//.AddSingleton(new SomeServiceClass());

        // When all your required services are in the collection, build the container.
        // Tip: There's an overload taking in a 'validateScopes' bool to make sure
        // you haven't made any mistakes in your dependency graph.
        return map.BuildServiceProvider();
    }

    // Example of a logging handler. This can be reused by add-ons
    // that ask for a Func<LogMessage, Task>.

    private static ConsoleColor GetTextColor(LogSeverity sv) => sv switch
    {
        LogSeverity.Critical => ConsoleColor.Red,
        LogSeverity.Error => ConsoleColor.Red,

        LogSeverity.Warning => ConsoleColor.Yellow,
        LogSeverity.Info => ConsoleColor.White,

        LogSeverity.Verbose => ConsoleColor.DarkGray,
        LogSeverity.Debug => ConsoleColor.DarkGray,
        _ => Console.ForegroundColor,
    };

    private static Task Log(LogMessage msg)
    {
        var text = $"[{msg.Severity,8}] {msg.Source}: {msg.Message} {msg.Exception}";
        Console.ForegroundColor = GetTextColor(msg.Severity);
        Console.WriteLine($"{DateTime.Now,-19} {text}");
        Console.ResetColor();

        LogUtil.LogText($"SysCord: {text}");

        return Task.CompletedTask;
    }

    private static async Task RespondToThanksMessage(SocketUserMessage msg)
    {
        var channel = msg.Channel;
        await channel.TriggerTypingAsync();
        await Task.Delay(500).ConfigureAwait(false);

        var responses = new List<string>
        {
            "You're welcome! ❤️",
            "No problem at all!",
            "Anytime, glad to help!",
            "It's my pleasure! ❤️",
            "Not a problem! You're welcome!",
            "Always here to help!",
            "Glad I could assist!",
            "Happy to serve!",
            "Of course! You're welcome!",
            "Sure thing!"
        };

        var randomResponse = responses[new Random().Next(responses.Count)];
        var finalResponse = $"{randomResponse}";

        await msg.Channel.SendMessageAsync(finalResponse).ConfigureAwait(false);
    }

    private static string TrimStatusEmoji(string channelName)
    {
        var onlineEmoji = SysCordSettings.Settings.OnlineEmoji;
        var offlineEmoji = SysCordSettings.Settings.OfflineEmoji;

        if (channelName.StartsWith(onlineEmoji))
        {
            return channelName[onlineEmoji.Length..].Trim();
        }

        if (channelName.StartsWith(offlineEmoji))
        {
            return channelName[offlineEmoji.Length..].Trim();
        }

        return channelName.Trim();
    }

    private Task Client_PresenceUpdated(SocketUser user, SocketPresence before, SocketPresence after)
    {
        return Task.CompletedTask;
    }

    private async Task HandleMessageAsync(SocketMessage arg)
    {
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        try
        {
            if (arg is not SocketUserMessage msg)
                return;

            if (msg.Channel is SocketGuildChannel guildChannel)
            {
                if (Manager.BlacklistedServers.Contains(guildChannel.Guild.Id))
                {
                    await guildChannel.Guild.LeaveAsync();
                    return;
                }
            }

            if (msg.Author.Id == _client.CurrentUser.Id || msg.Author.IsBot)
                return;

            string thanksText = msg.Content.ToLower();
            if (SysCordSettings.Settings.ReplyToThanks && (thanksText.Contains("thank") || thanksText.Contains("thx")))
            {
                await SysCord<T>.RespondToThanksMessage(msg).ConfigureAwait(false);
                return;
            }

            if (_pendingAIRequests.ContainsKey(msg.Author.Id))
            {
                if (await HandleAIPendingAsync(msg).ConfigureAwait(false))
                    return;
            }

            var correctPrefix = SysCordSettings.Settings.CommandPrefix;
            var content = msg.Content;
            var argPos = 0;

            if (msg.HasMentionPrefix(_client.CurrentUser, ref argPos) || msg.HasStringPrefix(correctPrefix, ref argPos))
            {
                var context = new SocketCommandContext(_client, msg);
                var handled = await TryHandleCommandAsync(msg, context, argPos);
                if (handled)
                    return;

                if (msg.HasMentionPrefix(_client.CurrentUser, ref argPos) && _aiService != null)
                {
                    await TryHandleAIAsync(msg, content[argPos..].Trim()).ConfigureAwait(false);
                    return;
                }
            }
            else if (content.Length > 1 && content[0] != correctPrefix[0])
            {
                var potentialPrefix = content[0].ToString();
                var command = content.Split(' ')[0][1..];
                if (_validCommands.Contains(command))
                {
                    await SafeSendMessageAsync(msg.Channel, $"Incorrect prefix! The correct command is **{correctPrefix}{command}**").ConfigureAwait(false);
                    return;
                }
            }

            if (msg.Attachments.Count > 0)
            {
                await TryHandleAttachmentAsync(msg).ConfigureAwait(false);
            }
        }
        catch (HttpException ex) when (ex.DiscordCode == DiscordErrorCode.InsufficientPermissions) // Missing Permissions
        {
            await Log(new LogMessage(LogSeverity.Warning, "Command", $"Missing permissions to handle a message in channel {arg.Channel.Name}")).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            await Log(new LogMessage(LogSeverity.Error, "Command", $"Unhandled exception in HandleMessageAsync: {ex.Message}", ex)).ConfigureAwait(false);
        }
        finally
        {
            stopwatch.Stop();
            if (stopwatch.ElapsedMilliseconds > 1000) // Log if processing takes more than 1 second
            {
                await Log(new LogMessage(LogSeverity.Warning, "Gateway",
                    $"A MessageReceived handler is blocking the gateway task. " +
                    $"Method: HandleMessageAsync, Execution Time: {stopwatch.ElapsedMilliseconds}ms, " +
                    $"Message Content: {arg.Content[..Math.Min(arg.Content.Length, 100)]}...")).ConfigureAwait(false);
            }
        }
    }

    private async Task LoadLoggingAndEcho()
    {
        if (MessageChannelsLoaded)
            return;

        // Restore Echoes
        EchoModule.RestoreChannels(_client, Hub.Config.Discord);

        // Restore Logging
        LogModule.RestoreLogging(_client, Hub.Config.Discord);
        TradeStartModule<T>.RestoreTradeStarting(_client);

        // Don't let it load more than once in case of Discord hiccups.
        await Log(new LogMessage(LogSeverity.Info, "LoadLoggingAndEcho()", "Logging and Echo channels loaded!")).ConfigureAwait(false);
        MessageChannelsLoaded = true;

        var game = Hub.Config.Discord.BotGameStatus;
        if (!string.IsNullOrWhiteSpace(game))
            await _client.SetGameAsync(game).ConfigureAwait(false);
    }

    private async Task MonitorStatusAsync(CancellationToken token)
    {
        const int Interval = 20; // seconds

        // Check datetime for update
        UserStatus state = UserStatus.Idle;
        while (!token.IsCancellationRequested)
        {
            var time = DateTime.Now;
            var lastLogged = LogUtil.LastLogged;
            if (Hub.Config.Discord.BotColorStatusTradeOnly)
            {
                var recent = Hub.Bots.ToArray()
                    .Where(z => z.Config.InitialRoutine.IsTradeBot())
                    .MaxBy(z => z.LastTime);
                lastLogged = recent?.LastTime ?? time;
            }
            var delta = time - lastLogged;
            var gap = TimeSpan.FromSeconds(Interval) - delta;

            bool noQueue = !Hub.Queues.Info.GetCanQueue();
            if (gap <= TimeSpan.Zero)
            {
                var idle = noQueue ? UserStatus.DoNotDisturb : UserStatus.Idle;
                if (idle != state)
                {
                    state = idle;
                    await _client.SetStatusAsync(state).ConfigureAwait(false);
                }
                await Task.Delay(2_000, token).ConfigureAwait(false);
                continue;
            }

            var active = noQueue ? UserStatus.DoNotDisturb : UserStatus.Online;
            if (active != state)
            {
                state = active;
                await _client.SetStatusAsync(state).ConfigureAwait(false);
            }
            await Task.Delay(gap, token).ConfigureAwait(false);
        }
    }


    private async Task TryHandleAttachmentAsync(SocketMessage msg)
    {
        var mgr = Manager;
        var cfg = mgr.Config;
        if (cfg.ConvertPKMToShowdownSet && (cfg.ConvertPKMReplyAnyChannel || mgr.CanUseCommandChannel(msg.Channel.Id)))
        {
            if (msg is SocketUserMessage userMessage)
            {
                foreach (var att in msg.Attachments)
                    await msg.Channel.RepostPKMAsShowdownAsync(att, userMessage).ConfigureAwait(false);
            }
        }
    }

    private async Task<bool> TryHandleCommandAsync(SocketUserMessage msg, SocketCommandContext context, int pos)
    {
        try
        {
            var AbuseSettings = Hub.Config.TradeAbuse;
            // Check if the user is in the bannedIDs list
            if (msg.Author is SocketGuildUser user && AbuseSettings.BannedIDs.List.Any(z => z.ID == user.Id))
            {
                await SysCord<T>.SafeSendMessageAsync(msg.Channel, "You are banned from using this bot.").ConfigureAwait(false);
                return true;
            }

            var mgr = Manager;
            if (!mgr.CanUseCommandUser(msg.Author.Id))
            {
                await SysCord<T>.SafeSendMessageAsync(msg.Channel, "You are not permitted to use this command.").ConfigureAwait(false);
                return true;
            }

            if (!mgr.CanUseCommandChannel(msg.Channel.Id) && msg.Author.Id != mgr.Owner)
            {
                if (Hub.Config.Discord.ReplyCannotUseCommandInChannel)
                    await SysCord<T>.SafeSendMessageAsync(msg.Channel, "You can't use that command here.").ConfigureAwait(false);
                return true;
            }

            var guild = msg.Channel is SocketGuildChannel g ? g.Guild.Name : "Unknown Guild";
            await Log(new LogMessage(LogSeverity.Info, "Command", $"Executing command from {guild}#{msg.Channel.Name}:@{msg.Author.Username}. Content: {msg}")).ConfigureAwait(false);

            var result = await _commands.ExecuteAsync(context, pos, _services).ConfigureAwait(false);

            if (result.Error == CommandError.UnknownCommand)
                return false;

            if (!result.IsSuccess)
                await SysCord<T>.SafeSendMessageAsync(msg.Channel, result.ErrorReason).ConfigureAwait(false);

            return true;
        }
        catch (Exception ex)
        {
            await Log(new LogMessage(LogSeverity.Error, "Command", $"Error executing command: {ex.Message}", ex)).ConfigureAwait(false);
            return false;
        }
    }

    private static async Task SafeSendMessageAsync(IMessageChannel channel, string message)
    {
        try
        {
            await channel.SendMessageAsync(message).ConfigureAwait(false);
        }
        catch (HttpException ex) when (ex.DiscordCode == DiscordErrorCode.InsufficientPermissions) // Missing Permissions
        {
            await Log(new LogMessage(LogSeverity.Warning, "Command", $"Missing permissions to send message in channel {channel.Name}")).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            await Log(new LogMessage(LogSeverity.Error, "Command", $"Error sending message: {ex.Message}", ex)).ConfigureAwait(false);
        }
    }

    private async Task TryHandleAIAsync(SocketUserMessage msg, string userRequest)
    {
        if (_aiService == null)
            return;

        try
        {
            await msg.Channel.TriggerTypingAsync();

            var botName = _client.CurrentUser?.Username ?? DudeBot.Name;
            var legalityContext = AI.PKHeXContextHelper.GetLegalityContext(userRequest);
            
            var systemPrompt = $"You are {botName}, the ultimate Pokemon assistant for a trade bot. " +
                         $"Your goal is to provide 100% legal, competitive, and authentic Pokemon Showdown sets. " +
                         $"\n\nSTRICT RULES:" +
                         $"\n1. LEGALITY: You MUST only provide legal Pokemon. Never suggest shiny-locked Pokemon as shiny (e.g., Koraidon, Miraidon, Victini, Hoopa). Verify that moves, abilities, and Pokeballs are legal for the specific species and game." +
                         $"\n2. SHOWDOWN FORMAT: Always provide sets in standard Pokemon Showdown format. Wrap them in [SHOWDOWN] and [/SHOWDOWN] tags." +
                         $"\n3. ALM OVERRIDES: You can use `~` overrides for complex legality requirements (e.g., `~Level: 50`, `~Shiny: Yes`, `~TeraType: Water`). This ensures the AutoLegality Mod (ALM) handles the specifics correctly." +
                         $"\n4. COMPETITIVE KNOWLEDGE: Use top-tier Smogon or VGC builds for competitive requests. Include optimized EVs, IVs, Natures, and Items." +
                         $"\n5. EVENTS & EGGS: You have complete knowledge of all historical events and egg moves. If an event Pokemon is requested, match its original OT, ID, and moveset perfectly." +
                         $"\n6. NO ILLEGALS: If a user asks for something illegal, politely explain why it's illegal and offer the closest legal alternative." +
                         $"\n\n{legalityContext}" +
                         $"\nExample Output:" +
                         $"\nUser: Give me a competitive Garchomp for Scarlet and Violet." +
                         $"\nAssistant: Here is a top-tier Jolly Garchomp for Scarlet and Violet singles:" +
                         $"\n[SHOWDOWN]" +
                         $"\nGarchomp @ Life Orb" +
                         $"\nAbility: Rough Skin" +
                         $"\nLevel: 100" +
                         $"\nTera Type: Ground" +
                         $"\nEVs: 252 Atk / 4 SpD / 252 Spe" +
                         $"\nJolly Nature" +
                         $"\n- Earthquake" +
                         $"\n- Dragon Claw" +
                         $"\n- Swords Dance" +
                         $"\n- Iron Head" +
                         $"\n~Ball: Luxury Ball" +
                         $"\n[/SHOWDOWN]" +
                         $"\n\nAlways be professional, concise, and helpful.";

            var userPrompt = $"Answer the following user request: {userRequest}";

            var response = await _aiService.GetAIResponseAsync(msg.Author.Id, userPrompt, systemPrompt);

            if (string.IsNullOrWhiteSpace(response))
            {
                await SafeSendMessageAsync(msg.Channel, "I'm sorry, I couldn't think of a response right now.");
                return;
            }

            await SafeSendMessageAsync(msg.Channel, response);

            // Check if there's a Showdown set in the response
            if (response.Contains("[SHOWDOWN]") && response.Contains("[/SHOWDOWN]"))
            {
                await HandleAIShowdownValidationAsync(msg, response, userRequest);
            }
        }
        catch (Exception ex)
        {
            await Log(new LogMessage(LogSeverity.Error, "AI", $"Error in TryHandleAIAsync: {ex.Message}", ex));
        }
    }

    private async Task HandleAIShowdownValidationAsync(SocketUserMessage msg, string response, string userRequest, int retryCount = 0)
    {
        var startIndex = response.IndexOf("[SHOWDOWN]") + "[SHOWDOWN]".Length;
        var endIndex = response.IndexOf("[/SHOWDOWN]");
        var showdownSet = response[startIndex..endIndex].Trim();

        if (string.IsNullOrWhiteSpace(showdownSet))
            return;

        var result = await Helpers<T>.ProcessShowdownSetAsync(showdownSet, false);
        if (result.Pokemon == null)
        {
            if (retryCount < 2 && _aiService != null)
            {
                await Log(new LogMessage(LogSeverity.Info, "AI", $"AI provided illegal set, requesting fix (Attempt {retryCount + 1}). Error: {result.Error}"));
                
                var legalityContext = AI.PKHeXContextHelper.GetLegalityContext(userRequest);
                var fixPrompt = $"{legalityContext}\n" +
                                $"The Showdown set you provided for '{userRequest}' is ILLEGAL. " +
                                $"Error: {result.Error}\n" +
                                $"Hint: {result.LegalizationHint}\n" +
                                $"Please provide a FIXED, 100% LEGAL version of this Pokemon set. " +
                                $"Remember to wrap the fixed set in [SHOWDOWN] and [/SHOWDOWN] tags.";
                
                var fixedResponse = await _aiService.GetAIResponseAsync(msg.Author.Id, fixPrompt);
                if (!string.IsNullOrWhiteSpace(fixedResponse) && fixedResponse.Contains("[SHOWDOWN]"))
                {
                    await HandleAIShowdownValidationAsync(msg, fixedResponse, userRequest, retryCount + 1);
                    return;
                }
            }

            await SafeSendMessageAsync(msg.Channel, $"I found a set, but it appears to be illegal: {result.Error}. I'm sorry I couldn't provide a legal version.");
            return;
        }

        _pendingAIRequests[msg.Author.Id] = showdownSet;
        await SafeSendMessageAsync(msg.Channel, "Would you like to be put in the queue for this Pokemon? (Yes/No)");
    }

    private async Task<bool> HandleAIPendingAsync(SocketUserMessage msg)
    {
        var content = msg.Content.ToLower().Trim();
        if (content == "yes" || content == "y")
        {
            if (_pendingAIRequests.TryRemove(msg.Author.Id, out var showdownSet))
            {
                await ProcessAIShowdownSetAsync(msg, showdownSet);
                return true;
            }
        }
        
        if (content == "no" || content == "n")
        {
            if (_pendingAIRequests.TryRemove(msg.Author.Id, out _))
            {
                await SafeSendMessageAsync(msg.Channel, "Understood. I won't add you to the queue.");
                return true;
            }
        }

        return false;
    }

    private async Task ProcessAIShowdownSetAsync(SocketUserMessage msg, string showdownSet)
    {
        try
        {
            var userID = msg.Author.Id;
            if (!await Helpers<T>.EnsureUserNotInQueueAsync(userID))
            {
                await SafeSendMessageAsync(msg.Channel, "You already have an existing trade in the queue. Please wait until it is processed.");
                return;
            }

            var result = await Helpers<T>.ProcessShowdownSetAsync(showdownSet, false);
            if (result.Pokemon == null)
            {
                await SafeSendMessageAsync(msg.Channel, "I found a Showdown set, but I couldn't process it. Make sure it's valid.");
                return;
            }

            var code = Hub.Queues.Info.GetRandomTradeCode(userID);
            var sig = msg.Author.GetFavor();

            await Helpers<T>.AddTradeToQueueAsync(
                new SocketCommandContext(_client, msg), code, msg.Author.Username, result.Pokemon, sig, msg.Author,
                lgcode: result.LgCode
            );

            await SafeSendMessageAsync(msg.Channel, "I've added that Pokemon to the queue for you!");
        }
        catch (Exception ex)
        {
            await Log(new LogMessage(LogSeverity.Error, "AI", $"Error processing AI Showdown set: {ex.Message}", ex));
        }
    }
}
