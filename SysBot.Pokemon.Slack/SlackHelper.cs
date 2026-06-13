using PKHeX.Core;
using PKHeX.Core.AutoMod;
using SysBot.Base;
using SysBot.Pokemon.Helpers;
using SlackNet;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SysBot.Pokemon.Slack;

public static class SlackHelper<T> where T : PKM, new()
{
    private static TradeQueueInfo<T> Info => SysSlack<T>.Runner.Hub.Queues.Info;

    public static async Task AddToQueueAsync(ISlackApiClient client, string channelId, int code, string trainerName, T pk, string userId, string userName, List<Pictocodes>? lgcode = null, bool isHiddenTrade = false)
    {
        var trainer = new PokeTradeTrainerInfo(trainerName, ConvertId(userId));
        var notifier = new SlackTradeNotifier<T>(pk, trainer, code, userId, client, lgcode);
        int uniqueTradeID = TradeUtil.GenerateUniqueTradeID();

        var detail = new PokeTradeDetail<T>(pk, trainer, notifier, PokeTradeType.Specific, code, false, lgcode, 1, 1, false, false, uniqueTradeID, false, isHiddenTrade);
        var trade = new TradeEntry<T>(detail, ConvertId(userId), PokeRoutineType.LinkTrade, userName, uniqueTradeID);

        var isSudo = SysSlackSettings.Settings.GlobalSudoList.Contains(ConvertId(userId));
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

    public static async Task AddBatchContainerToQueueAsync(ISlackApiClient client, string channelId, int code, string trainerName, T firstTrade, List<T> allTrades, string userId, string userName)
    {
        var trainer = new PokeTradeTrainerInfo(trainerName, ConvertId(userId));
        var notifier = new SlackTradeNotifier<T>(firstTrade, trainer, code, userId, client, null);
        int uniqueTradeID = TradeUtil.GenerateUniqueTradeID();

        var detail = new PokeTradeDetail<T>(firstTrade, trainer, notifier, PokeTradeType.Batch, code, false, null, 1, allTrades.Count, false, false, uniqueTradeID, false, false) { BatchTrades = allTrades };
        var trade = new TradeEntry<T>(detail, ConvertId(userId), PokeRoutineType.Batch, userName, uniqueTradeID: uniqueTradeID);

        var isSudo = SysSlackSettings.Settings.GlobalSudoList.Contains(ConvertId(userId));
        var added = Info.AddToTradeQueue(trade, ConvertId(userId), false, isSudo);

        if (added == QueueResultAdd.Added)
            await notifier.SendInitialQueueUpdate();
        else if (added == QueueResultAdd.AlreadyInQueue)
            await SendAsync(client, channelId, $"{userName}, you are already in the queue!");
        else if (added == QueueResultAdd.QueueFull)
            await SendAsync(client, channelId, "The queue is currently full. Please try again later.");
    }

    public static async Task<ProcessedPokemonResult<T>> ProcessShowdownSetAsync(string content)
    {
        return await Task.Run(() => PokeTradeHelper<T>.ProcessShowdownSet(content, SysSlack<T>.Runner.Hub)).ConfigureAwait(false);
    }

    public static async Task SendAsync(ISlackApiClient client, string channelId, string text)
    {
        try { await client.Chat.PostMessage(new SlackNet.WebApi.Message { Channel = channelId, Text = text }); } catch { }
    }

    // Convert string Slack ID (e.g. U123456) to a unique ulong for DudeBot internal queue tracking
    public static ulong ConvertId(string slackId)
    {
        ulong result = 0;
        foreach (char c in slackId) result = result * 31 + c;
        return result;
    }
}
