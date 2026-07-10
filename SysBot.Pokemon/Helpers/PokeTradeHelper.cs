using PKHeX.Core;
using PKHeX.Core.AutoMod;
using SysBot.Base;
using SysBot.Pokemon.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static SysBot.Pokemon.TradeSettings.TradeSettingsCategory;

namespace SysBot.Pokemon;

public static class PokeTradeHelper<T> where T : PKM, new()
{
    public static ProcessedPokemonResult<T> ProcessShowdownSet(string content, PokeTradeHub<T> hub, bool ignoreAutoOT = false, ulong userID = 0)
    {
        content = BatchNormalizer.NormalizeBatchCommands(content);
        bool isEgg = TradeExtensions<T>.IsEggCheck(content);

        // If it's not obviously a Showdown set, try translating it with auto-detection
        if (!ShowdownTranslator<T>.IsPS(content))
        {
            var translated = ShowdownTranslator<T>.TranslateToShowdown(content);
            if (!string.IsNullOrWhiteSpace(translated))
            {
                // Preserve batch commands from the original content
                var batchCommands = string.Join("\n", content.Split('\n').Where(l => l.TrimStart().StartsWith('.')));
                content = translated + (string.IsNullOrWhiteSpace(batchCommands) ? "" : "\n" + batchCommands);
            }
        }

        if (!ShowdownParsing.TryParseAnyLanguage(content, out ShowdownSet? set) || set == null || set.Species == 0)
        {
            return new ProcessedPokemonResult<T>
            {
                Error = "Unable to parse Showdown set. Could not identify the Pokémon species.",
                ShowdownSet = set
            };
        }

        byte finalLanguage = LanguageHelper.GetFinalLanguage(
            content, set,
            (byte)hub.Config.Legality.GenerateLanguage,
            TradeExtensions<T>.DetectShowdownLanguage
        );

        var template = AutoLegalityWrapper.GetTemplate(set);

        // Filter out batch commands (.) and filters (~) from invalid lines - these are handled by ALM
        var actualInvalidLines = set.InvalidLines.Where(line =>
        {
            var text = line.Value?.Trim();
            return !string.IsNullOrEmpty(text) && !text.StartsWith('.') && !text.StartsWith('~');
        }).ToList();

        if (actualInvalidLines.Count != 0)
        {
            return new ProcessedPokemonResult<T>
            {
                Error = $"Unable to parse Showdown Set:\n{string.Join("\n", actualInvalidLines.Select(l => l.Value))}",
                ShowdownSet = set
            };
        }

        var sav = LanguageHelper.GetTrainerInfoWithLanguage<T>((LanguageID)finalLanguage);

        PKM? pkm;
        string result;

        // Generate egg or normal pokemon based on isEgg flag
        var regenTemplate = new RegenTemplate(set);
        if (isEgg)
        {
            // Generate egg using ALM
            pkm = AutoLegalityWrapper.GenerateEgg(sav, regenTemplate, out var eggResult);
            result = eggResult.ToString();
        }
        else
        {
            // Use normal template for regular Pokémon
            pkm = sav.GetLegal(template, out result);
        }

        if (pkm != null)
        {
            if (APILegality.AllowTrainerOverride && regenTemplate.Regen.Trainer != null)
                pkm.SetAllTrainerData(regenTemplate.Regen.Trainer);

            // Re-apply Ball customization as ALM can override it
            if (regenTemplate.Regen.Extra.Ball != Ball.None && regenTemplate.Regen.Extra.Ball != Ball.Poke)
                pkm.Ball = (byte)regenTemplate.Regen.Extra.Ball;

            // Re-apply batch commands after ALM generation
            if (APILegality.AllowBatchCommands && regenTemplate.Regen.HasBatchSettings)
            {
                var b = regenTemplate.Regen.Batch;
                EntityBatchEditor.ScreenStrings(b.Filters);
                EntityBatchEditor.ScreenStrings(b.Instructions);
                EntityBatchEditor.Instance.TryModifyIsSuccess(pkm, b.Filters, b.Instructions);
                pkm.ApplyPostBatchFixes();
            }
        }

        if (pkm == null)
        {
            return new ProcessedPokemonResult<T>
            {
                Error = "Set took too long to legalize.",
                ShowdownSet = set
            };
        }

        var spec = GameInfo.Strings.Species[template.Species];

        // Apply standard item logic only for non-eggs
        if (!isEgg)
        {
            ApplyStandardItemLogic(pkm, hub.Config);
        }

        // ============================================================================
        // MAX LAIR POKEMON MOVE POPULATION BUG WORKAROUND
        // ============================================================================
        if (pkm is PK8 pk8 && !isEgg)
        {
            const int MaxLairLocationID = 244; // Max Lair in Crown Tundra
            bool hasNoMoves = pk8.Move1 == 0 && pk8.Move2 == 0 && pk8.Move3 == 0 && pk8.Move4 == 0;
            bool isFromMaxLair = pk8.MetLocation == MaxLairLocationID;

            if (hasNoMoves && isFromMaxLair)
            {
                pk8.SetSuggestedMoves();
                pk8.HealPP();
                pk8.RefreshChecksum();
            }
        }

        // Generate LGPE code if needed
        List<Pictocodes>? lgcode = null;
        if (pkm is PB7)
        {
            lgcode = userID != 0 ? hub.Queues.Info.GetRandomLGTradeCode(userID) : GenerateRandomPictocodes(3);
            if (pkm.Species == (int)Species.Mew && pkm.IsShiny)
            {
                return new ProcessedPokemonResult<T>
                {
                    Error = "Mew can **not** be Shiny in LGPE. PoGo Mew does not transfer and Pokeball Plus Mew is shiny locked.",
                    ShowdownSet = set
                };
            }
        }

        var la = new LegalityAnalysis(pkm);
        if (pkm is not T pk || !la.Valid)
        {
            var reason = GetFailureReason(result, spec);
            var hint = result == "Failed" ? GetLegalizationHint(template, sav, pkm, spec) : null;
            return new ProcessedPokemonResult<T>
            {
                Error = reason,
                LegalizationHint = hint,
                ShowdownSet = set
            };
        }

        // ============================================================================
        // ZA NATURE LEGALITY ENFORCEMENT
        // ============================================================================
        if (pk is PA9)
        {
            var contentLines = content.Split('\n');
            Nature requestedNature = set.Nature;
            bool userRequestedNature = requestedNature != Nature.Random;

            Nature? userExplicitStatNature = null;
            foreach (var line in contentLines)
            {
                var trimmed = line.Trim();
                if (trimmed.StartsWith(".StatAlignment=", StringComparison.OrdinalIgnoreCase))
                {
                    var value = trimmed[".StatAlignment=".Length..].Trim();
                    if (Enum.TryParse<Nature>(value, true, out var parsedSN))
                    {
                        userExplicitStatNature = parsedSN;
                        break;
                    }
                }
            }

            bool hasExplicitStatNature = userExplicitStatNature.HasValue;
            Nature userStatNature = userExplicitStatNature ?? Nature.Random;

            if (userRequestedNature && requestedNature != pk.Nature)
            {
                var clone = (PA9)pk.Clone();
                clone.Nature = requestedNature;
                clone.StatAlignment = hasExplicitStatNature ? userStatNature : requestedNature;
                clone.RefreshChecksum();

                if (new LegalityAnalysis(clone).Valid)
                {
                    pk.Nature = clone.Nature;
                    pk.StatAlignment = clone.StatAlignment;
                    pk.RefreshChecksum();
                    LogUtil.LogInfo(
                        $"{(Species)pk.Species}: Requested nature of {requestedNature} is legal for the set and is applied.",
                        "ZANature");
                }
                else
                {
                    var wantedStatNature = hasExplicitStatNature ? userStatNature : requestedNature;
                    var cloneMint = (PA9)pk.Clone();
                    cloneMint.StatAlignment = wantedStatNature;
                    cloneMint.RefreshChecksum();

                    if (new LegalityAnalysis(cloneMint).Valid)
                    {
                        pk.StatAlignment = wantedStatNature;
                        pk.RefreshChecksum();
                        LogUtil.LogInfo(
                            $"{(Species)pk.Species}: Requested nature of {requestedNature} is illegal for this encounter." +
                            $"Mint Applied! Nature: {pk.Nature} | Stat Nature: {pk.StatAlignment}.",
                            "ZANature");
                    }
                    else
                    {
                        LogUtil.LogInfo(
                            $"{(Species)pk.Species}: Requested nature of {requestedNature} is illegal and minting is " +
                            $"restricted for this encounter. Keeping forced nature of {pk.Nature} with Stat Nature of {pk.StatAlignment}.",
                            "ZANature");
                    }
                }
            }
            else if (userRequestedNature && requestedNature == pk.Nature)
            {
                if (!hasExplicitStatNature)
                {
                    pk.StatAlignment = pk.Nature;
                    pk.RefreshChecksum();
                }
            }
        }

        // ============================================================================
        // ZA TRADE EVOLUTION WORKAROUND
        // ============================================================================
        if (pkm is PA9 && hub.Config.Trade.TradeConfiguration.DisallowTradeEvolutionsZA && pkm.HeldItem != 110)
        {
            ushort[] tradeEvolutions = [61, 64, 67, 75, 79, 93, 95, 112, 117, 123, 125, 126, 137, 233, 349, 356, 366, 533, 588, 616, 682, 684, 708, 710];
            if (Array.IndexOf(tradeEvolutions, pkm.Species) >= 0)
            {
                return new ProcessedPokemonResult<T>
                {
                    Error = "Trade evolutions are disallowed in ZA to prevent game crashes. Please attach an Everstone if you wish to trade this Pokemon.",
                    ShowdownSet = set
                };
            }
        }

        // Final preparation
        PrepareForTrade(pk, set, finalLanguage);

        if (TradeExtensions<T>.HasAdName(pk, out _))
        {
            return new ProcessedPokemonResult<T>
            {
                Error = "Detected Adname in the Pokémon's name or trainer name, which is not allowed.",
                ShowdownSet = set
            };
        }

        // For SWSH (PK8), GO Pokemon can have AutoOT applied, so don't mark them as non-native
        la = new LegalityAnalysis(pk);
        var isNonNative = la.EncounterOriginal.Context != pk.Context || (pk.GO && pk is not PK8);

        return new ProcessedPokemonResult<T>
        {
            Pokemon = pk,
            ShowdownSet = set,
            LgCode = lgcode,
            IsNonNative = isNonNative
        };
    }

