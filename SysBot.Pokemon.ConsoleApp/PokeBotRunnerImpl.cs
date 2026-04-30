using PKHeX.Core;
using SysBot.Pokemon.Discord;
using SysBot.Pokemon.Kook;
using System.Threading;
using System.Threading.Tasks;

namespace SysBot.Pokemon.ConsoleApp;

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
        AddKookBot(Hub.Config.Kook);
    }

    private void AddDiscordBot(DiscordSettings config)
    {
        var token = config.Token;
        if (string.IsNullOrWhiteSpace(token))
            return;

        var bot = new SysCord<T>(this, _config);
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
