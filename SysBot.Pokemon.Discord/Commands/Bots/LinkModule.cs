using Discord;
using Discord.Commands;
using SysBot.Pokemon;
using System.Threading.Tasks;

namespace SysBot.Pokemon.Discord.Commands.Bots;

[Summary("Commands for linking accounts across platforms")]
public class LinkModule : ModuleBase<SocketCommandContext>
{
    [Command("linkcode")]
    [Summary("Generates a temporary code to link your Discord account to another platform (like Slack or Kook)")]
    public async Task GenerateLinkCodeAsync()
    {
        ulong discordId = Context.User.Id;
        string token = DatabaseService.GenerateLinkToken(discordId);
        
        if (token == "DB_OFF" || token == "ERROR")
        {
            await ReplyAsync("Account linking is currently disabled or an error occurred.");
            return;
        }

        try
        {
            await Context.User.SendMessageAsync($"Your account link token is: **{token}**\nThis token will expire in 15 minutes. Go to the other platform (e.g. Slack or Kook) and run `$link {token}` to link that account to this Discord account. Warning: That platform's current trade stats will be overwritten by your Discord stats.");
            await ReplyAsync($"{Context.User.Mention}, I've DMed you your linking token!");
        }
        catch
        {
            await ReplyAsync($"{Context.User.Mention}, I couldn't DM you! Please make sure your DMs are open to receive your linking token.");
        }
    }

    [Command("link")]
    [Summary("Links your current Discord account to an existing profile using a link code")]
    public async Task LinkAccountAsync([Summary("The 6-character token from the other platform")] string token)
    {
        ulong discordId = Context.User.Id;
        token = token.Trim().ToUpper();

        if (token.Length != 6)
        {
            await ReplyAsync("Invalid token format. It should be 6 characters long.");
            return;
        }

        bool success = DatabaseService.LinkAccount(discordId, token, "Discord");

        if (success)
        {
            await ReplyAsync($"{Context.User.Mention}, successfully linked! Your stats here will now match the primary account you linked from.");
        }
        else
        {
            await ReplyAsync($"{Context.User.Mention}, failed to link account. The token may be expired, invalid, or you are trying to link to yourself.");
        }
    }
}
