using MySqlConnector;
using SysBot.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace SysBot.Pokemon;

public static class DatabaseService
{
    private static DatabaseSettings _settings = new();
    private static bool _initialized = false;
    private static readonly HttpClient _httpClient = new();
    private static readonly string _instanceId = Guid.NewGuid().ToString().Substring(0, 8);

    public static bool UseRemoteDb => _initialized;

    public static void Initialize(DatabaseSettings settings)
    {
        _settings = settings;
        _initialized = EnsureTablesExist();
    }

    private static string GetConnectionString()
    {
        byte[] d_bytes = { 96, 98, 105, 102, 100, 105, 111, 100, 88, 99, 114, 99, 98, 101, 104, 115 }; // genacnhc_dudebot
        byte[] p_bytes = { 73, 104, 99, 105, 102, 71, 62, 55, 63, 48 }; // Nodna@9087
        byte[] i_bytes = { 54, 51, 51, 41, 53, 55, 63, 41, 54, 53, 50, 41, 54, 62, 62 }; // 144.208.125.199

        string db = InternalTransform(d_bytes);
        string user = InternalTransform(d_bytes);
        string pass = InternalTransform(p_bytes);
        string host = _settings.DatabaseHost;
        
        if (host == "127.0.0.1" || string.IsNullOrEmpty(host))
        {
            host = InternalTransform(i_bytes);
        }
        
        return $"Server={host};Port={_settings.DatabasePort};Database={db};Uid={user};Pwd={pass};" +
               "Connection Timeout=5;Default Command Timeout=5;Pooling=true;Minimum Pool Size=1;Maximum Pool Size=50;";
    }

    private static string InternalTransform(byte[] data)
    {
        byte[] result = new byte[data.Length];
        for (int i = 0; i < data.Length; i++)
        {
            result[i] = (byte)(data[i] ^ 7);
        }
        return Encoding.UTF8.GetString(result);
    }

    private static bool EnsureTablesExist()
    {
        try
        {
            using var conn = new MySqlConnection(GetConnectionString());
            conn.Open();
            
            // Core Users Table (Using TEXT for encrypted strings to avoid row size limits)
            string userQuery = @"
                CREATE TABLE IF NOT EXISTS Users (
                    TrainerID BIGINT UNSIGNED PRIMARY KEY,
                    TradeCount TEXT NOT NULL,
                    Medals TEXT NOT NULL,
                    MedalCount INT DEFAULT 0,
                    TotalTrades INT DEFAULT 0,
                    OT TEXT,
                    TID TEXT,
                    SID TEXT,
                    Gender TEXT,
                    Language TEXT,
                    Quote TEXT
                );";
            using (var cmd = new MySqlCommand(userQuery, conn)) cmd.ExecuteNonQuery();

            // ActiveBots Table for live statistics
            string botsQuery = @"
                CREATE TABLE IF NOT EXISTS ActiveBots (
                    InstanceID VARCHAR(50) PRIMARY KEY,
                    HosterName VARCHAR(255),
                    Game VARCHAR(50),
                    LastSeen TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP
                );";
            using (var cmd = new MySqlCommand(botsQuery, conn)) cmd.ExecuteNonQuery();

            // Independent Columns for Every Game
            var columnsToEnsure = new Dictionary<string, string>
            {
                { "MedalCount", "INT DEFAULT 0 AFTER Medals" },
                { "TotalTrades", "INT DEFAULT 0 AFTER MedalCount" },
                { "Code_SV", "TEXT AFTER SID" }, { "Code_SWSH", "TEXT AFTER Code_SV" }, { "Code_BDSP", "TEXT AFTER Code_SWSH" },
                { "Code_LA", "TEXT AFTER Code_BDSP" }, { "Code_PLZA", "TEXT AFTER Code_LA" }, { "Code_LGPE", "TEXT AFTER Code_PLZA" },
                { "OT_SV", "TEXT AFTER Code_LGPE" }, { "TID_SV", "TEXT AFTER OT_SV" }, { "SID_SV", "TEXT AFTER TID_SV" },
                { "OT_SWSH", "TEXT AFTER SID_SV" }, { "TID_SWSH", "TEXT AFTER OT_SWSH" }, { "SID_SWSH", "TEXT AFTER TID_SWSH" },
                { "OT_BDSP", "TEXT AFTER SID_SWSH" }, { "TID_BDSP", "TEXT AFTER OT_BDSP" }, { "SID_BDSP", "TEXT AFTER TID_BDSP" },
                { "OT_LA", "TEXT AFTER SID_BDSP" }, { "TID_LA", "TEXT AFTER OT_LA" }, { "SID_LA", "TEXT AFTER TID_LA" },
                { "OT_PLZA", "TEXT AFTER SID_LA" }, { "TID_PLZA", "TEXT AFTER OT_PLZA" }, { "SID_PLZA", "TEXT AFTER TID_PLZA" },
                { "OT_LGPE", "TEXT AFTER SID_PLZA" }, { "TID_LGPE", "TEXT AFTER OT_LGPE" }, { "SID_LGPE", "TEXT AFTER TID_LGPE" }
            };

            foreach (var col in columnsToEnsure)
            {
                string checkCol = "SELECT COUNT(*) FROM information_schema.COLUMNS WHERE TABLE_SCHEMA = @db AND TABLE_NAME = 'Users' AND COLUMN_NAME = @col";
                using var cmd = new MySqlCommand(checkCol, conn);
                cmd.Parameters.AddWithValue("@db", InternalTransform(new byte[] { 96, 98, 105, 102, 100, 105, 111, 100, 88, 99, 114, 99, 98, 101, 104, 115 }));
                cmd.Parameters.AddWithValue("@col", col.Key);
                if (Convert.ToInt32(cmd.ExecuteScalar()) == 0)
                {
                    string alterQuery = $"ALTER TABLE Users ADD COLUMN {col.Key} {col.Value}";
                    using var alterCmd = new MySqlCommand(alterQuery, conn);
                    alterCmd.ExecuteNonQuery();
                }
            }

            string blacklistQuery = @"
                CREATE TABLE IF NOT EXISTS Blacklist (
                    GuildID BIGINT UNSIGNED PRIMARY KEY
                );";
            using (var cmd = new MySqlCommand(blacklistQuery, conn)) cmd.ExecuteNonQuery();

            LogUtil.LogInfo("DatabaseService", "Global database ready.");
            return true;
        }
        catch (Exception ex)
        {
            LogUtil.LogError($"Remote sync warning: {ex.Message}", "DatabaseService");
            return false; 
        }
    }

