using Discord;
using Discord.WebSocket;
using PKHeX.Core;
using SysBot.Pokemon.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SysBot.Pokemon.Discord;

/// <summary>
/// Extracts and formats details from Pokémon data for Discord embed displays.
/// </summary>
/// <typeparam name="T">Type of Pokémon data structure.</typeparam>
public static class DetailsExtractor<T> where T : PKM, new()
{
    /// <summary>
    /// Adds additional text to the embed as configured in settings.
    /// </summary>
    /// <param name="embedBuilder">Discord embed builder to modify.</param>
    public static void AddAdditionalText(EmbedBuilder embedBuilder)
    {
        string additionalText = string.Join("\n", SysCordSettings.Settings.AdditionalEmbedText);
        if (!string.IsNullOrEmpty(additionalText))
        {
            embedBuilder.AddField("\u200B", additionalText, inline: false);
        }
    }

    /// <summary>
    /// Adds normal trade information fields to the embed.
    /// </summary>
    /// <param name="embedBuilder">Discord embed builder to modify.</param>
    /// <param name="embedData">Extracted Pokémon data.</param>
    /// <param name="trainerMention">Discord mention for the trainer.</param>
    /// <param name="pk">Pokémon data.</param>
    public static void AddNormalTradeFields(EmbedBuilder embedBuilder, EmbedData embedData, string trainerMention, T pk)
    {
        var settings = SysCord<T>.Runner.Config.Trade.TradeEmbedSettings;
        
        // Column 1: Core Attributes
        var attrList = new List<string>();
        if (settings.ShowBall) attrList.Add($"**Ball:** {embedData.Ball}");
        if (settings.ShowNature)
        {
            string natureDisplay = $"**Nature:** {embedData.Nature}";
            if (!string.IsNullOrEmpty(embedData.StatAlignment))
            {
                // In PLZA, Nature is the PID-based nature, and StatNature is the minted/intended nature.
                // We display it as: IntendedNature (Minted from: PIDNature)
                natureDisplay = $"**Nature:** {embedData.StatAlignment} (Minted from: {embedData.Nature})";
            }
            attrList.Add(natureDisplay);
        }
        if (settings.ShowAbility) attrList.Add($"**Ability:** {embedData.Ability}");
        if (settings.ShowLanguage) attrList.Add($"**Lang:** {embedData.Language}");
        
        // Column 2: Combat/Growth Stats
        var statsList = new List<string>();
        if (settings.ShowLevel) statsList.Add($"**Level:** {embedData.Level}");
        if (pk.Version is GameVersion.SL or GameVersion.VL && settings.ShowTeraType) statsList.Add($"**Tera:** {embedData.TeraType}");
        if (settings.ShowIVs) statsList.Add($"**IVs:** {embedData.IVsDisplay}");
        if (settings.ShowEVs && !string.IsNullOrWhiteSpace(embedData.EVsDisplay)) statsList.Add($"**EVs:** {embedData.EVsDisplay}");

        // Column 3: Moves
        string movesContent = embedData.MovesDisplay ?? string.Empty;

        string speciesHeader = $"{embedData.SpeciesName}{(string.IsNullOrEmpty(embedData.FormName) ? "" : $"-{embedData.FormName}")} {embedData.SpecialSymbols}";
        embedBuilder.WithTitle(speciesHeader);

        if (attrList.Count > 0)
            embedBuilder.AddField("Attributes", string.Join("\n", attrList), true);
        
        if (statsList.Count > 0)
            embedBuilder.AddField("Stats", string.Join("\n", statsList), true);

        if (!string.IsNullOrEmpty(movesContent))
            embedBuilder.AddField("Moves", movesContent, true);

        // Additional Information (Met, Scale, etc.)
        var additionalInfo = new List<string>();
        if (settings.ShowMetLevel) additionalInfo.Add($"**Met Level:** {embedData.MetLevel}");
        if (settings.ShowMetDate) additionalInfo.Add($"**Met Date:** {embedData.MetDate}");
        if (settings.ShowMetLocation) additionalInfo.Add($"**Met Location:** {embedData.MetLocation}");
        
        if (pk.Version is GameVersion.PLA or GameVersion.SL or GameVersion.VL && settings.ShowScale)
            additionalInfo.Add($"**Scale:** {embedData.Scale.Item1} ({embedData.Scale.Item2})");

        if (additionalInfo.Count > 0)
        {
            embedBuilder.AddField("Origin & Physical", string.Join(" | ", additionalInfo), false);
        }
        
        // Add User mention at the end or in a footer? 
        // Upstream had it at the start of leftSideContent.
        // Let's add it as a field or description if needed.
        embedBuilder.WithDescription($"**User:** {trainerMention}");
    }


