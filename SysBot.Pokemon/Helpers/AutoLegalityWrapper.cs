using PKHeX.Core;
using PKHeX.Core.AutoMod;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SysBot.Pokemon;

public static class AutoLegalityWrapper
{
    private static bool Initialized;

    public static void EnsureInitialized(LegalitySettings cfg)
    {
        if (Initialized)
            return;
        Initialized = true;
        InitializeAutoLegality(cfg);
    }

    private static void InitializeAutoLegality(LegalitySettings cfg)
    {
        InitializeCoreStrings();
        
        if (string.IsNullOrWhiteSpace(cfg.MGDBPath))
            cfg.MGDBPath = AppContext.BaseDirectory;
            
        SysBot.Pokemon.Helpers.MGDBUpdater.UpdateMGDBAsync(cfg.MGDBPath).GetAwaiter().GetResult();
        
        // Updated to convert the string to a ReadOnlySpan<string> array as required by the method signature.   
        EncounterEvent.RefreshMGDB([cfg.MGDBPath]);
        InitializeTrainerDatabase(cfg);
        InitializeSettings(cfg);
    }

    // The list of encounter types in the priority we prefer if no order is specified.
    private static readonly EncounterTypeGroup[] EncounterPriority = [EncounterTypeGroup.Egg, EncounterTypeGroup.Slot, EncounterTypeGroup.Static, EncounterTypeGroup.Mystery, EncounterTypeGroup.Trade];

    private static void InitializeSettings(LegalitySettings cfg)
    {
        // Disable expensive PID+ validation for PLZA shiny Pokemon
        // PKHeX will skip correlation checks when SearchShiny1 is false (see EncounterGift9a.TryGetSeed)       
        LumioseSolver.SearchShiny1 = false;

        APILegality.SetAllLegalRibbons = cfg.SetAllLegalRibbons;
        APILegality.SetMatchingBalls = cfg.SetMatchingBalls;
        APILegality.ForceSpecifiedBall = cfg.ForceSpecifiedBall;
        APILegality.ForceLevel100for50 = cfg.ForceLevel100for50;
        Legalizer.EnableEasterEggs = cfg.EnableEasterEggs;
        APILegality.AllowTrainerOverride = cfg.AllowTrainerDataOverride;
        APILegality.AllowBatchCommands = cfg.AllowBatchCommands;
        APILegality.GameVersionPriority =  (PKHeX.Core.AutoMod.GameVersionPriorityType)cfg.GameVersionPriority ;
        cfg.PriorityOrder = APILegality.PriorityOrder = SanitizePriorityOrder(cfg.PriorityOrder);

        APILegality.SetBattleVersion = cfg.SetBattleVersion;
        APILegality.Timeout = cfg.Timeout;
        var settings = ParseSettings.Settings;
        settings.WordFilter.CheckWordFilter = false;
        settings.Handler.CheckActiveHandler = false;
        settings.Handler.Restrictions.Disable();
        var validRestriction = new NicknameRestriction { NicknamedTrade = Severity.Fishy, NicknamedMysteryGift = Severity.Fishy };
        settings.Nickname.SetAllTo(validRestriction);

        // As of February 2024, the default setting in PKHeX is Invalid for missing HOME trackers.
        // If the host wants to allow missing HOME trackers, we need to disable the default setting.
        bool allowMissingHOME = !cfg.EnableHOMETrackerCheck;
        if (allowMissingHOME)
            settings.HOMETransfer.Disable();

        // We need all the encounter types present, so add the missing ones at the end.
        var missing = EncounterPriority.Except(cfg.PrioritizeEncounters);
        cfg.PrioritizeEncounters.AddRange(missing);
        cfg.PrioritizeEncounters = [.. cfg.PrioritizeEncounters.Distinct()]; // Don't allow duplicates.
        EncounterMovesetGenerator.PriorityList = cfg.PrioritizeEncounters;
    }
    private static List<GameVersion> SanitizePriorityOrder(List<GameVersion> versionList)
    {
        var validVersions = Enum.GetValues<GameVersion>().Where(GameUtil.IsValidSavedVersion).Reverse().ToList();

        foreach (var ver in validVersions)
        {
            if (!versionList.Contains(ver))
                versionList.Add(ver); // Add any missing versions.
        }

        // Remove any versions in versionList that are not in validVersions and clean up duplicates in the process.
        return [.. versionList.Intersect(validVersions)];
    }

