using Kook;
using Kook.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using PKHeX.Core;
using PKHeX.Core.AutoMod;
using SysBot.Base;
using SysBot.Pokemon.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace SysBot.Pokemon.Kook;

public static class SysKookSettings
{
    public static PokeTradeHubConfig HubConfig { get; internal set; } = default!;

    public static KookManager Manager { get; internal set; } = default!;

    public static KookSettings Settings => Manager.Config;
}

public sealed class SysKook<T> : IDisposable where T : PKM, new()
{
    public readonly PokeTradeHub<T> Hub;
    private readonly PokeBotRunner<T> _runner;
    private readonly ProgramConfig _config;
    private readonly KookSocketClient _client;

    private readonly HashSet<string> _validCommands = new HashSet<string>
    {
        "trade", "t", "clone", "fixOT", "fix", "f", "dittoTrade", "ditto", "dt", "itemTrade", "item", "it",
        "egg", "Egg", "hidetrade", "ht", "batchTrade", "bt", "listevents", "le",
        "eventrequest", "er", "battlereadylist", "brl", "battlereadyrequest", "brr", "pokepaste", "pp",
        "PokePaste", "PP", "randomteam", "rt", "RandomTeam", "Rt", "specialrequestpokemon", "srp",
        "queueStatus", "qs", "queueClear", "qc", "ts", "tc", "deleteTradeCode", "dtc", "mysteryegg", "me"
    };

    private readonly KookManager Manager;
    private readonly CancellationTokenSource _cts = new();

    public SysKook(PokeBotRunner<T> runner, ProgramConfig config)
    {
        _runner = runner;
        Runner = runner;
        Hub = runner.Hub;
        Manager = new KookManager(Hub.Config.Kook);
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

        SysKookSettings.Manager = Manager;
        SysKookSettings.HubConfig = Hub.Config;

        _client = new KookSocketClient(new KookSocketConfig
        {
            LogLevel = LogSeverity.Info,
        });

        _client.Log += Log;
        _client.Disconnected += OnDisconnected;
        _client.MessageReceived += OnMessageReceived;
    }

    private Task OnDisconnected(Exception exception)
    {
        LogUtil.LogText($"Kook connection lost. Reason: {exception?.Message ?? "Unknown"}");
        Task.Run(() => ReconnectAsync(_cts.Token), _cts.Token);
        return Task.CompletedTask;
    }

    private Task OnMessageReceived(SocketMessage msg, SocketUser author, ISocketMessageChannel channel) => HandleMessageAsync(msg);

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

    private void OnBotConnectionError(object? sender, Exception ex) => Task.Run(() => HandleBotStop(ex), _cts.Token);
    private void OnBotConnectionSuccess(object? sender, EventArgs e) => Task.Run(HandleBotStart, _cts.Token);

    public async Task AnnounceBotStatus(string status, bool isOnline)
    {
        if (!SysKookSettings.Settings.BotStatusAnnouncement)
            return;

        var botName = string.IsNullOrEmpty(Hub.Config.BotName) ? "DudeBot" : Hub.Config.BotName;
        var emoji = isOnline ? SysKookSettings.Settings.OnlineEmoji : SysKookSettings.Settings.OfflineEmoji;
        var fullStatusMessage = $"{emoji} **Status**: {botName} is {status}!";

        var cardBuilder = new CardBuilder()
            .AddModule<SectionModuleBuilder>(s => s.WithText(fullStatusMessage, true));

        var card = cardBuilder.Build();

        foreach (var channelId in SysKookSettings.Manager.Config.ChannelWhitelist.List.Select(c => c.ID))
        {
            try
            {
                var channel = await _client.GetChannelAsync(channelId) as IMessageChannel;
                if (channel != null)
                {
                    await channel.SendCardAsync(card);
                    LogUtil.LogInfo("SysKook", $"AnnounceBotStatus: {status} announced in channel {channelId}.");
                }
            }
            catch (Exception ex)
            {
                LogUtil.LogInfo("SysKook", $"AnnounceBotStatus: Exception in channel {channelId}: {ex.Message}");
            }
        }
    }

    private async Task HandleBotStop(Exception ex)
    {
        LogUtil.LogInfo("SysKook", $"Bot connection error: {ex.Message}. Notifying Kook users.");
        await AnnounceBotStatus("Offline", false);
    }

    private async Task HandleBotStart()
    {
        LogUtil.LogInfo("SysKook", "Bot connection success. Notifying Kook users.");
        await AnnounceBotStatus("Online", true);
    }

