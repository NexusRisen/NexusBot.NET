using PKHeX.Core;
using PKHeX.Core.AutoMod;
using SysBot.Base;
using SysBot.Pokemon.Helpers;
using SlackNet;
using SlackNet.Events;
using SlackNet.SocketMode;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SysBot.Pokemon.Slack;

public static class SysSlackSettings
{
    public static PokeTradeHubConfig HubConfig { get; internal set; } = default!;
    public static SlackSettings Settings => HubConfig.Slack;
}

public sealed class SysSlack<T> : IDisposable where T : PKM, new()
{
    public readonly PokeTradeHub<T> Hub;
    private readonly PokeBotRunner<T> _runner;
    private readonly ProgramConfig _config;
    
    private ISlackApiClient _apiClient;
    private ISlackSocketModeClient _socketClient;

    private readonly HashSet<string> _validCommands = new HashSet<string>
    {
        "trade", "t", "clone", "fixOT", "fix", "f", "dittoTrade", "ditto", "dt", "itemTrade", "item", "it",
        "egg", "Egg", "hidetrade", "ht", "batchTrade", "bt", "listevents", "le",
        "eventrequest", "er", "battlereadylist", "brl", "battlereadyrequest", "brr", "pokepaste", "pp",
        "PokePaste", "PP", "randomteam", "rt", "RandomTeam", "Rt", "specialrequestpokemon", "srp",
        "queueStatus", "qs", "queueClear", "qc", "ts", "tc", "deleteTradeCode", "dtc", "mysteryegg", "me", "id"
    };

    private readonly CancellationTokenSource _cts = new();

    public SysSlack(PokeBotRunner<T> runner, ProgramConfig config)
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

        SysSlackSettings.HubConfig = Hub.Config;
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
        if (!SysSlackSettings.Settings.BotStatusAnnouncement)
            return;

        var botName = string.IsNullOrEmpty(Hub.Config.BotName) ? "DudeBot" : Hub.Config.BotName;
        var emoji = isOnline ? SysSlackSettings.Settings.OnlineEmoji : SysSlackSettings.Settings.OfflineEmoji;
        var fullStatusMessage = $"{emoji} *Status*: {botName} is {status}!";