    private static void InitializeTrainerDatabase(LegalitySettings cfg)
    {
        var externalSource = cfg.GeneratePathTrainerInfo;
        if (Directory.Exists(externalSource))
            TrainerSettings.LoadTrainerDatabaseFromPath(externalSource);

        // Seed the Trainer Database with enough fake save files so that we return a generation sensitive format when needed.
        var fallback = GetDefaultTrainer(cfg);
        for (byte generation = 1; generation <= 9; generation++)
        {
            var versions = generation switch
            {
                1 => GameUtil.GetVersionsInGeneration(EntityContext.Gen1, GameVersion.Any),
                2 => GameUtil.GetVersionsInGeneration(EntityContext.Gen2, GameVersion.Any),
                3 => GameUtil.GetVersionsInGeneration(EntityContext.Gen3, GameVersion.Any),
                4 => GameUtil.GetVersionsInGeneration(EntityContext.Gen4, GameVersion.Any),
                5 => GameUtil.GetVersionsInGeneration(EntityContext.Gen5, GameVersion.Any),
                6 => GameUtil.GetVersionsInGeneration(EntityContext.Gen6, GameVersion.Any),
                7 => GameUtil.GetVersionsInGeneration(EntityContext.Gen7, GameVersion.Any),
                8 => GameUtil.GetVersionsInGeneration(EntityContext.Gen8, GameVersion.Any),
                9 => GameUtil.GetVersionsInGeneration(EntityContext.Gen9, GameVersion.Any),
                _ => []
            };

            foreach (var version in versions)
            {
                var context = GetContextSafe(version);
                if (context == EntityContext.None)
                    continue;
                RegisterIfNoneExist(fallback, context, version, generation);
            }
        }
        // Manually register for LGP/E since Gen7 above will only register the 3DS versions.  
        RegisterIfNoneExist(fallback, EntityContext.Gen7, GameVersion.GP, 7);
        RegisterIfNoneExist(fallback, EntityContext.Gen7, GameVersion.GE, 7);
    }

    private static EntityContext GetContextSafe(GameVersion v) => v switch
    {
        GameVersion.SW or GameVersion.SH => EntityContext.Gen8,
        GameVersion.BD or GameVersion.SP => EntityContext.Gen8b,
        GameVersion.PLA => EntityContext.Gen8a,
        GameVersion.SL or GameVersion.VL => EntityContext.Gen9,
        GameVersion.ZA => EntityContext.Gen9a,
        _ => EntityContext.None
    };

    private static SimpleTrainerInfo GetDefaultTrainer(LegalitySettings cfg)
    {
        var ot = cfg.GenerateOT;
        if (string.IsNullOrWhiteSpace(ot))
            ot = "Blank";

        return new SimpleTrainerInfo(GameVersion.Any)
        {
            Language = (byte)cfg.GenerateLanguage,
            TID16 = cfg.GenerateTID16,
            SID16 = cfg.GenerateSID16,
            OT = ot,
        };
    }

    private static void RegisterIfNoneExist(SimpleTrainerInfo fallback, EntityContext context, GameVersion version, byte generation)
    {
        if (context == EntityContext.None)
            return;

        var info = new SimpleTrainerInfo(version)
        {
            Language = fallback.Language,
            TID16 = fallback.TID16,
            SID16 = fallback.SID16,
            OT = fallback.OT,
            Generation = generation,
        };

        // Pass the version as the second argument and the fallback as the third to match the overload
        var exist = TrainerSettings.GetSavedTrainerData(context, version);
        if (exist is SimpleTrainerInfo) // not anything from files; this assumes ALM returns SimpleTrainerInfo for non-user-provided fake templates.
            TrainerSettings.Register(info);
    }

    private static void InitializeCoreStrings()
    {
        var lang = Thread.CurrentThread.CurrentCulture.TwoLetterISOLanguageName[..2];
        LocalizationUtil.SetLocalization(typeof(LegalityCheckResultCode), lang);
        LocalizationUtil.SetLocalization(typeof(MessageStrings), lang);

        // Pre-initialize BattleTemplateLocalization to prevent concurrent dictionary access issues
        // This forces all localizations to be loaded at startup before any concurrent operations
        _ = BattleTemplateLocalization.ForceLoadAll();
    }

