using PKHeX.Core;
using StoatSharp;
using SysBot.Base;
using SysBot.Pokemon.Stoat.Commands;
using SysBot.Pokemon.Stoat.Helpers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using SysBot.Pokemon;

namespace SysBot.Pokemon.Stoat;

public partial class SysStoat<T>
{
    [StoatCommand("help", "commands", "hi", "hello")]
    private async Task HandleHelpCommandAsync(UserMessage message, List<string> args)
    {
        var prefix = Hub.Config.Stoat.CommandPrefix;
        var description = $"Welcome to DudeBot!\n\n" +
                          $"**Core Commands**:\n" +
                          $"`{prefix}trade` or `{prefix}t` - Trade a Pokémon using a Showdown set or attachment.\n" +
                          $"`{prefix}batchtrade` or `{prefix}bt` - Trade multiple Pokémon at once.\n" +
                          $"`{prefix}egg` - Generate an egg from a Showdown set.\n" +
                          $"`{prefix}mysteryegg` or `{prefix}me` - Request a random Mystery Egg.\n" +
                          $"`{prefix}itemtrade` or `{prefix}it` - Trade for a specific item.\n\n" +
                          $"**Utility Commands**:\n" +
                          $"`{prefix}queuestatus` or `{prefix}qs` - Check your queue position.\n" +
                          $"`{prefix}queueclear` or `{prefix}qc` - Leave the queue.\n" +
                          $"`{prefix}about` - View bot stats and uptime.\n\n" +
                          $"For a full list of commands and detailed usage, visit our documentation: https://dudebot.org";

        var embed = EmbedHelper.CreateEmbed(
            title: "DudeBot Commands & Help",
            description: description,
            colorHex: "#00FF00"
        );
        await MessageHelper.SendMessageAsync(message.Channel, string.Empty, embeds: new[] { embed });
    }

    [StoatCommand("about", "info", "whoami", "owner")]
    private async Task HandleAboutCommandAsync(UserMessage message, List<string> args)
    {
        var uptime = (DateTime.Now - Process.GetCurrentProcess().StartTime).ToString(@"dd\.hh\:mm\:ss");
        var heapSize = Math.Round(GC.GetTotalMemory(true) / (1024.0 * 1024.0), 2).ToString(CultureInfo.CurrentCulture);
        var gameName = typeof(T).Name switch
        {
            nameof(PA9) => "Pokémon Legends: Z-A",
            nameof(PK9) => "Pokémon Scarlet & Violet",
            nameof(PK8) => "Pokémon Sword & Shield",
            nameof(PA8) => "Pokémon Legends: Arceus",
            nameof(PB8) => "Pokémon BDSP",
            _ => "Pokémon LGPE"
        };

        string description = $"**A high-performance Pokemon automation bot powered by PKHeX.Core.**\n\n" +
            $"**👑 Project Owners:**\n" +
            $"**Havok**: Logo & Asset Creation\n" +
            $"**Link**: Logo & Asset Creation\n\n" +
            $"**📊 Project Info:**\n" +
            $"**Main Developer**: [Nexus Risen](https://nexusrisen.net)\n" +
            $"**Mode**: {gameName}\n" +
            $"**Version**: {SysBot.Pokemon.Helpers.DudeBot.Version}\n\n" +
            $"**💻 System Stats:**\n" +
            $"**Uptime**: {uptime}\n" +
            $"**Memory**: {heapSize} MiB\n\n" +
            $"**👥 Contributors:**\n" +
            $"**Nexus Risen**: Project Lead & Developer\n" +
            $"**Secludedly**: Medals, Refactoring & Feature Enhancements\n" +
            $"**Lusamine**: Research & Data Analysis\n" +
            $"**Hexbyt3**: Core Engine Enhancements\n" +
            $"**SantaCrab2**: Auto-Legality Mod (ALM)\n\n" +
            $"**📦 Dependencies:**\n" +
            $"**PKHeX.Core**: {GetVersionInfo("PKHeX.Core")}\n" +
            $"**AutoLegality**: {GetVersionInfo("PKHeX.Core.AutoMod")}\n" +
            $"**Base System**: [SysBot.NET](https://github.com/kwsch/SysBot.NET)\n\n" +
            $"*OS: {RuntimeInformation.OSDescription} ({RuntimeInformation.OSArchitecture})*";

        var embed = EmbedHelper.CreateEmbed(
            title: "DudeBot.NET - Information",
            description: description,
            colorHex: "#FFD700",
            iconUrl: "https://raw.githubusercontent.com/NexusRisen/Nexus-Risen-Edition-Sprite-Images/main/Assets/Icons/Characters/dudebot.png"
        );

        await MessageHelper.SendMessageAsync(message.Channel, string.Empty, embeds: new[] { embed });
    }

