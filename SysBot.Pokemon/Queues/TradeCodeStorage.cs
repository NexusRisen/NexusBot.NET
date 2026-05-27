using SysBot.Base;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using PKHeX.Core;

namespace SysBot.Pokemon;

public class TradeCodeStorage
{
    private const string FileName = "tradecodes.json";
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        WriteIndented = true
    };
    private Dictionary<ulong, TradeCodeDetails>? _tradeCodeDetails;
    
    public TradeCodeStorage()
    {
        if (!DatabaseService.UseRemoteDb)
            LoadFromFile();
    }

    public bool DeleteTradeCode(ulong trainerID)
    {
        if (DatabaseService.UseRemoteDb)
        {
            return DatabaseService.DeleteUser(trainerID);
        }

        LoadFromFile();
        if (_tradeCodeDetails!.Remove(trainerID))
        {
            SaveToFile();
            return true;
        }
        return false;
    }

    public int GetTradeCode(ulong trainerID)
    {
        var game = GetCurrentGame();
        if (DatabaseService.UseRemoteDb)
        {
            var user = DatabaseService.GetUser(trainerID);
            if (user != null)
            {
                user.TradeCount++;
                user.Medals = CalculateMedals(user.TradeCount);
                
                var existingCode = GetCodeForGame(user, game);
                if (existingCode != null && int.TryParse(existingCode, out int codeInt))
                {
                    DatabaseService.SaveUser(trainerID, user);
                    return codeInt;
                }

                var newCode = GenerateRandomTradeCode();
                SetCodeForGame(user, game, newCode.ToString());
                DatabaseService.SaveUser(trainerID, user);
                return newCode;
            }
            
            var initialCode = GenerateRandomTradeCode();
            var newUser = new TradeCodeDetails { TradeCount = 1, Medals = 1 };
            SetCodeForGame(newUser, game, initialCode.ToString());
            DatabaseService.SaveUser(trainerID, newUser);
            return initialCode;
        }

        LoadFromFile();
        if (_tradeCodeDetails!.TryGetValue(trainerID, out var details))
        {
            details.TradeCount++;
            details.Medals = CalculateMedals(details.TradeCount);
            
            var existingCode = GetCodeForGame(details, game);
            if (existingCode != null && int.TryParse(existingCode, out int codeInt))
            {
                SaveToFile();
                return codeInt;
            }

            var newCode = GenerateRandomTradeCode();
            SetCodeForGame(details, game, newCode.ToString());
            SaveToFile();
            return newCode;
        }

        var localCode = GenerateRandomTradeCode();
        var newDetails = new TradeCodeDetails { TradeCount = 1, Medals = 1 };
        SetCodeForGame(newDetails, game, localCode.ToString());
        _tradeCodeDetails![trainerID] = newDetails;
        SaveToFile();
        return localCode;
    }

    public List<Pictocodes> GetLGTradeCode(ulong trainerID)
    {
        if (DatabaseService.UseRemoteDb)
        {
            var user = DatabaseService.GetUser(trainerID);
            if (user != null)
            {
                user.TradeCount++;
                user.Medals = CalculateMedals(user.TradeCount);
                
                if (!string.IsNullOrEmpty(user.Code_LGPE))
                {
                    DatabaseService.SaveUser(trainerID, user);
                    return StringToLGCode(user.Code_LGPE);
                }
                
                var newLg = GenerateRandomLGCode();
                user.Code_LGPE = LGCodeToString(newLg);
                DatabaseService.SaveUser(trainerID, user);
                return newLg;
            }
            
            var initialLg = GenerateRandomLGCode();
            var newUser = new TradeCodeDetails { TradeCount = 1, Medals = 1, Code_LGPE = LGCodeToString(initialLg) };
            DatabaseService.SaveUser(trainerID, newUser);
            return initialLg;
        }

        LoadFromFile();
        if (_tradeCodeDetails!.TryGetValue(trainerID, out var details))
        {
            details.TradeCount++;
            details.Medals = CalculateMedals(details.TradeCount);
            
            if (!string.IsNullOrEmpty(details.Code_LGPE))
            {
                SaveToFile();
                return StringToLGCode(details.Code_LGPE);
            }
            
            var newLg = GenerateRandomLGCode();
            details.Code_LGPE = LGCodeToString(newLg);
            SaveToFile();
            return newLg;
        }

        var localLg = GenerateRandomLGCode();
        var newDetails = new TradeCodeDetails { TradeCount = 1, Medals = 1, Code_LGPE = LGCodeToString(localLg) };
        _tradeCodeDetails![trainerID] = newDetails;
        SaveToFile();
        return localLg;
    }

    private static string GetCurrentGame()
    {
        // Try to identify the current bot's generation/game via type inspection or system config
        // This maps the active Bot implementation to a database column identifier
        var typeName = typeof(PKM).AssemblyQualifiedName ?? "";
        if (typeName.Contains("PK9")) return "SV";
        if (typeName.Contains("PA9")) return "PLZA";
        if (typeName.Contains("PK8")) return "SWSH";
        if (typeName.Contains("PB8")) return "BDSP";
        if (typeName.Contains("PA8")) return "LA";
        if (typeName.Contains("PB7")) return "LGPE";
        return "SV"; // Default to newest
    }

    private string? GetCodeForGame(TradeCodeDetails details, string game) => game switch
    {
        "SV" => details.Code_SV,
        "SWSH" => details.Code_SWSH,
        "BDSP" => details.Code_BDSP,
        "LA" => details.Code_LA,
        "PLZA" => details.Code_PLZA,
        "LGPE" => details.Code_LGPE,
        _ => details.Code_SV
    };

    private void SetCodeForGame(TradeCodeDetails details, string game, string code)
    {
        switch (game)
        {
            case "SV": details.Code_SV = code; break;
            case "SWSH": details.Code_SWSH = code; break;
            case "BDSP": details.Code_BDSP = code; break;
            case "LA": details.Code_LA = code; break;
            case "PLZA": details.Code_PLZA = code; break;
            case "LGPE": details.Code_LGPE = code; break;
            default: details.Code_SV = code; break;
        }
    }

    private static int CalculateMedals(int tradeCount)
    {
        if (tradeCount < 1) return 0;
        return 1 + Math.Min(20, tradeCount / 50);
    }

    public int GetTradeCount(ulong trainerID)
    {
        if (DatabaseService.UseRemoteDb) return DatabaseService.GetUser(trainerID)?.TradeCount ?? 0;
        LoadFromFile();
        return _tradeCodeDetails!.TryGetValue(trainerID, out var details) ? details.TradeCount : 0;
    }

    public TradeCodeDetails? GetTradeDetails(ulong trainerID)
    {
        if (DatabaseService.UseRemoteDb) return DatabaseService.GetUser(trainerID);
        LoadFromFile();
        return _tradeCodeDetails!.TryGetValue(trainerID, out var details) ? details : null;
    }

    public void UpdateTradeDetails(ulong trainerID, string ot, int tid, int sid)
    {
        if (DatabaseService.UseRemoteDb)
        {
            var user = DatabaseService.GetUser(trainerID);
            if (user != null) { user.OT = ot; user.TID = tid; user.SID = sid; DatabaseService.SaveUser(trainerID, user); }
            return;
        }
        LoadFromFile();
        if (_tradeCodeDetails!.TryGetValue(trainerID, out var details)) { details.OT = ot; details.TID = tid; details.SID = sid; SaveToFile(); }
    }

    public void UpdateTradeDetails(ulong trainerID, string ot, int tid, int sid, byte? gender = null, int? language = null)
    {
        if (DatabaseService.UseRemoteDb)
        {
            var user = DatabaseService.GetUser(trainerID);
            if (user != null) { user.OT = ot; user.TID = tid; user.SID = sid; if (gender.HasValue) user.Gender = gender; if (language.HasValue) user.Language = language; DatabaseService.SaveUser(trainerID, user); }
            return;
        }
        LoadFromFile();
        if (_tradeCodeDetails!.TryGetValue(trainerID, out var details)) { details.OT = ot; details.TID = tid; details.SID = sid; if (gender.HasValue) details.Gender = gender; if (language.HasValue) details.Language = language; SaveToFile(); }
    }

    public void UpdateTradeDetails(ulong trainerID, string ot, int tid, int sid, string? quote = null, byte? gender = null, int? language = null)
    {
        if (DatabaseService.UseRemoteDb)
        {
            var user = DatabaseService.GetUser(trainerID);
            if (user != null) { user.OT = ot; user.TID = tid; user.SID = sid; if (quote != null) user.Quote = quote; if (gender.HasValue) user.Gender = gender; if (language.HasValue) user.Language = language; DatabaseService.SaveUser(trainerID, user); }
            return;
        }
        LoadFromFile();
        if (_tradeCodeDetails!.TryGetValue(trainerID, out var details)) { details.OT = ot; details.TID = tid; details.SID = sid; if (quote != null) details.Quote = quote; if (gender.HasValue) details.Gender = gender; if (language.HasValue) details.Language = language; SaveToFile(); }
    }

    public bool UpdateTradeCode(ulong trainerID, int newCode)
    {
        var game = GetCurrentGame();
        if (DatabaseService.UseRemoteDb)
        {
            var user = DatabaseService.GetUser(trainerID);
            if (user != null) { SetCodeForGame(user, game, newCode.ToString()); DatabaseService.SaveUser(trainerID, user); return true; }
            return false;
        }
        LoadFromFile();
        if (_tradeCodeDetails!.TryGetValue(trainerID, out var details)) { SetCodeForGame(details, game, newCode.ToString()); SaveToFile(); return true; }
        return false;
    }

    private static int GenerateRandomTradeCode() => new TradeSettings().GetRandomTradeCode();

    private static List<Pictocodes> GenerateRandomLGCode()
    {
        var rnd = new Random();
        var code = new List<Pictocodes>();
        for (int i = 0; i < 3; i++) code.Add((Pictocodes)rnd.Next(10));
        return code;
    }

    private static string LGCodeToString(List<Pictocodes> code) => string.Join(",", code.Select(c => (int)c));
    private static List<Pictocodes> StringToLGCode(string s) => s.Split(',').Select(x => (Pictocodes)int.Parse(x)).ToList();

    private void LoadFromFile()
    {
        if (File.Exists(FileName)) _tradeCodeDetails = JsonSerializer.Deserialize<Dictionary<ulong, TradeCodeDetails>>(File.ReadAllText(FileName), SerializerOptions);
        else _tradeCodeDetails = [];
    }

    private void SaveToFile()
    {
        try { File.WriteAllText(FileName, JsonSerializer.Serialize(_tradeCodeDetails, SerializerOptions)); }
        catch (Exception ex) { LogUtil.LogInfo("TradeCodeStorage", $"Error: {ex.Message}"); }
    }

    public class TradeCodeDetails
    {
        public string? Code_SV { get; set; }
        public string? Code_SWSH { get; set; }
        public string? Code_BDSP { get; set; }
        public string? Code_LA { get; set; }
        public string? Code_PLZA { get; set; }
        public string? Code_LGPE { get; set; }

        public string? OT { get; set; }
        public int SID { get; set; }
        public int TID { get; set; }
        public int TradeCount { get; set; }
        public int Medals { get; set; }
        public byte? Gender { get; set; }
        public int? Language { get; set; }
        public string? Quote { get; set; }
    }
}