    /// <summary>
    /// Adds special trade information fields to the embed.
    /// </summary>
    /// <param name="embedBuilder">Discord embed builder to modify.</param>
    /// <param name="isMysteryEgg">Whether this is a mystery egg trade.</param>
    /// <param name="isSpecialRequest">Whether this is a special request trade.</param>
    /// <param name="isCloneRequest">Whether this is a clone request trade.</param>
    /// <param name="isFixOTRequest">Whether this is a fix OT request trade.</param>
    /// <param name="trainerMention">Discord mention for the trainer.</param>
    public static void AddSpecialTradeFields(EmbedBuilder embedBuilder, bool isMysteryEgg, bool isSpecialRequest, bool isCloneRequest, bool isFixOTRequest, string trainerMention)
    {
        string specialDescription = $"**Trainer:** {trainerMention}\n" +
                                    (isMysteryEgg ? "Mystery Egg" : isSpecialRequest ? "Special Request" : isCloneRequest ? "Clone Request" : isFixOTRequest ? "FixOT Request" : "Dump Request");
        embedBuilder.AddField("\u200B", specialDescription, inline: false);
    }

    /// <summary>
    /// Adds thumbnails to the embed based on trade type.
    /// </summary>
    /// <param name="embedBuilder">Discord embed builder to modify.</param>
    /// <param name="isCloneRequest">Whether this is a clone request trade.</param>
    /// <param name="isSpecialRequest">Whether this is a special request trade.</param>
    /// <param name="heldItemUrl">URL for the held item image.</param>
    public static void AddThumbnails(EmbedBuilder embedBuilder, bool isCloneRequest, bool isSpecialRequest, string heldItemUrl)
    {
        if (isCloneRequest || isSpecialRequest)
        {
            embedBuilder.WithThumbnailUrl("https://raw.githubusercontent.com/NexusRisen/Nexus-Risen-Edition-Sprite-Images/main/Assets/NPCs/profoak.png");
        }
        else if (!string.IsNullOrEmpty(heldItemUrl))
        {
            embedBuilder.WithThumbnailUrl(heldItemUrl);
        }
    }

