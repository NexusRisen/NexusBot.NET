using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PKHeX.Core;
using PKHeX.Core.AutoMod;

namespace SysBot.Pokemon.Discord.AI;

public static class PKHeXContextHelper
{
    public static async Task<string> GetLegalityContextAsync(string userRequest, string gameName)
    {
        var context = new StringBuilder();
        
        context.AppendLine("### PKHEX & AUTOLEGALITY (ALM) DOCUMENTATION");
        context.AppendLine("You are integrated with PKHeX.Core and AutoLegality Mod (ALM).");
        context.AppendLine("ALM can automatically legalize any Pokemon if you provide a valid Showdown set.");
        context.AppendLine("To force specific attributes that might be illegal otherwise, use Overrides:");
        context.AppendLine("- Use `~` for RegenTemplate overrides (e.g., `~Level: 50`, `~Shiny: Yes`, `~TeraType: Stellar`).");
        context.AppendLine("- Use `.` for Batch commands (e.g., `.OT=NexusBot`, `.TID=123456`).");
        context.AppendLine("- Example: `~Ball: Luxury Ball` ensures the Pokemon is in a Luxury Ball.");
        context.AppendLine("");

        var speciesList = DetectSpecies(userRequest);
        if (speciesList.Count > 0)
        {
            context.AppendLine($"### RELEVANT POKÉMON DATA (FROM PKHEX.CORE) - {gameName.ToUpper()}");
            foreach (var species in speciesList)
            {
                var name = GameInfo.Strings.Species[species];
                var info = GetSpeciesInfo(species, gameName);
                if (string.IsNullOrEmpty(info))
                    continue;

                context.AppendLine(info);

                // Fetch Smogon set online
                string smogonSet = await SmogonFetcher.GetShowdownSetAsync(name, gameName);
                if (!string.IsNullOrEmpty(smogonSet))
                {
                    context.AppendLine($"### OFFICIAL COMPETITIVE SMOGON SET FOR {name.ToUpper()}");
                    context.AppendLine($"CRITICAL RULE: The user wants a competitive/battle-ready {name}. You MUST use the following exact Showdown set (do not invent illegal moves or abilities):");
                    context.AppendLine("[SHOWDOWN]");
                    context.AppendLine(smogonSet);
                    context.AppendLine("[/SHOWDOWN]");
                }

                context.AppendLine("---");
            }
        }

        return context.ToString();
    }

    private static List<ushort> DetectSpecies(string text)
    {
        var found = new List<ushort>();
        var words = text.Split(new[] { ' ', ',', '.', '!', '?', '\n', '\r', '\t' }, StringSplitOptions.RemoveEmptyEntries);
        var speciesNames = GameInfo.Strings.Species;

        foreach (var word in words)
        {
            if (word.Length < 3) continue;

            // Simple search for species name in the words
            for (int i = 1; i < speciesNames.Count; i++)
            {
                if (string.Equals(speciesNames[i], word, StringComparison.OrdinalIgnoreCase))
                {
                    found.Add((ushort)i);
                    break;
                }
            }
        }

        return found.Distinct().ToList();
    }

    public static string GetGameName(string text, string defaultGame)
    {
        var lower = text.ToLowerInvariant();
        if (lower.Contains("sword") || lower.Contains("shield") || lower.Contains("swsh") || lower.Contains("gen 8") || lower.Contains("gen8"))
            return "Sword and Shield";
        if (lower.Contains("brilliant diamond") || lower.Contains("shining pearl") || lower.Contains("bdsp"))
            return "Brilliant Diamond and Shining Pearl";
        if (lower.Contains("arceus") || lower.Contains(" pla ") || lower.Contains(" la ") || lower.Contains("legends"))
            return "Legends: Arceus";
        if (lower.Contains("let's go") || lower.Contains("lgpe"))
            return "Let's Go Pikachu and Eevee";
        if (lower.Contains("sun") || lower.Contains("moon") || lower.Contains("usum") || lower.Contains("sm") || lower.Contains("gen 7"))
            return "Ultra Sun and Ultra Moon";
        
        return defaultGame;
    }