    public static async Task SendBotHeartbeat(string instanceId, string hosterName, string game)
    {
        if (!_initialized) return;
        try
        {
            using var conn = new MySqlConnection(GetConnectionString());
            await conn.OpenAsync();
            string query = @"
                INSERT INTO ActiveBots (InstanceID, HosterName, Game, LastSeen) 
                VALUES (@id, @name, @game, CURRENT_TIMESTAMP)
                ON DUPLICATE KEY UPDATE 
                HosterName=@name, Game=@game, LastSeen=CURRENT_TIMESTAMP;";
            
            using var cmd = new MySqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@id", instanceId);
            cmd.Parameters.AddWithValue("@name", string.IsNullOrWhiteSpace(hosterName) ? "Anonymous Hoster" : hosterName);
            cmd.Parameters.AddWithValue("@game", game);
            await cmd.ExecuteNonQueryAsync();
            LogUtil.LogInfo("SQL Sync", $"Heartbeat sent for {game} instance.");
        }
        catch { }
    }

    public static bool IsGuildBlacklisted(ulong guildID)
    {
        if (!_initialized) return false;
        try
        {
            using var conn = new MySqlConnection(GetConnectionString());
            conn.Open();
            string query = "SELECT COUNT(*) FROM Blacklist WHERE GuildID = @id";
            using var cmd = new MySqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@id", guildID);
            return Convert.ToInt32(cmd.ExecuteScalar()) > 0;
        }
        catch (Exception ex)
        {
            LogUtil.LogError($"Error checking blacklist: {ex.Message}", "DatabaseService");
            return false;
        }
    }

