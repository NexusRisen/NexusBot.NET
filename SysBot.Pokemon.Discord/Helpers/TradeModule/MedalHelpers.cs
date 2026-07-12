using Discord;
using Discord.WebSocket;
using System;

namespace SysBot.Pokemon.Discord;

public static class MedalHelpers
{
    public static int GetCurrentMilestone(int totalTrades)
    {
        if (totalTrades < 1) return 0;
        if (totalTrades < 50) return 1;
        
        int milestone = (totalTrades / 50) * 50;
        return Math.Min(milestone, 1000);
    }

    public static bool IsExactMilestone(int totalTrades)
    {
        if (totalTrades == 1) return true;
        if (totalTrades <= 0) return false;
        if (totalTrades % 50 == 0 && totalTrades <= 1000) return true;
        return false;
    }

    public static int CalculateTotalMedals(int tradeCount)
    {
        if (tradeCount < 1) return 0;
        return 1 + Math.Min(20, tradeCount / 50);
    }

    public static string GetMilestoneStatus(int milestone)
    {
        return milestone switch
        {
            1 => "Beginner Trainer",
            50 => "Rookie Trainer",
            100 => "Rising Star",
            150 => "Challenger",
            200 => "Advanced Trainer",
            250 => "Star Trainer",
            300 => "Ace Trainer",
            350 => "Veteran Trainer",
            400 => "Expert Trainer",
            450 => "Pokémon Trader",
            500 => "Pokémon Professor",
            550 => "Pokémon Champion",
            600 => "Pokémon Specialist",
            650 => "Pokémon Hero",
            700 => "Pokémon Elite",
            750 => "Pokémon Legend",
            800 => "Region Master",
            850 => "Pokémon Master",
            900 => "World Famous",
            950 => "Master Trader",
            1000 => "Pokémon God",
            _ => "New Trainer"
        };
    }

    public static string GetMilestoneCongratulations(int tradeCount)
    {
        if (tradeCount == 1)
            return $"Congratulations on your first trade!\n**Status:** {GetMilestoneStatus(1)}.";
        
        return $"You've reached {tradeCount} trades!\n**Status:** {GetMilestoneStatus(tradeCount)}.";
    }

    public static string GetMedalImageUrl(string baseUrl, int milestone)
    {
        try
        {
            return string.Format(baseUrl, milestone);
        }
        catch
        {
            return $"https://raw.githubusercontent.com/NexusRisen/Nexus-Risen-Edition-Sprite-Images/main/Assets/Medals/{milestone:D4}.png";
        }
    }

    public static Embed CreateMedalsEmbed(SocketUser user, int milestone, int totalTrades, string baseUrl)
    {
        string status = GetMilestoneStatus(milestone);
        string description = $"Total Trades: **{totalTrades}**\n**Current Status:** {status}";

        if (milestone > 0)
        {
            return new EmbedBuilder()
                .WithTitle($"{user.Username}'s Trading Status")
                .WithColor(new Color(255, 215, 0))
                .WithDescription(description)
                .WithThumbnailUrl(GetMedalImageUrl(baseUrl, milestone))
                .Build();
        }
        else
        {
            return new EmbedBuilder()
                .WithTitle($"{user.Username}'s Trading Status")
                .WithColor(new Color(0, 255, 0)) // Lime Green
                .WithDescription($"{description}\nNo trades on record yet, thank you for participating!")
                .WithThumbnailUrl(GetMedalImageUrl(baseUrl, 0))
                .Build();
        }
    }
}
