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
        public string SpecialSymbols { get; set; } = string.Empty;
        public string Scale { get; set; } = string.Empty;
        public string HeldItem { get; set; } = string.Empty;
    }

    public static PokemonDetails Extract(T pk)
    {
        var strings = GameInfo.GetStrings("en");
        var settings = SysStoat<T>.Runner.Config.Trade.TradeEmbedSettings;

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

        // Special Symbols
        details.SpecialSymbols = GetSpecialSymbols(pk, settings);

        // Gender (fallback to plain text if emojis not used, but GetSpecialSymbols handles gender emoji now)
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
        if (pk.StatAlignment != Nature.Random && pk.StatAlignment != pk.Nature)
        {
            string statName = strings.natures[(int)pk.StatAlignment];
            details.Nature = $"{statName} (Minted from: {baseName})";
        }
        else
        {
            details.Nature = $"{baseName} Nature";
        }

        // Language
        details.Language = GetLanguageDisplay(pk);

        // IVs & EVs
        details.IVsDisplay = GetIVsDisplay(pk);
        details.EVsDisplay = GetEVsDisplay(pk);

        // Met info
        details.MetDate = pk.MetDate.HasValue ? pk.MetDate.Value.ToString("M/d/yyyy") : string.Empty;
        var locationName = strings.GetLocationName(false, pk.MetLocation, pk.Format, pk.Generation, (GameVersion)pk.Version);
        details.MetLocation = string.IsNullOrWhiteSpace(locationName) ? $"ID: {pk.MetLocation}" : locationName;

        // Tera Type & Scale (SV only)
        if (pk is PK9 pk9)
        {
            details.TeraType = GetTeraTypeString(pk9, settings);
            details.Scale = $"{PokeSizeDetailedUtil.GetSizeRating(pk9.Scale)} ({pk9.Scale})";
        }
        else if (pk is PA8 pa8)
        {
            details.Scale = $"{PokeSizeDetailedUtil.GetSizeRating(pa8.Scale)} ({pa8.Scale})";
        }

        // Moves
        details.Moves = GetMoveNames(pk, strings, settings);

        // Held item
        if (pk.HeldItem > 0 && pk.HeldItem < strings.itemlist.Length)
            details.HeldItem = strings.itemlist[pk.HeldItem];

        // Sprite image
        details.ImageUrl = TradeExtensions<T>.PokeImg(pk, false, true, null);

        return details;
    }

    private static string GetSpecialSymbols(T pk, TradeSettings.TradeEmbedSettingsCategory settings)
    {
        var symbols = new List<string>();

        if (pk.IsShiny)
            symbols.Add(pk.ShinyXor == 0 ? "◼ " : "★ ");

        if (pk is IAlpha alpha && alpha.IsAlpha)
            symbols.Add(settings.AlphaPLAEmoji.EmojiString);

        if (pk is IRibbonSetMark9 ribbonSetMark)
        {
            if (ribbonSetMark.RibbonMarkMightiest)
                symbols.Add(settings.MightiestMarkEmoji.EmojiString);
            if (ribbonSetMark.RibbonMarkAlpha)
                symbols.Add(settings.AlphaMarkEmoji.EmojiString);
        }

        if (pk is IRibbonIndex ribbonIndex && TradeExtensions<T>.HasMark(ribbonIndex, out _, out string markTitle))
            symbols.Add(markTitle.Trim());

        if (pk.FatefulEncounter)
            symbols.Add(settings.MysteryGiftEmoji.EmojiString);

        string genderSymbol = GameInfo.GenderSymbolASCII[pk.Gender];
        string genderEmoji = genderSymbol switch
        {
            "M" => settings.MaleEmoji.EmojiString ?? "(M) ",
            "F" => settings.FemaleEmoji.EmojiString ?? "(F) ",
            _ => string.Empty
        };
        if (!string.IsNullOrEmpty(genderEmoji))
            symbols.Add(genderEmoji);

        return string.Join(" ", symbols.Where(s => !string.IsNullOrWhiteSpace(s))) + (symbols.Count > 0 ? " " : "");
    }

    private static string GetTeraTypeString(PK9 pk9, TradeSettings.TradeEmbedSettingsCategory settings)
    {
        var isStellar = pk9.TeraTypeOverride == (PKHeX.Core.MoveType)TeraTypeUtil.Stellar || (int)pk9.TeraType == 99;
        var teraType = isStellar ? TradeSettings.MoveType.Stellar : (TradeSettings.MoveType)pk9.TeraType;

        if (settings.UseTeraEmojis)
        {
            var emojiInfo = settings.TeraTypeEmojis.Find(e => e.MoveType == teraType);
            if (emojiInfo != null && !string.IsNullOrEmpty(emojiInfo.EmojiCode))
            {
                return emojiInfo.EmojiCode;
            }
        }
        return teraType.ToString();
    }

    private static string GetLanguageDisplay(T pk)
    {
        var languageList = GameInfo.LanguageDataSource(pk.Format, pk.Context);
        var entry = languageList.FirstOrDefault(l => l.Value == pk.Language);
        var langName = entry?.Text ?? ((LanguageID)pk.Language).GetLanguageCode();
        string code = ((LanguageID)pk.Language).GetLanguageCode().ToUpper();
        return $"{code} ({langName})";
    }

    private static string GetIVsDisplay(T pk)
    {
        Span<int> ivs = stackalloc int[6];
        pk.GetIVs(ivs);

        int[] displayOrder = [0, 1, 2, 4, 5, 3];
        string[] labels = ["HP", "Atk", "Def", "SpA", "SpD", "Spe"];

        int perfect = 0;
        foreach (var v in ivs) if (v == 31) perfect++;
        if (perfect == 6) return "6IV";

        var parts = new List<string>();
        for (int i = 0; i < displayOrder.Length; i++)
        {
            int idx = displayOrder[i];
            
            bool isHT = false;
            if (pk is IHyperTrain ht)
            {
                isHT = idx switch
                {
                    0 => ht.HT_HP,
                    1 => ht.HT_ATK,
                    2 => ht.HT_DEF,
                    3 => ht.HT_SPE,
                    4 => ht.HT_SPA,
                    5 => ht.HT_SPD,
                    _ => false
                };
            }

            parts.Add($"{ivs[idx]}{(isHT ? " (HT)" : "")} {labels[i]}");
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

    private static List<string> GetMoveNames(T pk, GameStrings strings, TradeSettings.TradeEmbedSettingsCategory settings)
    {
        ushort[] moves = new ushort[4];
        pk.GetMoves(moves.AsSpan());
        List<int> movePPs = new() { pk.Move1_PP, pk.Move2_PP, pk.Move3_PP, pk.Move4_PP };
        
        var result = new List<string>();
        var typeEmojis = settings.CustomTypeEmojis
            .Where(e => !string.IsNullOrEmpty(e.EmojiCode))
            .ToDictionary(e => (PKHeX.Core.MoveType)e.MoveType, e => $"{e.EmojiCode}");

        string plusEmoji = string.Empty;
        var plusEmojiString = settings.UsePlusMoveEmoji?.EmojiString;
        if (!string.IsNullOrWhiteSpace(plusEmojiString))
            plusEmoji = $" {plusEmojiString}";

        for (int i = 0; i < moves.Length; i++)
        {
            if (moves[i] == 0) continue;

            string moveName = strings.movelist[moves[i]];
            byte moveTypeId = MoveInfo.GetType(moves[i], default);
            PKHeX.Core.MoveType moveType = (PKHeX.Core.MoveType)moveTypeId;

            bool isPLZA = pk is PA9;
            string formattedMove = isPLZA ? $"*{moveName}*" : $"*{moveName}* ({movePPs[i]} PP)";

            if (settings.MoveTypeEmojis && typeEmojis.TryGetValue(moveType, out var moveEmoji))
                formattedMove = $"{moveEmoji} {formattedMove}";

            if (isPLZA && pk is PA9 pa9 && pa9.PersonalInfo is IPermitPlus plus)
            {
                int plusIndex = plus.PlusMoveIndexes.IndexOf(moves[i]);
                if (plusIndex >= 0 && pa9.GetMovePlusFlag(plusIndex))
                {
                    formattedMove += !string.IsNullOrWhiteSpace(plusEmoji) ? plusEmoji : " +";
                }
            }

            result.Add($"\u200B{formattedMove}");
        }
        return result;
    }
}