    /// <summary>
    /// Extracts detailed information from a Pokémon for display.
    /// </summary>
    /// <param name="pk">Pokémon data.</param>
    /// <param name="user">Discord user initiating the trade.</param>
    /// <param name="isMysteryEgg">Whether this is a mystery egg trade.</param>
    /// <param name="isCloneRequest">Whether this is a clone request trade.</param>
    /// <param name="isDumpRequest">Whether this is a dump request trade.</param>
    /// <param name="isFixOTRequest">Whether this is a fix OT request trade.</param>
    /// <param name="isSpecialRequest">Whether this is a special request trade.</param>
    /// <param name="isBatchTrade">Whether this is part of a batch trade.</param>
    /// <param name="batchTradeNumber">The number of this trade in the batch sequence.</param>
    /// <param name="totalBatchTrades">Total number of trades in the batch.</param>
    /// <returns>Structured Pokémon data for embed display.</returns>
    public static EmbedData ExtractPokemonDetails(T pk, SocketUser user, bool isMysteryEgg, bool isCloneRequest, bool isDumpRequest, bool isFixOTRequest, bool isSpecialRequest, bool isBatchTrade, int batchTradeNumber, int totalBatchTrades)
    {
        string langCode = ((LanguageID)pk.Language).GetLanguageCode();
        GameStrings strings = GameInfo.GetStrings(langCode);

        var originalLanguage = GameInfo.CurrentLanguage;
        GameInfo.CurrentLanguage = langCode;

        var embedData = new EmbedData
        {
            Moves = GetMoveNames(pk, strings),
            Level = pk.CurrentLevel
        };

        int languageId = pk.Language;
        string languageDisplay = GetLanguageDisplay(pk);
        embedData.Language = languageDisplay;

        if (pk is PK9 pk9)
        {
            embedData.TeraType = GetTeraTypeString(pk9);
            embedData.Scale = GetScaleDetails(pk9);
        }

        embedData.Ability = GetAbilityName(pk, strings);
        embedData.Nature = GetNatureName(pk, strings);

        // Extract Stat Nature if it differs from regular Nature and is not unminted (Nature.Random)
        if (pk.StatAlignment != Nature.Random && pk.StatAlignment != pk.Nature && strings.natures != null)
        {
            embedData.StatAlignment = strings.natures[(int)pk.StatAlignment];
        }

        embedData.SpeciesName = strings.Species[pk.Species];
        embedData.SpecialSymbols = GetSpecialSymbols(pk);
        embedData.FormName = ShowdownParsing.GetStringFromForm(pk.Form, strings, pk.Species, pk.Context);
        embedData.HeldItem = strings.itemlist[pk.HeldItem];
        embedData.Ball = strings.balllist[pk.Ball];

        Span<int> ivs = stackalloc int[6];
        pk.GetIVs(ivs);

        // Map PKHeX order to display order: HP / Atk / Def / SpA / SpD / Spe
        int[] displayOrder = { 0, 1, 2, 4, 5, 3 }; // indices in ivs[]
        string[] labels = { "HP", "Atk", "Def", "SpA", "SpD", "Spe" };

        // Count perfect IVs
        int perfectIVCount = 0;
        for (int i = 0; i < ivs.Length; i++)
        {
            if (ivs[i] == 31)
                perfectIVCount++;
        }

        // Build IV display strings
        var ivStrings = new List<string>();
        for (int i = 0; i < displayOrder.Length; i++)
        {
            int idx = displayOrder[i];
            int ivValue = ivs[idx];
            string label = labels[i];
            
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

            ivStrings.Add($"{ivValue}{(isHT ? " (HT)" : "")} {label}");
        }

        // Compose final display
        string ivsDisplay = perfectIVCount == 6 ? "6IV" : string.Join(" / ", ivStrings);
        embedData.IVsDisplay = ivsDisplay;

        int[] evs = GetEVs(pk);
        var evLabels = new[] { "HP", "Atk", "Def", "Spe", "SpA", "SpD" };
        var activeEVs = new List<string>();
        for (int i = 0; i < 6; i++)
        {
            // Map PKHeX order to display order: HP / Atk / Def / SpA / SpD / Spe
            int idx = i switch
            {
                0 => 0, // HP
                1 => 1, // Atk
                2 => 2, // Def
                3 => 4, // SpA
                4 => 5, // SpD
                5 => 3, // Spe
                _ => 0
            };
            if (evs[idx] > 0)
                activeEVs.Add($"{evs[idx]} {evLabels[idx]}");
        }
        embedData.EVsDisplay = activeEVs.Count > 0 ? string.Join(" / ", activeEVs) : "None";
        embedData.MetDate = pk.MetDate.ToString();
        embedData.MetLevel = pk.MetLevel;
        var metLocationName = strings.GetLocationName(false, pk.MetLocation, pk.Format, pk.Generation, (GameVersion)pk.Version);
        embedData.MetLocation = string.IsNullOrWhiteSpace(metLocationName) ? $"**ID:** {pk.MetLocation}" : $"{metLocationName} **(ID: {pk.MetLocation})**";
        embedData.MovesDisplay = embedData.Moves != null ? string.Join("\n", embedData.Moves) : string.Empty;
        embedData.PokemonDisplayName = pk.IsNicknamed ? pk.Nickname : embedData.SpeciesName;

        embedData.TradeTitle = GetTradeTitle(isMysteryEgg, isCloneRequest, isDumpRequest, isFixOTRequest, isSpecialRequest, isBatchTrade, batchTradeNumber, embedData.PokemonDisplayName, pk.IsShiny);
        embedData.AuthorName = GetAuthorName(user.Username, embedData.TradeTitle, isMysteryEgg, isFixOTRequest, isCloneRequest, isDumpRequest, isSpecialRequest, isBatchTrade, embedData.PokemonDisplayName, pk.IsShiny);

        GameInfo.CurrentLanguage = originalLanguage;

        return embedData;
    }

