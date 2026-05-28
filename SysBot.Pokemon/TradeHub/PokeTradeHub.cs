using PKHeX.Core;
using SysBot.Base;
using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace SysBot.Pokemon;

/// <summary>
/// Centralizes logic for trade bot coordination.
/// </summary>
/// <typeparam name="T">Type of <see cref="PKM"/> to distribute.</typeparam>
public class PokeTradeHub<T> : IDisposable where T : PKM, new()
{
    public static readonly PokeTradeLogNotifier<T> LogNotifier = new();

    /// <summary> Trade Bots only, used to delegate multi-player tasks </summary>
    public readonly ConcurrentPool<PokeRoutineExecutorBase> Bots = new();

    public readonly BotSynchronizer BotSync;

    public readonly PokeTradeHubConfig Config;

    public readonly TradeQueueManager<T> Queues;

    public PokeTradeHub(PokeTradeHubConfig config)
    {
        Config = config;
        DatabaseService.Initialize(config.Database);
        var pool = new PokemonPool<T>(config);
        Ledy = new LedyDistributor<T>(pool);
        BotSync = new BotSynchronizer(config.Distribution);
        BotSync.BarrierReleasingActions.Add(() => LogUtil.LogInfo("Barrier", $"{BotSync.Barrier.ParticipantCount} bots released."));

        Queues = new TradeQueueManager<T>(this);

        // Start background heartbeat to track live bots on the website
        Task.Run(async () =>
        {
            while (true)
            {
                try
                {
                    string game = typeof(T).Name.Replace("PK", "").Replace("PA", "");
                    await DatabaseService.SendBotHeartbeat(config.BotName, game).ConfigureAwait(false);
                }
                catch { }
                await Task.Delay(TimeSpan.FromMinutes(1)).ConfigureAwait(false);
            }
        });
    }

    public void Dispose()
    {
        BotSync.Dispose();
        Ledy.Dispose();
        GC.SuppressFinalize(this);
    }

    public void CleanupMaintenance()
    {
        Ledy.CleanupStaleUsers(TimeSpan.FromHours(24));
    }

    public bool TradeBotsReady => !Bots.All(z => z.Config.CurrentRoutineType == PokeRoutineType.Idle);

    #region Distribution Queue

    public readonly LedyDistributor<T> Ledy;

    #endregion Distribution Queue
}