    [StoatCommand("medals", "ml")]
    private async Task HandleMedalsCommandAsync(UserMessage message, List<string> args)
    {
        if (!Hub.Config.Stoat.EnableMedals)
        {
            await StoatHelper<T>.SendAsync(_client, message.ChannelId, "The medals system is currently disabled.");
            return;
        }

        ulong userIdNumeric = StoatHelper<T>.ConvertId(message.AuthorId);
        var tradeCodeStorage = new TradeCodeStorage();
        tradeCodeStorage.UpdateUsername(userIdNumeric, message.Author.Username);
        int totalTrades = tradeCodeStorage.GetTradeCount(userIdNumeric);

        if (totalTrades == 0)
        {
            var zeroEmbed = EmbedHelper.CreateEmbed(
                title: "🏅 Milestone Medal",
                description: $"{message.Author.Username}, you haven't made any trades yet.\nStart trading to earn your first medal!",
                colorHex: "#808080"
            );
            await MessageHelper.SendMessageAsync(message.Channel, string.Empty, embeds: new[] { zeroEmbed });
            return;
        }

        string description = totalTrades switch
        {
            1 => "Congratulations on your first trade!\n**Status:** Beginner Trainer.",
            50 => "You've reached 50 trades!\n**Status:** Rookie Trainer.",
            100 => "You've reached 100 trades!\n**Status:** Rising Star.",
            150 => "You've reached 150 trades!\n**Status:** Challenger.",
            200 => "You've reached 200 trades!\n**Status:** Master Baiter.",
            250 => "You've reached 250 trades!\n**Status:** Star Trainer.",
            300 => "You've reached 300 trades!\n**Status:** Ace Trainer.",
            350 => "You've reached 350 trades!\n**Status:** Veteran Trainer.",
            400 => "You've reached 400 trades!\n**Status:** Expert Trainer.",
            450 => "You've reached 450 trades!\n**Status:** Pokémon Trader.",
            500 => "You've reached 500 trades!\n**Status:** Pokémon Professor.",
            550 => "You've reached 550 trades!\n**Status:** Pokémon Champion.",
            600 => "You've reached 600 trades!\n**Status:** Pokémon Specialist.",
            650 => "You've reached 650 trades!\n**Status:** Pokémon Hero.",
            700 => "You've reached 700 trades!\n**Status:** Pokémon Elite.",
            750 => "You've reached 750 trades!\n**Status:** Pokémon Legend.",
            800 => "You've reached 800 trades!\n**Status:** Region Master.",
            850 => "You've reached 850 trades!\n**Status:** Pokémon Master.",
            900 => "You've reached 900 trades!\n**Status:** World Famous.",
            950 => "You've reached 950 trades!\n**Status:** Master Trader.",
            1000 => "You've reached 1000 trades!\n**Status:** Pokémon God.",
            _ => $"Congratulations on reaching {totalTrades} trades! Keep it going!"
        };

        var embed = EmbedHelper.CreateEmbed(
            title: $"**{message.Author.Username}'s Milestone Medal**",
            description: $"{description}\n**Total Trades**: {totalTrades}\n\n*DudeBot.NET {SysBot.Pokemon.Helpers.DudeBot.Version} | Synchronized via SQL*",
            colorHex: "#FFD700",
            iconUrl: "https://raw.githubusercontent.com/NexusRisen/Nexus-Risen-Edition-Sprite-Images/main/Assets/Icons/Characters/dudebot.png"
        );

        await MessageHelper.SendMessageAsync(message.Channel, string.Empty, embeds: new[] { embed });
    }