    private static int CalculateMedals(int tradeCount)
    {
        if (tradeCount < 1) return 0;
        return 1 + Math.Min(20, tradeCount / 50); // 1 for >= 1, then +1 for every 50 up to 1000
    }

    /// <summary>
    /// Gets user details for display.
    /// </summary>
    /// <param name="totalTradeCount">Total number of trades for this user.</param>
    /// <param name="tradeDetails">Trade code details if available.</param>
    /// <param name="trainerMention">If no details available, set a static message with Discord username.</param>
    /// <returns>Formatted user details string.</returns>
    public static string GetUserDetails(int totalTradeCount, TradeCodeStorage.TradeCodeDetails? tradeDetails, string trainerMention)
    {
        string userDetailsText = "";

        // Add Total User Trades + Medals
        if (totalTradeCount > 0)
        {
            if (SysCord<T>.Runner.Hub.Config.Discord.EnableMedals)
            {
                int totalMedals = CalculateMedals(totalTradeCount);
                userDetailsText += $"Total User Trades: {totalTradeCount} | Medals: {totalMedals}\n";
            }
            else
            {
                userDetailsText += $"Total User Trades: {totalTradeCount}\n";
            }
        }

        // First trade — no record exists yet
        if (tradeDetails == null)
        {
            userDetailsText += "First Trade, No Trainer Info Saved.";
            return userDetailsText;
        }

        // Display trainer info if storage enabled
        if (SysCord<T>.Runner.Config.Trade.TradeConfiguration.StoreTradeCodes)
        {
            List<string> trainerParts = new();

            if (!string.IsNullOrEmpty(tradeDetails.OT))
                trainerParts.Add($"OT: {tradeDetails.OT}");

            if (tradeDetails.TID > 0)
                trainerParts.Add($"TID: {tradeDetails.TID}");

            // SID is no longer force-rejected, we just show it if it exists
            if (tradeDetails.SID > 0)
                trainerParts.Add($"SID: {tradeDetails.SID}");

            // If user exists but no trainer fields are populated
            if (trainerParts.Count == 0)
                trainerParts.Add("Trainer Info Not Yet Recorded");

            userDetailsText += string.Join(" | ", trainerParts);
        }

        return userDetailsText;
    }

    private static string GetLanguageDisplay(T pk)
    {
        int safeLanguage = pk.Language;

        string languageName = "Unknown";
        var languageList = GameInfo.LanguageDataSource(pk.Format, pk.Context);
        var languageEntry = languageList.FirstOrDefault(l => l.Value == pk.Language);

        if (languageEntry != null)
        {
            languageName = languageEntry.Text;
        }
        else
        {
            languageName = ((LanguageID)pk.Language).GetLanguageCode();
        }

        if (safeLanguage != pk.Language)
        {
            string safeLanguageName = languageList.FirstOrDefault(l => l.Value == safeLanguage)?.Text ?? ((LanguageID)safeLanguage).GetLanguageCode();
            return $"{languageName} (Safe: {safeLanguageName})";
        }

        return languageName;
    }

    private static string GetAbilityName(T pk, GameStrings strings)
    {
        return strings.abilitylist[pk.Ability];
    }

    private static string GetAuthorName(string username, string tradeTitle, bool isMysteryEgg, bool isFixOTRequest, bool isCloneRequest, bool isDumpRequest, bool isSpecialRequest, bool isBatchTrade, string pokemonDisplayName, bool isShiny)
    {
        string isPkmShiny = isShiny ? "Shiny " : "";
        return isMysteryEgg || isFixOTRequest || isCloneRequest || isDumpRequest || isSpecialRequest || isBatchTrade ?
               $"{username}'s {tradeTitle}" :
               $"{username}'s {isPkmShiny}{pokemonDisplayName}";
    }

    private static int[] GetEVs(T pk)
    {
        int[] evs = new int[6];
        pk.GetEVs(evs);
        return evs;
    }

