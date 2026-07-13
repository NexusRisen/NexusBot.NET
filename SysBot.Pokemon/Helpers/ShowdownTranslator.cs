using PKHeX.Core;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System;

namespace SysBot.Pokemon
{
    public class ShowdownTranslator<T> where T : PKM
    {
        private static readonly LanguageID[] SupportedLanguages = {
            LanguageID.Japanese,
            LanguageID.French,
            LanguageID.Italian,
            LanguageID.German,
            LanguageID.Spanish,
            LanguageID.Korean,
            LanguageID.ChineseS,
            LanguageID.ChineseT,
            LanguageID.English
        };

        /// <summary>
        /// Automatically detects the language and translates the input to a standard English Showdown set.
        /// Cross-referenced with Secludedly's ZE-FusionBot for universal translation support.
        /// </summary>
        public static string TranslateToShowdown(string input)
        {
            if (string.IsNullOrWhiteSpace(input)) return string.Empty;

            // Try each supported language to find a species match
            foreach (var lang in SupportedLanguages)
            {
                var speciesCache = ShowdownTranslatorCache.GetSpeciesCache(lang);
                if (speciesCache.Keys.Any(k => input.Contains(k, StringComparison.OrdinalIgnoreCase)))
                {
                    return Any2Showdown(input, lang);
                }
            }

            return string.Empty;
        }

        public static string Chinese2Showdown(string input) => Any2Showdown(input, LanguageID.ChineseS);

