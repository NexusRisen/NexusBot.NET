using Discord;
using Discord.WebSocket;
using System;

namespace SysBot.Pokemon.Discord;

public static class MedalDiscordHelpers
{
    public static Embed CreateMedalsEmbed(SocketUser user, int milestone, int totalTrades)
    {
        string status = MedalHelpers.GetMilestoneStatus(milestone);
        string description = $"Total Trades: **{totalTrades}**\n**Current Status:** {status}";

        if (milestone > 0)
        {
            return new EmbedBuilder()
                .WithTitle($"{user.Username}'s Trading Status")
                .WithColor(new Color(255, 215, 0))
                .WithDescription(description)
                .WithThumbnailUrl(MedalHelpers.GetMedalImageUrl(milestone))
                .Build();
        }
        else
        {
            return new EmbedBuilder()
                .WithTitle($"{user.Username}'s Trading Status")
                .WithColor(new Color(0, 255, 0)) // Lime Green
                .WithDescription($"{description}\nNo trades on record yet, thank you for participating!")
                .WithThumbnailUrl(MedalHelpers.GetMedalImageUrl(0))
                .Build();
        }
    }
}
