using PKHeX.Core;
using StoatSharp;
using SysBot.Base;
using SysBot.Pokemon.Helpers;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SysBot.Pokemon.Stoat;

public class StoatTradeNotifier<T> : IPokeTradeNotifier<T>, IDisposable where T : PKM, new()
{
    private T Data { get; set; }
    private PokeTradeTrainerInfo Info { get; }
    private int Code { get; }
    private string UserId { get; }
    private List<Pictocodes>? LGCode { get; }
    private StoatClient _client;

    public StoatTradeNotifier(T data, PokeTradeTrainerInfo info, int code, string userId, StoatClient client, List<Pictocodes>? lgcode = null)
    {
        Data = data;
        Info = info;
        Code = code;
        UserId = userId;
        _client = client;
        LGCode = lgcode;
    }

    public Action<PokeRoutineExecutor<T>>? OnFinish { get; set; }

    public void UpdateBatchProgress(int currentBatchNumber, T currentPokemon, int uniqueTradeID) { Data = currentPokemon; }

    private async Task SendDM(string message)
    {
        try 
        {
            var user = await UserHelper.GetUserAsync(_client.Rest, UserId);
            if (user != null)
            {
                var dm = await UserHelper.GetUserDMChannelAsync(_client.Rest, UserId);
                if (dm != null)
                    await MessageHelper.SendMessageAsync(dm, message);
            }
        } catch (Exception ex) { LogUtil.LogError($"Failed to send DM to Stoat user {UserId}: {ex.Message}", "StoatTradeNotifier"); }
    }

    public Task SendInitialQueueUpdate() => SendDM("Your trade request has been queued.");

    public void TradeInitialize(PokeRoutineExecutor<T> routine, PokeTradeDetail<T> info)
    {
        _ = Task.Run(() => {
            var speciesName = SpeciesName.GetSpeciesName(Data.Species, 2);
            if (Data is PB7 && LGCode != null && LGCode.Count == 3)
                _ = SendDM($"Initializing trade for {speciesName}. Please be ready.\nCode: {LGCode[0]}, {LGCode[1]}, {LGCode[2]}");
            else
                _ = SendDM($"Initializing trade for {speciesName}. Code: {Code:D8}. Please be ready.");
        });
    }

    public void TradeSearching(PokeRoutineExecutor<T> routine, PokeTradeDetail<T> info)
    {
        var trainer = string.IsNullOrEmpty(Info.TrainerName) ? string.Empty : $" {Info.TrainerName}";
        _ = SendDM($"I'm waiting for you{trainer}! My IGN is *{routine.InGameName}*.");
    }

    public void TradeCanceled(PokeRoutineExecutor<T> routine, PokeTradeDetail<T> info, PokeTradeResult msg)
    {
        OnFinish?.Invoke(routine);
        _ = SendDM($"Trade canceled: {msg}");
    }

    public void TradeFinished(PokeRoutineExecutor<T> routine, PokeTradeDetail<T> info, T result) => _ = SendDM("Trade finished. Enjoy!");
    public void SendNotification(PokeRoutineExecutor<T> routine, PokeTradeDetail<T> info, string message) => _ = SendDM(message);
    public void SendNotification(PokeRoutineExecutor<T> routine, PokeTradeDetail<T> info, PokeTradeSummary message) => _ = SendDM(message.Summary);
    public void SendNotification(PokeRoutineExecutor<T> routine, PokeTradeDetail<T> info, T result, string message) => _ = SendDM(message);
    public void Dispose() { GC.SuppressFinalize(this); }
}
