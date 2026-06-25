using PKHeX.Core;
using PKHeX.Core.AutoMod;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;

namespace SysBot.Pokemon;

public class LegalitySettings
{
    private static readonly List<GameVersion> DefaultPriorityOrder = Enum.GetValues<GameVersion>().Where(GameUtil.IsValidSavedVersion).Reverse().ToList();
    private static readonly List<EncounterTypeGroup> DefaultPrioritizeEncounters =
    [
        EncounterTypeGroup.Slot, EncounterTypeGroup.Egg,
        EncounterTypeGroup.Static, EncounterTypeGroup.Mystery,
        EncounterTypeGroup.Trade,
    ];

    private string DefaultTrainerName = "NexusBot.NET";
    private const string Generate = nameof(Generate);
    private const string Misc = nameof(Misc);
    public override string ToString() => "Legality Generating Settings";

    // Generate
    [Category(Generate), Description("MGDB directory path for Wonder Cards.")]
    public string MGDBPath { get; set; } = string.Empty;

    [Category(Misc), Description("Apply valid PokÃ©mon with the trainer's OT/SID/TID (AutoOT)"), DisplayName("Use Auto-OT")]
    public bool UseTradePartnerInfo { get; set; } = true;

    [Category(Generate), Description("Allow users to submit custom trainer data in Showdown sets, overrides the bot's OT, TID, SID & OT Gender."), DisplayName("Allow Trainer Data Input")]
    public bool AllowTrainerDataOverride { get; set; } = true;

    [Category(Generate), Description("Folder for PKM files with trainer data to use for regenerated PKM files."), DisplayName("Bot Trainer info folderpath")]
    public string GeneratePathTrainerInfo { get; set; } = string.Empty;

    [Category(Generate), Description("Default Original Trainer name for PKM files that don't match any of the provided PKM files."), DisplayName("Bot's OT")]
    public string GenerateOT
    {
        get => DefaultTrainerName;
        set
        {
            if (string.IsNullOrWhiteSpace(value))
                return;
            if (!StringsUtil.IsSpammyString(value))
                DefaultTrainerName = value;
        }
    }

    [Category(Generate), Description("Default 16-bit Trainer ID (TID) for requests that don't match any of the provided trainer data files. This should be a 5-digit number."), DisplayName("Bot's TID")]
    public ushort GenerateTID16 { get; set; } = 12345;

    [Category(Generate), Description("Default 16-bit Secret ID (SID) for requests that don't match any of the provided trainer data files. This should be a 5-digit number."), DisplayName("Bot's SID")]
    public ushort GenerateSID16 { get; set; } = 54321;

    [Category(Generate), Description("Default language for PKM files that don't match any of the provided PKM files.")]
    public LanguageID GenerateLanguage { get; set; } = LanguageID.English;

    [Category(Generate), Description("Method of searching for encounters when generating PokÃ©mon. \"NativeOnly\" searches current game pair only, \"NewestFirst\" searches from most recent game, and \"PriorityOrder\" uses the order designated in the \"PriorityOrder\" setting.")]
    public GameVersionPriorityType GameVersionPriority { get; set; } = GameVersionPriorityType.NativeOnly;

    [Category(Generate), Description("The order of GameVersions ALM will attempt to legalize from.")]
    public List<GameVersion> PriorityOrder { get; set; } = [.. DefaultPriorityOrder];

    [Category(Generate), Description("Set all possible legal ribbons for any generated PokÃ©mon.")]
    public bool SetAllLegalRibbons { get; set; } = false;

    [Category(Generate), Description("Set a matching ball (based on color) for any generated PokÃ©mon.")]
    public bool SetMatchingBalls { get; set; } = true;

    [Category(Generate), Description("Force the specified ball if legal.")]
    public bool ForceSpecifiedBall { get; set; } = true;

    [Category(Generate), Description("Assumes level 50 sets are level 100 competitive sets."), DisplayName("Set lvl 50 to 100")]
    public bool ForceLevel100for50 { get; set; } = false;

    [Category(Generate), Description("Requires HOME tracker when trading PokÃ©mon that had to have traveled between the Switch games."), DisplayName("Require HOME Tracker for transfers")]
    public bool EnableHOMETrackerCheck { get; set; } = true;

    [Category(Generate), Description("Prevents trading PokÃ©mon that require a HOME Tracker, even if the file has one already."), DisplayName("Disallow Non-Native PokÃ©mon")]
    public bool DisallowNonNatives { get; set; } = false;

    [Category(Generate), Description("Prevents trading PokÃ©mon that already have a HOME Tracker."), DisplayName("Disallow HOME Tracked PokÃ©mon")]
    public bool DisallowTracked { get; set; } = false;

    [Category(Generate), Description("The order in which PokÃ©mon encounter types are attempted.")]
    public List<EncounterTypeGroup> PrioritizeEncounters { get; set; } = [.. DefaultPrioritizeEncounters];

    [Browsable(false)]
    [Category(Generate), Description("Adds Battle Version for games that support it (SWSH only) for using past-gen PokÃ©mon in online competitive play.")]
    public bool SetBattleVersion { get; set; }

    [Category(Generate), Description("Bot will create an Easter Egg PokÃ©mon if provided an illegal set.")]
    public bool EnableEasterEggs { get; set; } = false;

    [Category(Generate), Description("Allow users to submit further customization with Batch Editor commands.")]
    public bool AllowBatchCommands { get; set; } = true;

    [Category(Generate), Description("Maximum time in seconds to spend when generating a set before canceling. This prevents difficult sets from freezing the bot.")]
    public int Timeout { get; set; } = 15;

    // Misc
    [Browsable(false)]
    [Category(Misc), Description("Zero out HOME trackers for cloned and user-requested PKM files. It is recommended to leave this disabled to avoid creating invalid HOME data."), DisplayName("Reset HOME Tracker")]
    public bool ResetHOMETracker { get; set; } = false;

    public void CreateDefaults(string path)
    {
        // Standardize on TrainerDatabase and do not create it automatically.
        // This keeps the workspace clean and removes ambiguity between trainers/trainerData.
        GeneratePathTrainerInfo = Path.Combine(path, "TrainerDatabase");
    }
}