    public void Dispose()
    {
        _cts.Cancel();
        _cts.Dispose();

        _client.Log -= Log;
        _client.Disconnected -= OnDisconnected;
        _client.MessageReceived -= OnMessageReceived;

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
        
        if (ReferenceEquals(Runner, _runner))
            Runner = null!;

        if (ReferenceEquals(SysKookSettings.Manager, Manager))
            SysKookSettings.Manager = default!;
        if (ReferenceEquals(SysKookSettings.HubConfig, Hub.Config))
            SysKookSettings.HubConfig = default!;

        try
        {
            _client.StopAsync().GetAwaiter().GetResult();
            _client.Dispose();
        }
        catch { }
    }

    public static PokeBotRunner<T> Runner { get; private set; } = default!;

    private async Task ReconnectAsync(CancellationToken token)
    {
        const int maxRetries = 5;
        const int delayBetweenRetries = 5000;

        try { await Task.Delay(10000, token).ConfigureAwait(false); } catch (OperationCanceledException) { return; }

        for (int i = 0; i < maxRetries && !token.IsCancellationRequested; i++)
        {
            try
            {
                if (_client.ConnectionState == ConnectionState.Connected) return;

                await _client.LoginAsync(TokenType.Bot, Hub.Config.Kook.Token).ConfigureAwait(false);
                await _client.StartAsync().ConfigureAwait(false);
                return;
            }
            catch (Exception ex)
            {
                LogUtil.LogText($"Kook reconnection attempt {i + 1} failed: {ex.Message}");
                if (i < maxRetries - 1)
                {
                    try { await Task.Delay(delayBetweenRetries, token).ConfigureAwait(false); } catch (OperationCanceledException) { return; }
                }
            }
        }
    }

    public async Task MainAsync(string token, CancellationToken ct)
    {
        try
        {
            await _client.LoginAsync(TokenType.Bot, token).ConfigureAwait(false);
            await _client.StartAsync().ConfigureAwait(false);

            await Task.Delay(-1, ct).ConfigureAwait(false);
        }
        catch (OperationCanceledException) { }
        catch (Exception ex)
        {
            LogUtil.LogError(ex.Message, "SysKook");
        }
    }

    private Task Log(LogMessage msg)
    {
        LogUtil.LogText(msg.ToString());
        return Task.CompletedTask;
    }

    private async Task HandleMessageAsync(SocketMessage message)
    {
        if (message.Author.IsBot ?? false) return;
        if (!Manager.CanUseCommandChannel(message.Channel.Id)) return;

        var content = message.Content;
        var prefix = Hub.Config.Kook.CommandPrefix;
        if (!content.StartsWith(prefix)) return;
        
        var parts = content.Substring(prefix.Length).Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length == 0) return;
        
        var cmd = parts[0].ToLower();
        if (!_validCommands.Contains(cmd)) return;

        if (Hub.Config.Kook.UserBlacklist.Contains(message.Author.Id)) return;

        LogUtil.LogText($"Kook Command received: {cmd} from {message.Author.Username}");
        
        if (cmd == "ts")
        {
            var response = await message.Channel.SendTextAsync($"Hello {message.Author.Username}, I am online!");
            if (Hub.Config.Kook.MessageDeletionEnabled)
            {
                _ = Task.Run(async () =>
                {
                    await Task.Delay(Hub.Config.Kook.ErrorMessageDeleteDelaySeconds * 1000);
                    try { var msg = await response.DownloadAsync(); if (msg != null) await msg.DeleteAsync(); } catch { }
                });
            }
            return;
        }

