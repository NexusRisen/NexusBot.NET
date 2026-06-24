using PKHeX.Core;
using PKHeX.Core.AutoMod;
using System;
using System.Linq;

namespace SysBot.Pokemon.Helpers;

public static class LanguageHelper
{
    public static string GetLocalizedSpeciesName(int speciesIndex, LanguageID lang)
    {
        try
        {
            var strings = GameInfo.GetStrings(lang.ToString().ToLower().Replace("chineses", "zh").Replace("chineset", "zh"));
            if (strings?.Species == null || speciesIndex < 0 || speciesIndex >= strings.Species.Count)
                return "???";

            return strings.Species[speciesIndex];
        }
        catch
        {
            return "???";
        }
    }

    public static string GetLocalizedSpeciesLog(PKM pkm)
    {
        if (pkm == null)
            return "(Invalid PokÃ©mon)";

        var langID = (LanguageID)pkm.Language;
        var langName = GetLanguageName(langID);

        string localizedName = GetLocalizedSpeciesName(pkm.Species, langID);
        string englishName = GetLocalizedSpeciesName(pkm.Species, LanguageID.English);

        if (langID == LanguageID.English || localizedName == englishName)
            return englishName;

        return $"{localizedName} ({englishName}, {langName})";
    }

    public static byte GetFinalLanguage(string content, ShowdownSet? set, byte configLanguage, Func<string, byte> detectLanguageFunc)
    {
        // Check if user explicitly specified a language in the showdown set
        var lines = content.Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        foreach (var line in lines)
        {
            if (line.StartsWith("Language:", StringComparison.OrdinalIgnoreCase))
            {
                var languageValue = line["Language:".Length..].Trim();

                // Try to parse as LanguageID enum
                if (Enum.TryParse<LanguageID>(languageValue, true, out var langId))
                    return (byte)langId;

                // Handle common language names
                var explicitLang = languageValue.ToLowerInvariant() switch
                {
                    "japanese" or "jpn" or "æ—¥æœ¬èªž" => LanguageID.Japanese,
                    "english" or "eng" => LanguageID.English,
                    "french" or "fre" or "fra" => LanguageID.French,
                    "italian" or "ita" => LanguageID.Italian,
                    "german" or "ger" or "deu" => LanguageID.German,
                    "spanish" or "spa" or "esp" => LanguageID.Spanish,
                    "spanish-latam" or "spanishl" or "es-419" or "latam" => LanguageID.SpanishL,
                    "korean" or "kor" or "í•œêµ­ì–´" => LanguageID.Korean,
                    "chinese" or "chs" or "ä¸­æ–‡" => LanguageID.ChineseS,
                    "cht" => LanguageID.ChineseT,
                    _ => LanguageID.None
                };

                if (explicitLang != LanguageID.None)
                    return (byte)explicitLang;
            }
        }

        // No explicit language found, use detection
        byte detectedLanguage = detectLanguageFunc(content);

        // If no language was detected (0), use the config language setting
        return detectedLanguage == 0 ? configLanguage : detectedLanguage;
    }

    public static ITrainerInfo GetTrainerInfoWithLanguage<T>(LanguageID language) where T : PKM, new()
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
        return TrainerSettings.GetSavedTrainerData(version, language);
    }

    public static string GetLanguageName(LanguageID lang) => lang switch
    {
        LanguageID.Japanese => "Japanese",
        LanguageID.English => "English",
        LanguageID.French => "French",
        LanguageID.Italian => "Italian",
        LanguageID.German => "German",
        LanguageID.Spanish => "Spanish",
        LanguageID.SpanishL => "SpanishL",
        LanguageID.Korean => "Korean",
        LanguageID.ChineseT => "Chinese (Traditional)",
        LanguageID.ChineseS => "Chinese (Simplified)",
        _ => "Unknown"
    };

    /// <summary>
    /// Determines if a language uses Asian characters (which have a 6-character limit for OT names).
    /// </summary>
    public static bool IsAsianLanguage(LanguageID lang) => lang is LanguageID.Japanese or LanguageID.Korean or LanguageID.ChineseS or LanguageID.ChineseT;

    /// <summary>
    /// Determines if a language uses Asian characters based on language code.
    /// </summary>
    public static bool IsAsianLanguage(int languageCode) => IsAsianLanguage((LanguageID)languageCode);

    /// <summary>
    /// Truncates OT name to the appropriate length based on language.
    /// Asian languages (Japanese, Korean, Chinese) have a 6-character limit.
    /// Latin-based languages have a 12-character limit.
    /// Also sanitizes the name to remove non-printable characters and promotional keywords.
    /// Uses StringInfo to handle multi-byte characters (emojis, etc.) correctly.
    /// </summary>
    public static string SanitizeOTName(string otName, LanguageID language)
    {
        if (string.IsNullOrEmpty(otName))
            return otName;

        // Basic promotional keyword filtering
        var lower = otName.ToLowerInvariant();
        if (lower.Contains(".com") || lower.Contains(".co") || lower.Contains("discord") || lower.Contains(".gg/"))
            otName = "NexusBot.Net";

        // Remove control characters and handle multi-byte characters correctly
        var textElements = System.Globalization.StringInfo.GetTextElementEnumerator(otName);
        var result = new System.Text.StringBuilder();
        int maxLength = IsAsianLanguage(language) ? 6 : 12;
        int count = 0;

        while (textElements.MoveNext() && count < maxLength)
        {
            string element = textElements.GetTextElement();
            // Skip if the first char of the element is a control character
            if (char.IsControl(element[0]))
                continue;

            result.Append(element);
            count++;
        }

        return result.ToString().Trim();
    }

    /// <summary>
    /// Truncates OT name to the appropriate length based on language code.
    /// </summary>
    public static string SanitizeOTName(string otName, int languageCode) => SanitizeOTName(otName, (LanguageID)languageCode);

    /// <summary>
    /// Truncates OT name to the appropriate length based on language.
    /// Asian languages (Japanese, Korean, Chinese) have a 6-character limit.
    /// Latin-based languages have a 12-character limit.
    /// </summary>
    [Obsolete("Use SanitizeOTName instead for better stability.")]
    public static string TruncateOTName(string otName, LanguageID language) => SanitizeOTName(otName, language);
}