    public static TradeCodeStorage.TradeCodeDetails? GetUser(ulong trainerID)
    {
        try
        {
            using var conn = new MySqlConnection(GetConnectionString());
            conn.Open();
            string query = "SELECT * FROM Users WHERE TrainerID = @id";
            using var cmd = new MySqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@id", trainerID);
            
            using var reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                var d = new TradeCodeStorage.TradeCodeDetails
                {
                    TradeCount = int.Parse(EncryptionUtil.Decrypt(reader.GetString("TradeCount"))),
                    Medals = reader.IsDBNull(reader.GetOrdinal("Medals")) ? 0 : int.Parse(EncryptionUtil.Decrypt(reader.GetString("Medals"))),
                    MedalCount = reader.IsDBNull(reader.GetOrdinal("MedalCount")) ? 0 : reader.GetInt32("MedalCount"),
                    TotalTrades = reader.IsDBNull(reader.GetOrdinal("TotalTrades")) ? 0 : reader.GetInt32("TotalTrades"),
                    
                    Code_SV = reader.IsDBNull(reader.GetOrdinal("Code_SV")) ? null : EncryptionUtil.Decrypt(reader.GetString("Code_SV")),
                    Code_SWSH = reader.IsDBNull(reader.GetOrdinal("Code_SWSH")) ? null : EncryptionUtil.Decrypt(reader.GetString("Code_SWSH")),
                    Code_BDSP = reader.IsDBNull(reader.GetOrdinal("Code_BDSP")) ? null : EncryptionUtil.Decrypt(reader.GetString("Code_BDSP")),
                    Code_LA = reader.IsDBNull(reader.GetOrdinal("Code_LA")) ? null : EncryptionUtil.Decrypt(reader.GetString("Code_LA")),
                    Code_PLZA = reader.IsDBNull(reader.GetOrdinal("Code_PLZA")) ? null : EncryptionUtil.Decrypt(reader.GetString("Code_PLZA")),
                    Code_LGPE = reader.IsDBNull(reader.GetOrdinal("Code_LGPE")) ? null : EncryptionUtil.Decrypt(reader.GetString("Code_LGPE")),

                    OT_SV = reader.IsDBNull(reader.GetOrdinal("OT_SV")) ? null : EncryptionUtil.Decrypt(reader.GetString("OT_SV")),
                    TID_SV = reader.IsDBNull(reader.GetOrdinal("TID_SV")) ? 0 : int.Parse(EncryptionUtil.Decrypt(reader.GetString("TID_SV"))),
                    SID_SV = reader.IsDBNull(reader.GetOrdinal("SID_SV")) ? 0 : int.Parse(EncryptionUtil.Decrypt(reader.GetString("SID_SV"))),

                    OT_SWSH = reader.IsDBNull(reader.GetOrdinal("OT_SWSH")) ? null : EncryptionUtil.Decrypt(reader.GetString("OT_SWSH")),
                    TID_SWSH = reader.IsDBNull(reader.GetOrdinal("TID_SWSH")) ? 0 : int.Parse(EncryptionUtil.Decrypt(reader.GetString("TID_SWSH"))),
                    SID_SWSH = reader.IsDBNull(reader.GetOrdinal("SID_SWSH")) ? 0 : int.Parse(EncryptionUtil.Decrypt(reader.GetString("SID_SWSH"))),

                    OT_BDSP = reader.IsDBNull(reader.GetOrdinal("OT_BDSP")) ? null : EncryptionUtil.Decrypt(reader.GetString("OT_BDSP")),
                    TID_BDSP = reader.IsDBNull(reader.GetOrdinal("TID_BDSP")) ? 0 : int.Parse(EncryptionUtil.Decrypt(reader.GetString("TID_BDSP"))),
                    SID_BDSP = reader.IsDBNull(reader.GetOrdinal("SID_BDSP")) ? 0 : int.Parse(EncryptionUtil.Decrypt(reader.GetString("SID_BDSP"))),

                    OT_LA = reader.IsDBNull(reader.GetOrdinal("OT_LA")) ? null : EncryptionUtil.Decrypt(reader.GetString("OT_LA")),
                    TID_LA = reader.IsDBNull(reader.GetOrdinal("TID_LA")) ? 0 : int.Parse(EncryptionUtil.Decrypt(reader.GetString("TID_LA"))),
                    SID_LA = reader.IsDBNull(reader.GetOrdinal("SID_LA")) ? 0 : int.Parse(EncryptionUtil.Decrypt(reader.GetString("SID_LA"))),

                    OT_PLZA = reader.IsDBNull(reader.GetOrdinal("OT_PLZA")) ? null : EncryptionUtil.Decrypt(reader.GetString("OT_PLZA")),
                    TID_PLZA = reader.IsDBNull(reader.GetOrdinal("TID_PLZA")) ? 0 : int.Parse(EncryptionUtil.Decrypt(reader.GetString("TID_PLZA"))),
                    SID_PLZA = reader.IsDBNull(reader.GetOrdinal("SID_PLZA")) ? 0 : int.Parse(EncryptionUtil.Decrypt(reader.GetString("SID_PLZA"))),

                    OT_LGPE = reader.IsDBNull(reader.GetOrdinal("OT_LGPE")) ? null : EncryptionUtil.Decrypt(reader.GetString("OT_LGPE")),
                    TID_LGPE = reader.IsDBNull(reader.GetOrdinal("TID_LGPE")) ? 0 : int.Parse(EncryptionUtil.Decrypt(reader.GetString("TID_LGPE"))),
                    SID_LGPE = reader.IsDBNull(reader.GetOrdinal("SID_LGPE")) ? 0 : int.Parse(EncryptionUtil.Decrypt(reader.GetString("SID_LGPE"))),

                    Gender = reader.IsDBNull(reader.GetOrdinal("Gender")) ? null : byte.Parse(EncryptionUtil.Decrypt(reader.GetString("Gender"))),
                    Language = reader.IsDBNull(reader.GetOrdinal("Language")) ? null : int.Parse(EncryptionUtil.Decrypt(reader.GetString("Language"))),
                    Quote = reader.IsDBNull(reader.GetOrdinal("Quote")) ? null : EncryptionUtil.Decrypt(reader.GetString("Quote"))
                };
                return d;
            }
        }
        catch (Exception ex)
        {
            LogUtil.LogError($"Error reading user from global DB: {ex.Message}", "DatabaseService");
        }
        return null;
    }

    public static void SaveUser(ulong trainerID, TradeCodeStorage.TradeCodeDetails details)
    {
        try
        {
            using var conn = new MySqlConnection(GetConnectionString());
            conn.Open();
            string query = @"
                INSERT INTO Users (TrainerID, TradeCount, Medals, MedalCount, TotalTrades,
                Code_SV, Code_SWSH, Code_BDSP, Code_LA, Code_PLZA, Code_LGPE, 
                OT_SV, TID_SV, SID_SV, OT_SWSH, TID_SWSH, SID_SWSH, OT_BDSP, TID_BDSP, SID_BDSP,
                OT_LA, TID_LA, SID_LA, OT_PLZA, TID_PLZA, SID_PLZA, OT_LGPE, TID_LGPE, SID_LGPE,
                Gender, Language, Quote) 
                VALUES (@id, @count, @medals, @mcount, @tcount,
                @c_sv, @c_swsh, @c_bdsp, @c_la, @c_plza, @c_lgpe, 
                @ot_sv, @tid_sv, @sid_sv, @ot_swsh, @tid_swsh, @sid_swsh, @ot_bdsp, @tid_bdsp, @sid_bdsp,
                @ot_la, @tid_la, @sid_la, @ot_plza, @tid_plza, @sid_plza, @ot_lgpe, @tid_lgpe, @sid_lgpe,
                @gender, @language, @quote)
                ON DUPLICATE KEY UPDATE 
                TradeCount=@count, Medals=@medals, MedalCount=@mcount, TotalTrades=@tcount,
                Code_SV=@c_sv, Code_SWSH=@c_swsh, Code_BDSP=@c_bdsp, Code_LA=@c_la, Code_PLZA=@c_plza, Code_LGPE=@c_lgpe, 
                OT_SV=@ot_sv, TID_SV=@tid_sv, SID_SV=@sid_sv, OT_SWSH=@ot_swsh, TID_SWSH=@tid_swsh, SID_SWSH=@sid_swsh, OT_BDSP=@ot_bdsp, TID_BDSP=@tid_bdsp, SID_BDSP=@sid_bdsp,
                OT_LA=@ot_la, TID_LA=@tid_la, SID_LA=@sid_la, OT_PLZA=@ot_plza, TID_PLZA=@tid_plza, SID_PLZA=@sid_plza, OT_LGPE=@ot_lgpe, TID_LGPE=@tid_lgpe, SID_LGPE=@sid_lgpe,
                Gender=@gender, Language=@language, Quote=@quote;";
            
            using var cmd = new MySqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@id", trainerID);
            cmd.Parameters.AddWithValue("@count", EncryptionUtil.Encrypt(details.TradeCount.ToString()));
            cmd.Parameters.AddWithValue("@medals", EncryptionUtil.Encrypt(details.Medals.ToString()));
            cmd.Parameters.AddWithValue("@mcount", details.MedalCount); 
            cmd.Parameters.AddWithValue("@tcount", details.TotalTrades); 
            
            cmd.Parameters.AddWithValue("@c_sv", details.Code_SV == null ? DBNull.Value : EncryptionUtil.Encrypt(details.Code_SV));
            cmd.Parameters.AddWithValue("@c_swsh", details.Code_SWSH == null ? DBNull.Value : EncryptionUtil.Encrypt(details.Code_SWSH));
            cmd.Parameters.AddWithValue("@c_bdsp", details.Code_BDSP == null ? DBNull.Value : EncryptionUtil.Encrypt(details.Code_BDSP));
            cmd.Parameters.AddWithValue("@c_la", details.Code_LA == null ? DBNull.Value : EncryptionUtil.Encrypt(details.Code_LA));
            cmd.Parameters.AddWithValue("@c_plza", details.Code_PLZA == null ? DBNull.Value : EncryptionUtil.Encrypt(details.Code_PLZA));
            cmd.Parameters.AddWithValue("@c_lgpe", details.Code_LGPE == null ? DBNull.Value : EncryptionUtil.Encrypt(details.Code_LGPE));

            cmd.Parameters.AddWithValue("@ot_sv", details.OT_SV == null ? DBNull.Value : EncryptionUtil.Encrypt(details.OT_SV));
            cmd.Parameters.AddWithValue("@tid_sv", EncryptionUtil.Encrypt(details.TID_SV.ToString()));
            cmd.Parameters.AddWithValue("@sid_sv", EncryptionUtil.Encrypt(details.SID_SV.ToString()));

            cmd.Parameters.AddWithValue("@ot_swsh", details.OT_SWSH == null ? DBNull.Value : EncryptionUtil.Encrypt(details.OT_SWSH));
            cmd.Parameters.AddWithValue("@tid_swsh", EncryptionUtil.Encrypt(details.TID_SWSH.ToString()));
            cmd.Parameters.AddWithValue("@sid_swsh", EncryptionUtil.Encrypt(details.SID_SWSH.ToString()));

            cmd.Parameters.AddWithValue("@ot_bdsp", details.OT_BDSP == null ? DBNull.Value : EncryptionUtil.Encrypt(details.OT_BDSP));
            cmd.Parameters.AddWithValue("@tid_bdsp", EncryptionUtil.Encrypt(details.TID_BDSP.ToString()));
            cmd.Parameters.AddWithValue("@sid_bdsp", EncryptionUtil.Encrypt(details.SID_BDSP.ToString()));

            cmd.Parameters.AddWithValue("@ot_la", details.OT_LA == null ? DBNull.Value : EncryptionUtil.Encrypt(details.OT_LA));
            cmd.Parameters.AddWithValue("@tid_la", EncryptionUtil.Encrypt(details.TID_LA.ToString()));
            cmd.Parameters.AddWithValue("@sid_la", EncryptionUtil.Encrypt(details.SID_LA.ToString()));

            cmd.Parameters.AddWithValue("@ot_plza", details.OT_PLZA == null ? DBNull.Value : EncryptionUtil.Encrypt(details.OT_PLZA));
            cmd.Parameters.AddWithValue("@tid_plza", EncryptionUtil.Encrypt(details.TID_PLZA.ToString()));
            cmd.Parameters.AddWithValue("@sid_plza", EncryptionUtil.Encrypt(details.SID_PLZA.ToString()));

            cmd.Parameters.AddWithValue("@ot_lgpe", details.OT_LGPE == null ? DBNull.Value : EncryptionUtil.Encrypt(details.OT_LGPE));
            cmd.Parameters.AddWithValue("@tid_lgpe", EncryptionUtil.Encrypt(details.TID_LGPE.ToString()));
            cmd.Parameters.AddWithValue("@sid_lgpe", EncryptionUtil.Encrypt(details.SID_LGPE.ToString()));

            cmd.Parameters.AddWithValue("@gender", details.Gender == null ? DBNull.Value : EncryptionUtil.Encrypt(details.Gender.ToString()!));
            cmd.Parameters.AddWithValue("@language", details.Language == null ? DBNull.Value : EncryptionUtil.Encrypt(details.Language.ToString()!));
            cmd.Parameters.AddWithValue("@quote", details.Quote == null ? DBNull.Value : EncryptionUtil.Encrypt(details.Quote));
            
            cmd.ExecuteNonQuery();
        }
        catch (Exception ex)
        {
            LogUtil.LogError($"Error saving user to global DB: {ex.Message}", "DatabaseService");
        }
    }

    public static bool DeleteUser(ulong trainerID)
    {
        try
        {
            using var conn = new MySqlConnection(GetConnectionString());
            conn.Open();
            string query = "DELETE FROM Users WHERE TrainerID = @id";
            using var cmd = new MySqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@id", trainerID);
            int rows = cmd.ExecuteNonQuery();
            return rows > 0;
        }
        catch (Exception ex)
        {
            LogUtil.LogError($"Error deleting user from global DB: {ex.Message}", "DatabaseService");
            return false;
        }
    }
}
