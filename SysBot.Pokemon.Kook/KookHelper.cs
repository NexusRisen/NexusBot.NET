using Kook;
using Kook.WebSocket;
using PKHeX.Core;
using PKHeX.Core.AutoMod;
using SysBot.Base;
using SysBot.Pokemon.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SysBot.Pokemon.Kook;

public static class KookHelper<T> where T : PKM, new()
{
    private static TradeQueueInfo<T> Info => SysKook<T>.Runner.Hub.Queues.Info;

    public static async Task AddToQueueAsync(SocketMessage message, int code, string trainerName, T pk, SocketUser trader, KookSocketClient client, List<Pictocodes>? lgcode = null, bool isHiddenTrade = false)
    {
        var userID = trader.Id;
        var name = trader.Username;
        var trainer = new PokeTradeTrainerInfo(trainerName, userID);
        
        var notifier = new KookTradeNotifier<T>(pk, trainer, code, trader, client, lgcode);

        int uniqueTradeID = TradeUtil.GenerateUniqueTradeID();

        var detail = new PokeTradeDetail<T>(pk, trainer, notifier, PokeTradeType.Specific, code, false,
            lgcode, 1, 1, false, false, uniqueTradeID, false, isHiddenTrade);

        var trade = new TradeEntry<T>(detail, userID, PokeRoutineType.LinkTrade, name, uniqueTradeID);
        
        var isSudo = SysKookSettings.Settings.GlobalSudoList.Contains(userID);
        var added = Info.AddToTradeQueue(trade, userID, false, isSudo);

        if (added == QueueResultAdd.Added)
        {
            await notifier.SendInitialQueueUpdate();
        }
        else if (added == QueueResultAdd.AlreadyInQueue)
        {
            await message.Channel.SendTextAsync($"{trader.Username}, you are already in the queue!");
        }
        else if (added == QueueResultAdd.QueueFull)
        {
            await message.Channel.SendTextAsync("The queue is currently full. Please try again later.");
        }
        else if (added == QueueResultAdd.NotAllowedItem)
        {
            await message.Channel.SendTextAsync("Trade blocked: the held item cannot be traded.");
        }
    }

    public static async Task AddBatchContainerToQueueAsync(SocketMessage message, int code, string trainerName, T firstTrade, List<T> allTrades, SocketUser trader, KookSocketClient client)
    {
        var userID = trader.Id;
        var name = trader.Username;
        var trainer_info = new PokeTradeTrainerInfo(trainerName, userID);
        var notifier = new KookTradeNotifier<T>(firstTrade, trainer_info, code, trader, client, null);

        int uniqueTradeID = TradeUtil.GenerateUniqueTradeID();

        var detail = new PokeTradeDetail<T>(firstTrade, trainer_info, notifier, PokeTradeType.Batch, code,
            false, null, 1, allTrades.Count, false, false, uniqueTradeID, false, false)
        {
            BatchTrades = allTrades
        };

        var trade = new TradeEntry<T>(detail, userID, PokeRoutineType.Batch, name, uniqueTradeID: uniqueTradeID);
        
        var isSudo = SysKookSettings.Settings.GlobalSudoList.Contains(userID);
        var added = Info.AddToTradeQueue(trade, userID, false, isSudo);

        if (added == QueueResultAdd.Added)
        {
            await notifier.SendInitialQueueUpdate();
        }
        else if (added == QueueResultAdd.AlreadyInQueue)
        {
            await message.Channel.SendTextAsync($"{trader.Username}, you are already in the queue!");
        }
        else if (added == QueueResultAdd.QueueFull)
        {
            await message.Channel.SendTextAsync("The queue is currently full. Please try again later.");
        }
    }

    public static async Task<ProcessedPokemonResult<T>> ProcessShowdownSetAsync(string content)
    {
        return await Task.Run(() => PokeTradeHelper<T>.ProcessShowdownSet(content, SysKook<T>.Runner.Hub)).ConfigureAwait(false);
    }
}
