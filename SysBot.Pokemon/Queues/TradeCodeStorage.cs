using SysBot.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using PKHeX.Core;
using Microsoft.Data.Sqlite;

namespace SysBot.Pokemon;

public class TradeCodeStorage
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        WriteIndented = false
    };
    
    private readonly string _game;
    
    public TradeCodeStorage(string game = "SV")
    {
        _game = game;
    }

    private TradeCodeDetails? LoadFromDb(ulong trainerID)
    {
        try
        {
            using var conn = DatabaseHelper.GetConnection();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT Data FROM TradeCodes WHERE TrainerID = @id";
            cmd.Parameters.AddWithValue("@id", (long)trainerID);
            
            var result = cmd.ExecuteScalar() as string;
            if (!string.IsNullOrEmpty(result))
            {
                return JsonSerializer.Deserialize<TradeCodeDetails>(result, SerializerOptions);
            }
        }
        catch (Exception ex)
        {
            LogUtil.LogError("TradeCodeStorage", $"Load error: {ex}");
        }
        return null;
    }

    private void SaveToDb(ulong trainerID, TradeCodeDetails details)
    {
        try
        {
            using var conn = DatabaseHelper.GetConnection();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = "INSERT OR REPLACE INTO TradeCodes (TrainerID, Data) VALUES (@id, @data)";
            cmd.Parameters.AddWithValue("@id", (long)trainerID);
            cmd.Parameters.AddWithValue("@data", JsonSerializer.Serialize(details, SerializerOptions));
            cmd.ExecuteNonQuery();
        }
        catch (Exception ex)
        {
            LogUtil.LogError("TradeCodeStorage", $"Save error: {ex}");
        }
    }

    public bool DeleteTradeCode(ulong trainerID)
    {
        try
        {
            using var conn = DatabaseHelper.GetConnection();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = "DELETE FROM TradeCodes WHERE TrainerID = @id";
            cmd.Parameters.AddWithValue("@id", (long)trainerID);
            return cmd.ExecuteNonQuery() > 0;
        }
        catch (Exception ex)
        {
            LogUtil.LogError("TradeCodeStorage", $"Delete error: {ex}");
            return false;
        }
    }

    public int GetTradeCode(ulong trainerID)
    {
        var details = LoadFromDb(trainerID);
        if (details != null)
        {
            var existingCode = GetCodeForGame(details, _game);
            if (existingCode != null && int.TryParse(existingCode, out int codeInt))
            {
                SyncGenericFields(details, true);
                SaveToDb(trainerID, details);
                return codeInt;
            }

            var newCode = GenerateRandomTradeCode();
            SetCodeForGame(details, _game, newCode.ToString());
            SyncGenericFields(details, true);
            SaveToDb(trainerID, details);
            return newCode;
        }

        var localCode = GenerateRandomTradeCode();
        var newDetails = new TradeCodeDetails();
        SetCodeForGame(newDetails, _game, localCode.ToString());
        SyncGenericFields(newDetails, true);
        SaveToDb(trainerID, newDetails);
        new MedalStorage().AddTrade(trainerID, trainerID.ToString());
        return localCode;
    }

    public List<Pictocodes> GetLGTradeCode(ulong trainerID)
    {
        var details = LoadFromDb(trainerID);
        if (details != null)
        {
            if (!string.IsNullOrEmpty(details.Code_LGPE))
            {
                SyncGenericFields(details, true);
                SaveToDb(trainerID, details);
                return StringToLGCode(details.Code_LGPE);
            }
            
            var newLg = GenerateRandomLGCode();
            details.Code_LGPE = LGCodeToString(newLg);
            SyncGenericFields(details, true);
            SaveToDb(trainerID, details);
            return newLg;
        }

        var localLg = GenerateRandomLGCode();
        var newDetails = new TradeCodeDetails { Code_LGPE = LGCodeToString(localLg) };
        SyncGenericFields(newDetails, true);
        SaveToDb(trainerID, newDetails);
        new MedalStorage().AddTrade(trainerID, trainerID.ToString());
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
        var details = LoadFromDb(trainerID);
        if (details != null)
        {
            SyncGenericFields(details, true);
            return details;
        }
        return null;
    }

    public void UpdateTradeDetails(ulong trainerID, string ot, int tid, int sid, string? quote = null, byte? gender = null, int? language = null)
    {
        var details = LoadFromDb(trainerID);
        if (details != null) 
        { 
            details.OT = ot; details.TID = tid; details.SID = sid;
            SyncGenericFields(details, false);
            if (quote != null) details.Quote = quote; 
            if (gender.HasValue) details.Gender = gender; 
            if (language.HasValue) details.Language = language; 
            SaveToDb(trainerID, details); 
        }
    }

    public bool UpdateTradeCode(ulong trainerID, int newCode)
    {
        var details = LoadFromDb(trainerID);
        if (details != null) { SetCodeForGame(details, _game, newCode.ToString()); SaveToDb(trainerID, details); return true; }
        return false;
    }

    public void UpdateUsername(ulong trainerID, string username)
    {
        var details = LoadFromDb(trainerID);
        if (details != null)
        {
            if (details.Username != username)
            {
                details.Username = username;
                SaveToDb(trainerID, details);
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

    public class TradeCodeDetails
    {
        public string? Username { get; set; }
        public string? OT { get; set; }
        public int TID { get; set; }
        public int SID { get; set; }
        public string? Code_SV { get; set; }
        public string? Code_SWSH { get; set; }
        public string? Code_BDSP { get; set; }
        public string? Code_LA { get; set; }
        public string? Code_PLZA { get; set; }
        public string? Code_LGPE { get; set; }
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
        public string? Quote { get; set; }
        public byte? Gender { get; set; }
        public int? Language { get; set; }
    }
}
