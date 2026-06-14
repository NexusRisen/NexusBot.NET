using PKHeX.Core;
using PKHeX.Core.AutoMod;
using SysBot.Base;
using SysBot.Pokemon.Helpers;
using SysBot.Pokemon.Stoat.Helpers;
using StoatSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SysBot.Pokemon.Stoat;

public static class SysStoatSettings
{
    public static PokeTradeHubConfig HubConfig { get; internal set; } = default!;
    public static StoatSettings Settings => HubConfig.Stoat;
}

public sealed class SysStoat<T> : IDisposable where T : PKM, new()
{
    public readonly PokeTradeHub<T> Hub;
    private readonly PokeBotRunner<T> _runner;
    private readonly ProgramConfig _config;
    
    private StoatClient _client = null!;

    private readonly HashSet<string> _validCommands = new HashSet<string>
    {
        "trade", "t", "clone", "fixOT", "fix", "f", "dittoTrade", "ditto", "dt", "itemTrade", "item", "it",
        "egg", "Egg", "hidetrade", "ht", "batchTrade", "bt", "listevents", "le",
        "eventrequest", "er", "battlereadylist", "brl", "battlereadyrequest", "brr", "pokepaste", "pp",
        "PokePaste", "PP", "randomteam", "rt", "RandomTeam", "Rt", "specialrequestpokemon", "srp",
        "queueStatus", "qs", "queueClear", "qc", "ts", "tc", "deleteTradeCode", "dtc", "mysteryegg", "me", "id",
        "linkcode", "link", "medals", "ml", "leaderboard", "lb", "halloffame", "hof"
    };

    private readonly CancellationTokenSource _cts = new();

    public SysStoat(PokeBotRunner<T> runner, ProgramConfig config)
    {
        _runner = runner;
        Runner = runner;
        Hub = runner.Hub;
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

        SysStoatSettings.HubConfig = Hub.Config;
    }

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
        if (!SysStoatSettings.Settings.ChannelStatus)
            return;

        var botName = string.IsNullOrEmpty(Hub.Config.BotName) ? "DudeBot" : Hub.Config.BotName;
        var emoji = isOnline ? SysStoatSettings.Settings.OnlineEmoji : SysStoatSettings.Settings.OfflineEmoji;
        var fullStatusMessage = $"{emoji} **Status**: {botName} is {status}!";

