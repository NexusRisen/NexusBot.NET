using PKHeX.Core;
using StoatSharp;
using SysBot.Base;
using SysBot.Pokemon.Helpers;
using SysBot.Pokemon.Stoat.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
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
    
    private int BatchTradeNumber { get; set; }
    private int TotalBatchTrades { get; }
    private bool IsMysteryEgg { get; }

    private readonly ulong _traderID;
    private int _uniqueTradeID;
    private Timer? _periodicUpdateTimer;
    private const int PeriodicUpdateInterval = 60000;
    private bool _isTradeActive = true;
    private bool _initialUpdateSent = false;
    private bool _almostUpNotificationSent = false;
    private int _lastReportedPosition = -1;

    public readonly PokeTradeHub<T> Hub = SysStoat<T>.Runner.Hub;

    public StoatTradeNotifier(T data, PokeTradeTrainerInfo info, int code, string userId, StoatClient client, int batchTradeNumber = 1, int totalBatchTrades = 1, bool isMysteryEgg = false, List<Pictocodes>? lgcode = null)
    {
        Data = data;
        Info = info;
        Code = code;
        UserId = userId;
        _client = client;
        BatchTradeNumber = batchTradeNumber;
        TotalBatchTrades = totalBatchTrades;
        IsMysteryEgg = isMysteryEgg;
        LGCode = lgcode;
        _traderID = StoatHelper<T>.ConvertId(userId);
        _uniqueTradeID = GetUniqueTradeID();
    }

    public Action<PokeRoutineExecutor<T>>? OnFinish { get; set; }

    public void UpdateBatchProgress(int currentBatchNumber, T currentPokemon, int uniqueTradeID) 
    { 
        BatchTradeNumber = currentBatchNumber;
        Data = currentPokemon;
        _uniqueTradeID = uniqueTradeID;
    }

    public void UpdateUniqueTradeID(int uniqueTradeID)
    {
        _uniqueTradeID = uniqueTradeID;
    }

    private int GetUniqueTradeID()
    {
        return (int)(DateTime.UtcNow.Ticks % int.MaxValue);
    }

    private void StartPeriodicUpdates()
    {
        _periodicUpdateTimer?.Dispose();
        _isTradeActive = true;

        _periodicUpdateTimer = new Timer(async _ =>
        {
            if (!_isTradeActive) return;

            try
            {
                var position = Hub.Queues.Info.CheckPosition(_traderID, _uniqueTradeID, PokeRoutineType.LinkTrade);
                if (!position.InQueue) return;

                var currentPosition = position.Position < 1 ? 1 : position.Position;
                _lastReportedPosition = currentPosition;

                if (position.InQueue && position.Detail != null)
                {
                    if (currentPosition == 1 && _initialUpdateSent && !_almostUpNotificationSent)
                    {
                        _almostUpNotificationSent = true;
                        var batchInfo = TotalBatchTrades > 1 ? $"\n\n**Important:** This is a batch trade with {TotalBatchTrades} Pokémon. Please stay in the trade until all are completed!" : "";
                        
                        var dm = await UserHelper.GetUserDMChannelAsync(_client.Rest, UserId);
                        if (dm != null)
                        {
                            var embed = new EmbedBuilder()
                                .SetTitle("You're Up Next")
                                .SetDescription($"Your trade will begin very soon. Please be ready!{batchInfo}")
                                .SetColor(new StoatColor("#FFD700"))
                                .Build();
                            await MessageHelper.SendMessageAsync(dm, string.Empty, embeds: new[] { embed });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LogUtil.LogError($"Unexpected error in periodic trade update: {ex.Message}", "StartPeriodicUpdates");
            }
        }, null, PeriodicUpdateInterval, PeriodicUpdateInterval);
    }

    private void StopPeriodicUpdates()
    {
        _isTradeActive = false;
        _periodicUpdateTimer?.Dispose();
        _periodicUpdateTimer = null;
    }

    public async Task SendInitialQueueUpdate()
    {
        try
        {
            var position = Hub.Queues.Info.CheckPosition(_traderID, _uniqueTradeID, PokeRoutineType.LinkTrade);
            var currentPosition = position.Position < 1 ? 1 : position.Position;
            var botct = Hub.Bots.Count;
            var currentETA = currentPosition > botct ? Hub.Config.Queues.EstimateDelay(currentPosition, botct) : 0;

            _lastReportedPosition = currentPosition;

            var batchDescription = TotalBatchTrades > 1
                ? $"Your batch trade request ({TotalBatchTrades} Pokémon) has been queued.\n\n⚠️ **Important Instructions:**\n• Stay in the trade for all {TotalBatchTrades} trades\n• Have all {TotalBatchTrades} Pokémon ready to trade\n• Do not exit until you see the completion message\n\n**Queue Position**: {currentPosition}"
                : $"Your trade request has been queued.\n**Queue Position**: {currentPosition}";

            var dm = await UserHelper.GetUserDMChannelAsync(_client.Rest, UserId);
            if (dm != null)
            {
                var embed = new EmbedBuilder()
                    .SetTitle(TotalBatchTrades > 1 ? "Batch Trade Queued" : "Trade Request Queued")
                    .SetDescription(batchDescription + $"\n\nWait: {(currentETA > 0 ? $"{currentETA} min" : "<1 min")} | v{DudeBot.Version}")
                    .SetColor(new StoatColor("#008000"))
                    .Build();
                await MessageHelper.SendMessageAsync(dm, string.Empty, embeds: new[] { embed });
            }

            _initialUpdateSent = true;
            StartPeriodicUpdates();
        }
        catch (Exception ex)
        {
            LogUtil.LogError($"Unexpected error sending initial queue update: {ex.Message}", "SendInitialQueueUpdate");
        }
    }

    public async Task SendInitialQueueUpdateToChannel(string channelId, string userName)
    {
        try 
        {
            var ch = await _client.Rest.GetChannelAsync(channelId);
            if (ch != null)
            {
                string imageUrl = TradeExtensions<T>.PokeImg(Data, false, true, null);
                var position = Hub.Queues.Info.CheckPosition(_traderID, _uniqueTradeID, PokeRoutineType.LinkTrade);
                var currentPosition = position.Position < 1 ? 1 : position.Position;

                var text = $"**User:** {userName}\n\nYour trade request has been queued.\n**Queue Position**: {currentPosition}";
                
                var embed = new EmbedBuilder()
                    .SetTitle("Trade Request Queued")
                    .SetColor(new StoatColor("#663399"))
                    .SetDescription(text)
                    .SetImage(imageUrl)
                    .Build();
                await MessageHelper.SendMessageAsync(ch, string.Empty, embeds: new[] { embed });
            }
        } 
        catch (Exception ex) { LogUtil.LogError($"Failed to send channel update to Stoat channel {channelId}: {ex.Message}", "StoatTradeNotifier"); }
    }

    public void TradeInitialize(PokeRoutineExecutor<T> routine, PokeTradeDetail<T> info)
    {
        _uniqueTradeID = info.UniqueTradeID;
        StopPeriodicUpdates();
        _almostUpNotificationSent = true;

        int language = 2;
        var speciesName = IsMysteryEgg ? "Mystery Egg" : SpeciesName.GetSpeciesName(Data.Species, language);
        var receive = Data.Species == 0 ? string.Empty : (IsMysteryEgg ? "" : $" ({Data.Nickname})");

        if (Data is PK9)
        {
            string message;
            if (TotalBatchTrades > 1)
            {
                if (BatchTradeNumber == 1)
                {
                    message = $"Starting your batch trade! Trading {TotalBatchTrades} Pokémon.\n\n" +
                             $"**Trade 1/{TotalBatchTrades}**: {speciesName}{receive}\n\n" +
                             $"⚠️ **IMPORTANT:** Stay in the trade until all {TotalBatchTrades} trades are completed!";
                }
                else
                {
                    message = $"Preparing trade {BatchTradeNumber}/{TotalBatchTrades}: {speciesName}{receive}";
                }
            }
            else
            {
                message = $"Initializing trade{receive}. Please be ready.";
            }

            EmbedHelper.SendTradeInitializingEmbedAsync(_client, UserId, speciesName, Code, IsMysteryEgg, message).ConfigureAwait(false);
        }
        else if (Data is PB7)
        {
            if (LGCode != null && LGCode.Count == 3)
            {
                _ = SendDM($"Initializing trade for {speciesName}. Please be ready.\n**Code:** {LGCode[0]}, {LGCode[1]}, {LGCode[2]}");
            }
        }
        else
        {
            EmbedHelper.SendTradeInitializingEmbedAsync(_client, UserId, speciesName, Code, IsMysteryEgg).ConfigureAwait(false);
        }
    }

    public void TradeSearching(PokeRoutineExecutor<T> routine, PokeTradeDetail<T> info)
    {
        StopPeriodicUpdates();

        var name = Info.TrainerName;
        var trainer = string.IsNullOrEmpty(name) ? string.Empty : $" {name}";

        if (Data is PB7 && LGCode != null && LGCode.Count != 0)
        {
            var batchInfo = TotalBatchTrades > 1 ? $" (Trade {BatchTradeNumber}/{TotalBatchTrades})" : "";
            var message = $"I'm waiting for you{trainer}{batchInfo}! My IGN is **{routine.InGameName}**.";
            _ = SendDM(message);
        }
        else
        {
            string? additionalMessage = null;
            if (TotalBatchTrades > 1)
            {
                if (BatchTradeNumber == 1)
                {
                    additionalMessage = $"Starting batch trade ({TotalBatchTrades} Pokémon total). **Please select your first Pokémon!**";
                }
                else
                {
                    var speciesName = IsMysteryEgg ? "Mystery Egg" : SpeciesName.GetSpeciesName(Data.Species, 2);
                    additionalMessage = $"Trade {BatchTradeNumber}/{TotalBatchTrades}: Now trading {speciesName}. **Select your next Pokémon!**";
                }
            }

            EmbedHelper.SendTradeSearchingEmbedAsync(_client, UserId, trainer, routine.InGameName, additionalMessage).ConfigureAwait(false);
        }
    }

    public void TradeCanceled(PokeRoutineExecutor<T> routine, PokeTradeDetail<T> info, PokeTradeResult msg)
    {
        if (routine != null) OnFinish?.Invoke(routine);
        StopPeriodicUpdates();

        var cancelMessage = TotalBatchTrades > 1
            ? $"Batch trade canceled: {msg}. All remaining trades have been canceled."
            : msg.ToString();

        EmbedHelper.SendTradeCanceledEmbedAsync(_client, UserId, cancelMessage).ConfigureAwait(false);
    }

    public void TradeFinished(PokeRoutineExecutor<T> routine, PokeTradeDetail<T> info, T result)
    {
        if (TotalBatchTrades <= 1 || BatchTradeNumber == TotalBatchTrades)
        {
            if (routine != null) OnFinish?.Invoke(routine);
            StopPeriodicUpdates();
        }

        string message;
        if (TotalBatchTrades > 1)
        {
            if (BatchTradeNumber == TotalBatchTrades)
            {
                message = $"✅ **All {TotalBatchTrades} trades completed successfully!** Thank you for trading!";
            }
            else
            {
                var speciesName = IsMysteryEgg ? "Mystery Egg" : SpeciesName.GetSpeciesName(Data.Species, 2);
                message = $"✅ Trade {BatchTradeNumber}/{TotalBatchTrades} completed! ({speciesName})\n" +
                         $"Preparing trade {BatchTradeNumber + 1}/{TotalBatchTrades}...";
            }
        }
        else
        {
            message = Data.Species != 0 ? $"Trade finished. Enjoy!" : "Trade finished!";
        }

        _ = SendDM(message);
    }

    private async Task SendDM(string message)
    {
        try 
        {
            var dm = await UserHelper.GetUserDMChannelAsync(_client.Rest, UserId);
            if (dm != null)
            {
                await MessageHelper.SendMessageAsync(dm, message);
            }
        } catch (Exception ex) { LogUtil.LogError($"Failed to send DM to Stoat user {UserId}: {ex.Message}", "StoatTradeNotifier"); }
    }

    public void SendNotification(PokeRoutineExecutor<T> routine, PokeTradeDetail<T> info, string message)
    {
        if (TotalBatchTrades > 1 && !message.Contains("Trade") && !message.Contains("batch"))
        {
            message = $"Trade {BatchTradeNumber}/{TotalBatchTrades}: {message}";
        }

        EmbedHelper.SendNotificationEmbedAsync(_client, UserId, message).ConfigureAwait(false);
    }

    public void SendNotification(PokeRoutineExecutor<T> routine, PokeTradeDetail<T> info, PokeTradeSummary message)
    {
        var msg = message.Summary;
        if (message.Details.Count > 0)
            msg += ", " + string.Join(", ", message.Details.Select(z => $"{z.Heading}: {z.Detail}"));
        _ = SendDM(msg);
    }

    public void SendNotification(PokeRoutineExecutor<T> routine, PokeTradeDetail<T> info, T result, string message)
    {
        _ = SendDM(message);
    }

    public void Dispose() 
    { 
        StopPeriodicUpdates();
        GC.SuppressFinalize(this); 
    }
}
