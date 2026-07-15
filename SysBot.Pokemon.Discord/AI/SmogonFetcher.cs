using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace SysBot.Pokemon.Discord.AI;

public static class SmogonFetcher
{
    private static readonly HttpClient _httpClient = new();

    public static async Task<string> GetShowdownSetAsync(string speciesName, string gameName)
    {
        try
        {
            // Determine format based on game
            string format = GetSmogonFormat(gameName);
            string url = $"https://pkmn.github.io/smogon/data/sets/{format}.json";

            var response = await _httpClient.GetAsync(url);
            if (!response.IsSuccessStatusCode)
            {
                return string.Empty;
            }

            var jsonString = await response.Content.ReadAsStringAsync();
            var json = JObject.Parse(jsonString);

            // Capitalize first letter of species (or handle specific formatting if needed)
            var speciesKey = json.Properties().FirstOrDefault(p => p.Name.Equals(speciesName, StringComparison.OrdinalIgnoreCase))?.Name;
            
            if (speciesKey == null)
                return string.Empty;

            var sets = json[speciesKey] as JObject;
            if (sets == null || !sets.HasValues)
                return string.Empty;

            // Get the first available set
            var firstSetProp = sets.Properties().FirstOrDefault();
            if (firstSetProp == null)
                return string.Empty;

            var setDetails = firstSetProp.Value as JObject;
            if (setDetails == null)
                return string.Empty;

            return GenerateShowdownText(speciesKey, setDetails);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error fetching Smogon set for {speciesName}: {ex.Message}");
            return string.Empty;
        }
    }

    private static string GetSmogonFormat(string gameName)
    {
        return gameName switch
        {
            "Sword and Shield" => "gen8ou",
            "Brilliant Diamond and Shining Pearl" => "gen8bdspou",
            "Legends: Arceus" => "gen8ou", // PLA doesn't have standard Smogon competitive sets, fallback to gen8
            "Let's Go Pikachu and Eevee" => "gen7letsgou",
            "Ultra Sun and Ultra Moon" => "gen7ou",
            "Legends Z-A" => "gen9ou", // Fallback to Gen 9 OU until PLZA sets exist
            _ => "gen9ou"
        };
    }

    private static string GenerateShowdownText(string species, JObject setDetails)
    {
        var sb = new StringBuilder();

        // Species @ Item
        string item = GetFirstString(setDetails["item"]);
        if (!string.IsNullOrEmpty(item))
            sb.AppendLine($"{species} @ {item}");
        else
            sb.AppendLine(species);

        // Ability
        string ability = GetFirstString(setDetails["ability"]);
        if (!string.IsNullOrEmpty(ability))
            sb.AppendLine($"Ability: {ability}");

        // Level (default 100 if missing)
        int level = setDetails["level"]?.Value<int>() ?? 100;
        sb.AppendLine($"Level: {level}");

        // Tera Type (Gen 9)
        string teraType = GetFirstString(setDetails["teratypes"]);
        if (!string.IsNullOrEmpty(teraType))
            sb.AppendLine($"Tera Type: {teraType}");

        // EVs
        var evs = setDetails["evs"] as JObject;
        if (evs == null && setDetails["evs"] is JArray evArray && evArray.Count > 0)
        {
            evs = evArray[0] as JObject;
        }
        
        if (evs != null && evs.HasValues)
        {
            var evList = new List<string>();
            foreach (var ev in evs.Properties())
            {
                string stat = MapStatName(ev.Name);
                evList.Add($"{ev.Value} {stat}");
            }
            if (evList.Count > 0)
                sb.AppendLine($"EVs: {string.Join(" / ", evList)}");
        }

        // Nature
        string nature = GetFirstString(setDetails["nature"]);
        if (!string.IsNullOrEmpty(nature))
            sb.AppendLine($"{nature} Nature");

        // IVs
        var ivs = setDetails["ivs"] as JObject;
        if (ivs != null && ivs.HasValues)
        {
            var ivList = new List<string>();
            foreach (var iv in ivs.Properties())
            {
                string stat = MapStatName(iv.Name);
                ivList.Add($"{iv.Value} {stat}");
            }
            if (ivList.Count > 0)
                sb.AppendLine($"IVs: {string.Join(" / ", ivList)}");
        }

        // Moves
        var moves = setDetails["moves"] as JArray;
        if (moves != null)
        {
            foreach (var move in moves)
            {
                string moveName = GetFirstString(move);
                if (!string.IsNullOrEmpty(moveName))
                    sb.AppendLine($"- {moveName}");
            }
        }

        return sb.ToString().Trim();
    }

    private static string GetFirstString(JToken? token)
    {
        if (token == null) return string.Empty;
        if (token.Type == JTokenType.String) return token.Value<string>() ?? string.Empty;
        if (token.Type == JTokenType.Array && token.HasValues)
        {
            return GetFirstString(token.First);
        }
        return string.Empty;
    }

    private static string MapStatName(string stat)
    {
        return stat.ToLower() switch
        {
            "hp" => "HP",
            "atk" => "Atk",
            "def" => "Def",
            "spa" => "SpA",
            "spd" => "SpD",
            "spe" => "Spe",
            _ => stat.ToUpper()
        };
    }
}
