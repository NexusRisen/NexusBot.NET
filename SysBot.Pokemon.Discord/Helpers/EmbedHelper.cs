using Discord;
using PKHeX.Core;
using SysBot.Pokemon.Helpers;
using System;
using System.Threading.Tasks;

namespace SysBot.Pokemon.Discord;

public static class EmbedHelper
{
    // NexusRisen Theme Colors
    private static readonly Color ColorPrimary = new Color(43, 45, 49);     // Dark Gray (Discord-like)
    private static readonly Color ColorAccent = new Color(88, 101, 242);    // Blurple
    private static readonly Color ColorSuccess = new Color(87, 242, 135);   // Green
    private static readonly Color ColorDanger = new Color(237, 66, 69);     // Red
    private static readonly Color ColorWarning = new Color(254, 231, 92);   // Yellow
    private static readonly Color ColorInfo = new Color(88, 101, 242);      // Blue/Blurple

    // Common Footer
    private static readonly EmbedFooterBuilder Footer = new EmbedFooterBuilder()
        .WithText("NexusRisen PokeBot • Powered by SysBot.NET")
        .WithIconUrl("https://raw.githubusercontent.com/hexbyt3/sprites/main/pokeball.png");

    public static async Task SendNotificationEmbedAsync(IUser user, string message)
    {
        var embed = new EmbedBuilder()
            .WithTitle("📢 Notification")
            .WithDescription(message)
            .WithTimestamp(DateTimeOffset.Now)
            .WithThumbnailUrl("https://raw.githubusercontent.com/hexbyt3/sprites/main/exclamation.gif")
            .WithColor(ColorInfo)
            .WithFooter(Footer)
            .Build();

        await user.SendMessageAsync(embed: embed).ConfigureAwait(false);
    }

    public static async Task SendTradeCodeEmbedAsync(IUser user, int code)
    {
        var embed = new EmbedBuilder()
            .WithTitle("🔄 Ready to Trade!")
            .WithDescription($"Please enter the following Link Code:")
            .AddField("Link Code", $"`{code:0000 0000}`", true)
            .WithTimestamp(DateTimeOffset.Now)
            .WithThumbnailUrl("https://raw.githubusercontent.com/hexbyt3/sprites/main/tradecode.gif")
            .WithColor(ColorAccent)
            .WithFooter(Footer)
            .Build();

        await user.SendMessageAsync(embed: embed).ConfigureAwait(false);
    }

    public static async Task SendTradeFinishedEmbedAsync<T>(IUser user, string message, T pk, bool isMysteryEgg)
        where T : PKM, new()
    {
        string thumbnailUrl;
        string title = "✅ Trade Completed";

        if (isMysteryEgg)
        {
            thumbnailUrl = "https://raw.githubusercontent.com/hexbyt3/sprites/main/mysteryegg3.png";
            title = "🥚 Mystery Egg Sent!";
        }
        else
        {
            thumbnailUrl = TradeExtensions<T>.PokeImg(pk, false, true, null);
        }

        var embed = new EmbedBuilder()
            .WithTitle(title)
            .WithDescription(message)
            .WithTimestamp(DateTimeOffset.Now)
            .WithThumbnailUrl(thumbnailUrl)
            .WithColor(ColorSuccess)
            .WithFooter(Footer)
            .Build();

        await user.SendMessageAsync(embed: embed).ConfigureAwait(false);
    }

    public static async Task SendTradeInitializingEmbedAsync(IUser user, string speciesName, int code, bool isMysteryEgg, string? message = null)
    {
        if (isMysteryEgg)
        {
            speciesName = "**Mystery Egg**";
        }

        var embed = new EmbedBuilder()
            .WithTitle("🚀 Trade Initializing...")
            .AddField("Pokémon", speciesName, true)
            .AddField("Link Code", $"`{code:0000 0000}`", true)
            .WithTimestamp(DateTimeOffset.Now)
            .WithThumbnailUrl("https://raw.githubusercontent.com/hexbyt3/sprites/main/initializing.gif")
            .WithColor(ColorAccent) // Use accent color instead of orange
            .WithFooter(Footer);

        if (!string.IsNullOrEmpty(message))
        {
            embed.WithDescription(message);
        }
        else
        {
            embed.WithDescription("Please enter the Link Code and wait for the bot.");
        }

        var builtEmbed = embed.Build();
        await user.SendMessageAsync(embed: builtEmbed).ConfigureAwait(false);
    }

    public static async Task SendTradeSearchingEmbedAsync(IUser user, string trainerName, string inGameName, string? message = null)
    {
        var embed = new EmbedBuilder()
            .WithTitle($"🔍 Searching for Partner...")
            .AddField("Trainer", trainerName, true)
            .AddField("Bot IGN", inGameName, true)
            .WithTimestamp(DateTimeOffset.Now)
            .WithThumbnailUrl("https://raw.githubusercontent.com/hexbyt3/sprites/main/searching.gif")
            .WithColor(ColorWarning)
            .WithFooter(Footer);

        if (!string.IsNullOrEmpty(message))
        {
            embed.WithDescription(message);
        }
        else
        {
            embed.WithDescription("Please stand by, the trade will begin shortly.");
        }

        var builtEmbed = embed.Build();
        await user.SendMessageAsync(embed: builtEmbed).ConfigureAwait(false);
    }

    public static async Task SendTradeCanceledEmbedAsync(IUser user, string reason)
    {
        var embed = new EmbedBuilder()
            .WithTitle("⛔ Trade Canceled")
            .WithDescription(reason)
            .WithTimestamp(DateTimeOffset.Now)
            .WithColor(ColorDanger)
            .WithFooter(Footer)
            .Build();

        await user.SendMessageAsync(embed: embed).ConfigureAwait(false);
    }
}