        foreach (var channelId in SysStoatSettings.Settings.ChannelWhitelist.List.Select(c => c.ID.ToString())) // wait, ids might be custom
        {
            try
            {
                // We don't have the exact channel string easily from ulong config if the user configured it poorly, 
                // but we can try iterating through the actual string. Wait, we don't have a way to convert ulong back to Revolt string.
                // It's a limitation for Channel Status unless configured correctly or using Stoat-specific lists.
            }
            catch (Exception ex)
            {
                LogUtil.LogInfo("SysStoat", $"AnnounceBotStatus: Exception: {ex.Message}");
            }
        }
    }

    private async Task HandleBotStop(Exception ex)
    {
        LogUtil.LogInfo("SysStoat", $"Bot connection error: {ex.Message}.");
        await AnnounceBotStatus("Offline", false);
    }

    private async Task HandleBotStart()
    {
        LogUtil.LogInfo("SysStoat", "Bot connection success.");
        await AnnounceBotStatus("Online", true);
    }

    public void Dispose()
    {
        _cts.Cancel();
        _cts.Dispose();

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

        if (ReferenceEquals(SysStoatSettings.HubConfig, Hub.Config))
            SysStoatSettings.HubConfig = default!;

        try
        {
            _client?.StopAsync();
        }
        catch { }
    }

    public static PokeBotRunner<T> Runner { get; private set; } = default!;

    public async Task MainAsync(string token, CancellationToken ct)
    {
        try
        {
            _client = new StoatClient(ClientMode.WebSocket, new ClientConfig());
            await _client.LoginAsync(token, AccountType.Bot);
            
            _client.OnMessageRecieved += Client_OnMessageReceived;
            _client.OnReady += Client_OnReady;

            await _client.StartAsync().ConfigureAwait(false);

            await Task.Delay(-1, ct).ConfigureAwait(false);
        }
        catch (OperationCanceledException) { }
        catch (Exception ex)
        {
            LogUtil.LogError(ex.Message, "SysStoat");
        }
    }

    private void Client_OnReady(SelfUser selfUser)
    {
        LogUtil.LogInfo("Stoat Bot Connected successfully.", "SysStoat");
    }

    private async void Client_OnMessageReceived(Message message)
    {
        if (message is not UserMessage userMessage || message.Author.IsBot) return;

        ulong channelIdNumeric = StoatHelper<T>.ConvertId(message.ChannelId);
        if (!Hub.Config.Stoat.ChannelWhitelist.Contains(channelIdNumeric) && Hub.Config.Stoat.ChannelWhitelist.List.Count > 0)
            return;

        var content = userMessage.Content;
        if (string.IsNullOrEmpty(content)) return;

        var prefix = Hub.Config.Stoat.CommandPrefix;
        if (!content.StartsWith(prefix)) return;
        var parts = content.Substring(prefix.Length).Split(new[] { ' ', '\n' }, StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length == 0) return;
        
        var cmd = parts[0].ToLower();
        if (!_validCommands.Contains(cmd)) return;

        ulong userIdNumeric = StoatHelper<T>.ConvertId(message.AuthorId);

        if (Hub.Config.Stoat.UserBlacklist.Contains(userIdNumeric)) return;

        LogUtil.LogText($"Stoat Command received: {cmd} from {message.Author.Username}");

        try
        {
            if (cmd == "ts")
            {
                await StoatHelper<T>.SendAsync(_client, message.ChannelId, $"Hello <@{message.AuthorId}>, I am online!");
                return;
            }
            if (cmd == "id")
            {
                await StoatHelper<T>.SendAsync(_client, message.ChannelId, $"User ID: {message.AuthorId} (Numeric: {userIdNumeric})\nChannel ID: {message.ChannelId} (Numeric: {channelIdNumeric})");
                return;
            }

            if (cmd == "trade" || cmd == "t")
            {
                if (!await CheckPermissions(userMessage, Hub.Config.Stoat.RoleCanTrade)) return;
                await HandleTradeCommandAsync(userMessage, parts.Skip(1).ToList());
            }
            else if (cmd == "itemtrade" || cmd == "item" || cmd == "it")
            {
                if (!await CheckPermissions(userMessage, Hub.Config.Stoat.RoleCanTrade)) return;
                await HandleItemTradeCommandAsync(userMessage, parts.Skip(1).ToList());
            }
            else if (cmd == "clone" || cmd == "c")
            {
                if (!await CheckPermissions(userMessage, Hub.Config.Stoat.RoleCanClone)) return;
                await HandleCloneCommandAsync(userMessage, parts.Skip(1).ToList());
            }
            else if (cmd == "fixot" || cmd == "fix" || cmd == "f")
            {
                if (!await CheckPermissions(userMessage, Hub.Config.Stoat.RoleCanFixOT)) return;
                await HandleFixOTCommandAsync(userMessage, parts.Skip(1).ToList());
            }
            else if (cmd == "dittotrade" || cmd == "ditto" || cmd == "dt")
            {
                if (!await CheckPermissions(userMessage, Hub.Config.Stoat.RoleCanTrade)) return;
                await HandleDittoTradeCommandAsync(userMessage, parts.Skip(1).ToList());
            }
            else if (cmd == "queuestatus" || cmd == "qs")
            {
                await HandleQueueStatusCommandAsync(userMessage);
            }
            else if (cmd == "queueclear" || cmd == "qc" || cmd == "tc")
            {
                await HandleQueueClearCommandAsync(userMessage);
            }
            else if (cmd == "medals" || cmd == "ml")
            {
                await HandleMedalsCommandAsync(userMessage);
            }
            else if (cmd == "leaderboard" || cmd == "lb" || cmd == "halloffame" || cmd == "hof")
            {
                await HandleLeaderboardCommandAsync(userMessage);
            }
            else if (cmd == "egg")
            {
                if (!await CheckPermissions(userMessage, Hub.Config.Stoat.RoleCanTrade)) return;
                await HandleEggCommandAsync(userMessage, parts.Skip(1).ToList());
            }
            else if (cmd == "mysteryegg" || cmd == "me")
            {
                if (!await CheckPermissions(userMessage, Hub.Config.Stoat.RoleCanTrade)) return;
                await HandleMysteryEggCommandAsync(userMessage);
            }
            else if (cmd == "hidetrade" || cmd == "ht")
            {
                if (!await CheckPermissions(userMessage, Hub.Config.Stoat.RoleCanTrade)) return;
                await HandleTradeCommandAsync(userMessage, parts.Skip(1).ToList(), true);
            }
            else if (cmd == "listevents" || cmd == "le")
            {
                await HandleListEventsCommandAsync(userMessage, parts.Skip(1).ToList());
            }
            else if (cmd == "eventrequest" || cmd == "er")
            {
                if (!await CheckPermissions(userMessage, Hub.Config.Stoat.RoleCanTrade)) return;
                await HandleEventRequestCommandAsync(userMessage, parts.Skip(1).ToList());
            }
            else if (cmd == "battlereadylist" || cmd == "brl")
            {
                await HandleBattleReadyListCommandAsync(userMessage, parts.Skip(1).ToList());
            }
            else if (cmd == "battlereadyrequest" || cmd == "brr")
            {
                if (!await CheckPermissions(userMessage, Hub.Config.Stoat.RoleCanTrade)) return;
                await HandleBattleReadyRequestCommandAsync(userMessage, parts.Skip(1).ToList());
            }
            else if (cmd == "batchtrade" || cmd == "bt")
            {
                if (!await CheckPermissions(userMessage, Hub.Config.Stoat.RoleCanTrade)) return;
                await HandleBatchTradeCommandAsync(userMessage, parts.Skip(1).ToList());
            }
            else if (cmd == "pokepaste" || cmd == "pp")
            {
                if (!await CheckPermissions(userMessage, Hub.Config.Stoat.RoleCanTrade)) return;
                await HandlePokepasteCommandAsync(userMessage, parts.Skip(1).ToList());
            }
            else if (cmd == "specialrequestpokemon" || cmd == "srp")
            {
                if (!await CheckPermissions(userMessage, Hub.Config.Stoat.RoleCanTrade)) return;
                await HandleSpecialRequestPokemonCommandAsync(userMessage, parts.Skip(1).ToList());
            }
            else if (cmd == "deletetradecode" || cmd == "dtc")
            {
                await HandleDeleteTradeCodeCommandAsync(userMessage);
            }
            else if (cmd == "linkcode")
            {
                string token = DatabaseService.GenerateLinkToken(userIdNumeric);
                if (token == "DB_OFF" || token == "ERROR")
                    await StoatHelper<T>.SendAsync(_client, userMessage.ChannelId, "Account linking is currently disabled or an error occurred.");
                else
                    await StoatHelper<T>.SendAsync(_client, userMessage.ChannelId, $"<@{userMessage.AuthorId}> Your account link token is: **{token}**\nThis token will expire in 15 minutes. Go to the other platform and run `link {token}` to link that account.");
            }
            else if (cmd == "link")
            {
                if (parts.Length < 2) 
                {
                    await StoatHelper<T>.SendAsync(_client, userMessage.ChannelId, "Please provide the 6-character token.");
                    return;
                }
                string token = parts[1].Trim().ToUpper();
                if (token.Length != 6)
                {
                    await StoatHelper<T>.SendAsync(_client, userMessage.ChannelId, "Invalid token format. It should be 6 characters long.");
                    return;
                }
                bool success = DatabaseService.LinkAccount(userIdNumeric, token, "Stoat");
                if (success)
                    await StoatHelper<T>.SendAsync(_client, userMessage.ChannelId, $"<@{userMessage.AuthorId}> successfully linked! Your stats here will now match the primary account you linked from.");
                else
                    await StoatHelper<T>.SendAsync(_client, userMessage.ChannelId, $"<@{userMessage.AuthorId}> failed to link account. The token may be expired, invalid, or you are trying to link to yourself.");
            }
        }
        catch (Exception ex)
        {
            LogUtil.LogError($"Error processing Stoat command: {ex.Message}", "SysStoat");
        }
    }

    private async Task<bool> CheckPermissions(UserMessage message, RemoteControlAccessList allowedRoles, bool sudoOnly = false)
    {
        ulong userIdNumeric = StoatHelper<T>.ConvertId(message.AuthorId);
        if (Hub.Config.Stoat.GlobalSudoList.Contains(userIdNumeric)) return true;
        if (sudoOnly) 
        {
            await StoatHelper<T>.SendAsync(_client, message.ChannelId, "This command is restricted to bot administrators.");
            return false;
        }

        if (allowedRoles.AllowIfEmpty && allowedRoles.List.Count == 0) return true;

        await StoatHelper<T>.SendAsync(_client, message.ChannelId, "You do not have the required permissions to use this command.");
        return false;
    }

    private async Task HandleTradeCommandAsync(UserMessage message, List<string> args, bool isHiddenTrade = false)
    {
        if (args.Count == 0 && message.Attachments.Count == 0)
        {
            await StoatHelper<T>.SendAsync(_client, message.ChannelId, "Please provide a Showdown set or a trade code.");
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

        ulong userIdNumeric = StoatHelper<T>.ConvertId(message.AuthorId);

        if (lgCode == null)
        {
            if (args.Count > 0 && int.TryParse(args[0], out code))
                showdownSet = string.Join("\n", args.Skip(1));
            else
            {
                code = Hub.Queues.Info.GetRandomTradeCode(userIdNumeric);
                showdownSet = string.Join("\n", args);
            }
        }

        if (string.IsNullOrWhiteSpace(showdownSet) && message.Attachments.Count > 0)
        {
            // Try download attachment if available
            // Note: Stoat attachments might need HTTP get from attachment URL
            try {
                showdownSet = await SysBot.Pokemon.Helpers.NetUtil.HttpClient.GetStringAsync("https://autumn.revolt.chat/attachments/" + message.Attachments[0].Id);
            } catch { }
        }

        if (string.IsNullOrWhiteSpace(showdownSet))
        {
            await StoatHelper<T>.SendAsync(_client, message.ChannelId, "Please provide a Showdown set.");
            return;
        }

        var ignoreAutoOT = showdownSet.Contains("OT:") || showdownSet.Contains("TID:") || showdownSet.Contains("SID:");
        var result = await StoatHelper<T>.ProcessShowdownSetAsync(showdownSet, ignoreAutoOT, userIdNumeric);
        if (result.Pokemon == null)
        {
            var errorMsg = $"Oops! I couldn't parse that Showdown set.\nReason: {result.Error}";
            if (!string.IsNullOrEmpty(result.LegalizationHint))
                errorMsg += $"\nHint: {result.LegalizationHint}";
            await StoatHelper<T>.SendAsync(_client, message.ChannelId, errorMsg);
            return;
        }

        await StoatHelper<T>.AddToQueueAsync(_client, message.ChannelId, code, message.Author.Username, result.Pokemon, message.AuthorId, message.Author.Username, lgCode, isHiddenTrade);
    }

    private async Task HandleItemTradeCommandAsync(UserMessage message, List<string> args)
    {
        if (args.Count == 0)
        {
            await StoatHelper<T>.SendAsync(_client, message.ChannelId, "Please provide at least one item name.");
            return;
        }

        var itemInput = string.Join(" ", args);
        var itemNames = itemInput.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        
        if (typeof(T) == typeof(PB7) && itemNames.Length > 1)
        {
            await StoatHelper<T>.SendAsync(_client, message.ChannelId, "Batch trades are not supported in Let's Go Pikachu/Eevee.");
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
            await StoatHelper<T>.SendAsync(_client, message.ChannelId, "No valid items could be processed.");
            return;
        }

        ulong userIdNumeric = StoatHelper<T>.ConvertId(message.AuthorId);
        int code = Hub.Queues.Info.GetRandomTradeCode(userIdNumeric);
        if (pkmList.Count == 1)
            await StoatHelper<T>.AddToQueueAsync(_client, message.ChannelId, code, message.Author.Username, pkmList[0], message.AuthorId, message.Author.Username);
        else
            await StoatHelper<T>.AddBatchContainerToQueueAsync(_client, message.ChannelId, code, message.Author.Username, pkmList[0], pkmList, message.AuthorId, message.Author.Username);
    }

    private async Task HandleCloneCommandAsync(UserMessage message, List<string> args)
    {
        ulong userIdNumeric = StoatHelper<T>.ConvertId(message.AuthorId);
        if (Hub.Queues.Info.IsUserInQueue(userIdNumeric))
        {
            await StoatHelper<T>.SendAsync(_client, message.ChannelId, "You are already in the queue.");
            return;
        }

        int code = args.Count > 0 && int.TryParse(args[0], out var c) ? c : Hub.Queues.Info.GetRandomTradeCode(userIdNumeric);
        var lgcode = Hub.Queues.Info.GetRandomLGTradeCode(userIdNumeric);
        var notifier = new StoatTradeNotifier<T>(new T(), new PokeTradeTrainerInfo(message.Author.Username, userIdNumeric), code, message.AuthorId, _client, lgcode);
        int uniqueID = TradeUtil.GenerateUniqueTradeID();
        var detail = new PokeTradeDetail<T>(new T(), new PokeTradeTrainerInfo(message.Author.Username, userIdNumeric), notifier, PokeTradeType.Clone, code, false, lgcode, 1, 1, false, false, uniqueID);
        var trade = new TradeEntry<T>(detail, userIdNumeric, PokeRoutineType.Clone, message.Author.Username, uniqueID);
        
        if (Hub.Queues.Info.AddToTradeQueue(trade, userIdNumeric, false, Hub.Config.Stoat.GlobalSudoList.Contains(userIdNumeric)) == QueueResultAdd.Added)
        {
            await StoatHelper<T>.SendAsync(_client, message.ChannelId, $"Processing clone request... Code: {code:D8}");
            await notifier.SendInitialQueueUpdate();
        }
    }

    private async Task HandleFixOTCommandAsync(UserMessage message, List<string> args)
    {
        ulong userIdNumeric = StoatHelper<T>.ConvertId(message.AuthorId);
        if (Hub.Queues.Info.IsUserInQueue(userIdNumeric))
        {
            await StoatHelper<T>.SendAsync(_client, message.ChannelId, "You are already in the queue.");
            return;
        }

        int code = args.Count > 0 && int.TryParse(args[0], out var c) ? c : Hub.Queues.Info.GetRandomTradeCode(userIdNumeric);
        var lgcode = Hub.Queues.Info.GetRandomLGTradeCode(userIdNumeric);
        var notifier = new StoatTradeNotifier<T>(new T(), new PokeTradeTrainerInfo(message.Author.Username, userIdNumeric), code, message.AuthorId, _client, lgcode);
        int uniqueID = TradeUtil.GenerateUniqueTradeID();
        var detail = new PokeTradeDetail<T>(new T(), new PokeTradeTrainerInfo(message.Author.Username, userIdNumeric), notifier, PokeTradeType.FixOT, code, false, lgcode, 1, 1, false, false, uniqueID);
        var trade = new TradeEntry<T>(detail, userIdNumeric, PokeRoutineType.FixOT, message.Author.Username, uniqueID);
        
        if (Hub.Queues.Info.AddToTradeQueue(trade, userIdNumeric, false, Hub.Config.Stoat.GlobalSudoList.Contains(userIdNumeric)) == QueueResultAdd.Added)
        {
            await StoatHelper<T>.SendAsync(_client, message.ChannelId, $"Processing fixOT request... Code: {code:D8}");
            await notifier.SendInitialQueueUpdate();
        }
    }

    private async Task HandleDittoTradeCommandAsync(UserMessage message, List<string> args)
    {
        if (args.Count < 3)
        {
            await StoatHelper<T>.SendAsync(_client, message.ChannelId, "Usage: ditto <IVs/Keyword> <Language> <Nature>");
            return;
        }

        ulong userIdNumeric = StoatHelper<T>.ConvertId(message.AuthorId);
        if (Hub.Queues.Info.IsUserInQueue(userIdNumeric))
        {
            await StoatHelper<T>.SendAsync(_client, message.ChannelId, "You are already in the queue.");
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
            await StoatHelper<T>.AddToQueueAsync(_client, message.ChannelId, Hub.Queues.Info.GetRandomTradeCode(userIdNumeric), message.Author.Username, pk, message.AuthorId, message.Author.Username);
        }
    }

    private async Task HandleQueueStatusCommandAsync(UserMessage message)
    {
        ulong userIdNumeric = StoatHelper<T>.ConvertId(message.AuthorId);
        var pos = Hub.Queues.Info.CheckPosition(userIdNumeric, 0);
        if (pos.Position == -1)
            await StoatHelper<T>.SendAsync(_client, message.ChannelId, $"<@{message.AuthorId}>, you are not in the queue.");
        else
            await StoatHelper<T>.SendAsync(_client, message.ChannelId, $"<@{message.AuthorId}>, position {pos.Position}. ETA: {Hub.Config.Queues.EstimateDelay(pos.Position, Hub.Bots.Count):F1} min(s).");
    }

    private async Task HandleQueueClearCommandAsync(UserMessage message)
    {
        ulong userIdNumeric = StoatHelper<T>.ConvertId(message.AuthorId);
        if (Hub.Queues.Info.ClearTrade(userIdNumeric) != QueueResultRemove.NotInQueue)
            await StoatHelper<T>.SendAsync(_client, message.ChannelId, $"<@{message.AuthorId}>, removed from queue.");
        else
            await StoatHelper<T>.SendAsync(_client, message.ChannelId, $"<@{message.AuthorId}>, not in queue.");
    }

    private async Task HandleDeleteTradeCodeCommandAsync(UserMessage message)
    {
        ulong userIdNumeric = StoatHelper<T>.ConvertId(message.AuthorId);
        if (new TradeCodeStorage().DeleteTradeCode(userIdNumeric))
            await StoatHelper<T>.SendAsync(_client, message.ChannelId, "Deleted stored trade code.");
        else
            await StoatHelper<T>.SendAsync(_client, message.ChannelId, "No stored trade code found.");
    }

    private async Task HandleEggCommandAsync(UserMessage message, List<string> args)
    {
        ulong userIdNumeric = StoatHelper<T>.ConvertId(message.AuthorId);
        if (args.Count == 0 || Hub.Queues.Info.IsUserInQueue(userIdNumeric)) return;
        var sav = AutoLegalityWrapper.GetTrainerInfo<T>();
        var template = AutoLegalityWrapper.GetTemplate(new ShowdownSet(string.Join(" ", args)));
        var pkm = sav.GenerateEgg(template, out var res);
        if (res == LegalizationResult.Regenerated && pkm != null)
        {
            if (APILegality.AllowTrainerOverride && template.Regen.Trainer != null)
                pkm.SetAllTrainerData(template.Regen.Trainer);

            var pk = EntityConverter.ConvertToType(pkm, typeof(T), out _) as T ?? (T)pkm;
            pk.ResetPartyStats();
            await StoatHelper<T>.AddToQueueAsync(_client, message.ChannelId, Hub.Queues.Info.GetRandomTradeCode(userIdNumeric), message.Author.Username, pk, message.AuthorId, message.Author.Username);
        }
    }

    private async Task HandleMysteryEggCommandAsync(UserMessage message)
    {
        ulong userIdNumeric = StoatHelper<T>.ConvertId(message.AuthorId);
        if (Hub.Queues.Info.IsUserInQueue(userIdNumeric)) return;
        var egg = TradeModuleHelpers.GenerateLegalMysteryEgg<T>();
        if (egg != null)
            await StoatHelper<T>.AddToQueueAsync(_client, message.ChannelId, Hub.Queues.Info.GetRandomTradeCode(userIdNumeric), message.Author.Username, egg, message.AuthorId, message.Author.Username);
    }

    private async Task HandleListEventsCommandAsync(UserMessage message, List<string> args)
    {
        var folderPath = Hub.Config.Folder.EventsFolder;
        if (string.IsNullOrEmpty(folderPath))
        {
            await StoatHelper<T>.SendAsync(_client, message.ChannelId, "This bot does not have this feature set up.");
            return;
        }

        var files = System.IO.Directory.GetFiles(folderPath)
            .Select(System.IO.Path.GetFileNameWithoutExtension)
            .Where(file => file != null)
            .OrderBy(file => file)
            .ToList();

        if (files.Count == 0)
        {
            await StoatHelper<T>.SendAsync(_client, message.ChannelId, "No events found.");
            return;
        }

        int itemsPerPage = 20;
        int page = 1;
        if (args.Count > 0 && int.TryParse(args[0], out int p)) page = p;
        
        var pageCount = (int)Math.Ceiling(files.Count / (double)itemsPerPage);
        page = Math.Clamp(page, 1, pageCount);
        
        var pageItems = files.Skip((page - 1) * itemsPerPage).Take(itemsPerPage).ToList();
        
        var text = "";
        for (int i = 0; i < pageItems.Count; i++)
        {
            var item = pageItems[i];
            var index = files.IndexOf(item) + 1;
            text += $"{index}. {item}\n";
        }
        
        await StoatHelper<T>.SendAsync(_client, message.ChannelId, $"**Available Events (Page {page}/{pageCount})**\n{text}\nUse `{Hub.Config.Stoat.CommandPrefix}er [index]` to request an event.");
    }

    private async Task HandleEventRequestCommandAsync(UserMessage message, List<string> args)
    {
        if (args.Count == 0 || !int.TryParse(args[0], out int index)) return;
        var folderPath = Hub.Config.Folder.EventsFolder;
        if (string.IsNullOrEmpty(folderPath)) return;

        ulong userIdNumeric = StoatHelper<T>.ConvertId(message.AuthorId);
        if (Hub.Queues.Info.IsUserInQueue(userIdNumeric)) return;

        var files = System.IO.Directory.GetFiles(folderPath)
            .Select(System.IO.Path.GetFileName)
            .Where(x => x != null)
            .OrderBy(x => x)
            .ToList();

        if (index < 1 || index > files.Count) return;

        var fileData = await System.IO.File.ReadAllBytesAsync(System.IO.Path.Combine(folderPath, files[index - 1]!));
        var rawData = PKHeX.Core.EntityFormat.GetFromBytes(fileData);
        var pk = rawData as T ?? PKHeX.Core.EntityConverter.ConvertToType(rawData!, typeof(T), out _) as T;

        if (pk != null)
        {
            await StoatHelper<T>.SendAsync(_client, message.ChannelId, "Event request added to queue.");
            await StoatHelper<T>.AddToQueueAsync(_client, message.ChannelId, Hub.Queues.Info.GetRandomTradeCode(userIdNumeric), message.Author.Username, pk, message.AuthorId, message.Author.Username, Hub.Queues.Info.GetRandomLGTradeCode(userIdNumeric));
        }
    }

    private async Task HandleBattleReadyListCommandAsync(UserMessage message, List<string> args)
    {
        var folderPath = Hub.Config.Folder.HOMEReadyPKMFolder;
        if (string.IsNullOrEmpty(folderPath))
        {
            await StoatHelper<T>.SendAsync(_client, message.ChannelId, "This bot does not have this feature set up.");
            return;
        }

        var files = System.IO.Directory.GetFiles(folderPath)
            .Select(System.IO.Path.GetFileNameWithoutExtension)
            .Where(file => file != null)
            .OrderBy(file => file)
            .ToList();

        if (files.Count == 0)
        {
            await StoatHelper<T>.SendAsync(_client, message.ChannelId, "No battle ready pokemon found.");
            return;
        }

        int itemsPerPage = 20;
        int page = 1;
        if (args.Count > 0 && int.TryParse(args[0], out int p)) page = p;
        
        var pageCount = (int)Math.Ceiling(files.Count / (double)itemsPerPage);
        page = Math.Clamp(page, 1, pageCount);
        
        var pageItems = files.Skip((page - 1) * itemsPerPage).Take(itemsPerPage).ToList();
        
        var text = "";
        for (int i = 0; i < pageItems.Count; i++)
        {
            var item = pageItems[i];
            var index = files.IndexOf(item) + 1;
            text += $"{index}. {item}\n";
        }
        
        await StoatHelper<T>.SendAsync(_client, message.ChannelId, $"**Available Battle Ready (Page {page}/{pageCount})**\n{text}\nUse `{Hub.Config.Stoat.CommandPrefix}brr [index]` to request a pokemon.");
    }

    private async Task HandleBattleReadyRequestCommandAsync(UserMessage message, List<string> args)
    {
        if (args.Count == 0 || !int.TryParse(args[0], out int index)) return;
        var folderPath = Hub.Config.Folder.HOMEReadyPKMFolder;
        if (string.IsNullOrEmpty(folderPath)) return;

        ulong userIdNumeric = StoatHelper<T>.ConvertId(message.AuthorId);
        if (Hub.Queues.Info.IsUserInQueue(userIdNumeric)) return;

        var files = System.IO.Directory.GetFiles(folderPath)
            .Select(System.IO.Path.GetFileName)
            .Where(x => x != null)
            .OrderBy(x => x)
            .ToList();

        if (index < 1 || index > files.Count) return;

        var fileData = await System.IO.File.ReadAllBytesAsync(System.IO.Path.Combine(folderPath, files[index - 1]!));
        var rawData = PKHeX.Core.EntityFormat.GetFromBytes(fileData);
        var pk = rawData as T ?? PKHeX.Core.EntityConverter.ConvertToType(rawData!, typeof(T), out _) as T;

        if (pk != null)
        {
            await StoatHelper<T>.SendAsync(_client, message.ChannelId, "Battle Ready request added to queue.");
            await StoatHelper<T>.AddToQueueAsync(_client, message.ChannelId, Hub.Queues.Info.GetRandomTradeCode(userIdNumeric), message.Author.Username, pk, message.AuthorId, message.Author.Username, Hub.Queues.Info.GetRandomLGTradeCode(userIdNumeric));
        }
    }

    private async Task HandleBatchTradeCommandAsync(UserMessage message, List<string> args)
    {
        ulong userIdNumeric = StoatHelper<T>.ConvertId(message.AuthorId);
        if (args.Count == 0 || Hub.Queues.Info.IsUserInQueue(userIdNumeric)) return;
        var sets = TradeModuleHelpers.ParseBatchTradeContent(BatchNormalizer.NormalizeBatchCommands(string.Join(" ", args)));
        if (sets.Count > Hub.Config.Trade.BatchSettings.MaxPkmsPerTrade) return;

        var pkmList = new List<T>();
        foreach (var s in sets)
        {
            var ignoreAutoOT = s.Contains("OT:") || s.Contains("TID:") || s.Contains("SID:");
            var res = await StoatHelper<T>.ProcessShowdownSetAsync(s, ignoreAutoOT, userIdNumeric);
            if (res.Pokemon != null) pkmList.Add(res.Pokemon);
        }

        if (pkmList.Count > 0)
            await StoatHelper<T>.AddBatchContainerToQueueAsync(_client, message.ChannelId, Hub.Queues.Info.GetRandomTradeCode(userIdNumeric), message.Author.Username, pkmList[0], pkmList, message.AuthorId, message.Author.Username);
    }

    private async Task HandlePokepasteCommandAsync(UserMessage message, List<string> args)
    {
        if (args.Count == 0) return;
        try
        {
            var sets = ParseShowdownSets(await SysBot.Pokemon.Helpers.NetUtil.HttpClient.GetStringAsync(args[0]));
            if (sets.Count == 0) return;
            await StoatHelper<T>.SendAsync(_client, message.ChannelId, $"Found {sets.Count} Pokémon. Processing...");
            _ = Task.Run(async () => {
                ulong userIdNumeric = StoatHelper<T>.ConvertId(message.AuthorId);
                foreach (var s in sets) {
                    var content = string.Join("\n", s.GetSetLines());
                    var ignoreAutoOT = content.Contains("OT:") || content.Contains("TID:") || content.Contains("SID:");
                    var res = await StoatHelper<T>.ProcessShowdownSetAsync(content, ignoreAutoOT, userIdNumeric);
                    if (res.Pokemon != null) await StoatHelper<T>.SendAsync(_client, message.ChannelId, $"Generated {res.Pokemon.Species} from Pokepaste for <@{message.AuthorId}>.");
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

    private async Task HandleSpecialRequestPokemonCommandAsync(UserMessage message, List<string> args)
    {
        if (args.Count == 0) return;
        var eventData = TradeModuleHelpers.GetEventData(args[0]);
        if (eventData == null)
        {
            await StoatHelper<T>.SendAsync(_client, message.ChannelId, $"Invalid generation or game: {args[0]}");
            return;
        }

        ulong userIdNumeric = StoatHelper<T>.ConvertId(message.AuthorId);

        if (args.Count > 1 && int.TryParse(args[1], out int index))
        {
            if (Hub.Queues.Info.IsUserInQueue(userIdNumeric)) return;
            var entityEvents = eventData.Where(gift => gift.IsEntity && !gift.IsItem).ToArray();
            if (index < 1 || index > entityEvents.Length) return;
            var pk = TradeModuleHelpers.ConvertEventToPKM<T>(entityEvents[index - 1]);
            if (pk != null)
                await StoatHelper<T>.AddToQueueAsync(_client, message.ChannelId, Hub.Queues.Info.GetRandomTradeCode(userIdNumeric), message.Author.Username, pk, message.AuthorId, message.Author.Username);
        }
    }
    private async Task HandleMedalsCommandAsync(UserMessage message)
    {
        if (!Hub.Config.Stoat.EnableMedals)
        {
            await StoatHelper<T>.SendAsync(_client, message.ChannelId, "The medals system is currently disabled.");
            return;
        }

        ulong userIdNumeric = StoatHelper<T>.ConvertId(message.AuthorId);
        var tradeCodeStorage = new TradeCodeStorage();
        int totalTrades = tradeCodeStorage.GetTradeCount(userIdNumeric);

        if (totalTrades == 0)
        {
            await StoatHelper<T>.SendAsync(_client, message.ChannelId, $"{message.Author.Username}, you haven't made any trades yet.\nStart trading to earn your first medal!");
            return;
        }

        string description = totalTrades switch
        {
            1 => "Congratulations on your first trade!\n**Status:** Beginner Trainer.",
            50 => "You've reached 50 trades!\n**Status:** Rookie Trainer.",
            100 => "You've reached 100 trades!\n**Status:** Rising Star.",
            150 => "You've reached 150 trades!\n**Status:** Challenger.",
            200 => "You've reached 200 trades!\n**Status:** Master Baiter.",
            250 => "You've reached 250 trades!\n**Status:** Star Trainer.",
            300 => "You've reached 300 trades!\n**Status:** Ace Trainer.",
            350 => "You've reached 350 trades!\n**Status:** Veteran Trainer.",
            400 => "You've reached 400 trades!\n**Status:** Expert Trainer.",
            450 => "You've reached 450 trades!\n**Status:** Pokémon Trader.",
            500 => "You've reached 500 trades!\n**Status:** Pokémon Professor.",
            550 => "You've reached 550 trades!\n**Status:** Pokémon Champion.",
            600 => "You've reached 600 trades!\n**Status:** Pokémon Specialist.",
            650 => "You've reached 650 trades!\n**Status:** Pokémon Hero.",
            700 => "You've reached 700 trades!\n**Status:** Pokémon Elite.",
            750 => "You've reached 750 trades!\n**Status:** Pokémon Legend.",
            800 => "You've reached 800 trades!\n**Status:** Region Master.",
            850 => "You've reached 850 trades!\n**Status:** Pokémon Master.",
            900 => "You've reached 900 trades!\n**Status:** World Famous.",
            950 => "You've reached 950 trades!\n**Status:** Master Trader.",
            1000 => "You've reached 1000 trades!\n**Status:** Pokémon God.",
            _ => $"Congratulations on reaching {totalTrades} trades! Keep it going!"
        };

        string response = $"**{message.Author.Username}'s Milestone Medal**\n" +
                          $"{description}\n" +
                          $"**Total Trades**: {totalTrades}\n\n" +
                          $"*DudeBot.NET v{DudeBot.Version} | Synchronized via SQL*";

        await StoatHelper<T>.SendAsync(_client, message.ChannelId, response);
    }

    private async Task HandleLeaderboardCommandAsync(UserMessage message)
    {
        string response = "**🏆 GLOBAL MEDALS LEADERBOARD**\n" +
                          "Check out the top trainers and the community Hall of Fame on our official website!\n\n" +
                          "🌐 **Official Hall of Fame**: https://dudebot.org/leaderboard/\n" +
                          "⚡ **Real-Time Stats**: Rankings are updated globally across all bot hosters.\n\n" +
                          $"*DudeBot.NET v{DudeBot.Version} | Synchronized via SQL*";

        await StoatHelper<T>.SendAsync(_client, message.ChannelId, response);
    }
}
