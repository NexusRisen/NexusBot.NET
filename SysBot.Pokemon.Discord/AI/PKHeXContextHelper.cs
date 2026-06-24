using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PKHeX.Core;
using PKHeX.Core.AutoMod;

namespace SysBot.Pokemon.Discord.AI;

public static class PKHeXContextHelper
{
    public static string GetLegalityContext(string userRequest)
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
            context.AppendLine("### RELEVANT POKÃ‰MON DATA (FROM PKHEX.CORE)");
            foreach (var species in speciesList)
            {
                var info = GetSpeciesInfo(species);
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

    private static string GetSpeciesInfo(ushort speciesId)
    {
        try
        {
            var sb = new StringBuilder();
            var name = GameInfo.Strings.Species[speciesId];
            sb.AppendLine($"PokÃ©mon: {name}");

            // Accessing PersonalTable for Gen 9
            var personal = PersonalTable.SV[speciesId];
            if (personal != null)
            {
                sb.Append("Abilities: ");
                var abilities = new List<string>();
                if (personal.Ability1 != 0 && personal.Ability1 < GameInfo.Strings.Ability.Count) 
                    abilities.Add(GameInfo.Strings.Ability[personal.Ability1]);
                if (personal.Ability2 != 0 && personal.Ability2 < GameInfo.Strings.Ability.Count) 
                    abilities.Add(GameInfo.Strings.Ability[personal.Ability2]);
                if (personal.AbilityH != 0 && personal.AbilityH < GameInfo.Strings.Ability.Count) 
                    abilities.Add(GameInfo.Strings.Ability[personal.AbilityH] + " (Hidden)");
                
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
