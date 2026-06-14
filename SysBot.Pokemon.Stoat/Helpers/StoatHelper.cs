using PKHeX.Core;
using PKHeX.Core.AutoMod;
using StoatSharp;
using SysBot.Base;
using SysBot.Pokemon.Helpers;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SysBot.Pokemon.Stoat.Helpers;

public static class StoatHelper<T> where T : PKM, new()
{
    private static TradeQueueInfo<T> Info => SysStoat<T>.Runner.Hub.Queues.Info;

    public static async Task AddToQueueAsync(StoatClient client, string channelId, int code, string trainerName, T pk, string userId, string userName, List<Pictocodes>? lgcode = null, bool isHiddenTrade = false)
    {
        var trainer = new PokeTradeTrainerInfo(trainerName, ConvertId(userId));
        var notifier = new StoatTradeNotifier<T>(pk, trainer, code, userId, client, lgcode);
        int uniqueTradeID = TradeUtil.GenerateUniqueTradeID();

        var detail = new PokeTradeDetail<T>(pk, trainer, notifier, PokeTradeType.Specific, code, false, lgcode, 1, 1, false, false, uniqueTradeID, false, isHiddenTrade);
        var trade = new TradeEntry<T>(detail, ConvertId(userId), PokeRoutineType.LinkTrade, userName, uniqueTradeID);

        var isSudo = SysStoatSettings.Settings.GlobalSudoList.Contains(ConvertId(userId));
        var added = Info.AddToTradeQueue(trade, ConvertId(userId), false, isSudo);

        if (added == QueueResultAdd.Added)
            await notifier.SendInitialQueueUpdate();
        else if (added == QueueResultAdd.AlreadyInQueue)
            await SendAsync(client, channelId, $"{userName}, you are already in the queue!");
        else if (added == QueueResultAdd.QueueFull)
            await SendAsync(client, channelId, "The queue is currently full. Please try again later.");
        else if (added == QueueResultAdd.NotAllowedItem)
            await SendAsync(client, channelId, "Trade blocked: the held item cannot be traded.");
    }

    public static async Task AddBatchContainerToQueueAsync(StoatClient client, string channelId, int code, string trainerName, T firstTrade, List<T> allTrades, string userId, string userName)
    {
        var trainer = new PokeTradeTrainerInfo(trainerName, ConvertId(userId));
        var notifier = new StoatTradeNotifier<T>(firstTrade, trainer, code, userId, client, null);
        int uniqueTradeID = TradeUtil.GenerateUniqueTradeID();

        var detail = new PokeTradeDetail<T>(firstTrade, trainer, notifier, PokeTradeType.Batch, code, false, null, 1, allTrades.Count, false, false, uniqueTradeID, false, false) { BatchTrades = allTrades };
        var trade = new TradeEntry<T>(detail, ConvertId(userId), PokeRoutineType.Batch, userName, uniqueTradeID: uniqueTradeID);

        var isSudo = SysStoatSettings.Settings.GlobalSudoList.Contains(ConvertId(userId));
        var added = Info.AddToTradeQueue(trade, ConvertId(userId), false, isSudo);

        if (added == QueueResultAdd.Added)
            await notifier.SendInitialQueueUpdate();
        else if (added == QueueResultAdd.AlreadyInQueue)
            await SendAsync(client, channelId, $"{userName}, you are already in the queue!");
        else if (added == QueueResultAdd.QueueFull)
            await SendAsync(client, channelId, "The queue is currently full. Please try again later.");
    }

    public static async Task<ProcessedPokemonResult<T>> ProcessShowdownSetAsync(string content, bool ignoreAutoOT = false, ulong userID = 0)
    {
        return await Task.Run(() => PokeTradeHelper<T>.ProcessShowdownSet(content, SysStoat<T>.Runner.Hub, ignoreAutoOT, userID)).ConfigureAwait(false);
    }

    public static async Task SendAsync(StoatClient client, string channelId, string text)
    {
        try { 
            var ch = await client.Rest.GetChannelAsync(channelId);
            if (ch is TextChannel tc)
            {
                await tc.SendMessageAsync(text);
            }
        } catch { }
    }

    // Convert string Stoat ID to a unique ulong for DudeBot internal queue tracking
    public static ulong ConvertId(string stoatId)
    {
        ulong result = 0;
        foreach (char c in stoatId) result = result * 31 + c;
        return result;
    }
}
