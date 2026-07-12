using Discord;
using Discord.Commands;
using System.Threading.Tasks;

namespace SysBot.Pokemon.Discord;

public class PingModule : ModuleBase<SocketCommandContext>
{
    [Command("ping")]
    [Summary("Makes the bot respond, indicating that it is running.")]
    public async Task PingAsync()
    {
        var latency = Context.Client.Latency;
        var embed = new EmbedBuilder()
            .WithTitle("Ping Response")
            .WithDescription($"Pong! Latency: **{latency}ms**. Now stop @&#$?&! pinging me.")
            .WithColor(Color.Green)
            .Build();

        await ReplyAsync(embed: embed).ConfigureAwait(false);
    }
}
