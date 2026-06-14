using StoatSharp;
using PKHeX.Core;
using SysBot.Base;
using SysBot.Pokemon.Helpers;
using System;
using System.Threading.Tasks;

namespace SysBot.Pokemon.Stoat.Helpers;

public static class EmbedHelper
{
    public static Embed CreateEmbed(string title, string description, string colorHex, string? iconUrl = null)
    {
        var builder = new EmbedBuilder()
            .SetTitle(title)
            .SetDescription(description)
            .SetColor(new StoatColor(colorHex));
            
        if (!string.IsNullOrEmpty(iconUrl))
            builder.SetIconUrl(iconUrl);

        return builder.Build();
    }

    public static async Task SendNotificationEmbedAsync(StoatClient client, string userId, string message)
    {
        try
        {
            var dm = await UserHelper.GetUserDMChannelAsync(client.Rest, userId);
            if (dm != null)
            {
                var embed = CreateEmbed("Notice", message, "#FFA500", "https://raw.githubusercontent.com/NexusRisen/Nexus-Risen-Edition-Sprite-Images/main/Assets/Bot/DM/dm-legalityerror.gif");
                await MessageHelper.SendMessageAsync(dm, string.Empty, embeds: new[] { embed });
            }
        }
        catch (Exception ex)
        {
            LogUtil.LogError($"Error sending notification embed: {ex.Message}", "SendNotificationEmbedAsync");
        }
    }

    public static async Task SendTradeCanceledEmbedAsync(StoatClient client, string userId, string reason)
    {
        try
        {
            var dm = await UserHelper.GetUserDMChannelAsync(client.Rest, userId);
            if (dm != null)
            {
                var embed = new EmbedBuilder()
                    .SetTitle("Trade Canceled")
                    .SetDescription(reason)
                    .SetIconUrl("https://raw.githubusercontent.com/NexusRisen/Nexus-Risen-Edition-Sprite-Images/main/Assets/Bot/DM/dm-uhoherror.gif")
                    .SetColor(new StoatColor("#FF0000")) // Red
                    .Build();

                await MessageHelper.SendMessageAsync(dm, string.Empty, embeds: new[] { embed });
            }
        }
        catch (Exception ex)
        {
            LogUtil.LogError($"Error sending trade canceled embed: {ex.Message}", "SendTradeCanceledEmbedAsync");
        }
    }

    public static async Task SendTradeCodeEmbedAsync(StoatClient client, string userId, int code)
    {
        try
        {
            var dm = await UserHelper.GetUserDMChannelAsync(client.Rest, userId);
            if (dm != null)
            {
                var embed = new EmbedBuilder()
                    .SetTitle("Link Trade Code")
                    .SetDescription($"**Code:** `{code:0000 0000}`\n\n*I will notify you when it is time to search.*")
                    .SetIconUrl("https://raw.githubusercontent.com/NexusRisen/Nexus-Risen-Edition-Sprite-Images/main/Assets/Bot/DM/dm-tradecode.gif")
                    .SetColor(new StoatColor("#FFD700")) // Gold
                    .Build();

                await MessageHelper.SendMessageAsync(dm, string.Empty, embeds: new[] { embed });
            }
        }
        catch (Exception ex)
        {
            LogUtil.LogError($"Error sending trade code embed: {ex.Message}", "SendTradeCodeEmbedAsync");
        }
    }

    public static async Task SendTradeFinishedEmbedAsync<T>(StoatClient client, string userId, string message, T pk, bool isMysteryEgg)
        where T : PKM, new()
    {
        try
        {
            string thumbnailUrl = isMysteryEgg 
                ? "https://raw.githubusercontent.com/NexusRisen/Nexus-Risen-Edition-Sprite-Images/main/Assets/Eggs/mysteryegg3.png" 
                : TradeExtensions<T>.PokeImg(pk, false, true, null);

            var dm = await UserHelper.GetUserDMChannelAsync(client.Rest, userId);
            if (dm != null)
            {
                var embed = new EmbedBuilder()
                    .SetTitle("Trade Completed")
                    .SetDescription(message)
                    .SetIconUrl(thumbnailUrl)
                    .SetColor(new StoatColor("#008080")) // Teal
                    .Build();

                await MessageHelper.SendMessageAsync(dm, string.Empty, embeds: new[] { embed });
            }
        }
        catch (Exception ex)
        {
            LogUtil.LogError($"Error sending trade finished embed: {ex.Message}", "SendTradeFinishedEmbedAsync");
        }
    }

    public static async Task SendTradeInitializingEmbedAsync(StoatClient client, string userId, string speciesName, int code, bool isMysteryEgg, string? message = null)
    {
        try
        {
            var dm = await UserHelper.GetUserDMChannelAsync(client.Rest, userId);
            if (dm != null)
            {
                var description = $"**Pokémon:** {(isMysteryEgg ? "Mystery Egg" : speciesName)}\n**Code:** `{code:0000 0000}`";
                if (!string.IsNullOrEmpty(message))
                {
                    description += $"\n\n{message}";
                }

                var embed = new EmbedBuilder()
                    .SetTitle("Initializing Trade")
                    .SetDescription(description)
                    .SetIconUrl("https://raw.githubusercontent.com/NexusRisen/Nexus-Risen-Edition-Sprite-Images/main/Assets/Bot/DM/dm-initializingbot.gif")
                    .SetColor(new StoatColor("#0000FF")) // Blue
                    .Build();

                await MessageHelper.SendMessageAsync(dm, string.Empty, embeds: new[] { embed });
            }
        }
        catch (Exception ex)
        {
            LogUtil.LogError($"Error sending trade initializing embed: {ex.Message}", "SendTradeInitializingEmbedAsync");
        }
    }

    public static async Task SendTradeSearchingEmbedAsync(StoatClient client, string userId, string trainerName, string inGameName, string? message = null)
    {
        try
        {
            var dm = await UserHelper.GetUserDMChannelAsync(client.Rest, userId);
            if (dm != null)
            {
                var description = $"**Waiting For:** {trainerName}\n**My IGN:** {inGameName}\n\n*Please begin searching now using your code.*";
                if (!string.IsNullOrEmpty(message))
                {
                    description += $"\n\n{message}";
                }

                var embed = new EmbedBuilder()
                    .SetTitle("Searching for Trade")
                    .SetDescription(description)
                    .SetIconUrl("https://raw.githubusercontent.com/NexusRisen/Nexus-Risen-Edition-Sprite-Images/main/Assets/Bot/DM/dm-nowsearching.gif")
                    .SetColor(new StoatColor("#008000")) // Green
                    .Build();

                await MessageHelper.SendMessageAsync(dm, string.Empty, embeds: new[] { embed });
            }
        }
        catch (Exception ex)
        {
            LogUtil.LogError($"Error sending trade searching embed: {ex.Message}", "SendTradeSearchingEmbedAsync");
        }
    }
}
