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
        var description = $"Welcome to NexusBot!\n\n" +
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
                          $"For a full list of commands and detailed usage, visit our documentation: https://nexusbot.org";

        var embed = EmbedHelper.CreateEmbed(
            title: "NexusBot Commands & Help",
            description: description,
            colorHex: "#00FF00"
        );
        await MessageHelper.SendMessageAsync(message.Channel!, string.Empty, embeds: new[] { embed });
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
            $"**ðŸ“Š Project Info:**\n" +
            $"**Main Developer**: [Nexus Risen](https://nexusrisen.net)\n" +
            $"**Mode**: {gameName}\n" +
            $"**Version**: {SysBot.Pokemon.Helpers.NexusBot.Version}\n\n" +
            $"**ðŸ’» System Stats:**\n" +
            $"**Uptime**: {uptime}\n" +
            $"**Memory**: {heapSize} MiB\n\n" +
            $"**ðŸ‘¥ Contributors:**\n" +
            $"**Nexus Risen**: Project Lead & Developer\n" +
            $"**Secludedly**: Medals, Refactoring & Feature Enhancements\n" +
            $"**Lusamine**: Research & Data Analysis\n" +
            $"**Hexbyt3**: Core Engine Enhancements\n" +
            $"**SantaCrab2**: Auto-Legality Mod (ALM)\n\n" +
            $"**ðŸ“¦ Dependencies:**\n" +
            $"**PKHeX.Core**: {GetVersionInfo("PKHeX.Core")}\n" +
            $"**AutoLegality**: {GetVersionInfo("PKHeX.Core.AutoMod")}\n" +
            $"**Base System**: [SysBot.NET](https://github.com/kwsch/SysBot.NET)\n\n" +
            $"*OS: {RuntimeInformation.OSDescription} ({RuntimeInformation.OSArchitecture})*";

        var embed = EmbedHelper.CreateEmbed(
            title: "NexusBot.NET - Information",
            description: description,
            colorHex: "#FFD700",
            iconUrl: SysBot.Pokemon.Helpers.AssetManager.GetAssetUrl("Assets/Icons/Characters/nexusbot.png")
        );

        await MessageHelper.SendMessageAsync(message.Channel!, string.Empty, embeds: new[] { embed });
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
            await MessageHelper.SendMessageAsync(message.Channel!, string.Empty, embeds: new[] { embed });
            return;
        }

        string posString = Hub.Queues.Info.GetPositionString(userIdNumeric, 0);
        int totalInQueue = Hub.Queues.Info.Count;
        var embedStatus = EmbedHelper.CreateEmbed(
            title: "Queue Status",
            description: $"You are currently {posString} out of **{totalInQueue}** in the queue.",
            colorHex: "#00FF00"
        );
        await MessageHelper.SendMessageAsync(message.Channel!, string.Empty, embeds: new[] { embedStatus });
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
            await MessageHelper.SendMessageAsync(message.Channel!, string.Empty, embeds: new[] { embed });
        }
        else
        {
            var embed = EmbedHelper.CreateEmbed(
                title: "Queue Status",
                description: "You are not currently in the queue or cannot be removed.",
                colorHex: "#FF0000"
            );
            await MessageHelper.SendMessageAsync(message.Channel!, string.Empty, embeds: new[] { embed });
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
        await MessageHelper.SendMessageAsync(message.Channel!, string.Empty, embeds: new[] { embed });
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
        await MessageHelper.SendMessageAsync(message.Channel!, string.Empty, embeds: new[] { embed });
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
