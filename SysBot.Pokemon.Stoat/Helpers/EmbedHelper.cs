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

    public static async Task SendTradeDetailsEmbedAsync<T>(StoatClient client, string userId, string trainerMention, T pk, bool isMysteryEgg)
        where T : PKM, new()
    {
        try
        {
            var dm = await UserHelper.GetUserDMChannelAsync(client.Rest, userId);
            if (dm == null) return;

            if (isMysteryEgg || pk.IsEgg)
            {
                var eggEmbed = new EmbedBuilder()
                    .SetTitle("Trade Complete - Mystery Egg")
                    .SetDescription($"**User:** {trainerMention}\n\nYour mystery egg has been traded! Good luck with the hatch!")
                    .SetIconUrl("https://raw.githubusercontent.com/NexusRisen/Nexus-Risen-Edition-Sprite-Images/main/Assets/Eggs/mysteryegg3.png")
                    .SetColor(new StoatColor("#FFD700"))
                    .Build();
                await MessageHelper.SendMessageAsync(dm, string.Empty, embeds: new[] { eggEmbed });
                return;
            }

            var details = PokemonDetailsHelper<T>.Extract(pk);

            // Build title line: e.g. "★ Haunter-Gmax (F)"
            string formSuffix = string.IsNullOrEmpty(details.FormName) ? "" : $"-{details.FormName}";
            string title = $"{details.SpecialSymbols}{details.SpeciesName}{formSuffix}";

            // Build description: User line + core stats block
            var desc = new System.Text.StringBuilder();
            var settings = SysStoat<T>.Runner.Config.Trade.TradeEmbedSettings;

            SysBot.Base.LogUtil.LogInfo($"TradeEmbedSettings -> UseEmbeds: {settings.UseEmbeds}, ShowLevel: {settings.ShowLevel}, Level: {details.Level}, Moves Count: {details.Moves.Count}", "Stoat Embed");

            desc.AppendLine($"**User:** {trainerMention}");
            
            if (settings.ShowScale && !string.IsNullOrEmpty(details.Scale))
                desc.AppendLine($"**Scale:** {details.Scale}");
            if (settings.ShowLevel) desc.AppendLine($"**Level:** {details.Level}");
            if (settings.ShowBall) desc.AppendLine($"**Ball:** {details.Ball}");
            if (settings.ShowMetLevel) desc.AppendLine($"**Met Level:** {details.MetLevel}");
            if (settings.ShowMetDate && !string.IsNullOrEmpty(details.MetDate))
                desc.AppendLine($"**Met Date:** {details.MetDate}");
            if (settings.ShowMetLocation && !string.IsNullOrEmpty(details.MetLocation))
                desc.AppendLine($"**Met Location:** {details.MetLocation}");
            if (settings.ShowAbility) desc.AppendLine($"**Ability:** {details.Ability}");
            if (settings.ShowNature) desc.AppendLine($"**{details.Nature}**");
            if (settings.ShowLanguage) desc.AppendLine($"**Language:** {details.Language}");
            if (!string.IsNullOrEmpty(details.HeldItem))
                desc.AppendLine($"**Held Item:** {details.HeldItem}");
            if (settings.ShowTeraType && !string.IsNullOrEmpty(details.TeraType))
                desc.AppendLine($"**Tera Type:** {details.TeraType}");
            if (settings.ShowIVs) desc.AppendLine($"**IVs:** {details.IVsDisplay}");
            if (settings.ShowEVs) desc.AppendLine($"**EVs:** {details.EVsDisplay}");

            // Moves section
            if (details.Moves.Count > 0)
            {
                desc.AppendLine();
                desc.AppendLine("**MOVES**");
                foreach (var move in details.Moves)
                    desc.AppendLine(move);
            }

            string color = details.IsSquareShiny ? "#FFD700" : details.IsShiny ? "#C0C0C0" : "#7B68EE";

            var embed = new EmbedBuilder()
                .SetTitle($"Trade Complete [v{SysBot.Pokemon.Helpers.DudeBot.Version}] - {title}")
                .SetDescription(desc.ToString().TrimEnd())
                .SetImage(details.ImageUrl)
                .SetColor(new StoatColor(color))
                .Build();

            await MessageHelper.SendMessageAsync(dm, string.Empty, embeds: new[] { embed });
        }
        catch (Exception ex)
        {
            LogUtil.LogError($"Error sending trade details embed: {ex.Message}", "SendTradeDetailsEmbedAsync");
        }
    }
}
