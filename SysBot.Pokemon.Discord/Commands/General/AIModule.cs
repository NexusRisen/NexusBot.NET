using Discord;
using Discord.Commands;
using System.Threading.Tasks;

namespace SysBot.Pokemon.Discord;

[Summary("Commands related to the AI Chatbot.")]
public class AIModule : ModuleBase<SocketCommandContext>
{
    [Command("ai")]
    [Summary("Shows information about how to use the AI Chatbot.")]
    public async Task AIHelpAsync()
    {
        var botMention = Context.Client.CurrentUser.Mention;
        var prefix = SysCordSettings.Settings.CommandPrefix;
        
        var embed = new EmbedBuilder()
            .WithTitle("🤖 AI Chatbot Help")
            .WithDescription($"You can chat with me using advanced AI powered by Hugging Face!")
            .AddField("How to Chat", $"Just mention me and ask a question!\nExample: {botMention} Give me a competitive Garchomp set.")
            .AddField("Memory", "I remember the last few messages in our conversation, so you can ask follow-up questions.")
            .AddField("Commands", $"`{prefix}clearAI` - Clears your conversation history if I get confused.")
            .WithColor(Color.Blue)
            .WithFooter("Note: I can only provide legal Pokemon sets!")
            .Build();

        await ReplyAsync(embed: embed).ConfigureAwait(false);
    }

    [Command("clearAI")]
    [Summary("Clears your AI conversation history to start a fresh chat.")]
    public async Task ClearAIAsync()
    {
        if (SysCordSettings.AIService == null)
        {
            var prefix = SysCordSettings.Settings.CommandPrefix;
            await ReplyAsync($"AI Chatbot is not enabled. Enable it in the settings and use {prefix}clearAI after it's active.").ConfigureAwait(false);
            return;
        }

        SysCordSettings.AIService.ClearHistory(Context.User.Id);
        await ReplyAsync("Your AI conversation history has been cleared! 🧹").ConfigureAwait(false);
    }
}