    // Scrape move names with PP and type emojis
    private static List<string> GetMoveNames(T pk, GameStrings strings)
    {
        ushort[] moves = new ushort[4];
        pk.GetMoves(moves.AsSpan());
        List<int> movePPs = new() { pk.Move1_PP, pk.Move2_PP, pk.Move3_PP, pk.Move4_PP };
        var moveNames = new List<string>();

        // Prepare type emojis dictionary
        var typeEmojis = SysCord<T>.Runner.Config.Trade.TradeEmbedSettings.CustomTypeEmojis
            .Where(e => !string.IsNullOrEmpty(e.EmojiCode))
            .ToDictionary(e => (PKHeX.Core.MoveType)e.MoveType, e => $"{e.EmojiCode}");

        // PLUS MOVE emoji
        string plusEmoji = string.Empty;
        var plusEmojiString = SysCord<T>.Runner.Config.Trade.TradeEmbedSettings.UsePlusMoveEmoji?.EmojiString;
        if (!string.IsNullOrWhiteSpace(plusEmojiString))
            plusEmoji = $" {plusEmojiString}";

        for (int i = 0; i < moves.Length; i++)
        {
            if (moves[i] == 0) continue;

            string moveName = strings.movelist[moves[i]];
            byte moveTypeId = MoveInfo.GetType(moves[i], default);
            PKHeX.Core.MoveType moveType = (PKHeX.Core.MoveType)moveTypeId;

            // For PLZA (PA9) we skip the PP entirely
            bool isPLZA = pk is PA9;

            string formattedMove = isPLZA
                ? $"*{moveName}*" // no PP
                : $"*{moveName}* ({movePPs[i]} PP)"; // normal games include PP

            // Add type emoji
            if (SysCord<T>.Runner.Config.Trade.TradeEmbedSettings.MoveTypeEmojis && typeEmojis.TryGetValue(moveType, out var moveEmoji))
            {
                formattedMove = $"{moveEmoji} {formattedMove}";
            }

            // PLUS MOVE LOGIC (PLZA only)
            if (isPLZA && pk is PA9 pa9 && pa9.PersonalInfo is IPermitPlus plus)
            {
                int plusIndex = plus.PlusMoveIndexes.IndexOf(moves[i]);
                if (plusIndex >= 0 && pa9.GetMovePlusFlag(plusIndex))
                {
                    formattedMove += !string.IsNullOrWhiteSpace(plusEmoji) ? plusEmoji : " +";
                }
            }

            moveNames.Add($"\u200B{formattedMove}");
        }

        return moveNames;
    }

    private static string GetNatureName(T pk, GameStrings strings)
    {
        return strings.natures[(int)pk.Nature];
    }

    private static (string, byte) GetScaleDetails(PK9 pk9)
    {
        string scaleText = $"{PokeSizeDetailedUtil.GetSizeRating(pk9.Scale)}";
        byte scaleNumber = pk9.Scale;
        return (scaleText, scaleNumber);
    }

    private static string GetSpecialSymbols(T pk)
    {
        var settings = SysCord<T>.Runner.Config.Trade.TradeEmbedSettings;
        var symbols = new List<string>();

        // Shiny status
        if (pk.IsShiny)
        {
            symbols.Add(pk.ShinyXor == 0 ? "◼ " : "★ ");
        }

        // Alpha status (PLA/LA)
        if (pk is IAlpha alpha && alpha.IsAlpha)
        {
            symbols.Add(settings.AlphaPLAEmoji.EmojiString);
        }

        // Marks & Ribbons (Gen 9+)
        if (pk is IRibbonSetMark9 ribbonSetMark)
        {
            if (ribbonSetMark.RibbonMarkMightiest)
                symbols.Add(settings.MightiestMarkEmoji.EmojiString);
            if (ribbonSetMark.RibbonMarkAlpha)
                symbols.Add(settings.AlphaMarkEmoji.EmojiString);
        }

        // Common Marks/Titles
        if (pk is IRibbonIndex ribbonIndex && TradeExtensions<T>.HasMark(ribbonIndex, out _, out string markTitle))
        {
            symbols.Add(markTitle.Trim());
        }

        // Fateful Encounter (Mystery Gift)
        if (pk.FatefulEncounter)
        {
            symbols.Add(settings.MysteryGiftEmoji.EmojiString);
        }

        // Gender symbols
        string genderSymbol = GameInfo.GenderSymbolASCII[pk.Gender];
        string genderEmoji = genderSymbol switch
        {
            "M" => settings.MaleEmoji.EmojiString ?? "(M) ",
            "F" => settings.FemaleEmoji.EmojiString ?? "(F) ",
            _ => string.Empty
        };
        if (!string.IsNullOrEmpty(genderEmoji))
        {
            symbols.Add(genderEmoji);
        }

        return string.Join(" ", symbols.Where(s => !string.IsNullOrWhiteSpace(s))) + (symbols.Count > 0 ? " " : "");
    }

