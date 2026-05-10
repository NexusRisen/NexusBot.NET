using Discord;
using Discord.Net;
using PKHeX.Core;
using SysBot.Base;
using SysBot.Pokemon.Helpers;
using System;
using System.Threading.Tasks;

namespace SysBot.Pokemon.Discord;

public static class EmbedHelper
{
    public static async Task SendNotificationEmbedAsync(IUser user, string message)
    {
        try
        {
            var dm = await SysCordSettings.Manager.GetOrCreateDMAsync(user).ConfigureAwait(false);
            if (dm == null)
            {
                LogUtil.LogError($"Could not create DM channel for user {user.Username} ({user.Id}). Skipping notification.", "SendNotificationEmbedAsync");
                return;
            }

            var embed = new EmbedBuilder()
                .WithTitle("Notice")
                .WithDescription(message)
                .WithTimestamp(DateTimeOffset.Now)
                .WithThumbnailUrl("https://raw.githubusercontent.com/NexusRisen/Nexus-Risen-Edition-Sprite-Images/main/Assets/Bot/DM/dm-legalityerror.gif")
                .WithColor(Color.Orange)
                .Build();

            await dm.SendMessageAsync(embed: embed).ConfigureAwait(false);
        }
        catch (ObjectDisposedException)
        {
            LogUtil.LogError("Discord client disposed when sending notification embed.", "SendNotificationEmbedAsync");
        }
        catch (HttpException ex) when (ex.DiscordCode.HasValue && ex.DiscordCode.Value == (DiscordErrorCode)40003)
        {
            LogUtil.LogError($"Opening DMs too fast! User: {user.Username} ({user.Id})", "SendNotificationEmbedAsync");
            SysCordSettings.Manager.ClearDMChannelCache(user.Id);
        }
        catch (Exception ex)
        {
            LogUtil.LogError($"Error sending notification embed: {ex.Message}", "SendNotificationEmbedAsync");
        }
    }

    public static async Task SendTradeCanceledEmbedAsync(IUser user, string reason)
    {
        try
        {
            var dm = await SysCordSettings.Manager.GetOrCreateDMAsync(user).ConfigureAwait(false);
            if (dm == null)
            {
                LogUtil.LogError($"Could not create DM channel for user {user.Username} ({user.Id}). Skipping trade canceled message.", "SendTradeCanceledEmbedAsync");
                return;
            }

            var embed = new EmbedBuilder()
                .WithTitle("Trade Canceled")
                .WithDescription(reason)
                .WithTimestamp(DateTimeOffset.Now)
                .WithThumbnailUrl("https://raw.githubusercontent.com/NexusRisen/Nexus-Risen-Edition-Sprite-Images/main/Assets/Bot/DM/dm-uhoherror.gif")
                .WithColor(Color.Red)
                .Build();

            await dm.SendMessageAsync(embed: embed).ConfigureAwait(false);
        }
        catch (ObjectDisposedException)
        {
            LogUtil.LogError("Discord client disposed when sending trade canceled embed.", "SendTradeCanceledEmbedAsync");
        }
        catch (HttpException ex) when (ex.DiscordCode.HasValue && ex.DiscordCode.Value == (DiscordErrorCode)40003)
        {
            LogUtil.LogError($"Opening DMs too fast! User: {user.Username} ({user.Id})", "SendTradeCanceledEmbedAsync");
            SysCordSettings.Manager.ClearDMChannelCache(user.Id);
        }
        catch (Exception ex)
        {
            LogUtil.LogError($"Error sending trade canceled embed: {ex.Message}", "SendTradeCanceledEmbedAsync");
        }
    }

    public static async Task SendTradeCodeEmbedAsync(IUser user, int code)
    {
        try
        {
            var dm = await SysCordSettings.Manager.GetOrCreateDMAsync(user).ConfigureAwait(false);
            if (dm == null)
            {
                LogUtil.LogError($"Could not create DM channel for user {user.Username} ({user.Id}). Skipping trade code message.", "SendTradeCodeEmbedAsync");
                return;
            }

            var embed = new EmbedBuilder()
                .WithTitle("Link Trade Code")
                .WithDescription($"**Code:** `{code:0000 0000}`\n\n*I will notify you when it is time to search.*")
                .WithTimestamp(DateTimeOffset.Now)
                .WithThumbnailUrl("https://raw.githubusercontent.com/NexusRisen/Nexus-Risen-Edition-Sprite-Images/main/Assets/Bot/DM/dm-tradecode.gif")
                .WithColor(Color.Gold)
                .Build();

            await dm.SendMessageAsync(embed: embed).ConfigureAwait(false);
        }
        catch (ObjectDisposedException)
        {
            LogUtil.LogError("Discord client disposed when sending trade code embed.", "SendTradeCodeEmbedAsync");
        }
        catch (HttpException ex) when (ex.DiscordCode.HasValue && ex.DiscordCode.Value == (DiscordErrorCode)40003)
        {
            LogUtil.LogError($"Opening DMs too fast! User: {user.Username} ({user.Id})", "SendTradeCodeEmbedAsync");
            SysCordSettings.Manager.ClearDMChannelCache(user.Id);
        }
        catch (Exception ex)
        {
            LogUtil.LogError($"Error sending trade code embed: {ex.Message}", "SendTradeCodeEmbedAsync");
        }
    }

