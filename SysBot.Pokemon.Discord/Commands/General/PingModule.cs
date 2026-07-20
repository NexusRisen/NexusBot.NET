using Discord;
using Discord.Commands;
using System;
using System.Threading.Tasks;

namespace SysBot.Pokemon.Discord;

public class PingModule : ModuleBase<SocketCommandContext>
{
    private static readonly string[] _responses = 
    {
        "Systems online. All parameters nominal.",
        "Awaiting input... Connection established.",
        "Diagnostic complete. No anomalies detected.",
        "Uplink successful. Ready for commands.",
        "Processing request... Target acquired.",
        "Mainframe synced. Operations optimal.",
        "Signal received loud and clear.",
        "Sensors active. Scanning environment..."
    };
    private static readonly Random _rng = new();

    [Command("ping")]
    [Summary("Makes the bot respond, indicating that it is running.")]
    public async Task PingAsync()
    {
        var latency = Context.Client.Latency;
        var response = _responses[_rng.Next(_responses.Length)];

        var embed = new EmbedBuilder()
            .WithTitle("System Diagnostic")
            .WithDescription($"{response}\n\n*Latency: {latency}ms*")
            .WithColor(Color.Green)
            .Build();

        await ReplyAsync(embed: embed).ConfigureAwait(false);
    }
}
