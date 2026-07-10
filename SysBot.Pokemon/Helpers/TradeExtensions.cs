using PKHeX.Core;
using PKHeX.Core.AutoMod;
using SysBot.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using static SysBot.Pokemon.TradeSettings;

namespace SysBot.Pokemon.Helpers
{
    public abstract class TradeExtensions<T> where T : PKM, new()
    {
        public static readonly ushort[] ExplicitlyBlockedHeldItems = [534, 535];

        public static readonly string[] MarkTitle =
        [
            " The Peckish", " The Sleepy", " The Dozy", " The Early Riser", " The Cloud Watcher", " The Sodden",
            " The Thunderstruck", " The Snow Frolicker", " The Shivering", " The Parched", " The Sandswept",
            " The Mist Drifter", " The Chosen One", " The Catch of The Day", " The Curry Connoisseur",
            " The Sociable", " The Recluse", " The Rowdy", " The Spacey", " The Anxious", " The Giddy",
            " The Radiant", " The Serene", " The Feisty", " The Daydreamer", " The Joyful", " The Furious",
            " The Beaming", " The Teary-Eyed", " The Chipper", " The Grumpy", " The Scholar", " The Rampaging",
            " The Opportunist", " The Stern", " The Kindhearted", " The Easily Flustered", " The Driven",
            " The Apathetic", " The Arrogant", " The Reluctant", " The Humble", " The Pompous", " The Lively",
            " The Worn-Out", " Of The Distant Past", " The Twinkling Star", " The Paldea Champion", " The Great",
            " The Teeny", " The Treasure Hunter", " The Reliable Partner", " The Gourmet", " The One-in-a-Million",
            " The Former Alpha", " The Unrivaled", " The Former Titan",
        ];

        private static readonly HashSet<ushort> ShinyLockSet =
        [
            (ushort)Species.Victini, (ushort)Species.Keldeo, (ushort)Species.Volcanion, (ushort)Species.Cosmog,
            (ushort)Species.Cosmoem, (ushort)Species.Magearna, (ushort)Species.Marshadow, (ushort)Species.Eternatus,
            (ushort)Species.Kubfu, (ushort)Species.Urshifu, (ushort)Species.Zarude, (ushort)Species.Glastrier,
            (ushort)Species.Spectrier, (ushort)Species.Calyrex
        ];

        private static readonly Dictionary<string, byte> LanguageMap = new(StringComparer.OrdinalIgnoreCase)
        {
            { "japanese", 1 }, { "jpn", 1 }, { "ja", 1 },
            { "english", 2 }, { "eng", 2 }, { "en", 2 },
            { "french", 3 }, { "français", 3 }, { "fra", 3 }, { "fr", 3 },
            { "italian", 4 }, { "italiano", 4 }, { "ita", 4 }, { "it", 4 },
            { "german", 5 }, { "deutsch", 5 }, { "deu", 5 }, { "de", 5 },
            { "spanish", 7 }, { "español", 7 }, { "spa", 7 }, { "es", 7 },
            { "korean", 8 }, { "kor", 8 }, { "ko", 8 },
            { "chinese simplified", 9 }, { "中文简体", 9 }, { "chs", 9 }, { "zh-cn", 9 },
            { "chinese traditional", 10 }, { "中文繁體", 10 }, { "cht", 10 }, { "zh-tw", 10 },
            { "spanish-latam", 11 }, { "spanishl", 11 }, { "es-419", 11 }, { "latam", 11 }
        };

        public static T CherishHandler(MysteryGift mg, ITrainerInfo info)
        {
            var mgPkm = mg.ConvertToPKM(info);
            if (mgPkm == null) return new();

            if (EntityConverter.IsConvertibleToFormat(mgPkm, info.Generation))
                mgPkm = EntityConverter.ConvertToType(mgPkm, typeof(T), out var result) ?? mgPkm;

            var laTemp = new LegalityAnalysis(mgPkm);
            mgPkm.SetHandlerandMemory(info, laTemp.EncounterMatch);

            if (mgPkm.TID16 == 0 && mgPkm.SID16 == 0)
            {
                mgPkm.TID16 = info.TID16;
                mgPkm.SID16 = info.SID16;
            }

            mgPkm.CurrentLevel = mg.LevelMin;
            mgPkm.HeldItem = mgPkm.Species switch
            {
                (ushort)Species.Giratina when mgPkm.Form > 0 => 112,
                (ushort)Species.Silvally when mgPkm.Form > 0 => (ushort)(mgPkm.Form + 903),
                _ => 0
            };

            mgPkm = TrashBytes((T)mgPkm, laTemp);

            if (!new LegalityAnalysis(mgPkm).Valid)
            {
                mgPkm.SetRandomIVs(6);
                var pk = AutoLegalityWrapper.GetLegal(info, AutoLegalityWrapper.GetTemplate(new ShowdownSet(ShowdownParsing.GetShowdownText(mgPkm))), out _);
                if (pk != null)
                {
                    pk.SetAllTrainerData(info);
                    return (T)pk;
                }
            }
            return (T)mgPkm;
        }

