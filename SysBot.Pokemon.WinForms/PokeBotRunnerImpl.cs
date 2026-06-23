using PKHeX.Core;
using SysBot.Pokemon.Discord;
using SysBot.Pokemon.Kook;
using SysBot.Base;

using SysBot.Pokemon.Stoat;
using System.Threading;
using System.Threading.Tasks;

namespace SysBot.Pokemon.WinForms;

/// <summary>
/// Bot Environment implementation with Integrations added.
/// </summary>
public class PokeBotRunnerImpl<T> : PokeBotRunner<T> where T : PKM, new()
{
    private readonly ProgramConfig _config;

    public PokeBotRunnerImpl(PokeTradeHub<T> hub, BotFactory<T> fac, ProgramConfig config) : base(hub, fac)
    {
        _config = config;
    }

    protected override void AddIntegrations()
    {
        AddDiscordBot(Hub.Config.Discord);
        AddStoatBot(Hub.Config.Stoat);
        AddKookBot(Hub.Config.Kook);

    }

    private void AddDiscordBot(DiscordSettings config)
    {
        var token = config.Token;
        if (string.IsNullOrWhiteSpace(token))
            return;

        Task.Run(async () =>
        {
            while (!IntegrationTokenSource.Token.IsCancellationRequested)
            {
                var bot = new SysCord<T>(this, _config);
                lock (Integrations)
                    Integrations.Add(bot);
                
                try
                {
                    await bot.MainAsync(token, IntegrationTokenSource.Token);
                }
                catch (System.Exception ex)
                {
                    LogUtil.LogText($"SysCord encountered a critical error: {ex.Message}");
                }
                finally
                {
                    lock (Integrations)
                        Integrations.Remove(bot);
                    
                    bot.Dispose();

                    if (!IntegrationTokenSource.Token.IsCancellationRequested)
                    {
                        LogUtil.LogText("Rebuilding SysCord in 5 seconds...");
                        try { await Task.Delay(5000, IntegrationTokenSource.Token); } catch { }
                    }
                }
            }
        }, IntegrationTokenSource.Token);
    }

    private void AddStoatBot(StoatSettings config)
    {
        var token = config.Token;
        if (string.IsNullOrWhiteSpace(token))
            return;

        var bot = new SysStoat<T>(this, _config);
        Integrations.Add(bot);
        Task.Run(() => bot.MainAsync(token, IntegrationTokenSource.Token), IntegrationTokenSource.Token);
    }

    private void AddKookBot(KookSettings config)
    {
        var token = config.Token;
        if (string.IsNullOrWhiteSpace(token))
            return;

        var bot = new SysKook<T>(this, _config);
        Integrations.Add(bot);
        Task.Run(() => bot.MainAsync(token, IntegrationTokenSource.Token), IntegrationTokenSource.Token);
    }


}
