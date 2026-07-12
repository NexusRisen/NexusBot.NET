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
        var embed = new EmbedBuilder()
            .WithTitle("Ping Response")
            .WithDescription("Pong! Now stop @&#$?&! pinging me.")
            .WithImageUrl(SysBot.Pokemon.Helpers.AssetManager.GetPingGifUrl())
            .WithColor(Color.Green)
            .Build();

        await ReplyAsync(embed: embed).ConfigureAwait(false);
    }
}
