using SysBot.Base;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using PKHeX.Core;
using System.Threading.Tasks;

namespace SysBot.Pokemon;

public class TradeCodeStorage
{
    private readonly string _fileName;
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        WriteIndented = true
    };
    private Dictionary<ulong, TradeCodeDetails>? _tradeCodeDetails;
    private readonly string _game;
    
    public TradeCodeStorage(string game = "SV")
    {
        _game = game;
        _fileName = Path.Combine("data", $"tradecodes_{_game}.json");
        
        if (!Directory.Exists("data"))
            Directory.CreateDirectory("data");

        LoadFromFile();
    }

    public bool DeleteTradeCode(ulong trainerID)
    {
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
        LoadFromFile();
        if (_tradeCodeDetails!.TryGetValue(trainerID, out var details))
        {

            
            var existingCode = GetCodeForGame(details, _game);
            if (existingCode != null && int.TryParse(existingCode, out int codeInt))
            {
                SyncGenericFields(details, true);
                SaveToFile();
                return codeInt;
            }

            var newCode = GenerateRandomTradeCode();
            SetCodeForGame(details, _game, newCode.ToString());
            SyncGenericFields(details, true);
            SaveToFile();
            return newCode;
        }

        var localCode = GenerateRandomTradeCode();
        var newDetails = new TradeCodeDetails();
        SetCodeForGame(newDetails, _game, localCode.ToString());
        SyncGenericFields(newDetails, true);
        _tradeCodeDetails![trainerID] = newDetails;
        new MedalStorage().AddTrade(trainerID, trainerID.ToString());
        SaveToFile();
        return localCode;
    }

    public List<Pictocodes> GetLGTradeCode(ulong trainerID)
    {
        LoadFromFile();
        if (_tradeCodeDetails!.TryGetValue(trainerID, out var details))
        {

            
            if (!string.IsNullOrEmpty(details.Code_LGPE))
            {
                SyncGenericFields(details, true);
                SaveToFile();
                return StringToLGCode(details.Code_LGPE);
            }
            
            var newLg = GenerateRandomLGCode();
            details.Code_LGPE = LGCodeToString(newLg);
            SyncGenericFields(details, true);
            SaveToFile();
            return newLg;
        }

        var localLg = GenerateRandomLGCode();
        var newDetails = new TradeCodeDetails { Code_LGPE = LGCodeToString(localLg) };
        SyncGenericFields(newDetails, true);
        _tradeCodeDetails![trainerID] = newDetails;
        new MedalStorage().AddTrade(trainerID, trainerID.ToString());
        SaveToFile();
        return localLg;
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

    private void SyncGenericFields(TradeCodeDetails details, bool fromSpecific)
    {
        if (fromSpecific)
        {
            switch (_game)
            {
                case "SV": details.OT = details.OT_SV; details.TID = details.TID_SV; details.SID = details.SID_SV; break;
                case "SWSH": details.OT = details.OT_SWSH; details.TID = details.TID_SWSH; details.SID = details.SID_SWSH; break;
                case "BDSP": details.OT = details.OT_BDSP; details.TID = details.TID_BDSP; details.SID = details.SID_BDSP; break;
                case "LA": details.OT = details.OT_LA; details.TID = details.TID_LA; details.SID = details.SID_LA; break;
                case "PLZA": details.OT = details.OT_PLZA; details.TID = details.TID_PLZA; details.SID = details.SID_PLZA; break;
                case "LGPE": details.OT = details.OT_LGPE; details.TID = details.TID_LGPE; details.SID = details.SID_LGPE; break;
            }
        }
        else
        {
            switch (_game)
            {
                case "SV": details.OT_SV = details.OT; details.TID_SV = details.TID; details.SID_SV = details.SID; break;
                case "SWSH": details.OT_SWSH = details.OT; details.TID_SWSH = details.TID; details.SID_SWSH = details.SID; break;
                case "BDSP": details.OT_BDSP = details.OT; details.TID_BDSP = details.TID; details.SID_BDSP = details.SID; break;
                case "LA": details.OT_LA = details.OT; details.TID_LA = details.TID; details.SID_LA = details.SID; break;
                case "PLZA": details.OT_PLZA = details.OT; details.TID_PLZA = details.TID; details.SID_PLZA = details.SID; break;
                case "LGPE": details.OT_LGPE = details.OT; details.TID_LGPE = details.TID; details.SID_LGPE = details.SID; break;
            }
        }
    }



    public TradeCodeDetails? GetTradeDetails(ulong trainerID)
    {
        LoadFromFile();
        if (_tradeCodeDetails!.TryGetValue(trainerID, out var details))
        {
            SyncGenericFields(details, true);
            return details;
        }
        return null;
    }

    public void UpdateTradeDetails(ulong trainerID, string ot, int tid, int sid, string? quote = null, byte? gender = null, int? language = null)
    {
        LoadFromFile();
        if (_tradeCodeDetails!.TryGetValue(trainerID, out var details)) 
        { 
            details.OT = ot; details.TID = tid; details.SID = sid;
            SyncGenericFields(details, false);
            if (quote != null) details.Quote = quote; 
            if (gender.HasValue) details.Gender = gender; 
            if (language.HasValue) details.Language = language; 
            SaveToFile(); 
        }
    }

    public bool UpdateTradeCode(ulong trainerID, int newCode)
    {
        LoadFromFile();
        if (_tradeCodeDetails!.TryGetValue(trainerID, out var details)) { SetCodeForGame(details, _game, newCode.ToString()); SaveToFile(); return true; }
        return false;
    }

    public void UpdateUsername(ulong trainerID, string username)
    {
        LoadFromFile();
        if (_tradeCodeDetails!.TryGetValue(trainerID, out var details))
        {
            if (details.Username != username)
            {
                details.Username = username;
                SaveToFile();
            }
        }
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
        if (File.Exists(_fileName)) _tradeCodeDetails = JsonSerializer.Deserialize<Dictionary<ulong, TradeCodeDetails>>(File.ReadAllText(_fileName), SerializerOptions);
        else _tradeCodeDetails = [];
    }

    private void SaveToFile()
    {
        try { File.WriteAllText(_fileName, JsonSerializer.Serialize(_tradeCodeDetails, SerializerOptions)); }
        catch (Exception ex) { LogUtil.LogInfo("TradeCodeStorage", $"Error: {ex.Message}"); }
    }

    public class TradeCodeDetails
    {
        public string? Username { get; set; }

        // Bot Logic Compatibility Fields
        public string? OT { get; set; }
        public int TID { get; set; }
        public int SID { get; set; }

        // Independent Game Codes
        public string? Code_SV { get; set; }
        public string? Code_SWSH { get; set; }
        public string? Code_BDSP { get; set; }
        public string? Code_LA { get; set; }
        public string? Code_PLZA { get; set; }
        public string? Code_LGPE { get; set; }

        // Independent Game Trainer Info
        public string? OT_SV { get; set; }
        public int TID_SV { get; set; }
        public int SID_SV { get; set; }

        public string? OT_SWSH { get; set; }
        public int TID_SWSH { get; set; }
        public int SID_SWSH { get; set; }

        public string? OT_BDSP { get; set; }
        public int TID_BDSP { get; set; }
        public int SID_BDSP { get; set; }

        public string? OT_LA { get; set; }
        public int TID_LA { get; set; }
        public int SID_LA { get; set; }

        public string? OT_PLZA { get; set; }
        public int TID_PLZA { get; set; }
        public int SID_PLZA { get; set; }

        public string? OT_LGPE { get; set; }
        public int TID_LGPE { get; set; }
        public int SID_LGPE { get; set; }


        
        // Optional Shared Profile Data
        public string? Quote { get; set; }
        public byte? Gender { get; set; }
        public int? Language { get; set; }
    }
}
