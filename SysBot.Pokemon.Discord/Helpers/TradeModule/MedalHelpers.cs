using Discord;
using Discord.WebSocket;
using System;

namespace SysBot.Pokemon.Discord;

public static class MedalHelpers
{
    private const int MaxMedalMilestone = 1000;
    private const string DefaultBaseUrl = "https://raw.githubusercontent.com/NexusRisen/Nexus-Risen-Edition-Sprite-Images/main/Assets/Medals/Progress/{0:D4}.png?v=4";

    public static int GetCurrentMilestone(int totalTrades)
    {
        if (totalTrades < 1) return 0;
        if (totalTrades < 50) return 1;
        
        int milestone = (totalTrades / 50) * 50;
        return Math.Min(milestone, MaxMedalMilestone);
    }

    public static bool IsExactMilestone(int totalTrades)
    {
        if (totalTrades == 1) return true;
        if (totalTrades <= 0) return false;
        // Congratulate infinitely every 50 trades
        return totalTrades % 50 == 0;
    }

    public static int CalculateTotalMedals(int tradeCount)
    {
        if (tradeCount < 1) return 0;
        return 1 + Math.Min(MaxMedalMilestone / 50, tradeCount / 50);
    }

    public static string GetMilestoneStatus(int milestone)
    {
        int clampedMilestone = GetCurrentMilestone(milestone);
        return clampedMilestone switch
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
        // Clamp the milestone so requesting >1000 trades falls back to the max 1000 medal image
        int clampedMilestone = GetCurrentMilestone(milestone);
        
        try
        {
            if (string.IsNullOrWhiteSpace(baseUrl))
            {
                return string.Format(DefaultBaseUrl, clampedMilestone);
            }
            return string.Format(baseUrl, clampedMilestone);
        }
        catch
        {
            return string.Format(DefaultBaseUrl, clampedMilestone);
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