        if (cmd == "trade" || cmd == "t")
        {
            if (!await CheckPermissions(message, Hub.Config.Kook.RoleCanTrade)) return;
            await HandleTradeCommandAsync(message, parts.Skip(1).ToList());
        }
        else if (cmd == "itemtrade" || cmd == "item" || cmd == "it")
        {
            if (!await CheckPermissions(message, Hub.Config.Kook.RoleCanTrade)) return;
            await HandleItemTradeCommandAsync(message, parts.Skip(1).ToList());
        }
        else if (cmd == "clone" || cmd == "c")
        {
            if (!await CheckPermissions(message, Hub.Config.Kook.RoleCanClone)) return;
            await HandleCloneCommandAsync(message, parts.Skip(1).ToList());
        }
        else if (cmd == "fixot" || cmd == "fix" || cmd == "f")
        {
            if (!await CheckPermissions(message, Hub.Config.Kook.RoleCanFixOT)) return;
            await HandleFixOTCommandAsync(message, parts.Skip(1).ToList());
        }
        else if (cmd == "dittotrade" || cmd == "ditto" || cmd == "dt")
        {
            if (!await CheckPermissions(message, Hub.Config.Kook.RoleCanTrade)) return;
            await HandleDittoTradeCommandAsync(message, parts.Skip(1).ToList());
        }
        else if (cmd == "queuestatus" || cmd == "qs")
        {
            await HandleQueueStatusCommandAsync(message);
        }
        else if (cmd == "queueclear" || cmd == "qc" || cmd == "tc")
        {
            await HandleQueueClearCommandAsync(message);
        }
        else if (cmd == "egg")
        {
            if (!await CheckPermissions(message, Hub.Config.Kook.RoleCanTrade)) return;
            await HandleEggCommandAsync(message, parts.Skip(1).ToList());
        }
        else if (cmd == "mysteryegg" || cmd == "me")
        {
            if (!await CheckPermissions(message, Hub.Config.Kook.RoleCanTrade)) return;
            await HandleMysteryEggCommandAsync(message);
        }
        else if (cmd == "batchtrade" || cmd == "bt")
        {
            if (!await CheckPermissions(message, Hub.Config.Kook.RoleCanTrade)) return;
            await HandleBatchTradeCommandAsync(message, parts.Skip(1).ToList());
        }
        else if (cmd == "pokepaste" || cmd == "pp")
        {
            if (!await CheckPermissions(message, Hub.Config.Kook.RoleCanTrade)) return;
            await HandlePokepasteCommandAsync(message, parts.Skip(1).ToList());
        }
        else if (cmd == "specialrequestpokemon" || cmd == "srp")
        {
            if (!await CheckPermissions(message, Hub.Config.Kook.RoleCanTrade)) return;
            await HandleSpecialRequestPokemonCommandAsync(message, parts.Skip(1).ToList());
        }
        else if (cmd == "deletetradecode" || cmd == "dtc")
        {
            await HandleDeleteTradeCodeCommandAsync(message);
        }
    }

    private async Task<bool> CheckPermissions(SocketMessage message, RemoteControlAccessList allowedRoles, bool sudoOnly = false)
    {
        if (Hub.Config.Kook.GlobalSudoList.Contains(message.Author.Id)) return true;
        if (sudoOnly) 
        {
            await message.Channel.SendTextAsync("This command is restricted to bot administrators.");
            return false;
        }

        if (allowedRoles.AllowIfEmpty && allowedRoles.List.Count == 0) return true;

        if (message.Author is IGuildUser guildUser)
        {
            foreach (var roleId in guildUser.RoleIds)
            {
                if (allowedRoles.Contains(roleId)) return true;
                if (Hub.Config.Kook.RoleSudo.Contains(roleId)) return true;
            }
        }

        await message.Channel.SendTextAsync("You do not have the required roles to use this command.");
        return false;
    }

    private async Task HandleTradeCommandAsync(SocketMessage message, List<string> args)
    {
        if (args.Count == 0)
        {
            await message.Channel.SendTextAsync("Please provide a Showdown set or a trade code.");
            return;
        }

        List<Pictocodes>? lgCode = null;
        int code = 0;
        string showdownSet = string.Empty;

        if (args.Count >= 3)
        {
            var potentialLgCode = PictocodeConverter.ConvertFromStrings(args.Take(3).ToList());
            if (potentialLgCode.Count == 3)
            {
                lgCode = potentialLgCode;
                showdownSet = string.Join("\n", args.Skip(3));
            }
        }

        if (lgCode == null)
        {
            if (int.TryParse(args[0], out code))
                showdownSet = string.Join("\n", args.Skip(1));
            else
            {
                code = Hub.Queues.Info.GetRandomTradeCode(message.Author.Id);
                showdownSet = string.Join("\n", args);
            }
        }

        if (string.IsNullOrWhiteSpace(showdownSet))
        {
            await message.Channel.SendTextAsync("Please provide a Showdown set.");
            return;
        }

        var result = await KookHelper<T>.ProcessShowdownSetAsync(showdownSet);
        if (result.Pokemon == null)
        {
            var errorMsg = $"Oops! I couldn't parse that Showdown set.\nReason: {result.Error}";
            if (!string.IsNullOrEmpty(result.LegalizationHint))
                errorMsg += $"\nHint: {result.LegalizationHint}";
            await message.Channel.SendTextAsync(errorMsg);
            return;
        }

        await KookHelper<T>.AddToQueueAsync(message, code, message.Author.Username, result.Pokemon, message.Author, _client, result.LgCode);
    }

    private async Task HandleItemTradeCommandAsync(SocketMessage message, List<string> args)
    {
        if (args.Count == 0)
        {
            await message.Channel.SendTextAsync("Please provide at least one item name.");
            return;
        }

        var itemInput = string.Join(" ", args);
        var itemNames = itemInput.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        
        if (typeof(T) == typeof(PB7) && itemNames.Length > 1)
        {
            await message.Channel.SendTextAsync("Batch trades are not supported in Let's Go Pikachu/Eevee.");
            return;
        }

        var batchSettings = Hub.Config.Trade.BatchSettings;
        if (itemNames.Length > 1 && (!batchSettings.AllowBatchTrades || itemNames.Length > batchSettings.MaxItemBatchAmount))
        {
            await message.Channel.SendTextAsync($"Batch trades are limited to {batchSettings.MaxItemBatchAmount} items.");
            return;
        }

        var pkmList = new List<T>();
        var tradeConfig = Hub.Config.Trade.TradeConfiguration;
        Species species = tradeConfig.ItemTradeSpecies == Species.None ? Species.Pikachu : tradeConfig.ItemTradeSpecies;
        var baseName = SpeciesName.GetSpeciesNameGeneration((ushort)species, 2, 8);

        foreach (var itemName in itemNames)
        {
            var set = new ShowdownSet($"{baseName} @ {itemName}");
            var template = AutoLegalityWrapper.GetTemplate(set);
            var sav = AutoLegalityWrapper.GetTrainerInfo<T>();
            var pkm = sav.GetLegal(template, out _);

            if (pkm != null)
            {
                pkm = EntityConverter.ConvertToType(pkm, typeof(T), out _) ?? pkm;
                if (pkm.HeldItem != 0 && !TradeRestrictions.IsUntradableHeld(pkm.Context, pkm.HeldItem) && pkm is T pk && new LegalityAnalysis(pk).Valid)
                {
                    pk.ResetPartyStats();
                    pkmList.Add(pk);
                }
            }
        }

        if (pkmList.Count == 0)
        {
            await message.Channel.SendTextAsync("No valid items could be processed.");
            return;
        }

        int code = Hub.Queues.Info.GetRandomTradeCode(message.Author.Id);
        if (pkmList.Count == 1)
            await KookHelper<T>.AddToQueueAsync(message, code, message.Author.Username, pkmList[0], message.Author, _client);
        else
            await KookHelper<T>.AddBatchContainerToQueueAsync(message, code, message.Author.Username, pkmList[0], pkmList, message.Author, _client);
    }

    private async Task HandleCloneCommandAsync(SocketMessage message, List<string> args)
    {
        if (typeof(T) == typeof(PB8))
        {
            await message.Channel.SendTextAsync("Cloning in BDSP is disabled.");
            return;
        }

        if (Hub.Queues.Info.IsUserInQueue(message.Author.Id))
        {
            await message.Channel.SendTextAsync("You are already in the queue.");
            return;
        }

        int code = args.Count > 0 && int.TryParse(args[0], out var c) ? c : Hub.Queues.Info.GetRandomTradeCode(message.Author.Id);
        var lgcode = Hub.Queues.Info.GetRandomLGTradeCode(message.Author.Id);
        var notifier = new KookTradeNotifier<T>(new T(), new PokeTradeTrainerInfo(message.Author.Username, message.Author.Id), code, message.Author, _client, lgcode);
        int uniqueID = TradeUtil.GenerateUniqueTradeID();
        var detail = new PokeTradeDetail<T>(new T(), new PokeTradeTrainerInfo(message.Author.Username, message.Author.Id), notifier, PokeTradeType.Clone, code, false, lgcode, 1, 1, false, false, uniqueID);
        var trade = new TradeEntry<T>(detail, message.Author.Id, PokeRoutineType.Clone, message.Author.Username, uniqueID);
        
        if (Hub.Queues.Info.AddToTradeQueue(trade, message.Author.Id, false, Hub.Config.Kook.GlobalSudoList.Contains(message.Author.Id)) == QueueResultAdd.Added)
        {
            await message.Channel.SendTextAsync($"Processing clone request... Code: {code:D8}");
            await notifier.SendInitialQueueUpdate();
        }
    }

    private async Task HandleFixOTCommandAsync(SocketMessage message, List<string> args)
    {
        if (Hub.Queues.Info.IsUserInQueue(message.Author.Id))
        {
            await message.Channel.SendTextAsync("You are already in the queue.");
            return;
        }

        int code = args.Count > 0 && int.TryParse(args[0], out var c) ? c : Hub.Queues.Info.GetRandomTradeCode(message.Author.Id);
        var lgcode = Hub.Queues.Info.GetRandomLGTradeCode(message.Author.Id);
        var notifier = new KookTradeNotifier<T>(new T(), new PokeTradeTrainerInfo(message.Author.Username, message.Author.Id), code, message.Author, _client, lgcode);
        int uniqueID = TradeUtil.GenerateUniqueTradeID();
        var detail = new PokeTradeDetail<T>(new T(), new PokeTradeTrainerInfo(message.Author.Username, message.Author.Id), notifier, PokeTradeType.FixOT, code, false, lgcode, 1, 1, false, false, uniqueID);
        var trade = new TradeEntry<T>(detail, message.Author.Id, PokeRoutineType.FixOT, message.Author.Username, uniqueID);
        
        if (Hub.Queues.Info.AddToTradeQueue(trade, message.Author.Id, false, Hub.Config.Kook.GlobalSudoList.Contains(message.Author.Id)) == QueueResultAdd.Added)
        {
            await message.Channel.SendTextAsync($"Processing fixOT request... Code: {code:D8}");
            await notifier.SendInitialQueueUpdate();
        }
    }

    private async Task HandleDittoTradeCommandAsync(SocketMessage message, List<string> args)
    {
        if (args.Count < 3)
        {
            await message.Channel.SendTextAsync("Usage: ditto <IVs/Keyword> <Language> <Nature>");
            return;
        }

        if (Hub.Queues.Info.IsUserInQueue(message.Author.Id))
        {
            await message.Channel.SendTextAsync("You are already in the queue.");
            return;
        }

        if (!Enum.TryParse(args[1], true, out LanguageID lang)) return;
        var nature = args[2].Trim()[..1].ToUpper() + args[2].Trim()[1..].ToLower();
        var set = new ShowdownSet($"{args[0]}(Ditto)\nLanguage: {lang}\nNature: {nature}");
        var pkm = AutoLegalityWrapper.GetTrainerInfo<T>().GetLegal(AutoLegalityWrapper.GetTemplate(set), out _);

        if (pkm != null)
        {
            TradeExtensions<T>.DittoTrade((T)pkm);
            var pk = (T)pkm; pk.ResetPartyStats();
            await KookHelper<T>.AddToQueueAsync(message, Hub.Queues.Info.GetRandomTradeCode(message.Author.Id), message.Author.Username, pk, message.Author, _client);
        }
    }

    private async Task HandleQueueStatusCommandAsync(SocketMessage message)
    {
        var pos = Hub.Queues.Info.CheckPosition(message.Author.Id, 0);
        if (pos.Position == -1)
            await message.Channel.SendTextAsync($"{message.Author.Username}, you are not in the queue.");
        else
            await message.Channel.SendTextAsync($"{message.Author.Username}, position {pos.Position}. ETA: {Hub.Config.Queues.EstimateDelay(pos.Position, Hub.Bots.Count):F1} min(s).");
    }

    private async Task HandleQueueClearCommandAsync(SocketMessage message)
    {
        if (Hub.Queues.Info.ClearTrade(message.Author.Id) != QueueResultRemove.NotInQueue)
            await message.Channel.SendTextAsync($"{message.Author.Username}, removed from queue.");
        else
            await message.Channel.SendTextAsync($"{message.Author.Username}, not in queue.");
    }

    private async Task HandleDeleteTradeCodeCommandAsync(SocketMessage message)
    {
        if (new TradeCodeStorage().DeleteTradeCode(message.Author.Id))
            await message.Channel.SendTextAsync("Deleted stored trade code.");
        else
            await message.Channel.SendTextAsync("No stored trade code found.");
    }

    private async Task HandleEggCommandAsync(SocketMessage message, List<string> args)
    {
        if (args.Count == 0 || Hub.Queues.Info.IsUserInQueue(message.Author.Id)) return;
        var pkm = AutoLegalityWrapper.GetTrainerInfo<T>().GenerateEgg(AutoLegalityWrapper.GetTemplate(new ShowdownSet(string.Join(" ", args))), out var res);
        if (res == LegalizationResult.Regenerated && pkm != null)
        {
            var pk = EntityConverter.ConvertToType(pkm, typeof(T), out _) as T ?? (T)pkm;
            pk.ResetPartyStats();
            await KookHelper<T>.AddToQueueAsync(message, Hub.Queues.Info.GetRandomTradeCode(message.Author.Id), message.Author.Username, pk, message.Author, _client);
        }
    }

    private async Task HandleMysteryEggCommandAsync(SocketMessage message)
    {
        if (Hub.Queues.Info.IsUserInQueue(message.Author.Id)) return;
        var egg = TradeModuleHelpers.GenerateLegalMysteryEgg<T>();
        if (egg != null)
            await KookHelper<T>.AddToQueueAsync(message, Hub.Queues.Info.GetRandomTradeCode(message.Author.Id), message.Author.Username, egg, message.Author, _client);
    }

    private async Task HandleBatchTradeCommandAsync(SocketMessage message, List<string> args)
    {
        if (args.Count == 0 || Hub.Queues.Info.IsUserInQueue(message.Author.Id)) return;
        var sets = TradeModuleHelpers.ParseBatchTradeContent(BatchNormalizer.NormalizeBatchCommands(string.Join(" ", args)));
        if (sets.Count > Hub.Config.Trade.BatchSettings.MaxPkmsPerTrade) return;

        var pkmList = new List<T>();
        foreach (var s in sets)
        {
            var res = await KookHelper<T>.ProcessShowdownSetAsync(s);
            if (res.Pokemon != null) pkmList.Add(res.Pokemon);
        }

        if (pkmList.Count > 0)
            await KookHelper<T>.AddBatchContainerToQueueAsync(message, Hub.Queues.Info.GetRandomTradeCode(message.Author.Id), message.Author.Username, pkmList[0], pkmList, message.Author, _client);
    }

    private async Task HandlePokepasteCommandAsync(SocketMessage message, List<string> args)
    {
        if (args.Count == 0) return;
        try
        {
            var sets = ParseShowdownSets(await SysBot.Pokemon.Helpers.NetUtil.HttpClient.GetStringAsync(args[0]));
            if (sets.Count == 0) return;
            await message.Channel.SendTextAsync($"Found {sets.Count} Pokémon. Processing...");
            _ = Task.Run(async () => {
                foreach (var s in sets) {
                    var res = await KookHelper<T>.ProcessShowdownSetAsync(string.Join("\n", s.GetSetLines()));
                    if (res.Pokemon != null) await message.Author.SendTextAsync($"Generated {res.Pokemon.Species} from Pokepaste.");
                }
            });
        } catch { }
    }

    private static List<ShowdownSet> ParseShowdownSets(string html)
    {
        var list = new List<ShowdownSet>();
        foreach (System.Text.RegularExpressions.Match m in new System.Text.RegularExpressions.Regex(@"<pre>(.*?)</pre>", System.Text.RegularExpressions.RegexOptions.Singleline).Matches(html))
            list.Add(new ShowdownSet(System.Net.WebUtility.HtmlDecode(System.Text.RegularExpressions.Regex.Replace(m.Groups[1].Value, "<.*?>", string.Empty))));
        return list;
    }

    private async Task HandleSpecialRequestPokemonCommandAsync(SocketMessage message, List<string> args)
    {
        if (args.Count == 0) return;
        var eventData = TradeModuleHelpers.GetEventData(args[0]);
        if (eventData == null) return;

        if (args.Count > 1 && int.TryParse(args[1], out int index))
        {
            if (Hub.Queues.Info.IsUserInQueue(message.Author.Id)) return;
            var entityEvents = eventData.Where(gift => gift.IsEntity && !gift.IsItem).ToArray();
            if (index < 1 || index > entityEvents.Length) return;
            var pk = TradeModuleHelpers.ConvertEventToPKM<T>(entityEvents[index - 1]);
            if (pk != null)
                await KookHelper<T>.AddToQueueAsync(message, Hub.Queues.Info.GetRandomTradeCode(message.Author.Id), message.Author.Username, pk, message.Author, _client);
        }
        else await message.Channel.SendTextAsync("Event listing restricted to Discord.");
    }
}

public class KookManager(KookSettings Config)
{
    public readonly KookSettings Config = Config;
    public bool CanUseCommandChannel(ulong id) => Config.ChannelWhitelist.List.Count == 0 ? Config.ChannelWhitelist.AllowIfEmpty : Config.ChannelWhitelist.Contains(id);
}