        public static string Any2Showdown(string input, LanguageID sourceLang)
        {
            if (string.IsNullOrWhiteSpace(input)) return string.Empty;

            var langCode = sourceLang.GetLanguageCode();
            var strings = GameInfo.GetStrings(langCode);
            var stringsEn = GameInfo.GetStrings("en");

            string result = "";

            // Species lookup using cache
            int specieNo = -1;
            string matchedSpeciesName = "";
            var speciesCache = ShowdownTranslatorCache.GetSpeciesCache(sourceLang);
            
            foreach (var kvp in speciesCache)
            {
                if (input.Contains(kvp.Key, StringComparison.OrdinalIgnoreCase))
                {
                    specieNo = kvp.Value;
                    matchedSpeciesName = kvp.Key;
                    break;
                }
            }

            // Fallback to English species if not found in source lang
            if (specieNo <= 0)
            {
                var enCache = ShowdownTranslatorCache.GetSpeciesCache(LanguageID.English);
                foreach (var kvp in enCache)
                {
                    if (input.Contains(kvp.Key, StringComparison.OrdinalIgnoreCase))
                    {
                        specieNo = kvp.Value;
                        matchedSpeciesName = kvp.Key;
                        break;
                    }
                }
            }

            if (specieNo <= 0) return string.Empty;

            result = specieNo switch
            {
                (int)Species.NidoranF => "Nidoran-F",
                (int)Species.NidoranM => "Nidoran-M",
                _ => stringsEn.Species[specieNo],
            };

            var speciesRegex = new Regex(Regex.Escape(matchedSpeciesName));
            input = speciesRegex.Replace(input, "", 1);

            // Gender check (Species specific)
            if (((Species)specieNo is Species.Meowstic or Species.Indeedee or Species.Basculegion or Species.Oinkologne))
            {
                foreach (var kvp in ShowdownTranslatorDictionary.FemaleKeywords)
                {
                    if (input.Contains(kvp.Key))
                    {
                        result += "-F";
                        input = input.Replace(kvp.Key, "");
                        break;
                    }
                }
            }

            // Form matching
            foreach (var s in ShowdownTranslatorDictionary.formDict)
            {
                if (input.Contains(s.Key))
                {
                    result += $"-{s.Value}";
                    input = input.Replace(s.Key, "");
                    break;
                }
            }
            // English form fallbacks
            foreach (var s in ShowdownTranslatorDictionary.FormFallbacks)
            {
                if (input.Contains(s.Key))
                {
                    result += $"-{s.Value}";
                    input = input.Replace(s.Key, "");
                    break;
                }
            }

            // Egg
            if (input.Contains("Egg") || input.Contains("蛋") || input.Contains("Œuf") || input.Contains("Ei") || input.Contains("Uovo") || input.Contains("Huevo") || input.Contains("알"))
            {
                result = $"Egg ({result})";
            }

            // Gender
            foreach (var kvp in ShowdownTranslatorDictionary.MaleKeywords)
            {
                if (input.Contains(kvp.Key))
                {
                    result += " (M)";
                    input = input.Replace(kvp.Key, "");
                    break;
                }
            }
            foreach (var kvp in ShowdownTranslatorDictionary.FemaleKeywords)
            {
                if (input.Contains(kvp.Key))
                {
                    if (!result.EndsWith("-F")) result += " (F)";
                    input = input.Replace(kvp.Key, "");
                    break;
                }
            }

            // Item
            if (ShowdownTranslatorDictionary.HoldItemKeywords.TryGetValue(sourceLang, out var itemKeywords))
            {
                foreach (var kw in itemKeywords)
                {
                    if (!input.Contains(kw)) continue;
                    for (int i = 1; i < strings.Item.Count; i++)
                    {
                        if (string.IsNullOrEmpty(strings.Item[i])) continue;
                        string fullPattern = kw + strings.Item[i];
                        if (input.Contains(fullPattern))
                        {
                            result += $" @ {stringsEn.Item[i]}";
                            input = input.Replace(fullPattern, "");
                            goto itemFound;
                        }
                    }
                }
            }
            // Fallback for item without keyword
            for (int i = 1; i < strings.Item.Count; i++)
            {
                if (string.IsNullOrEmpty(strings.Item[i])) continue;
                if (input.Contains("@ " + strings.Item[i]) || input.Contains("@" + strings.Item[i]))
                {
                    result += $" @ {stringsEn.Item[i]}";
                    input = input.Replace(strings.Item[i], "");
                    break;
                }
            }
        itemFound:;

            // Level
            if (ShowdownTranslatorDictionary.LevelKeywords.TryGetValue(sourceLang, out var lvlKw))
            {
                var lvlMatch = Regex.Match(input, $@"(\d{{1,3}}){Regex.Escape(lvlKw)}");
                if (lvlMatch.Success)
                {
                    result += $"\nLevel: {lvlMatch.Groups[1].Value}";
                    input = input.Replace(lvlMatch.Value, "");
                }
            }

            // Shiny
            foreach (var kvp in ShowdownTranslatorDictionary.ShinyKeywords)
            {
                if (input.Contains(kvp.Key))
                {
                    result += kvp.Value;
                    input = input.Replace(kvp.Key, "");
                    break;
                }
            }

            // Alpha
            if ((typeof(T) == typeof(PA8) || typeof(T) == typeof(PA9)) && (input.Contains("Alpha") || input.Contains("头目") || input.Contains("オヤブン") || input.Contains("Baron") || input.Contains("Elite") || input.Contains("Alpha") || input.Contains("Alfa") || input.Contains("우두머리")))
            {
                result += "\nAlpha: Yes";
            }

            // Ball
            for (int i = 1; i < strings.balllist.Length; i++)
            {
                if (string.IsNullOrEmpty(strings.balllist[i])) continue;
                if (input.Contains(strings.balllist[i]))
                {
                    var ballStr = stringsEn.balllist[i];
                    if ((typeof(T) == typeof(PA8) || typeof(T) == typeof(PA9)) && ballStr is "Poké Ball" or "Great Ball" or "Ultra Ball") 
                        ballStr = "LA" + ballStr;
                    result += $"\nBall: {ballStr}";
                    input = input.Replace(strings.balllist[i], "");
                    break;
                }
            }

            // Ability
            for (int i = 1; i < strings.Ability.Count; i++)
            {
                if (string.IsNullOrEmpty(strings.Ability[i])) continue;
                if (input.Contains(strings.Ability[i]))
                {
                    result += $"\nAbility: {stringsEn.Ability[i]}";
                    input = input.Replace(strings.Ability[i], "");
                    break;
                }
            }

            // Nature
            for (int i = 0; i < strings.Natures.Count; i++)
            {
                if (string.IsNullOrEmpty(strings.Natures[i])) continue;
                if (input.Contains(strings.Natures[i]))
                {
                    result += $"\n{stringsEn.Natures[i]} Nature";
                    input = input.Replace(strings.Natures[i], "");
                    break;
                }
            }

            // IVs
            foreach (var kvp in ShowdownTranslatorDictionary.ivCombos)
            {
                if (input.ToUpper().Contains(kvp.Key))
                {
                    result += "\nIVs: " + kvp.Value;
                    input = Regex.Replace(input, Regex.Escape(kvp.Key), "", RegexOptions.IgnoreCase);
                    break;
                }
            }

            // EVs
            if (ShowdownTranslatorDictionary.EvKeywords.TryGetValue(sourceLang, out var evKw) && input.Contains(evKw))
            {
                StringBuilder sb = new();
                sb.Append("\nEVs: ");
                input = input.Replace(evKw, "");

                foreach (var stat in ShowdownTranslatorDictionary.statsDict)
                {
                    string pattern = $@"(\d{{1,3}}){Regex.Escape(stat.Key)}";
                    var match = Regex.Match(input, pattern);
                    if (match.Success)
                    {
                        sb.Append($"{match.Groups[1].Value} {stat.Value} / ");
                        input = input.Replace(match.Value, "");
                    }
                }
                string evsResult = sb.ToString();
                if (evsResult.EndsWith("/ ")) evsResult = evsResult[..^2];
                result += evsResult;
            }

            // Tera Type
            if (typeof(T) == typeof(PK9))
            {
                for (int i = 0; i < strings.Types.Count; i++)
                {
                    if (string.IsNullOrEmpty(strings.Types[i])) continue;
                    if (input.Contains(strings.Types[i]))
                    {
                        result += $"\nTera Type: {stringsEn.Types[i]}";
                        input = input.Replace(strings.Types[i], "");
                        break;
                    }
                }
            }

            // Ribbons/Marks
            foreach (var kvp in ShowdownTranslatorDictionary.ribbonMarks)
            {
                if (input.Contains(kvp.Key))
                {
                    result += kvp.Value;
                    input = input.Replace(kvp.Key, "");
                }
            }

            // Moves
            var moveCache = ShowdownTranslatorCache.GetMoveCache(sourceLang);
            for (int moveCount = 0; moveCount < 4; moveCount++)
            {
                int matchedMoveIndex = -1;
                string matchedMoveName = "";
                foreach (var kvp in moveCache)
                {
                    string pattern = "-" + kvp.Key;
                    if (input.Contains(pattern))
                    {
                        matchedMoveIndex = kvp.Value;
                        matchedMoveName = pattern;
                        break;
                    }
                }
                if (matchedMoveIndex >= 0)
                {
                    result += $"\n-{stringsEn.Move[matchedMoveIndex]}";
                    input = input.Replace(matchedMoveName, "");
                }
            }

            return result;
        }

        public static bool IsPS(string str) 
        {
            var enCache = ShowdownTranslatorCache.GetSpeciesCache(LanguageID.English);
            return enCache.Keys.Any(k => str.Contains(k, StringComparison.OrdinalIgnoreCase));
        }
    }
}