    [StoatCommand("leaderboard", "lb", "halloffame", "hof")]
    private async Task HandleLeaderboardCommandAsync(UserMessage message, List<string> args)
    {
        string response = "Check out the top trainers and the community Hall of Fame on our official website!\n\n" +
                          "🌐 **Official Hall of Fame**: https://dudebot.org/leaderboard/\n" +
                          "⚡ **Real-Time Stats**: Rankings are updated globally across all bot hosters.\n\n" +
                          $"*DudeBot.NET {SysBot.Pokemon.Helpers.DudeBot.Version} | Synchronized via SQL*";

        var embed = EmbedHelper.CreateEmbed(
            title: "🏆 GLOBAL MEDALS LEADERBOARD",
            description: response,
            colorHex: "#FFD700"
        );

        await MessageHelper.SendMessageAsync(message.Channel, string.Empty, embeds: new[] { embed });
    }

    [StoatCommand("queuestatus", "qs")]
    private async Task HandleQueueStatusCommandAsync(UserMessage message, List<string> args)
    {
        ulong userIdNumeric = StoatHelper<T>.ConvertId(message.AuthorId);
        if (!Hub.Queues.Info.IsUserInQueue(userIdNumeric))
        {
            var embed = EmbedHelper.CreateEmbed(
                title: "Queue Status",
                description: "You are not currently in the queue.",
                colorHex: "#FF0000"
            );
            await MessageHelper.SendMessageAsync(message.Channel, string.Empty, embeds: new[] { embed });
            return;
        }

        string posString = Hub.Queues.Info.GetPositionString(userIdNumeric, 0);
        int totalInQueue = Hub.Queues.Info.Count;
        var embedStatus = EmbedHelper.CreateEmbed(
            title: "Queue Status",
            description: $"You are currently {posString} out of **{totalInQueue}** in the queue.",
            colorHex: "#00FF00"
        );
        await MessageHelper.SendMessageAsync(message.Channel, string.Empty, embeds: new[] { embedStatus });
    }

    [StoatCommand("queueclear", "qc", "tc")]
    private async Task HandleQueueClearCommandAsync(UserMessage message, List<string> args)
    {
        ulong userIdNumeric = StoatHelper<T>.ConvertId(message.AuthorId);
        var res = Hub.Queues.Info.ClearTrade(userIdNumeric);

        if (res == QueueResultRemove.Removed || res == QueueResultRemove.CurrentlyProcessingRemoved)
        {
            var embed = EmbedHelper.CreateEmbed(
                title: "Queue Status",
                description: "You have been removed from the queue.",
                colorHex: "#00FF00"
            );
            await MessageHelper.SendMessageAsync(message.Channel, string.Empty, embeds: new[] { embed });
        }
        else
        {
            var embed = EmbedHelper.CreateEmbed(
                title: "Queue Status",
                description: "You are not currently in the queue or cannot be removed.",
                colorHex: "#FF0000"
            );
            await MessageHelper.SendMessageAsync(message.Channel, string.Empty, embeds: new[] { embed });
        }
    }

    [StoatCommand("deletetradecode", "dtc")]
    private async Task HandleDeleteTradeCodeCommandAsync(UserMessage message, List<string> args)
    {
        ulong userIdNumeric = StoatHelper<T>.ConvertId(message.AuthorId);
        bool success = Hub.Queues.Info.DeleteTradeCode(userIdNumeric);

        var embed = EmbedHelper.CreateEmbed(
            title: "Trade Code Storage",
            description: success ? "Your saved trade code has been deleted." : "You do not have a saved trade code.",
            colorHex: success ? "#00FF00" : "#FF0000"
        );
        await MessageHelper.SendMessageAsync(message.Channel, string.Empty, embeds: new[] { embed });
    }