    public static async Task SendTradeFinishedEmbedAsync<T>(IUser user, string message, T pk, bool isMysteryEgg)
        where T : PKM, new()
    {
        try
        {
            string thumbnailUrl = isMysteryEgg 
                ? "https://raw.githubusercontent.com/NexusRisen/Nexus-Risen-Edition-Sprite-Images/main/Assets/Eggs/mysteryegg3.png" 
                : TradeExtensions<T>.PokeImg(pk, false, true, null);

            var embed = new EmbedBuilder()
                .WithTitle("Trade Completed")
                .WithDescription(message)
                .WithTimestamp(DateTimeOffset.Now)
                .WithThumbnailUrl(thumbnailUrl)
                .WithColor(Color.Teal)
                .Build();

            await user.SendMessageAsync(embed: embed).ConfigureAwait(false);
        }
        catch (ObjectDisposedException)
        {
            LogUtil.LogError("Discord client disposed when sending trade finished embed.", "SendTradeFinishedEmbedAsync");
        }
        catch (Exception ex)
        {
            LogUtil.LogError($"Error sending trade finished embed: {ex.Message}", "SendTradeFinishedEmbedAsync");
        }
    }

    public static async Task SendTradeInitializingEmbedAsync(IUser user, string speciesName, int code, bool isMysteryEgg, string? message = null)
    {
        try
        {
            var dm = await SysCordSettings.Manager.GetOrCreateDMAsync(user).ConfigureAwait(false);
            if (dm == null)
            {
                LogUtil.LogError($"Could not create DM channel for user {user.Username} ({user.Id}). Skipping trade initializing message.", "SendTradeInitializingEmbedAsync");
                return;
            }

            var embed = new EmbedBuilder()
                .WithTitle("Initializing Trade")
                .WithDescription($"**Pokémon:** {(isMysteryEgg ? "Mystery Egg" : speciesName)}\n**Code:** `{code:0000 0000}`")
                .WithTimestamp(DateTimeOffset.Now)
                .WithThumbnailUrl("https://raw.githubusercontent.com/NexusRisen/Nexus-Risen-Edition-Sprite-Images/main/Assets/Bot/DM/dm-initializingbot.gif")
                .WithColor(Color.Blue);

            if (!string.IsNullOrEmpty(message))
            {
                embed.WithDescription($"{embed.Description}\n\n{message}");
            }

            await dm.SendMessageAsync(embed: embed.Build()).ConfigureAwait(false);
        }
        catch (ObjectDisposedException)
        {
            LogUtil.LogError("Discord client disposed when sending trade initializing embed.", "SendTradeInitializingEmbedAsync");
        }
        catch (HttpException ex) when (ex.DiscordCode.HasValue && ex.DiscordCode.Value == (DiscordErrorCode)40003)
        {
            LogUtil.LogError($"Opening DMs too fast! User: {user.Username} ({user.Id})", "SendTradeInitializingEmbedAsync");
            SysCordSettings.Manager.ClearDMChannelCache(user.Id);
        }
        catch (Exception ex)
        {
            LogUtil.LogError($"Error sending trade initializing embed: {ex.Message}", "SendTradeInitializingEmbedAsync");
        }
    }

    public static async Task SendTradeSearchingEmbedAsync(IUser user, string trainerName, string inGameName, string? message = null)
    {
        try
        {
            var dm = await SysCordSettings.Manager.GetOrCreateDMAsync(user).ConfigureAwait(false);
            if (dm == null)
            {
                LogUtil.LogError($"Could not create DM channel for user {user.Username} ({user.Id}). Skipping trade searching message.", "SendTradeSearchingEmbedAsync");
                return;
            }

            var embed = new EmbedBuilder()
                .WithTitle("Searching for Trade")
                .WithDescription($"**Waiting For:** {trainerName}\n**My IGN:** {inGameName}\n\n*Please begin searching now using your code.*")
                .WithTimestamp(DateTimeOffset.Now)
                .WithThumbnailUrl("https://raw.githubusercontent.com/NexusRisen/Nexus-Risen-Edition-Sprite-Images/main/Assets/Bot/DM/dm-nowsearching.gif")
                .WithColor(Color.Green);

            if (!string.IsNullOrEmpty(message))
            {
                embed.WithDescription($"{embed.Description}\n\n{message}");
            }

            await dm.SendMessageAsync(embed: embed.Build()).ConfigureAwait(false);
        }
        catch (ObjectDisposedException)
        {
            LogUtil.LogError("Discord client disposed when sending trade searching embed.", "SendTradeSearchingEmbedAsync");
        }
        catch (HttpException ex) when (ex.DiscordCode.HasValue && ex.DiscordCode.Value == (DiscordErrorCode)40003)
        {
            LogUtil.LogError($"Opening DMs too fast! User: {user.Username} ({user.Id})", "SendTradeSearchingEmbedAsync");
            SysCordSettings.Manager.ClearDMChannelCache(user.Id);
        }
        catch (Exception ex)
        {
            LogUtil.LogError($"Error sending trade searching embed: {ex.Message}", "SendTradeSearchingEmbedAsync");
        }
    }
}
