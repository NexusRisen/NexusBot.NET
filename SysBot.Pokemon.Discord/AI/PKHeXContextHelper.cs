using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PKHeX.Core;
using PKHeX.Core.AutoMod;

namespace SysBot.Pokemon.Discord.AI;

public static class PKHeXContextHelper
{
    public static string GetLegalityContext(string userRequest, string gameName)
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
                var info = GetSpeciesInfo(species, gameName);
                if (string.IsNullOrEmpty(info))
                    continue;

                context.AppendLine(info);
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
            sb.AppendLine($"Legality Note: Ensure all moves and the Pokeball are legal for {name} in the requested game.");

            return sb.ToString();
        }
        catch
        {
            return string.Empty;
        }
    }
}