        foreach (var channelId in SysSlackSettings.Settings.ChannelWhitelist.List.Select(c => c.ID.ToString()))
        {
            try
            {
                await SlackHelper<T>.SendAsync(_apiClient, channelId, fullStatusMessage);
                LogUtil.LogInfo("SysSlack", $"AnnounceBotStatus: {status} announced in channel {channelId}.");
            }
            catch (Exception ex)
            {
                LogUtil.LogInfo("SysSlack", $"AnnounceBotStatus: Exception in channel {channelId}: {ex.Message}");
            }
        }
    }

    private async Task HandleBotStop(Exception ex)
    {
        LogUtil.LogInfo("SysSlack", $"Bot connection error: {ex.Message}. Notifying Slack users.");
        await AnnounceBotStatus("Offline", false);
    }

    private async Task HandleBotStart()
    {
        LogUtil.LogInfo("SysSlack", "Bot connection success. Notifying Slack users.");
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

        if (ReferenceEquals(SysSlackSettings.HubConfig, Hub.Config))
            SysSlackSettings.HubConfig = default!;

        try
        {
            _socketClient?.Disconnect();
            _socketClient?.Dispose();
        }
        catch { }
    }

    public static PokeBotRunner<T> Runner { get; private set; } = default!;

    public async Task MainAsync(CancellationToken ct)
    {
        try
        {
            var builder = new SlackServiceBuilder()
                .UseApiToken(Hub.Config.Slack.BotToken)
                .UseAppLevelToken(Hub.Config.Slack.AppLevelToken)
                .RegisterEventHandler(new SlackMessageHandler(OnMessageReceived));
            
            _apiClient = builder.GetApiClient();
            _socketClient = builder.GetSocketModeClient();

            await _socketClient.Connect().ConfigureAwait(false);

            await Task.Delay(-1, ct).ConfigureAwait(false);
        }
        catch (OperationCanceledException) { }
        catch (Exception ex)
        {
            LogUtil.LogError(ex.Message, "SysSlack");
        }
    }

    private async void OnMessageReceived(MessageEvent message)
    {
        if (message.Subtype != null || message.User == null) return;
        
        // We do channel checking. Wait, in Slack channel ID is string like C123456
        // Let's use simple hash if whitelist is ulong or skip
        ulong channelIdNumeric = SlackHelper<T>.ConvertId(message.Channel);
        if (!Hub.Config.Slack.ChannelWhitelist.Contains(channelIdNumeric) && Hub.Config.Slack.ChannelWhitelist.List.Count > 0)
            return; // Skip if not whitelisted and whitelist is active

        var content = message.Text;
        if (string.IsNullOrEmpty(content)) return;

        var prefix = Hub.Config.Slack.CommandPrefix;
        if (!content.StartsWith(prefix)) return;
        var parts = content.Substring(prefix.Length).Split(new[] { ' ', '\n' }, StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length == 0) return;
        
        var cmd = parts[0].ToLower();
        if (!_validCommands.Contains(cmd)) return;

        ulong userIdNumeric = SlackHelper<T>.ConvertId(message.User);

        if (Hub.Config.Slack.UserBlacklist.Contains(userIdNumeric)) return;

        LogUtil.LogText($"Slack Command received: {cmd} from {message.User}");

        try
        {
            if (cmd == "ts")
            {
                await SlackHelper<T>.SendAsync(_apiClient, message.Channel, $"Hello <@{message.User}>, I am online!");
                return;
            }
            if (cmd == "id")
            {
                await SlackHelper<T>.SendAsync(_apiClient, message.Channel, $"User ID: {message.User} (Numeric: {userIdNumeric})\nChannel ID: {message.Channel} (Numeric: {channelIdNumeric})");
                return;
            }

            if (cmd == "trade" || cmd == "t")
            {
                if (!await CheckPermissions(message, Hub.Config.Slack.RoleCanTrade)) return;
                await HandleTradeCommandAsync(message, parts.Skip(1).ToList());
            }
            else if (cmd == "itemtrade" || cmd == "item" || cmd == "it")
            {
                if (!await CheckPermissions(message, Hub.Config.Slack.RoleCanTrade)) return;
                await HandleItemTradeCommandAsync(message, parts.Skip(1).ToList());
            }
            else if (cmd == "clone" || cmd == "c")
            {
                if (!await CheckPermissions(message, Hub.Config.Slack.RoleCanClone)) return;
                await HandleCloneCommandAsync(message, parts.Skip(1).ToList());
            }
            else if (cmd == "fixot" || cmd == "fix" || cmd == "f")
            {
                if (!await CheckPermissions(message, Hub.Config.Slack.RoleCanFixOT)) return;
                await HandleFixOTCommandAsync(message, parts.Skip(1).ToList());
            }
            else if (cmd == "dittotrade" || cmd == "ditto" || cmd == "dt")
            {
                if (!await CheckPermissions(message, Hub.Config.Slack.RoleCanTrade)) return;
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
                if (!await CheckPermissions(message, Hub.Config.Slack.RoleCanTrade)) return;
                await HandleEggCommandAsync(message, parts.Skip(1).ToList());
            }
            else if (cmd == "mysteryegg" || cmd == "me")
            {
                if (!await CheckPermissions(message, Hub.Config.Slack.RoleCanTrade)) return;
                await HandleMysteryEggCommandAsync(message);
            }
            else if (cmd == "hidetrade" || cmd == "ht")
            {
                if (!await CheckPermissions(message, Hub.Config.Slack.RoleCanTrade)) return;
                await HandleTradeCommandAsync(message, parts.Skip(1).ToList(), true);
            }
            else if (cmd == "listevents" || cmd == "le")
            {
                await HandleListEventsCommandAsync(message, parts.Skip(1).ToList());
            }
            else if (cmd == "eventrequest" || cmd == "er")
            {
                if (!await CheckPermissions(message, Hub.Config.Slack.RoleCanTrade)) return;
                await HandleEventRequestCommandAsync(message, parts.Skip(1).ToList());
            }
            else if (cmd == "battlereadylist" || cmd == "brl")
            {
                await HandleBattleReadyListCommandAsync(message, parts.Skip(1).ToList());
            }
            else if (cmd == "battlereadyrequest" || cmd == "brr")
            {
                if (!await CheckPermissions(message, Hub.Config.Slack.RoleCanTrade)) return;
                await HandleBattleReadyRequestCommandAsync(message, parts.Skip(1).ToList());
            }
            else if (cmd == "batchtrade" || cmd == "bt")
            {
                if (!await CheckPermissions(message, Hub.Config.Slack.RoleCanTrade)) return;
                await HandleBatchTradeCommandAsync(message, parts.Skip(1).ToList());
            }
            else if (cmd == "pokepaste" || cmd == "pp")
            {
                if (!await CheckPermissions(message, Hub.Config.Slack.RoleCanTrade)) return;
                await HandlePokepasteCommandAsync(message, parts.Skip(1).ToList());
            }
            else if (cmd == "specialrequestpokemon" || cmd == "srp")
            {
                if (!await CheckPermissions(message, Hub.Config.Slack.RoleCanTrade)) return;
                await HandleSpecialRequestPokemonCommandAsync(message, parts.Skip(1).ToList());
            }
            else if (cmd == "deletetradecode" || cmd == "dtc")
            {
                await HandleDeleteTradeCodeCommandAsync(message);
            }
            // More commands like batchtrade can be added similarly, but standard commands are covered.
        }
        catch (Exception ex)
        {
            LogUtil.LogError($"Error processing Slack command: {ex.Message}", "SysSlack");
        }
    }

    private async Task<bool> CheckPermissions(MessageEvent message, RemoteControlAccessList allowedRoles, bool sudoOnly = false)
    {
        ulong userIdNumeric = SlackHelper<T>.ConvertId(message.User);
        if (Hub.Config.Slack.GlobalSudoList.Contains(userIdNumeric)) return true;
        if (sudoOnly) 
        {
            await SlackHelper<T>.SendAsync(_apiClient, message.Channel, "This command is restricted to bot administrators.");
            return false;
        }

        if (allowedRoles.AllowIfEmpty && allowedRoles.List.Count == 0) return true;

        await SlackHelper<T>.SendAsync(_apiClient, message.Channel, "You do not have the required permissions to use this command.");
        return false;
    }

    private async Task HandleTradeCommandAsync(MessageEvent message, List<string> args, bool isHiddenTrade = false)
    {
        if (args.Count == 0)
        {
            await SlackHelper<T>.SendAsync(_apiClient, message.Channel, "Please provide a Showdown set or a trade code.");
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

        ulong userIdNumeric = SlackHelper<T>.ConvertId(message.User);

        if (lgCode == null)
        {
            if (int.TryParse(args[0], out code))
                showdownSet = string.Join("\n", args.Skip(1));
            else
            {
                code = Hub.Queues.Info.GetRandomTradeCode(userIdNumeric);
                showdownSet = string.Join("\n", args);
            }
        }

        if (string.IsNullOrWhiteSpace(showdownSet))
        {
            await SlackHelper<T>.SendAsync(_apiClient, message.Channel, "Please provide a Showdown set.");
            return;
        }

        var result = await SlackHelper<T>.ProcessShowdownSetAsync(showdownSet);
        if (result.Pokemon == null)
        {
            var errorMsg = $"Oops! I couldn't parse that Showdown set.\nReason: {result.Error}";
            if (!string.IsNullOrEmpty(result.LegalizationHint))
                errorMsg += $"\nHint: {result.LegalizationHint}";
            await SlackHelper<T>.SendAsync(_apiClient, message.Channel, errorMsg);
            return;
        }

        await SlackHelper<T>.AddToQueueAsync(_apiClient, message.Channel, code, message.User, result.Pokemon, message.User, message.User, lgCode, isHiddenTrade);
    }

    private async Task HandleItemTradeCommandAsync(MessageEvent message, List<string> args)
    {
        if (args.Count == 0)
        {
            await SlackHelper<T>.SendAsync(_apiClient, message.Channel, "Please provide at least one item name.");
            return;
        }

        var itemInput = string.Join(" ", args);
        var itemNames = itemInput.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        
        if (typeof(T) == typeof(PB7) && itemNames.Length > 1)
        {
            await SlackHelper<T>.SendAsync(_apiClient, message.Channel, "Batch trades are not supported in Let's Go Pikachu/Eevee.");
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
            await SlackHelper<T>.SendAsync(_apiClient, message.Channel, "No valid items could be processed.");
            return;
        }

        ulong userIdNumeric = SlackHelper<T>.ConvertId(message.User);
        int code = Hub.Queues.Info.GetRandomTradeCode(userIdNumeric);
        if (pkmList.Count == 1)
            await SlackHelper<T>.AddToQueueAsync(_apiClient, message.Channel, code, message.User, pkmList[0], message.User, message.User);
        else
            await SlackHelper<T>.AddBatchContainerToQueueAsync(_apiClient, message.Channel, code, message.User, pkmList[0], pkmList, message.User, message.User);
    }

    private async Task HandleCloneCommandAsync(MessageEvent message, List<string> args)
    {
        ulong userIdNumeric = SlackHelper<T>.ConvertId(message.User);
        if (Hub.Queues.Info.IsUserInQueue(userIdNumeric))
        {
            await SlackHelper<T>.SendAsync(_apiClient, message.Channel, "You are already in the queue.");
            return;
        }

        int code = args.Count > 0 && int.TryParse(args[0], out var c) ? c : Hub.Queues.Info.GetRandomTradeCode(userIdNumeric);
        var lgcode = Hub.Queues.Info.GetRandomLGTradeCode(userIdNumeric);
        var notifier = new SlackTradeNotifier<T>(new T(), new PokeTradeTrainerInfo(message.User, userIdNumeric), code, message.User, _apiClient, lgcode);
        int uniqueID = TradeUtil.GenerateUniqueTradeID();
        var detail = new PokeTradeDetail<T>(new T(), new PokeTradeTrainerInfo(message.User, userIdNumeric), notifier, PokeTradeType.Clone, code, false, lgcode, 1, 1, false, false, uniqueID);
        var trade = new TradeEntry<T>(detail, userIdNumeric, PokeRoutineType.Clone, message.User, uniqueID);
        
        if (Hub.Queues.Info.AddToTradeQueue(trade, userIdNumeric, false, Hub.Config.Slack.GlobalSudoList.Contains(userIdNumeric)) == QueueResultAdd.Added)
        {
            await SlackHelper<T>.SendAsync(_apiClient, message.Channel, $"Processing clone request... Code: {code:D8}");
            await notifier.SendInitialQueueUpdate();
        }
    }

    private async Task HandleFixOTCommandAsync(MessageEvent message, List<string> args)
    {
        ulong userIdNumeric = SlackHelper<T>.ConvertId(message.User);
        if (Hub.Queues.Info.IsUserInQueue(userIdNumeric))
        {
            await SlackHelper<T>.SendAsync(_apiClient, message.Channel, "You are already in the queue.");
            return;
        }

        int code = args.Count > 0 && int.TryParse(args[0], out var c) ? c : Hub.Queues.Info.GetRandomTradeCode(userIdNumeric);
        var lgcode = Hub.Queues.Info.GetRandomLGTradeCode(userIdNumeric);
        var notifier = new SlackTradeNotifier<T>(new T(), new PokeTradeTrainerInfo(message.User, userIdNumeric), code, message.User, _apiClient, lgcode);
        int uniqueID = TradeUtil.GenerateUniqueTradeID();
        var detail = new PokeTradeDetail<T>(new T(), new PokeTradeTrainerInfo(message.User, userIdNumeric), notifier, PokeTradeType.FixOT, code, false, lgcode, 1, 1, false, false, uniqueID);
        var trade = new TradeEntry<T>(detail, userIdNumeric, PokeRoutineType.FixOT, message.User, uniqueID);
        
        if (Hub.Queues.Info.AddToTradeQueue(trade, userIdNumeric, false, Hub.Config.Slack.GlobalSudoList.Contains(userIdNumeric)) == QueueResultAdd.Added)
        {
            await SlackHelper<T>.SendAsync(_apiClient, message.Channel, $"Processing fixOT request... Code: {code:D8}");
            await notifier.SendInitialQueueUpdate();
        }
    }

    private async Task HandleDittoTradeCommandAsync(MessageEvent message, List<string> args)
    {
        if (args.Count < 3)
        {
            await SlackHelper<T>.SendAsync(_apiClient, message.Channel, "Usage: ditto <IVs/Keyword> <Language> <Nature>");
            return;
        }

        ulong userIdNumeric = SlackHelper<T>.ConvertId(message.User);
        if (Hub.Queues.Info.IsUserInQueue(userIdNumeric))
        {
            await SlackHelper<T>.SendAsync(_apiClient, message.Channel, "You are already in the queue.");
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
            await SlackHelper<T>.AddToQueueAsync(_apiClient, message.Channel, Hub.Queues.Info.GetRandomTradeCode(userIdNumeric), message.User, pk, message.User, message.User);
        }
    }

    private async Task HandleQueueStatusCommandAsync(MessageEvent message)
    {
        ulong userIdNumeric = SlackHelper<T>.ConvertId(message.User);
        var pos = Hub.Queues.Info.CheckPosition(userIdNumeric, 0);
        if (pos.Position == -1)
            await SlackHelper<T>.SendAsync(_apiClient, message.Channel, $"{message.User}, you are not in the queue.");
        else
            await SlackHelper<T>.SendAsync(_apiClient, message.Channel, $"{message.User}, position {pos.Position}. ETA: {Hub.Config.Queues.EstimateDelay(pos.Position, Hub.Bots.Count):F1} min(s).");
    }

    private async Task HandleQueueClearCommandAsync(MessageEvent message)
    {
        ulong userIdNumeric = SlackHelper<T>.ConvertId(message.User);
        if (Hub.Queues.Info.ClearTrade(userIdNumeric) != QueueResultRemove.NotInQueue)
            await SlackHelper<T>.SendAsync(_apiClient, message.Channel, $"{message.User}, removed from queue.");
        else
            await SlackHelper<T>.SendAsync(_apiClient, message.Channel, $"{message.User}, not in queue.");
    }

    private async Task HandleDeleteTradeCodeCommandAsync(MessageEvent message)
    {
        ulong userIdNumeric = SlackHelper<T>.ConvertId(message.User);
        if (new TradeCodeStorage().DeleteTradeCode(userIdNumeric))
            await SlackHelper<T>.SendAsync(_apiClient, message.Channel, "Deleted stored trade code.");
        else
            await SlackHelper<T>.SendAsync(_apiClient, message.Channel, "No stored trade code found.");
    }

    private async Task HandleEggCommandAsync(MessageEvent message, List<string> args)
    {
        ulong userIdNumeric = SlackHelper<T>.ConvertId(message.User);
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
            await SlackHelper<T>.AddToQueueAsync(_apiClient, message.Channel, Hub.Queues.Info.GetRandomTradeCode(userIdNumeric), message.User, pk, message.User, message.User);
        }
    }

    private async Task HandleMysteryEggCommandAsync(MessageEvent message)
    {
        ulong userIdNumeric = SlackHelper<T>.ConvertId(message.User);
        if (Hub.Queues.Info.IsUserInQueue(userIdNumeric)) return;
        var egg = TradeModuleHelpers.GenerateLegalMysteryEgg<T>();
        if (egg != null)
            await SlackHelper<T>.AddToQueueAsync(_apiClient, message.Channel, Hub.Queues.Info.GetRandomTradeCode(userIdNumeric), message.User, egg, message.User, message.User);
    }

    private async Task HandleListEventsCommandAsync(MessageEvent message, List<string> args)
    {
        var folderPath = Hub.Config.Folder.EventsFolder;
        if (string.IsNullOrEmpty(folderPath))
        {
            await SlackHelper<T>.SendAsync(_apiClient, message.Channel, "This bot does not have this feature set up.");
            return;
        }

        var files = System.IO.Directory.GetFiles(folderPath)
            .Select(System.IO.Path.GetFileNameWithoutExtension)
            .Where(file => file != null)
            .OrderBy(file => file)
            .ToList();

        if (files.Count == 0)
        {
            await SlackHelper<T>.SendAsync(_apiClient, message.Channel, "No events found.");
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
        
        await SlackHelper<T>.SendAsync(_apiClient, message.Channel, $"*Available Events (Page {page}/{pageCount})*\n{text}\nUse `{Hub.Config.Slack.CommandPrefix}er [index]` to request an event.");
    }

    private async Task HandleEventRequestCommandAsync(MessageEvent message, List<string> args)
    {
        if (args.Count == 0 || !int.TryParse(args[0], out int index)) return;
        var folderPath = Hub.Config.Folder.EventsFolder;
        if (string.IsNullOrEmpty(folderPath)) return;

        ulong userIdNumeric = SlackHelper<T>.ConvertId(message.User);
        if (Hub.Queues.Info.IsUserInQueue(userIdNumeric)) return;

        var files = System.IO.Directory.GetFiles(folderPath)
            .Select(System.IO.Path.GetFileName)
            .Where(x => x != null)
            .OrderBy(x => x)
            .ToList();

        if (index < 1 || index > files.Count) return;

        var fileData = await System.IO.File.ReadAllBytesAsync(System.IO.Path.Combine(folderPath, files[index - 1]));
        var rawData = PKHeX.Core.EntityFormat.GetFromBytes(fileData);
        var pk = rawData as T ?? PKHeX.Core.EntityConverter.ConvertToType(rawData, typeof(T), out _) as T;

        if (pk != null)
        {
            await SlackHelper<T>.SendAsync(_apiClient, message.Channel, "Event request added to queue.");
            await SlackHelper<T>.AddToQueueAsync(_apiClient, message.Channel, Hub.Queues.Info.GetRandomTradeCode(userIdNumeric), message.User, pk, message.User, message.User, Hub.Queues.Info.GetRandomLGTradeCode(userIdNumeric));
        }
    }

    private async Task HandleBattleReadyListCommandAsync(MessageEvent message, List<string> args)
    {
        var folderPath = Hub.Config.Folder.HOMEReadyPKMFolder;
        if (string.IsNullOrEmpty(folderPath))
        {
            await SlackHelper<T>.SendAsync(_apiClient, message.Channel, "This bot does not have this feature set up.");
            return;
        }

        var files = System.IO.Directory.GetFiles(folderPath)
            .Select(System.IO.Path.GetFileNameWithoutExtension)
            .Where(file => file != null)
            .OrderBy(file => file)
            .ToList();

        if (files.Count == 0)
        {
            await SlackHelper<T>.SendAsync(_apiClient, message.Channel, "No battle ready pokemon found.");
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
        
        await SlackHelper<T>.SendAsync(_apiClient, message.Channel, $"*Available Battle Ready (Page {page}/{pageCount})*\n{text}\nUse `{Hub.Config.Slack.CommandPrefix}brr [index]` to request a pokemon.");
    }

    private async Task HandleBattleReadyRequestCommandAsync(MessageEvent message, List<string> args)
    {
        if (args.Count == 0 || !int.TryParse(args[0], out int index)) return;
        var folderPath = Hub.Config.Folder.HOMEReadyPKMFolder;
        if (string.IsNullOrEmpty(folderPath)) return;

        ulong userIdNumeric = SlackHelper<T>.ConvertId(message.User);
        if (Hub.Queues.Info.IsUserInQueue(userIdNumeric)) return;

        var files = System.IO.Directory.GetFiles(folderPath)
            .Select(System.IO.Path.GetFileName)
            .Where(x => x != null)
            .OrderBy(x => x)
            .ToList();

        if (index < 1 || index > files.Count) return;

        var fileData = await System.IO.File.ReadAllBytesAsync(System.IO.Path.Combine(folderPath, files[index - 1]));
        var rawData = PKHeX.Core.EntityFormat.GetFromBytes(fileData);
        var pk = rawData as T ?? PKHeX.Core.EntityConverter.ConvertToType(rawData, typeof(T), out _) as T;

        if (pk != null)
        {
            await SlackHelper<T>.SendAsync(_apiClient, message.Channel, "Battle Ready request added to queue.");
            await SlackHelper<T>.AddToQueueAsync(_apiClient, message.Channel, Hub.Queues.Info.GetRandomTradeCode(userIdNumeric), message.User, pk, message.User, message.User, Hub.Queues.Info.GetRandomLGTradeCode(userIdNumeric));
        }
    }

    private async Task HandleBatchTradeCommandAsync(MessageEvent message, List<string> args)
    {
        ulong userIdNumeric = SlackHelper<T>.ConvertId(message.User);
        if (args.Count == 0 || Hub.Queues.Info.IsUserInQueue(userIdNumeric)) return;
        var sets = TradeModuleHelpers.ParseBatchTradeContent(BatchNormalizer.NormalizeBatchCommands(string.Join(" ", args)));
        if (sets.Count > Hub.Config.Trade.BatchSettings.MaxPkmsPerTrade) return;

        var pkmList = new List<T>();
        foreach (var s in sets)
        {
            var res = await SlackHelper<T>.ProcessShowdownSetAsync(s);
            if (res.Pokemon != null) pkmList.Add(res.Pokemon);
        }

        if (pkmList.Count > 0)
            await SlackHelper<T>.AddBatchContainerToQueueAsync(_apiClient, message.Channel, Hub.Queues.Info.GetRandomTradeCode(userIdNumeric), message.User, pkmList[0], pkmList, message.User, message.User);
    }

    private async Task HandlePokepasteCommandAsync(MessageEvent message, List<string> args)
    {
        if (args.Count == 0) return;
        try
        {
            var sets = ParseShowdownSets(await SysBot.Pokemon.Helpers.NetUtil.HttpClient.GetStringAsync(args[0]));
            if (sets.Count == 0) return;
            await SlackHelper<T>.SendAsync(_apiClient, message.Channel, $"Found {sets.Count} Pokémon. Processing...");
            _ = Task.Run(async () => {
                foreach (var s in sets) {
                    var res = await SlackHelper<T>.ProcessShowdownSetAsync(string.Join("\n", s.GetSetLines()));
                    if (res.Pokemon != null) await SlackHelper<T>.SendAsync(_apiClient, message.Channel, $"Generated {res.Pokemon.Species} from Pokepaste for <@{message.User}>.");
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

    private async Task HandleSpecialRequestPokemonCommandAsync(MessageEvent message, List<string> args)
    {
        if (args.Count == 0) return;
        var eventData = TradeModuleHelpers.GetEventData(args[0]);
        if (eventData == null)
        {
            await SlackHelper<T>.SendAsync(_apiClient, message.Channel, $"Invalid generation or game: {args[0]}");
            return;
        }

        ulong userIdNumeric = SlackHelper<T>.ConvertId(message.User);

        if (args.Count > 1 && int.TryParse(args[1], out int index))
        {
            if (Hub.Queues.Info.IsUserInQueue(userIdNumeric)) return;
            var entityEvents = eventData.Where(gift => gift.IsEntity && !gift.IsItem).ToArray();
            if (index < 1 || index > entityEvents.Length) return;
            var pk = TradeModuleHelpers.ConvertEventToPKM<T>(entityEvents[index - 1]);
            if (pk != null)
                await SlackHelper<T>.AddToQueueAsync(_apiClient, message.Channel, Hub.Queues.Info.GetRandomTradeCode(userIdNumeric), message.User, pk, message.User, message.User);
        }
        else
        {
            var entityEvents = eventData.Where(gift => gift.IsEntity && !gift.IsItem).ToArray();
            int itemsPerPage = 25;
            int page = 1;
            if (args.Count > 1 && args[1].StartsWith("page", StringComparison.OrdinalIgnoreCase) && int.TryParse(args[1].AsSpan(4), out int pageNumber))
            {
                page = pageNumber;
            }

            var pageCount = (int)Math.Ceiling((double)entityEvents.Length / itemsPerPage);
            page = Math.Clamp(page, 1, pageCount);
            
            var pageItems = entityEvents.Skip((page - 1) * itemsPerPage).Take(itemsPerPage).ToArray();
            int offset = (page - 1) * itemsPerPage;
            
            var text = "";
            for (int i = 0; i < pageItems.Length; i++)
            {
                var gift = pageItems[i];
                string species = PKHeX.Core.GameInfo.Strings.Species[gift.Species];
                string eventDetails = $"{offset + i + 1}. {gift.CardHeader} - {species} | Lvl.{gift.Level} | OT: {gift.OriginalTrainerName}";
                text += eventDetails + "\n";
            }
            
            await SlackHelper<T>.SendAsync(_apiClient, message.Channel, $"*Available Events - {args[0].ToUpperInvariant()} (Page {page}/{pageCount})*\n{text}\nUse `{Hub.Config.Slack.CommandPrefix}srp {args[0]} [index]` to request an event, or `page[num]` to view more pages.");
        }
    }

    private class SlackMessageHandler : IEventHandler<MessageEvent>
    {
        private readonly Action<MessageEvent> _handler;
        public SlackMessageHandler(Action<MessageEvent> handler) => _handler = handler;
        public Task Handle(MessageEvent slackEvent)
        {
            _handler(slackEvent);
            return Task.CompletedTask;
        }
    }
}
