using PKHeX.Core;
using PKHeX.Core.AutoMod;
using StoatSharp;
using SysBot.Base;
using SysBot.Pokemon.Helpers;
using SysBot.Pokemon.Stoat.Commands;
using SysBot.Pokemon.Stoat.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SysBot.Pokemon.Stoat;

public partial class SysStoat<T>
{
    [StoatCommand("trade", "t")]
    private async Task HandleTradeCommandAsync(UserMessage message, List<string> args)
    {
        await ProcessTradeCommandAsync(message, args, isHiddenTrade: false);
    }

    [StoatCommand("hidetrade", "ht")]
    private async Task HandleHideTradeCommandAsync(UserMessage message, List<string> args)
    {
        await ProcessTradeCommandAsync(message, args, isHiddenTrade: true);
    }

    private async Task ProcessTradeCommandAsync(UserMessage message, List<string> args, bool isHiddenTrade)
    {
        if (!await CheckPermissions(message, Hub.Config.Stoat.RoleCanTrade)) return;

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
            try 
            {
                var data = await SysBot.Pokemon.Helpers.NetUtil.HttpClient.GetByteArrayAsync("https://autumn.revolt.chat/attachments/" + message.Attachments[0].Id);
                var pkmInfo = EntityFormat.GetFromBytes(data);
                if (pkmInfo == null)
                {
                    showdownSet = System.Text.Encoding.UTF8.GetString(data);
                }
                else
                {
                    var set = new ShowdownSet(pkmInfo);
                    showdownSet = set.Text;
                }
            } 
            catch { }
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

    [StoatCommand("itemtrade", "item", "it")]
    private async Task HandleItemTradeCommandAsync(UserMessage message, List<string> args)
    {
        if (!await CheckPermissions(message, Hub.Config.Stoat.RoleCanTrade)) return;

        if (args.Count == 0)
        {
            await StoatHelper<T>.SendAsync(_client, message.ChannelId, "Please provide an item name.");
            return;
        }

        int code = 0;
        string itemNamesStr = string.Empty;

        if (int.TryParse(args[0], out code))
            itemNamesStr = string.Join(" ", args.Skip(1));
        else
        {
            ulong userIdNumeric = StoatHelper<T>.ConvertId(message.AuthorId);
            code = Hub.Queues.Info.GetRandomTradeCode(userIdNumeric);
            itemNamesStr = string.Join(" ", args);
        }

        var itemNames = itemNamesStr.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        var batchSettings = Hub.Config.Trade.BatchSettings;
        var maxItemBatch = batchSettings.MaxItemBatchAmount;

        if (typeof(T) == typeof(PB7) && itemNames.Length > 1)
        {
            await StoatHelper<T>.SendAsync(_client, message.ChannelId, "Batch trades are not supported in Let's Go Pikachu/Eevee. You can only request one item at a time.");
            return;
        }

        if (itemNames.Length > 1 && !batchSettings.AllowBatchTrades)
        {
            await StoatHelper<T>.SendAsync(_client, message.ChannelId, "Batch trades are currently disabled. You can only request one item at a time.");
            return;
        }

        if (itemNames.Length > maxItemBatch)
        {
            await StoatHelper<T>.SendAsync(_client, message.ChannelId, $"You can only request up to {maxItemBatch} items at a time.");
            return;
        }

        var pkmList = new List<T>();
        Species species = Hub.Config.Trade.TradeConfiguration.ItemTradeSpecies == Species.None
            ? Species.Pikachu
            : Hub.Config.Trade.TradeConfiguration.ItemTradeSpecies;
        var baseSpeciesName = SpeciesName.GetSpeciesNameGeneration((ushort)species, 2, 8);

        foreach (var itemName in itemNames)
        {
            var set = new ShowdownSet($"{baseSpeciesName} @ {itemName}");
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

        ulong numericId = StoatHelper<T>.ConvertId(message.AuthorId);
        if (pkmList.Count == 1)
            await StoatHelper<T>.AddToQueueAsync(_client, message.ChannelId, code, message.Author.Username, pkmList[0], message.AuthorId, message.Author.Username);
        else
            await StoatHelper<T>.AddBatchContainerToQueueAsync(_client, message.ChannelId, code, message.Author.Username, pkmList[0], pkmList, message.AuthorId, message.Author.Username);
    }

    [StoatCommand("clone", "c")]
    private async Task HandleCloneCommandAsync(UserMessage message, List<string> args)
    {
        if (!await CheckPermissions(message, Hub.Config.Stoat.RoleCanClone)) return;

        ulong userIdNumeric = StoatHelper<T>.ConvertId(message.AuthorId);
        if (Hub.Queues.Info.IsUserInQueue(userIdNumeric))
        {
            await StoatHelper<T>.SendAsync(_client, message.ChannelId, "You are already in the queue.");
            return;
        }

        int code = args.Count > 0 && int.TryParse(args[0], out var c) ? c : Hub.Queues.Info.GetRandomTradeCode(userIdNumeric);
        var lgcode = Hub.Queues.Info.GetRandomLGTradeCode(userIdNumeric);
        var notifier = new StoatTradeNotifier<T>(new T(), new PokeTradeTrainerInfo(message.Author.Username, userIdNumeric), code, message.AuthorId, _client, 1, 1, false, lgcode);
        int uniqueID = TradeUtil.GenerateUniqueTradeID();
        var detail = new PokeTradeDetail<T>(new T(), new PokeTradeTrainerInfo(message.Author.Username, userIdNumeric), notifier, PokeTradeType.Clone, code, false, lgcode, 1, 1, false, false, uniqueID);
        var trade = new TradeEntry<T>(detail, userIdNumeric, PokeRoutineType.Clone, message.Author.Username, uniqueID);
        
        if (Hub.Queues.Info.AddToTradeQueue(trade, userIdNumeric, false, Hub.Config.Stoat.GlobalSudoList.Contains(userIdNumeric)) == QueueResultAdd.Added)
        {
            await notifier.SendInitialQueueUpdate();
            await notifier.SendInitialQueueUpdateToChannel(message.ChannelId, message.Author.Username);
        }
    }

    [StoatCommand("fixot", "fix", "f")]
    private async Task HandleFixOTCommandAsync(UserMessage message, List<string> args)
    {
        if (!await CheckPermissions(message, Hub.Config.Stoat.RoleCanFixOT)) return;

        ulong userIdNumeric = StoatHelper<T>.ConvertId(message.AuthorId);
        if (Hub.Queues.Info.IsUserInQueue(userIdNumeric))
        {
            await StoatHelper<T>.SendAsync(_client, message.ChannelId, "You are already in the queue.");
            return;
        }

        int code = args.Count > 0 && int.TryParse(args[0], out var c) ? c : Hub.Queues.Info.GetRandomTradeCode(userIdNumeric);
        var lgcode = Hub.Queues.Info.GetRandomLGTradeCode(userIdNumeric);
        var notifier = new StoatTradeNotifier<T>(new T(), new PokeTradeTrainerInfo(message.Author.Username, userIdNumeric), code, message.AuthorId, _client, 1, 1, false, lgcode);
        int uniqueID = TradeUtil.GenerateUniqueTradeID();
        var detail = new PokeTradeDetail<T>(new T(), new PokeTradeTrainerInfo(message.Author.Username, userIdNumeric), notifier, PokeTradeType.FixOT, code, false, lgcode, 1, 1, false, false, uniqueID);
        var trade = new TradeEntry<T>(detail, userIdNumeric, PokeRoutineType.FixOT, message.Author.Username, uniqueID);
        
        if (Hub.Queues.Info.AddToTradeQueue(trade, userIdNumeric, false, Hub.Config.Stoat.GlobalSudoList.Contains(userIdNumeric)) == QueueResultAdd.Added)
        {
            await notifier.SendInitialQueueUpdate();
            await notifier.SendInitialQueueUpdateToChannel(message.ChannelId, message.Author.Username);
        }
    }

    [StoatCommand("dittotrade", "ditto", "dt")]
    private async Task HandleDittoTradeCommandAsync(UserMessage message, List<string> args)
    {
        if (!await CheckPermissions(message, Hub.Config.Stoat.RoleCanTrade)) return;

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
            int code = Hub.Queues.Info.GetRandomTradeCode(userIdNumeric);
            await StoatHelper<T>.AddToQueueAsync(_client, message.ChannelId, code, message.Author.Username, pk, message.AuthorId, message.Author.Username);
        }
    }

    [StoatCommand("egg")]
    private async Task HandleEggCommandAsync(UserMessage message, List<string> args)
    {
        if (!await CheckPermissions(message, Hub.Config.Stoat.RoleCanTrade)) return;

        int code = 0;
        string eggName = string.Empty;

        if (args.Count > 0 && int.TryParse(args[0], out code))
            eggName = string.Join(" ", args.Skip(1));
        else
        {
            ulong userIdNumeric = StoatHelper<T>.ConvertId(message.AuthorId);
            code = Hub.Queues.Info.GetRandomTradeCode(userIdNumeric);
            eggName = string.Join(" ", args);
        }

        if (string.IsNullOrWhiteSpace(eggName) && message.Attachments.Count > 0)
        {
            try 
            {
                var data = await SysBot.Pokemon.Helpers.NetUtil.HttpClient.GetByteArrayAsync("https://autumn.revolt.chat/attachments/" + message.Attachments[0].Id);
                var pkmInfo = EntityFormat.GetFromBytes(data);
                if (pkmInfo == null)
                    eggName = System.Text.Encoding.UTF8.GetString(data);
                else
                {
                    var set = new ShowdownSet(pkmInfo);
                    eggName = set.Text;
                }
            } 
            catch { }
        }

        if (string.IsNullOrWhiteSpace(eggName))
        {
            await StoatHelper<T>.SendAsync(_client, message.ChannelId, "Please provide a Showdown set for the egg.");
            return;
        }

        var sav = AutoLegalityWrapper.GetTrainerInfo<T>();
        var template = AutoLegalityWrapper.GetTemplate(new ShowdownSet(eggName));
        var pkm = sav.GenerateEgg(template, out var res);
        
        if (res == LegalizationResult.Regenerated && pkm != null)
        {
            var pk = (T)pkm;
            await StoatHelper<T>.AddToQueueAsync(_client, message.ChannelId, code, message.Author.Username, pk, message.AuthorId, message.Author.Username);
        }
        else
        {
            await StoatHelper<T>.SendAsync(_client, message.ChannelId, "I couldn't process your egg request.");
        }
    }

    [StoatCommand("mysteryegg", "me")]
    private async Task HandleMysteryEggCommandAsync(UserMessage message, List<string> args)
    {
        if (!await CheckPermissions(message, Hub.Config.Stoat.RoleCanTrade)) return;

        ulong userIdNumeric = StoatHelper<T>.ConvertId(message.AuthorId);
        int code = args.Count > 0 && int.TryParse(args[0], out var c) ? c : Hub.Queues.Info.GetRandomTradeCode(userIdNumeric);

        var pkm = TradeModuleHelpers.GenerateLegalMysteryEgg<T>();
        if (pkm == null)
        {
            await StoatHelper<T>.SendAsync(_client, message.ChannelId, "Mystery Eggs are currently disabled or could not be generated.");
            return;
        }

        var result = pkm;

        var trainer = new PokeTradeTrainerInfo(message.Author.Username, userIdNumeric);
        var notifier = new StoatTradeNotifier<T>(result, trainer, code, message.AuthorId, _client, 1, 1, true);
        int uniqueID = TradeUtil.GenerateUniqueTradeID();
        var detail = new PokeTradeDetail<T>(result, trainer, notifier, PokeTradeType.Specific, code, false, null, 1, 1, false, false, uniqueID);
        var trade = new TradeEntry<T>(detail, userIdNumeric, PokeRoutineType.LinkTrade, message.Author.Username, uniqueID);

        if (Hub.Queues.Info.AddToTradeQueue(trade, userIdNumeric, false, Hub.Config.Stoat.GlobalSudoList.Contains(userIdNumeric)) == QueueResultAdd.Added)
        {
            await notifier.SendInitialQueueUpdate();
            await notifier.SendInitialQueueUpdateToChannel(message.ChannelId, message.Author.Username);
        }
    }

    [StoatCommand("batchtrade", "bt")]
    private async Task HandleBatchTradeCommandAsync(UserMessage message, List<string> args)
    {
        if (!await CheckPermissions(message, Hub.Config.Stoat.RoleCanTrade)) return;

        string content = string.Join(" ", args);
        if (string.IsNullOrWhiteSpace(content) && message.Attachments.Count > 0)
        {
            try 
            {
                var data = await SysBot.Pokemon.Helpers.NetUtil.HttpClient.GetByteArrayAsync("https://autumn.revolt.chat/attachments/" + message.Attachments[0].Id);
                var pkmInfo = EntityFormat.GetFromBytes(data);
                if (pkmInfo == null)
                {
                    content = System.Text.Encoding.UTF8.GetString(data);
                }
                else
                {
                    var set = new ShowdownSet(pkmInfo);
                    content = set.Text;
                }
            } 
            catch { }
        }

        if (string.IsNullOrWhiteSpace(content))
        {
            await StoatHelper<T>.SendAsync(_client, message.ChannelId, "Please provide a list of Showdown sets separated by '---'.");
            return;
        }

        ulong userIdNumeric = StoatHelper<T>.ConvertId(message.AuthorId);
        var batchSettings = Hub.Config.Trade.BatchSettings;
        if (!batchSettings.AllowBatchTrades)
        {
            await StoatHelper<T>.SendAsync(_client, message.ChannelId, "Batch trades are disabled.");
            return;
        }

        var sets = TradeModuleHelpers.ParseBatchTradeContent(BatchNormalizer.NormalizeBatchCommands(content));
        if (sets.Count == 0 || sets.Count > batchSettings.MaxPkmsPerTrade)
        {
            await StoatHelper<T>.SendAsync(_client, message.ChannelId, $"You must provide between 1 and {batchSettings.MaxPkmsPerTrade} sets.");
            return;
        }

        int code = Hub.Queues.Info.GetRandomTradeCode(userIdNumeric);
        var pkmList = new List<T>();
        foreach (var setStr in sets)
        {
            var ignoreAutoOT = setStr.Contains("OT:") || setStr.Contains("TID:") || setStr.Contains("SID:");
            var res = await StoatHelper<T>.ProcessShowdownSetAsync(setStr, ignoreAutoOT, userIdNumeric);
            if (res.Pokemon != null) pkmList.Add(res.Pokemon);
        }

        if (pkmList.Count == 0)
        {
            await StoatHelper<T>.SendAsync(_client, message.ChannelId, "No valid sets found.");
            return;
        }

        await StoatHelper<T>.AddBatchContainerToQueueAsync(_client, message.ChannelId, code, message.Author.Username, pkmList[0], pkmList, message.AuthorId, message.Author.Username);
    }

    [StoatCommand("listevents", "le")]
    private async Task HandleListEventsCommandAsync(UserMessage message, List<string> args)
    {
        var folderPath = Hub.Config.Folder.EventsFolder;
        if (string.IsNullOrEmpty(folderPath))
        {
            await StoatHelper<T>.SendAsync(_client, message.ChannelId, "This bot does not have events set up.");
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

    [StoatCommand("eventrequest", "er")]
    private async Task HandleEventRequestCommandAsync(UserMessage message, List<string> args)
    {
        if (!await CheckPermissions(message, Hub.Config.Stoat.RoleCanTrade)) return;
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

    [StoatCommand("battlereadylist", "brl")]
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

    [StoatCommand("battlereadyrequest", "brr")]
    private async Task HandleBattleReadyRequestCommandAsync(UserMessage message, List<string> args)
    {
        if (!await CheckPermissions(message, Hub.Config.Stoat.RoleCanTrade)) return;
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

    [StoatCommand("pokepaste", "pp")]
    private async Task HandlePokepasteCommandAsync(UserMessage message, List<string> args)
    {
        if (!await CheckPermissions(message, Hub.Config.Stoat.RoleCanTrade)) return;
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

    [StoatCommand("specialrequestpokemon", "srp")]
    private async Task HandleSpecialRequestPokemonCommandAsync(UserMessage message, List<string> args)
    {
        if (!await CheckPermissions(message, Hub.Config.Stoat.RoleCanTrade)) return;
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
}
