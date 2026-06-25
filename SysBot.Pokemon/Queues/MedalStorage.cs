using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using SysBot.Base;

namespace SysBot.Pokemon;

public class MedalStorage
{
    private static readonly string FileName = Path.Combine("data", "medals.json");
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        WriteIndented = true
    };
    
    private Dictionary<ulong, MedalDetails>? _medals;

    public MedalStorage()
    {
        if (!Directory.Exists("data"))
            Directory.CreateDirectory("data");

        LoadFromFile();
    }

    public void AddTrade(ulong trainerID, string username)
    {
        LoadFromFile();
        if (!_medals!.TryGetValue(trainerID, out var details))
        {
            details = new MedalDetails { Username = username, TradeCount = 0 };
            _medals[trainerID] = details;
        }

        details.TradeCount++;
        details.Username = username;
        details.Medals = CalculateMedals(details.TradeCount);
        SaveToFile();
    }

    public int GetTradeCount(ulong trainerID)
    {
        LoadFromFile();
        return _medals!.TryGetValue(trainerID, out var details) ? details.TradeCount : 0;
    }

    private static int CalculateMedals(int tradeCount)
    {
        if (tradeCount < 1) return 0;
        return 1 + Math.Min(20, tradeCount / 50);
    }

    private void LoadFromFile()
    {
        if (File.Exists(FileName))
            _medals = JsonSerializer.Deserialize<Dictionary<ulong, MedalDetails>>(File.ReadAllText(FileName), SerializerOptions) ?? new Dictionary<ulong, MedalDetails>();
        else
            _medals = new Dictionary<ulong, MedalDetails>();
    }

    private void SaveToFile()
    {
        try { File.WriteAllText(FileName, JsonSerializer.Serialize(_medals, SerializerOptions)); }
        catch (Exception ex) { LogUtil.LogInfo("MedalStorage", $"Error: {ex.Message}"); }
    }

    public class MedalDetails
    {
        public string? Username { get; set; }
        public int TradeCount { get; set; }
        public int Medals { get; set; }
    }
}
