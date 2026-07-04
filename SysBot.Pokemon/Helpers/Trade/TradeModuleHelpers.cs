using PKHeX.Core;
using PKHeX.Core.AutoMod;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SysBot.Pokemon.Helpers;

public static class TradeModuleHelpers
{
    private static readonly Dictionary<EntityContext, List<ushort>> BreedableSpeciesCache = [];
    private static readonly Dictionary<EntityContext, List<ushort>> LegalSpeciesCache = [];
    private const int DefaultMaxGenerationAttempts = 30;

    public static List<string> ParseBatchTradeContent(string content)
    {
        var delimiters = new[] { "---", "—-" };
        return [.. content.Split(delimiters, StringSplitOptions.RemoveEmptyEntries).Select(trade => trade.Trim())];
    }

    public static MysteryGift[]? GetEventData(string generationOrGame)
    {
        return generationOrGame.ToLowerInvariant() switch
        {
            "4" or "gen4" => EncounterEvent.MGDB_G4,
            "5" or "gen5" => EncounterEvent.MGDB_G5,
            "6" or "gen6" => EncounterEvent.MGDB_G6,
            "7" or "gen7" => EncounterEvent.MGDB_G7,
            "gg" or "lgpe" => EncounterEvent.MGDB_G7GG,
            "swsh" => EncounterEvent.MGDB_G8,
            "pla" or "la" => EncounterEvent.MGDB_G8A,
            "bdsp" => EncounterEvent.MGDB_G8B,
            "9" or "gen9" => EncounterEvent.MGDB_G9,
            "plza" or "9a" or "gen9a" => EncounterEvent.MGDB_G9A,
            _ => null,
        };
    }

    public static T? ConvertEventToPKM<T>(MysteryGift selectedEvent, byte? requestedLanguage = null, string? metDate = null) where T : PKM, new()
    {
        var trainer = new SimpleTrainerInfo(selectedEvent.Version)
        {
            Language = requestedLanguage ?? (byte)LanguageID.English,
        };

        PKM? pkm = selectedEvent.ConvertToPKM(trainer, EncounterCriteria.Unrestricted);

        if (pkm is null)
            return null;

        if (!string.IsNullOrEmpty(metDate) && !pkm.IsEgg)
        {
            bool dateParseSuccess = false;
            if (metDate.Length == 8 && DateTime.TryParseExact(metDate, "yyyyMMdd", 
                System.Globalization.CultureInfo.InvariantCulture, 
                System.Globalization.DateTimeStyles.None, out DateTime parsedDate))
            {
                dateParseSuccess = true;
            }
            else if (DateTime.TryParse(metDate, out parsedDate))
            {
                dateParseSuccess = true;
            }
            
            if (dateParseSuccess)
            {
                var dateOnly = new DateOnly(parsedDate.Year, parsedDate.Month, parsedDate.Day);
                if (pkm is PK9 pk9) pk9.MetDate = dateOnly;
                else if (pkm is PK8 pk8) pk8.MetDate = dateOnly;
                else if (pkm is PA8 pa8) pa8.MetDate = dateOnly;
                else if (pkm is PB8 pb8) pb8.MetDate = dateOnly;
                else if (pkm is PA9 pa9) pa9.MetDate = dateOnly;
                else
                {
                    pkm.MetDay = (byte)parsedDate.Day;
                    pkm.MetMonth = (byte)parsedDate.Month;
                    pkm.MetYear = (byte)(parsedDate.Year - 2000);
                }
            }
        }

        if (pkm is T pk)
            return pk;

        return EntityConverter.ConvertToType(pkm, typeof(T), out _) as T;
    }

    public static T? GenerateLegalMysteryEgg<T>(int maxAttempts = DefaultMaxGenerationAttempts) where T : PKM, new()
    {
        var context = GetContext<T>();
        if (context == EntityContext.None)
            return null;

        var breedableSpecies = GetBreedableSpecies(context);
        if (breedableSpecies.Count == 0)
            return null;

        var random = new Random();
        var shuffled = breedableSpecies.OrderBy(_ => random.Next()).Take(maxAttempts).ToList();

        var sav = AutoLegalityWrapper.GetTrainerInfo<T>();
        var originalPriority = APILegality.PriorityOrder?.ToList() ?? [];
        APILegality.PriorityOrder = GetPriorityOrder(context);

        try
        {
            foreach (var species in shuffled)
            {
                var set = CreateEggShowdownSet(species, context);
                var template = AutoLegalityWrapper.GetTemplate(set);
                var pk = AutoLegalityWrapper.GenerateEgg(sav, template, out var result);

                if (pk == null || result != LegalizationResult.Regenerated)
                    continue;

                pk = EntityConverter.ConvertToType(pk, typeof(T), out _) ?? pk;
                if (pk is not T validPk)
                    continue;

                var la = new LegalityAnalysis(validPk);
                if (la.Valid)
                    return validPk;
            }
        }
        finally
        {
            APILegality.PriorityOrder = originalPriority;
        }

        return null;
    }