    private static string GetTeraTypeString(PK9 pk9)
    {
        var isStellar = pk9.TeraTypeOverride == (MoveType)TeraTypeUtil.Stellar || (int)pk9.TeraType == 99;
        var teraType = isStellar ? TradeSettings.MoveType.Stellar : (TradeSettings.MoveType)pk9.TeraType;

        if (SysCord<T>.Runner.Config.Trade.TradeEmbedSettings.UseTeraEmojis)
        {
            var emojiInfo = SysCord<T>.Runner.Config.Trade.TradeEmbedSettings.TeraTypeEmojis.Find(e => e.MoveType == teraType);
            if (emojiInfo != null && !string.IsNullOrEmpty(emojiInfo.EmojiCode))
            {
                return emojiInfo.EmojiCode;
            }
        }

        return teraType.ToString();
    }

    private static string GetTradeTitle(bool isMysteryEgg, bool isCloneRequest, bool isDumpRequest, bool isFixOTRequest, bool isSpecialRequest, bool isBatchTrade, int batchTradeNumber, string pokemonDisplayName, bool isShiny)
    {
        string shinyEmoji = isShiny ? "✨ " : "";
        return isMysteryEgg ? "Mystery Egg Request!" :
               isBatchTrade ? $"Batch Trade #{batchTradeNumber} - {shinyEmoji}{pokemonDisplayName}" :
               isFixOTRequest ? "FixOT Request!" :
               isSpecialRequest ? "Special Request!" :
               isCloneRequest ? "Clone Request!" :
               isDumpRequest ? "Dump Request!" :
               "";
    }
}

/// <summary>
/// Container for Pokémon data formatted for Discord embed display.
/// </summary>
public class EmbedData
{
    /// <summary>Pokémon ability name.</summary>
    public string? Ability { get; set; }

    /// <summary>Author name for the embed.</summary>
    public string? AuthorName { get; set; }

    /// <summary>Poké Ball name.</summary>
    public string? Ball { get; set; }

    /// <summary>URL for embed image.</summary>
    public string? EmbedImageUrl { get; set; }

    /// <summary>Formatted EVs display string.</summary>
    public string? EVsDisplay { get; set; }

    /// <summary>Form name.</summary>
    public string? FormName { get; set; }

    /// <summary>Held item name.</summary>
    public string? HeldItem { get; set; }

    /// <summary>URL for held item image.</summary>
    public string? HeldItemUrl { get; set; }

    /// <summary>Whether the image is from a local file.</summary>
    public bool IsLocalFile { get; set; }

    /// <summary>Formatted IVs display string.</summary>
    public string? IVsDisplay { get; set; }

    /// <summary>Pokémon language.</summary>
    public string? Language { get; set; }

    /// <summary>Pokémon level.</summary>
    public int Level { get; set; }

    /// <summary>Met date.</summary>
    public string? MetDate { get; set; }

    /// <summary>Met level.</summary>
    public byte MetLevel { get; set; }

    /// <summary>Met location name.</summary>
    public string? MetLocation { get; set; }

    /// <summary>List of move names.</summary>
    public List<string>? Moves { get; set; }

    /// <summary>Formatted moves display string.</summary>
    public string? MovesDisplay { get; set; }

    /// <summary>Nature name.</summary>
    public string? Nature { get; set; }

    /// <summary>Displayed Pokémon name (nickname or species).</summary>
    public string? PokemonDisplayName { get; set; }

    /// <summary>Stat Nature name (for minted Natures in PLZA).</summary>
    public string? StatAlignment { get; set; }

    /// <summary>Size scale rating and number.</summary>
    public (string, byte) Scale { get; set; }

    /// <summary>Special symbol indicators (shiny, gender, etc.).</summary>
    public string? SpecialSymbols { get; set; }

    /// <summary>Species name.</summary>
    public string? SpeciesName { get; set; }

    /// <summary>Tera type for PLA/SV.</summary>
    public string? TeraType { get; set; }

    /// <summary>Trade title for the embed.</summary>
    public string? TradeTitle { get; set; }
}