        public static void DittoTrade(PKM pkm)
        {
            var nickname = pkm.Nickname.ToLower();
            pkm.StatAlignment = Nature.Random;
            pkm.MetLocation = pkm switch { PB8 => 400, PK9 => 28, _ => 162 };
            pkm.MetLevel = pkm switch { PB8 => 29, PK9 => 34, _ => pkm.MetLevel };
            
            if (pkm is PK9 pk9)
            {
                pk9.ObedienceLevel = pk9.MetLevel;
                pk9.TeraTypeOriginal = PKHeX.Core.MoveType.Normal;
                pk9.TeraTypeOverride = (PKHeX.Core.MoveType)19;
            }

            pkm.Ball = 21;
            pkm.IVs = [31, nickname.Contains("atk") ? 0 : 31, 31, nickname.Contains("spe") ? 0 : 31, nickname.Contains("spa") ? 0 : 31, 31];
            TrashBytes(pkm, new LegalityAnalysis(pkm));
        }

        public static string FormOutput(ushort species, byte form, out string[] formString)
        {
            var strings = GameInfo.GetStrings("en");
            formString = FormConverter.GetFormList(species, strings.Types, strings.forms, GameInfo.GenderSymbolASCII, typeof(T) == typeof(PK8) ? EntityContext.Gen8 : EntityContext.Gen4);
            if (formString.Length == 0) return string.Empty;
            
            var f = form < formString.Length ? form : (byte)(formString.Length - 1);
            var result = formString[f];
            if (string.IsNullOrEmpty(result) || result == "0") return string.Empty;
            return result.Contains('-') ? result : $"-{result}";
        }

        public static bool HasAdName(T pk, out string ad)
        {
            const string domainPattern = @"(?<=\w)\.(com|org|net|gg|xyz|io|tv|co|me|us|uk|ca|de|fr|jp|au|eu|ch|it|nl|ru|br|in|fun|info|blog|int|gov|edu|app|art|biz|bot|buzz|dev|eco|fan|fans|forum|free|game|help|host|inc|icu|live|lol|market|media|news|ninja|now|one|ong|online|page|porn|pro|red|sale|sex|sexy|shop|site|store|stream|tech|tel|top|tube|uno|vip|website|wiki|work|world|wtf|xxx|zero|youtube|zone|nyc|onion|bit|crypto|meme)\b";
            bool ot = Regex.IsMatch(pk.OriginalTrainerName, domainPattern, RegexOptions.IgnoreCase);
            bool nick = Regex.IsMatch(pk.Nickname, domainPattern, RegexOptions.IgnoreCase);
            ad = ot ? pk.OriginalTrainerName : nick ? pk.Nickname : "";
            return ot || nick;
        }

        public static bool HasMark(IRibbonIndex pk, out RibbonIndex result, out string markTitle)
        {
            result = default;
            markTitle = string.Empty;
            if (pk is IRibbonSetMark9 rsm)
            {
                if (rsm.RibbonMarkMightiest) { result = RibbonIndex.MarkMightiest; markTitle = " The Unrivaled"; return true; }
                if (rsm.RibbonMarkAlpha) { result = RibbonIndex.MarkAlpha; markTitle = pk is PA9 pa9 ? (new LegalityAnalysis(pa9).EncounterOriginal.Context == pa9.Context ? " The Alpha" : " The Former Alpha") : " The Former Alpha"; return true; }
                if (rsm.RibbonMarkTitan) { result = RibbonIndex.MarkTitan; markTitle = " The Former Titan"; return true; }
                if (rsm.RibbonMarkJumbo) { result = RibbonIndex.MarkJumbo; markTitle = " The Great"; return true; }
                if (rsm.RibbonMarkMini) { result = RibbonIndex.MarkMini; markTitle = " The Teeny"; return true; }
            }
            for (var mark = RibbonIndex.MarkLunchtime; mark <= RibbonIndex.MarkSlump; mark++)
            {
                if (pk.GetRibbon((int)mark)) { result = mark; markTitle = MarkTitle[(int)mark - (int)RibbonIndex.MarkLunchtime]; return true; }
            }
            return false;
        }