    private static IPersonalTable GetPersonalTable(string gameName) => gameName switch
    {
        "Sword and Shield" => PersonalTable.SWSH,
        "Brilliant Diamond and Shining Pearl" => PersonalTable.BDSP,
        "Legends: Arceus" => PersonalTable.LA,
        "Let's Go Pikachu and Eevee" => PersonalTable.GG,
        "Ultra Sun and Ultra Moon" => PersonalTable.USUM,
        _ => PersonalTable.SV
    };

    private static GameVersion GetGameVersion(string gameName) => gameName switch
    {
        "Sword and Shield" => GameVersion.SWSH,
        "Brilliant Diamond and Shining Pearl" => GameVersion.BDSP,
        "Legends: Arceus" => GameVersion.PLA,
        "Let's Go Pikachu and Eevee" => GameVersion.GG,
        "Ultra Sun and Ultra Moon" => GameVersion.USUM,
        _ => GameVersion.SV
    };

    private static string GetSpeciesInfo(ushort speciesId, string gameName)
    {
        try
        {
            var sb = new StringBuilder();
            var name = GameInfo.Strings.Species[speciesId];
            sb.AppendLine($"Pokémon: {name}");

            // Accessing PersonalTable for requested game
            var personalTable = GetPersonalTable(gameName);
            var personal = personalTable.GetFormEntry(speciesId, 0);
            if (personal is IPersonalAbility12 piAbility)
            {
                sb.Append("Abilities: ");
                var abilities = new List<string>();
                if (piAbility.Ability1 != 0 && piAbility.Ability1 < GameInfo.Strings.Ability.Count) 
                    abilities.Add(GameInfo.Strings.Ability[piAbility.Ability1]);
                if (piAbility.Ability2 != 0 && piAbility.Ability2 < GameInfo.Strings.Ability.Count) 
                    abilities.Add(GameInfo.Strings.Ability[piAbility.Ability2]);
                if (personal is IPersonalAbility12H piH && piH.AbilityH != 0 && piH.AbilityH < GameInfo.Strings.Ability.Count) 
                    abilities.Add(GameInfo.Strings.Ability[piH.AbilityH] + " (Hidden)");
                
                sb.AppendLine(string.Join(", ", abilities.Distinct()));
            }

            // We can't easily get the full movepool without a specific method, 
            // but we can tell the AI to prioritize legal moves for this species.
            sb.AppendLine($"CRITICAL RULE: You MUST use one of the Abilities listed above. Do NOT invent, hallucinate, or use any other Ability.");
            sb.AppendLine($"CRITICAL RULE: NEVER use made-up PokeBalls like 'Choice Ball'. ONLY use valid standard balls (e.g., Poke Ball, Great Ball, Ultra Ball, Master Ball, Premier Ball, Luxury Ball, Beast Ball, Apricorn Balls).");
            sb.AppendLine($"CRITICAL RULE: Only use extremely common or universally legal moves for this species (e.g., standard level-up or highly common competitive TMs/TRs). Do NOT guess random moves (like Scald on Bagon) or the set will fail legality checks.");
            sb.AppendLine($"Legality Note: Ensure all moves and the Pokeball are legal for {name} in {gameName}.");

            var gameVersion = GetGameVersion(gameName);
            var ls = PKHeX.Core.GameData.GetLearnSource(gameVersion);
            var learnset = ls.GetLearnset((ushort)speciesId, 0);
            var rawMoves = learnset.GetAllMoves().ToArray();
            var validMoves = rawMoves
                .Select(m => GameInfo.Strings.Move[m])
                .Where(x => !string.IsNullOrEmpty(x))
                .Distinct()
                .ToList();

            if (validMoves.Count > 0)
            {
                var movesStr = string.Join(", ", validMoves);
                sb.AppendLine($"LEGAL MOVES WARNING: The following are known level-up moves for {name} in {gameName}: {movesStr}.");
                sb.AppendLine($"CRITICAL RULE: If you add TM/TR/Egg moves not listed here, you MUST be absolutely certain they are legal for {name} in {gameName}. (e.g. Do not give Scald to Bagon in Gen 9).");
            }
            else
            {
                sb.AppendLine($"CRITICAL WARNING: PKHeX data shows NO known level-up moves for {name} in {gameName}. This usually means the Pokemon is NOT AVAILABLE in {gameName} (or the game hasn't been released yet, e.g. Legends Z-A). Proceed with caution and provide a standard Gen 9 / SV set if you must.");
            }

            return sb.ToString();
        }
        catch
        {
            return string.Empty;
        }
    }
}
