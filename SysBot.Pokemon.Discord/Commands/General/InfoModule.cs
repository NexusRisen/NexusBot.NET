using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using PKHeX.Core;
using SysBot.Pokemon.Helpers;

namespace SysBot.Pokemon.Discord;

public class InfoModule<T> : ModuleBase<SocketCommandContext> where T : PKM, new()
{
    private const string WebsiteUrl = "https://nexusrisen.net";
    private const string ThumbnailUrl = SysBot.Pokemon.Helpers.AssetManager.GetAssetUrl("Assets/Icons/Characters/nexusbot.png");

    [Command("Info")]
    [Alias("about", "whoami", "owner")]
    public async Task InfoAsync()
    {
        var app = await Context.Client.GetApplicationInfoAsync().ConfigureAwait(false);
        var gameName = GetGameName();
        var uptime = GetUptime();
        var heapSize = GetHeapSize();

        var embed = new EmbedBuilder()
            .WithTitle("NexusBot.NET - Information")
            .WithDescription("A high-performance Pokemon automation bot powered by PKHeX.Core.")
            .WithColor(Color.Gold)
            .WithThumbnailUrl(ThumbnailUrl)
            .AddField("ðŸ“Š Project Info",
                $"{Format.Bold("Main Developer")}: [Nexus Risen]({WebsiteUrl})\n" +
                $"{Format.Bold("Owner")}: {app.Owner.Mention}\n" +
                $"{Format.Bold("Mode")}: {gameName}\n" +
                $"{Format.Bold("Version")}: {NexusBot.Version}", inline: true)
            .AddField("ðŸ› ï¸  System Stats",
                $"{Format.Bold("Uptime")}: {uptime}\n" +
                $"{Format.Bold("Guilds")}: {Context.Client.Guilds.Count}\n" +
                $"{Format.Bold("Users")}: {Context.Client.Guilds.Sum(g => (long)g.MemberCount)}\n" +
                $"{Format.Bold("Memory")}: {heapSize} MiB", inline: true)
            .AddField("ðŸ‘¥ Contributors",
                $"{Format.Bold("Nexus Risen")}: Project Lead & Developer\n" +
                $"{Format.Bold("Secludedly")}: Medals, Refactoring & Feature Enhancements\n" +
                $"{Format.Bold("Lusamine")}: Research & Data Analysis\n" +
                $"{Format.Bold("Hexbyt3")}: Core Engine Enhancements\n" +
                $"{Format.Bold("SantaCrab2")}: Auto-Legality Mod (ALM)")
            .AddField("ðŸ“¦ Dependencies",
                $"{Format.Bold("PKHeX.Core")}: {GetVersionInfo("PKHeX.Core")}\n" +
                $"{Format.Bold("AutoLegality")}: {GetVersionInfo("PKHeX.Core.AutoMod")}\n" +
                $"{Format.Bold("Base System")}: [SysBot.NET](https://github.com/kwsch/SysBot.NET)")
            .WithFooter(footer => footer.Text = $"OS: {RuntimeInformation.OSDescription} ({RuntimeInformation.OSArchitecture})")
            .Build();

        await ReplyAsync(embed: embed).ConfigureAwait(false);
    }

    private static string GetGameName() => typeof(T).Name switch
    {
        nameof(PA9) => "Pokémon Legends: Z-A",
        nameof(PK9) => "Pokémon Scarlet & Violet",
        nameof(PK8) => "Pokémon Sword & Shield",
        nameof(PA8) => "Pokémon Legends: Arceus",
        nameof(PB8) => "Pokémon BDSP",
        _ => "Pokémon LGPE"
    };

    private static string GetHeapSize() =>
        Math.Round(GC.GetTotalMemory(true) / (1024.0 * 1024.0), 2).ToString(CultureInfo.CurrentCulture);

    private static string GetUptime() =>
        (DateTime.Now - Process.GetCurrentProcess().StartTime).ToString(@"dd\.hh\:mm\:ss");

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