    [StoatCommand("linkcode", "link")]
    private async Task HandleLinkCommandAsync(UserMessage message, List<string> args)
    {
        ulong userIdNumeric = StoatHelper<T>.ConvertId(message.AuthorId);

        if (args.Count == 0) // Treat as "linkcode"
        {
            string token = DatabaseService.GenerateLinkToken(userIdNumeric);
            if (token == "DB_OFF" || token == "ERROR")
            {
                var embed = EmbedHelper.CreateEmbed("Account Linking", "Account linking is currently disabled or an error occurred.", "#FF0000");
                await MessageHelper.SendMessageAsync(message.Channel, string.Empty, embeds: new[] { embed });
            }
            else
            {
                var embed = EmbedHelper.CreateEmbed("Account Linking", $"<@{message.AuthorId}> Your account link token is: **{token}**\nThis token will expire in 15 minutes. Go to the other platform and run `link {token}` to link that account.", "#00FF00");
                await MessageHelper.SendMessageAsync(message.Channel, string.Empty, embeds: new[] { embed });
            }
        }
        else // Treat as "link" with token
        {
            string token = args[0].Trim().ToUpper();
            if (token.Length != 6)
            {
                var embed = EmbedHelper.CreateEmbed("Account Linking", "Invalid token format. It should be 6 characters long.", "#FF0000");
                await MessageHelper.SendMessageAsync(message.Channel, string.Empty, embeds: new[] { embed });
                return;
            }
            bool success = DatabaseService.LinkAccount(userIdNumeric, token, "Stoat");
            if (success)
            {
                var embed = EmbedHelper.CreateEmbed("Account Linking", $"<@{message.AuthorId}> successfully linked! Your stats here will now match the primary account you linked from.", "#00FF00");
                await MessageHelper.SendMessageAsync(message.Channel, string.Empty, embeds: new[] { embed });
            }
            else
            {
                var embed = EmbedHelper.CreateEmbed("Account Linking", $"<@{message.AuthorId}> failed to link account. The token may be expired, invalid, or you are trying to link to yourself.", "#FF0000");
                await MessageHelper.SendMessageAsync(message.Channel, string.Empty, embeds: new[] { embed });
            }
        }
    }

    [StoatCommand("ts", "tradestart")]
    private async Task HandleTsCommandAsync(UserMessage message, List<string> args)
    {
        await StoatHelper<T>.SendAsync(_client, message.ChannelId, $"Hello <@{message.AuthorId}>, I am online!");
    }

    [StoatCommand("id")]
    private async Task HandleIdCommandAsync(UserMessage message, List<string> args)
    {
        ulong userIdNumeric = StoatHelper<T>.ConvertId(message.AuthorId);
        ulong channelIdNumeric = StoatHelper<T>.ConvertId(message.ChannelId);
        var embed = EmbedHelper.CreateEmbed("ID Information", $"User ID: {message.AuthorId} (Numeric: {userIdNumeric})\nChannel ID: {message.ChannelId} (Numeric: {channelIdNumeric})", "#00FF00");
        await MessageHelper.SendMessageAsync(message.Channel, string.Empty, embeds: new[] { embed });
    }

    private static string GetVersionInfo(string assemblyName, bool includeVersion = true)
    {
        var assembly = AppDomain.CurrentDomain.GetAssemblies()
            .FirstOrDefault(x => x.GetName().Name == assemblyName);

        var attribute = assembly?.GetCustomAttribute<AssemblyInformationalVersionAttribute>();
        if (attribute == null)
            return "Unknown";

        var info = attribute.InformationalVersion;
        var split = info.Split('+');
        if (split.Length < 2)
            return includeVersion ? info : "Unknown";

        var version = split[0];
        var revision = split[1];

        if (DateTime.TryParseExact(revision, "yyMMddHHmmss", CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out var buildTime))
        {
            var timeStr = buildTime.ToString("yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture);
            return includeVersion ? $"{version} ({timeStr})" : timeStr;
        }

        return includeVersion ? version : "Unknown";
    }
}
