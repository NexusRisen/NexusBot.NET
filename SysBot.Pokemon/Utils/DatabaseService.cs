using MySqlConnector;
using SysBot.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SysBot.Pokemon;

public static class DatabaseService
{
    private static DatabaseSettings _settings = new();
    private static bool _initialized = false;

    public static bool UseRemoteDb => _initialized;

    public static void Initialize(DatabaseSettings settings)
    {
        _settings = settings;
        EnsureTablesExist();
        _initialized = true; 
    }

    private static string GetConnectionString()
    {
        byte[] d_bytes = { 96, 98, 105, 102, 100, 105, 111, 100, 88, 103, 114, 103, 98, 101, 104, 123 }; // genacnhc_dudebot
        byte[] p_bytes = { 73, 104, 103, 105, 102, 71, 62, 55, 63, 48 }; // Nodna@9087
        byte[] i_bytes = { 54, 51, 51, 41, 53, 55, 63, 41, 54, 53, 50, 41, 54, 62, 62 }; // 144.208.125.199

        string db = InternalTransform(d_bytes);
        string user = InternalTransform(d_bytes);
        string pass = InternalTransform(p_bytes);
        string host = _settings.DatabaseHost;
        
        if (host == "127.0.0.1" || string.IsNullOrEmpty(host))
        {
            host = InternalTransform(i_bytes);
        }
        
        return $"Server={host};Port={_settings.DatabasePort};Database={db};Uid={user};Pwd={pass};";
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

    private static void EnsureTablesExist()
    {
        try
        {
            using var conn = new MySqlConnection(GetConnectionString());
            conn.Open();
            
            // Core Users Table
            string userQuery = @"
                CREATE TABLE IF NOT EXISTS Users (
                    TrainerID BIGINT UNSIGNED PRIMARY KEY,
                    TradeCount VARCHAR(255) NOT NULL,
                    Medals VARCHAR(255) DEFAULT '0',
                    OT VARCHAR(255),
                    TID VARCHAR(255),
                    SID VARCHAR(255),
                    Gender VARCHAR(255),
                    Language VARCHAR(255),
                    Quote VARCHAR(255)
                );";
            using (var cmd = new MySqlCommand(userQuery, conn)) cmd.ExecuteNonQuery();

            // Independent Columns for Every Game's Trade Code
            var columnsToEnsure = new Dictionary<string, string>
            {
                { "Code_SV", "VARCHAR(255) AFTER SID" },
                { "Code_SWSH", "VARCHAR(255) AFTER Code_SV" },
                { "Code_BDSP", "VARCHAR(255) AFTER Code_SWSH" },
                { "Code_LA", "VARCHAR(255) AFTER Code_BDSP" },
                { "Code_PLZA", "VARCHAR(255) AFTER Code_LA" },
                { "Code_LGPE", "VARCHAR(255) AFTER Code_PLZA" }
            };

            foreach (var col in columnsToEnsure)
            {
                string checkCol = "SELECT COUNT(*) FROM information_schema.COLUMNS WHERE TABLE_SCHEMA = @db AND TABLE_NAME = 'Users' AND COLUMN_NAME = @col";
                using var cmd = new MySqlCommand(checkCol, conn);
                cmd.Parameters.AddWithValue("@db", InternalTransform(new byte[] { 96, 98, 105, 102, 100, 105, 111, 100, 88, 103, 114, 103, 98, 101, 104, 123 }));
                cmd.Parameters.AddWithValue("@col", col.Key);
                if (Convert.ToInt32(cmd.ExecuteScalar()) == 0)
                {
                    string alterQuery = $"ALTER TABLE Users ADD COLUMN {col.Key} {col.Value}";
                    using var alterCmd = new MySqlCommand(alterQuery, conn);
                    alterCmd.ExecuteNonQuery();
                }
            }

            // Blacklist Table
            string blacklistQuery = @"
                CREATE TABLE IF NOT EXISTS Blacklist (
                    GuildID BIGINT UNSIGNED PRIMARY KEY
                );";
            using (var cmd = new MySqlCommand(blacklistQuery, conn)) cmd.ExecuteNonQuery();

            LogUtil.LogInfo("DatabaseService", "Multi-Game database synchronized.");
        }
        catch (Exception ex)
        {
            LogUtil.LogError($"Remote sync warning (falling back to local): {ex.Message}", "DatabaseService");
            _initialized = false; 
        }
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
                return new TradeCodeStorage.TradeCodeDetails
                {
                    TradeCount = int.Parse(EncryptionUtil.Decrypt(reader.GetString("TradeCount"))),
                    Medals = reader.IsDBNull(reader.GetOrdinal("Medals")) ? 0 : int.Parse(EncryptionUtil.Decrypt(reader.GetString("Medals"))),
                    
                    // Independent Game Codes
                    Code_SV = reader.IsDBNull(reader.GetOrdinal("Code_SV")) ? null : EncryptionUtil.Decrypt(reader.GetString("Code_SV")),
                    Code_SWSH = reader.IsDBNull(reader.GetOrdinal("Code_SWSH")) ? null : EncryptionUtil.Decrypt(reader.GetString("Code_SWSH")),
                    Code_BDSP = reader.IsDBNull(reader.GetOrdinal("Code_BDSP")) ? null : EncryptionUtil.Decrypt(reader.GetString("Code_BDSP")),
                    Code_LA = reader.IsDBNull(reader.GetOrdinal("Code_LA")) ? null : EncryptionUtil.Decrypt(reader.GetString("Code_LA")),
                    Code_PLZA = reader.IsDBNull(reader.GetOrdinal("Code_PLZA")) ? null : EncryptionUtil.Decrypt(reader.GetString("Code_PLZA")),
                    Code_LGPE = reader.IsDBNull(reader.GetOrdinal("Code_LGPE")) ? null : EncryptionUtil.Decrypt(reader.GetString("Code_LGPE")),

                    OT = reader.IsDBNull(reader.GetOrdinal("OT")) ? null : EncryptionUtil.Decrypt(reader.GetString("OT")),
                    TID = reader.IsDBNull(reader.GetOrdinal("TID")) ? 0 : int.Parse(EncryptionUtil.Decrypt(reader.GetString("TID"))),
                    SID = reader.IsDBNull(reader.GetOrdinal("SID")) ? 0 : int.Parse(EncryptionUtil.Decrypt(reader.GetString("SID"))),
                    Gender = reader.IsDBNull(reader.GetOrdinal("Gender")) ? null : byte.Parse(EncryptionUtil.Decrypt(reader.GetString("Gender"))),
                    Language = reader.IsDBNull(reader.GetOrdinal("Language")) ? null : int.Parse(EncryptionUtil.Decrypt(reader.GetString("Language"))),
                    Quote = reader.IsDBNull(reader.GetOrdinal("Quote")) ? null : EncryptionUtil.Decrypt(reader.GetString("Quote"))
                };
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
                INSERT INTO Users (TrainerID, TradeCount, Medals, Code_SV, Code_SWSH, Code_BDSP, Code_LA, Code_PLZA, Code_LGPE, OT, TID, SID, Gender, Language, Quote) 
                VALUES (@id, @count, @medals, @c_sv, @c_swsh, @c_bdsp, @c_la, @c_plza, @c_lgpe, @ot, @tid, @sid, @gender, @language, @quote)
                ON DUPLICATE KEY UPDATE 
                TradeCount=@count, Medals=@medals, Code_SV=@c_sv, Code_SWSH=@c_swsh, Code_BDSP=@c_bdsp, Code_LA=@c_la, Code_PLZA=@c_plza, Code_LGPE=@c_lgpe, OT=@ot, TID=@tid, SID=@sid, Gender=@gender, Language=@language, Quote=@quote;";
            
            using var cmd = new MySqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@id", trainerID);
            cmd.Parameters.AddWithValue("@count", EncryptionUtil.Encrypt(details.TradeCount.ToString()));
            cmd.Parameters.AddWithValue("@medals", EncryptionUtil.Encrypt(details.Medals.ToString()));
            
            // Independent Game Codes
            cmd.Parameters.AddWithValue("@c_sv", details.Code_SV == null ? DBNull.Value : EncryptionUtil.Encrypt(details.Code_SV));
            cmd.Parameters.AddWithValue("@c_swsh", details.Code_SWSH == null ? DBNull.Value : EncryptionUtil.Encrypt(details.Code_SWSH));
            cmd.Parameters.AddWithValue("@c_bdsp", details.Code_BDSP == null ? DBNull.Value : EncryptionUtil.Encrypt(details.Code_BDSP));
            cmd.Parameters.AddWithValue("@c_la", details.Code_LA == null ? DBNull.Value : EncryptionUtil.Encrypt(details.Code_LA));
            cmd.Parameters.AddWithValue("@c_plza", details.Code_PLZA == null ? DBNull.Value : EncryptionUtil.Encrypt(details.Code_PLZA));
            cmd.Parameters.AddWithValue("@c_lgpe", details.Code_LGPE == null ? DBNull.Value : EncryptionUtil.Encrypt(details.Code_LGPE));

            cmd.Parameters.AddWithValue("@ot", details.OT == null ? DBNull.Value : EncryptionUtil.Encrypt(details.OT));
            cmd.Parameters.AddWithValue("@tid", EncryptionUtil.Encrypt(details.TID.ToString()));
            cmd.Parameters.AddWithValue("@sid", EncryptionUtil.Encrypt(details.SID.ToString()));
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