        public static string PokeImg(PKM pkm, bool canGmax, bool fullSize, ImageSize? preferredImageSize = null)
        {
            if (pkm.IsEgg)
            {
                return "https://raw.githubusercontent.com/NexusRisen/Nexus-Risen-Edition-Sprite-Images/main/Assets/Eggs/egg.png";
            }

            canGmax = canGmax || (pkm is IGigantamax g && g.CanGigantamax);

            string shinyFolder = pkm.IsShiny ? "Shiny" : "Non-Shiny";
            var strings = GameInfo.GetStrings("en");
            string speciesName = strings.Species[pkm.Species];
            string rangeFolder = GetAlphabeticalRange(speciesName);

            string baseSpecies = speciesName.ToLower()
                .Replace(" ", "-")
                .Replace(":", "_")
                .Replace("é", "e");

            // Special cases for species names
            if (pkm.Species == (ushort)Species.NidoranF) baseSpecies = "nidoran-f";
            if (pkm.Species == (ushort)Species.NidoranM) baseSpecies = "nidoran-m";

            string formSuffix = "";
            if (canGmax)
            {
                if (pkm.Species == (ushort)Species.Urshifu && pkm.Form == 1)
                    formSuffix = "-rapid-strike-Gigantamax";
                else
                    formSuffix = "-Gigantamax";
            }
            else if (pkm.Form > 0)
            {
                string f = ShowdownParsing.GetStringFromForm(pkm.Form, strings, pkm.Species, pkm.Context) ?? string.Empty;
                if (!string.IsNullOrEmpty(f) && f != "0" && !f.Equals("Normal", StringComparison.OrdinalIgnoreCase))
                {
                    f = f.ToLower().Replace(" ", "-");
                    f = f.Replace("alolan", "alola")
                         .Replace("galarian", "galar")
                         .Replace("hisuian", "hisui")
                         .Replace("paldean", "paldea");

                    // Specific sprite repository exceptions
                    string[] missingRepoForms = {
                        "aegislash", "calyrex", "cramorant", "eiscue", "furfrou",
                        "kyurem", "meloetta", "mimikyu", "morpeko", "necrozma",
                        "zacian", "zamazenta", "scatterbug", "spewpa", "mothim", "wishiwashi"
                    };
                    if (missingRepoForms.Contains(baseSpecies)) f = "";
                    
                    if (baseSpecies == "alcremie" && f != "" && f != "gigantamax") f += "-strawberry";
                    if (baseSpecies == "minior" && f != "") {
                        if (f == "meteor") f = "";
                        else f = "c-" + f;
                    }
                    
                    if (baseSpecies == "ogerpon" && f.StartsWith("*")) f = "";
                    if (baseSpecies == "pikachu" && f == "starter") f = "";
                    if (f.StartsWith("mega")) f = "";
                    
                    if ((baseSpecies == "pumpkaboo" || baseSpecies == "gourgeist") && f == "jumbo") f = "super";
                    
                    if (baseSpecies == "basculin" && f == "white-striped") f = "white";
                    if (baseSpecies == "terapagos" && (f == "terastal" || f == "stellar")) f = "";
                    if (baseSpecies == "palafin" && f == "hero") f = "";
                    if (baseSpecies == "zygarde" && f == "complete" && pkm.IsShiny) f = "100";
                    
                    if (!string.IsNullOrEmpty(f))
                        formSuffix = "-" + f;
                    
                    // Tatsugiri Shiny Droopy repository typo workaround
                    if (baseSpecies == "tatsugiri" && f == "droopy" && pkm.IsShiny)
                    {
                        baseSpecies = "tatsugiridroopy";
                        formSuffix = "";
                    }
                }
            }

            return $"https://raw.githubusercontent.com/NexusRisen/Nexus-Risen-Edition-Sprite-Images/main/{shinyFolder}/PNG/{rangeFolder}/{baseSpecies}{formSuffix}.png";
        }

        private static string GetAlphabeticalRange(string speciesName)
        {
            if (string.IsNullOrEmpty(speciesName)) return "A-G";
            char firstChar = char.ToUpper(speciesName[0]);
            if (firstChar >= 'A' && firstChar <= 'G') return "A-G";
            if (firstChar >= 'H' && firstChar <= 'N') return "H-N";
            if (firstChar >= 'O' && firstChar <= 'T') return "O-T";
            if (firstChar >= 'U' && firstChar <= 'Z') return "U-Z";
            return "A-G";
        }

