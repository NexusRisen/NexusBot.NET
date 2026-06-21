using Discord;
using Discord.Commands;
using System.Threading.Tasks;

namespace SysBot.Pokemon.Discord.Commands
{
    public class ReportModule : ModuleBase<SocketCommandContext>
    {
        [Command("report")]
        [Summary("Get a button to open the Report Issue modal.")]
        public async Task ReportCommandAsync()
        {
            var builder = new ComponentBuilder()
                .WithButton("Report an Issue", "report_issue_btn", ButtonStyle.Primary);

            await ReplyAsync("Click the button below to report an issue to the developers.", components: builder.Build());
        }
    }
}