    public static void ApplyStandardItemLogic(PKM pkm, PokeTradeHubConfig config)
    {
        pkm.HeldItem = pkm switch
        {
            PA9 or PA8 => (int)HeldItem.None,
            _ when pkm.HeldItem == 0 && !pkm.IsEgg => (int)config.Trade.TradeConfiguration.DefaultHeldItem,
            _ => pkm.HeldItem
        };
    }

    public static void PrepareForTrade(T pk, ShowdownSet set, byte finalLanguage)
    {
        // Only set EggMetDate for hatched Pokemon, not for unhatched eggs
        if (pk.WasEgg && !pk.IsEgg)
            pk.EggMetDate = pk.MetDate;

        pk.Language = finalLanguage;

        if (!set.Nickname.Equals(pk.Nickname) && string.IsNullOrEmpty(set.Nickname))
            _ = pk.ClearNickname();

        pk.ResetPartyStats();
    }

    public static string GetFailureReason(string result, string speciesName)
    {
        return result switch
        {
            "Timeout" => $"That {speciesName} set took too long to generate.",
            "VersionMismatch" => "Request refused: PKHeX and Auto-Legality Mod version mismatch.",
            _ => $"I wasn't able to create a {speciesName} from that set."
        };
    }

    public static string GetLegalizationHint(IBattleTemplate template, ITrainerInfo sav, PKM pkm, string speciesName)
    {
        var hint = AutoLegalityWrapper.GetLegalizationHint(template, sav, pkm);
        if (hint.Contains("Requested shiny value (ShinyType."))
        {
            hint = $"{speciesName} **cannot** be shiny. Please try again.";
        }
        return hint;
    }

    public static List<Pictocodes> GenerateRandomPictocodes(int count)
    {
        Random rnd = new();
        List<Pictocodes> randomPictocodes = [];
        Array pictocodeValues = Enum.GetValues<Pictocodes>();

        for (int i = 0; i < count; i++)
        {
            Pictocodes randomPictocode = (Pictocodes)pictocodeValues.GetValue(rnd.Next(pictocodeValues.Length))!;
            randomPictocodes.Add(randomPictocode);
        }

        return randomPictocodes;
    }
}