        public static bool ShinyLockCheck(ushort species, string form, string ball = "")
        {
            if (ShinyLockSet.Contains(species)) return true;
            if (!string.IsNullOrEmpty(form) && (species is (ushort)Species.Zapdos or (ushort)Species.Moltres or (ushort)Species.Articuno)) return true;
            if (ball.Contains("Beast") && (species is (ushort)Species.Poipole or (ushort)Species.Naganadel)) return true;
            if (typeof(T) == typeof(PB8) && (species is (ushort)Species.Manaphy or (ushort)Species.Mew or (ushort)Species.Jirachi)) return true;
            if (species == (ushort)Species.Pikachu && !string.IsNullOrEmpty(form) && form != "-Partner") return true;
            if ((species is (ushort)Species.Zacian or (ushort)Species.Zamazenta) && !ball.Contains("Cherish")) return true;
            return false;
        }

        public static PKM TrashBytes(PKM pkm, LegalityAnalysis? la = null)
        {
            var result = (T)pkm.Clone();
            if (result.Version != GameVersion.GO) result.MetDate = DateOnly.FromDateTime(DateTime.Now);
            if (la?.Valid == true) { var withNickname = (T)result.Clone(); withNickname.IsNicknamed = true; withNickname.Nickname = "UwU"; withNickname.SetDefaultNickname(la); result = withNickname; }
            return result;
        }

        public static bool IsEggCheck(string showdownSet)
        {
            var firstLine = showdownSet.Split('\n').FirstOrDefault();
            if (string.IsNullOrWhiteSpace(firstLine)) return false;
            int atIdx = firstLine.IndexOf('@');
            if (atIdx > 0) firstLine = firstLine[..atIdx].Trim();
            return firstLine.Split([' ', '('], StringSplitOptions.RemoveEmptyEntries).Any(w => string.Equals(w, "Egg", StringComparison.OrdinalIgnoreCase));
        }

        public static byte DetectShowdownLanguage(string content)
        {
            var batchMatch = Regex.Match(content, @"\.Language=(\d+)");
            if (batchMatch.Success && byte.TryParse(batchMatch.Groups[1].Value, out byte langCode)) return langCode is >= 1 and <= 11 ? langCode : (byte)2;
            
            var lines = content.Split('\n');
            foreach (var line in lines)
            {
                if (line.StartsWith("Language", StringComparison.OrdinalIgnoreCase))
                {
                    var lang = line[(line.IndexOf(':') + 1)..].Trim();
                    if (LanguageMap.TryGetValue(lang, out var code)) return code;
                    return 2;
                }
            }

            if (content.Contains("Talent") || content.Contains("Bonheur") || content.Contains("Chromatique")) return 3; // French
            if (content.Contains("Abilità") || content.Contains("Natura") || content.Contains("Amicizia")) return 4; // Italian
            if (content.Contains("Fähigkeit") || content.Contains("Wesen") || content.Contains("Freundschaft")) return 5; // German
            if (content.Contains("Habilidad") || content.Contains("Naturaleza") || content.Contains("Felicidad")) return 7; // Spanish
            if (content.Contains("特性") || content.Contains("性格") || content.Contains("なつき度")) return 1; // Japanese
            if (content.Contains("특성") || content.Contains("성격") || content.Contains("친밀도")) return 8; // Korean
            if (content.Contains("太晶属性") || content.Contains("异色")) return 9; // Simplified
            if (content.Contains("太晶屬性") || content.Contains("發光寶")) return 10; // Traditional
            return 0;
        }

        public static bool IsItemBlocked(PKM pkm)
        {
            var held = pkm.HeldItem;
            if (held <= 0) return false;
            if (pkm.Context == EntityContext.Gen9a && ExplicitlyBlockedHeldItems.Contains((ushort)held)) 
            { 
                pkm.HeldItem = 796; 
                LogUtil.LogInfo($"Replaced Illegal item '{GameInfo.Strings.Item[held]}' with '{GameInfo.Strings.Item[796]}' for {GameInfo.Strings.Species[pkm.Species]}", "BlockItem"); 
            }
            return !ItemRestrictions.IsHeldItemAllowed(held, pkm.Context) || TradeRestrictions.IsUntradableHeld(pkm.Context, held);
        }
    }
}