    public static T? GenerateLegalMysteryPokemon<T>(int maxAttempts = DefaultMaxGenerationAttempts) where T : PKM, new()
    {
        var context = GetContext<T>();
        if (context == EntityContext.None)
            return null;

        var speciesList = GetLegalSpecies(context);
        if (speciesList.Count == 0)
            return null;

        var random = new Random();
        var shuffled = speciesList.OrderBy(_ => random.Next()).Take(maxAttempts).ToList();

        var sav = AutoLegalityWrapper.GetTrainerInfo<T>();
        var originalPriority = APILegality.PriorityOrder?.ToList() ?? [];
        APILegality.PriorityOrder = GetPriorityOrder(context);

        try
        {
            foreach (var species in shuffled)
            {
                var speciesName = GameInfo.Strings.Species[species];
                var setString = $"{speciesName}\nShiny: Yes\nIVs: 31/31/31/31/31/31";
                
                var hiddenAbilityName = GetHiddenAbilityName(species, context);
                if (!string.IsNullOrEmpty(hiddenAbilityName))
                    setString += $"\nAbility: {hiddenAbilityName}";

                var set = new ShowdownSet(setString);
                var template = AutoLegalityWrapper.GetTemplate(set);
                var pk = sav.GetLegal(template, out var result);

                if (pk == null)
                    continue;

                pk = EntityConverter.ConvertToType(pk, typeof(T), out _) ?? pk;
                if (pk is not T validPk)
                    continue;

                var la = new LegalityAnalysis(validPk);
                if (la.Valid)
                    return validPk;
            }
        }
        finally
        {
            APILegality.PriorityOrder = originalPriority;
        }

        return null;
    }

    private static ShowdownSet CreateEggShowdownSet(ushort species, EntityContext context)
    {
        var speciesName = GameInfo.Strings.Species[species];
        var setString = $"{speciesName}\nShiny: Yes\nIVs: 31/31/31/31/31/31";

        var hiddenAbilityName = GetHiddenAbilityName(species, context);
        if (!string.IsNullOrEmpty(hiddenAbilityName))
            setString += $"\nAbility: {hiddenAbilityName}";

        return new ShowdownSet(setString);
    }

    private static string? GetHiddenAbilityName(ushort species, EntityContext context)
    {
        var personalTable = GetPersonalTable(context);
        if (personalTable == null)
            return null;

        try
        {
            var pi = personalTable.GetFormEntry(species, 0);
            if (pi is IPersonalAbility12H piH)
            {
                var hiddenAbilityID = piH.AbilityH;
                if (hiddenAbilityID > 0 && hiddenAbilityID < GameInfo.Strings.Ability.Count)
                    return GameInfo.Strings.Ability[hiddenAbilityID];
            }
        }
        catch { }

        return null;
    }

    private static List<ushort> GetBreedableSpecies(EntityContext context)
    {
        lock (BreedableSpeciesCache)
        {
            if (BreedableSpeciesCache.TryGetValue(context, out var cached))
                return cached;
        }

        var personalTable = GetPersonalTable(context);
        if (personalTable == null)
            return [];

        var breedable = new List<ushort>();
        for (ushort species = 1; species <= personalTable.MaxSpeciesID; species++)
        {
            if (!Breeding.CanHatchAsEgg(species))
                continue;

            if (!personalTable.IsSpeciesInGame(species))
                continue;

            breedable.Add(species);
        }

        lock (BreedableSpeciesCache)
        {
            BreedableSpeciesCache[context] = breedable;
        }

        return breedable;
    }

    private static List<ushort> GetLegalSpecies(EntityContext context)
    {
        lock (LegalSpeciesCache)
        {
            if (LegalSpeciesCache.TryGetValue(context, out var cached))
                return cached;
        }

        var personalTable = GetPersonalTable(context);
        if (personalTable == null)
            return [];

        var legal = new List<ushort>();
        for (ushort species = 1; species <= personalTable.MaxSpeciesID; species++)
        {
            if (personalTable.IsSpeciesInGame(species))
                legal.Add(species);
        }

        lock (LegalSpeciesCache)
        {
            LegalSpeciesCache[context] = legal;
        }

        return legal;
    }

    private static EntityContext GetContext<T>() where T : PKM, new() => typeof(T).Name switch
    {
        "PB8" => EntityContext.Gen8b,
        "PK8" => EntityContext.Gen8,
        "PA8" => EntityContext.Gen8a,
        "PK9" => EntityContext.Gen9,
        "PA9" => EntityContext.Gen9,
        "PB7" => EntityContext.Gen7b,
        _ => EntityContext.None
    };

    private static List<GameVersion> GetPriorityOrder(EntityContext context) => context switch
    {
        EntityContext.Gen8b => [GameVersion.BD, GameVersion.SP],
        EntityContext.Gen8  => [GameVersion.SW, GameVersion.SH],
        EntityContext.Gen8a => [GameVersion.PLA],
        EntityContext.Gen9  => [GameVersion.SL, GameVersion.VL],
        EntityContext.Gen7b => [GameVersion.GP, GameVersion.GE],
        _ => []
    };

    private static IPersonalTable? GetPersonalTable(EntityContext context) => context switch
    {
        EntityContext.Gen8b => PersonalTable.BDSP,
        EntityContext.Gen8  => PersonalTable.SWSH,
        EntityContext.Gen8a => PersonalTable.LA,
        EntityContext.Gen9  => PersonalTable.SV,
        EntityContext.Gen7b => PersonalTable.GG,
        _ => null
    };
}
