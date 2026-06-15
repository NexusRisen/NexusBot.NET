using PKHeX.Core;
using SysBot.Pokemon.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SysBot.Pokemon.Stoat.Helpers;

/// <summary>
/// Extracts and formats Pokémon details for rich Stoat embed display.
/// No emojis, no external links — clean text only.
/// </summary>
public static class PokemonDetailsHelper<T> where T : PKM, new()
{
    public class PokemonDetails
    {
        public string SpeciesName { get; set; } = string.Empty;
        public string FormName { get; set; } = string.Empty;
        public string Gender { get; set; } = string.Empty;
        public string Nickname { get; set; } = string.Empty;
        public bool IsNicknamed { get; set; }
        public bool IsShiny { get; set; }
        public bool IsSquareShiny { get; set; }
        public int Level { get; set; }
        public string Ball { get; set; } = string.Empty;
        public string Ability { get; set; } = string.Empty;
        public string Nature { get; set; } = string.Empty;
        public string Language { get; set; } = string.Empty;
        public string IVsDisplay { get; set; } = string.Empty;
        public string MetDate { get; set; } = string.Empty;
        public byte MetLevel { get; set; }
        public string MetLocation { get; set; } = string.Empty;
        public List<string> Moves { get; set; } = [];
        public string TeraType { get; set; } = string.Empty;
        public string ImageUrl { get; set; } = string.Empty;
        public bool IsEgg { get; set; }
        public string EVsDisplay { get; set; } = string.Empty;
        public string HeldItem { get; set; } = string.Empty;
    }

    public static PokemonDetails Extract(T pk)
    {
        var strings = GameInfo.GetStrings("en");

        var details = new PokemonDetails
        {
            IsEgg = pk.IsEgg,
            IsShiny = pk.IsShiny,
            IsSquareShiny = pk.IsShiny && pk.ShinyXor == 0,
            Level = pk.CurrentLevel,
            MetLevel = pk.MetLevel,
            IsNicknamed = pk.IsNicknamed,
        };

        // Species & form
        details.SpeciesName = strings.Species[pk.Species];
        details.FormName = ShowdownParsing.GetStringFromForm(pk.Form, strings, pk.Species, pk.Context) ?? string.Empty;

        // Nickname
        details.Nickname = pk.IsNicknamed ? pk.Nickname : details.SpeciesName;

        // Gender — plain text, no emoji
        details.Gender = pk.Gender switch
        {
            0 => "(M)",
            1 => "(F)",
            _ => string.Empty
        };

        // Ball
        details.Ball = strings.balllist[pk.Ball];

        // Ability
        details.Ability = strings.abilitylist[pk.Ability];

        // Nature (handle minted)
        string baseName = strings.natures[(int)pk.Nature];
        if (pk.StatNature != pk.Nature)
        {
            string statName = strings.natures[(int)pk.StatNature];
            details.Nature = $"{statName} (Minted from: {baseName})";
        }
        else
        {
            details.Nature = $"{baseName} Nature";
        }

        // Language
        details.Language = GetLanguageDisplay(pk);

        // IVs
        details.IVsDisplay = GetIVsDisplay(pk);

        // EVs
        details.EVsDisplay = GetEVsDisplay(pk);

        // Met date
        details.MetDate = pk.MetDate.HasValue
            ? pk.MetDate.Value.ToString("M/d/yyyy")
            : string.Empty;

        // Met location
        var locationName = strings.GetLocationName(false, pk.MetLocation, pk.Format, pk.Generation, (GameVersion)pk.Version);
        details.MetLocation = string.IsNullOrWhiteSpace(locationName) ? $"ID: {pk.MetLocation}" : locationName;

        // Tera Type (SV only)
        if (pk is PK9 pk9)
        {
            var isStellar = pk9.TeraTypeOverride == (MoveType)TeraTypeUtil.Stellar || (int)pk9.TeraType == 99;
            details.TeraType = isStellar ? "Stellar" : pk9.TeraType.ToString();
        }

        // Moves — plain names, no emoji, no PP
        details.Moves = GetMoveNames(pk, strings);

        // Held item
        if (pk.HeldItem > 0 && pk.HeldItem < strings.itemlist.Length)
            details.HeldItem = strings.itemlist[pk.HeldItem];

        // Sprite image
        details.ImageUrl = TradeExtensions<T>.PokeImg(pk, false, true, null);

        return details;
    }

    private static string GetLanguageDisplay(T pk)
    {
        var languageList = GameInfo.LanguageDataSource(pk.Format, pk.Context);
        var entry = languageList.FirstOrDefault(l => l.Value == pk.Language);
        var langName = entry?.Text ?? ((LanguageID)pk.Language).GetLanguageCode();

        // Build short code like "ENG (English)"
        string code = ((LanguageID)pk.Language).GetLanguageCode().ToUpper();
        return $"{code} ({langName})";
    }

    private static string GetIVsDisplay(T pk)
    {
        Span<int> ivs = stackalloc int[6];
        pk.GetIVs(ivs);

        // Display order: HP / Atk / Def / SpA / SpD / Spe
        int[] displayOrder = [0, 1, 2, 4, 5, 3];
        string[] labels = ["HP", "Atk", "Def", "SpA", "SpD", "Spe"];

        int perfect = 0;
        foreach (var v in ivs) if (v == 31) perfect++;
        if (perfect == 6) return "6IV";

        var parts = new List<string>();
        for (int i = 0; i < displayOrder.Length; i++)
        {
            int idx = displayOrder[i];
            parts.Add($"{ivs[idx]} {labels[i]}");
        }
        return string.Join(" / ", parts);
    }

    private static string GetEVsDisplay(T pk)
    {
        Span<int> evs = stackalloc int[6];
        pk.GetEVs(evs);

        int[] displayOrder = [0, 1, 2, 4, 5, 3];
        string[] labels = ["HP", "Atk", "Def", "SpA", "SpD", "Spe"];

        var parts = new List<string>();
        for (int i = 0; i < displayOrder.Length; i++)
        {
            int idx = displayOrder[i];
            if (evs[idx] > 0)
                parts.Add($"{evs[idx]} {labels[i]}");
        }
        return parts.Count > 0 ? string.Join(" / ", parts) : "None";
    }

    private static List<string> GetMoveNames(T pk, GameStrings strings)
    {
        ushort[] moves = new ushort[4];
        pk.GetMoves(moves.AsSpan());

        var result = new List<string>();
        foreach (var move in moves)
        {
            if (move == 0) continue;
            result.Add(strings.movelist[move]);
        }
        return result;
    }
}