    public static bool CanBeTraded(this PKM pkm)
    {
        if (pkm.IsNicknamed && StringsUtil.IsSpammyString(pkm.Nickname))
            return false;
        if (StringsUtil.IsSpammyString(pkm.OriginalTrainerName) && !IsFixedOT(new LegalityAnalysis(pkm).EncounterOriginal, pkm))
            return false;
        return !FormInfo.IsFusedForm(pkm.Species, pkm.Form, pkm.Format);
    }

    public static bool IsFixedOT(IEncounterTemplate t, PKM pkm) => t switch
    {
        IFixedTrainer { IsFixedTrainer: true } => true,
        MysteryGift g => !g.IsEgg && g switch
        {
            WA9 wa9 => wa9.GetHasOT(pkm.Language),
            WC9 wc9 => wc9.GetHasOT(pkm.Language),
            WA8 wa8 => wa8.GetHasOT(pkm.Language),
            WB8 wb8 => wb8.GetHasOT(pkm.Language),
            WC8 wc8 => wc8.GetHasOT(pkm.Language),
            WB7 wb7 => wb7.GetHasOT(pkm.Language),
            { Generation: >= 5 } gift => gift.OriginalTrainerName.Length > 0,
            _ => true,
        },
        _ => false,
    };

    public static ITrainerInfo GetTrainerInfo<T>() where T : PKM, new()
    {
        var version = typeof(T) switch
        {
            Type t when t == typeof(PK8) => GameVersion.SWSH,
            Type t when t == typeof(PB8) => GameVersion.BDSP,
            Type t when t == typeof(PA8) => GameVersion.PLA,
            Type t when t == typeof(PK9) => GameVersion.SV,
            Type t when t == typeof(PA9) => GameVersion.ZA,
            Type t when t == typeof(PB7) => GameVersion.GG,
            _ => throw new ArgumentException("Type does not have a recognized trainer fetch.", typeof(T).Name)
        };
        return TrainerSettings.GetSavedTrainerData(version);
    }

    // Change GetTrainerInfo(byte) to convert generation byte to an EntityContext before calling TrainerSettings
    public static ITrainerInfo GetTrainerInfo(byte gen)
    {
        // Convert the numeric generation into a representative GameVersion, then to an EntityContext
        var representativeVersion = gen switch
        {
            1 => GameVersion.RBY,
            2 => GameVersion.GSC,
            3 => GameVersion.RSE,
            4 => GameVersion.DPPt,
            5 => GameVersion.B2W2,
            6 => GameVersion.ORAS,
            7 => GameVersion.USUM,
            8 => GameVersion.SWSH,
            9 => GameVersion.SV,
            _ => GameVersion.Any
        };
        var context = GetContextSafe(representativeVersion);
        return TrainerSettings.GetSavedTrainerData(context);
    }

    public static PKM GetLegal(this ITrainerInfo sav, IBattleTemplate set, out string res)
    {
        var task = Task.Factory.StartNew(() => sav.GetLegalFromSet(set), CancellationToken.None, TaskCreationOptions.LongRunning, TaskScheduler.Default);
        if (task.Wait(TimeSpan.FromSeconds(30)))
        {
            var result = task.Result;
            res = result.Status switch
            {
                LegalizationResult.Regenerated => "Regenerated",
                LegalizationResult.Failed => "Failed",
                LegalizationResult.Timeout => "Timeout",
                LegalizationResult.VersionMismatch => "VersionMismatch",
                _ => "",
            };

            var pkm = result.Created;
            if (pkm != null && set is RegenTemplate rt && APILegality.AllowTrainerOverride && rt.Regen.Trainer != null)
                pkm.SetAllTrainerData(rt.Regen.Trainer);

            return pkm!;
        }
        else
        {
            res = "Timeout";
            return null!; // Explicitly return null with suppression since the res parameter indicates the failure
        }
    }

    public static string GetLegalizationHint(IBattleTemplate set, ITrainerInfo sav, PKM pk) => set.SetAnalysis(sav, pk);
    public static PKM LegalizePokemon(this PKM pk) => pk.Legalize();
    public static RegenTemplate GetTemplate(ShowdownSet set) => new RegenTemplate(set);
}
