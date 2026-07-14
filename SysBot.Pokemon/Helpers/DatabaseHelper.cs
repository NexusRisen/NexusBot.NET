using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using Microsoft.Data.Sqlite;
using SysBot.Base;

namespace SysBot.Pokemon
{
    public static class DatabaseHelper
    {
        private static readonly string DbPath = Path.Combine("data", "nexusbot.db");
        public static readonly string ConnectionString = $"Data Source={DbPath}";
        private static readonly JsonSerializerOptions SerializerOptions = new()
        {
            PropertyNameCaseInsensitive = true,
            WriteIndented = false
        };

        private static bool _initialized = false;

        public static void Initialize()
        {
            if (_initialized) return;
            _initialized = true;
            
            if (!Directory.Exists("data"))
                Directory.CreateDirectory("data");

            InitializeSchema();
            MigrateOldData();
        }

        private static void InitializeSchema()
        {
            using var connection = new SqliteConnection(ConnectionString);
            connection.Open();

            using var command = connection.CreateCommand();
            command.CommandText = @"
                CREATE TABLE IF NOT EXISTS Medals (
                    TrainerID INTEGER PRIMARY KEY,
                    Username TEXT,
                    TradeCount INTEGER,
                    Medals INTEGER
                );

                CREATE TABLE IF NOT EXISTS TradeCodes (
                    TrainerID INTEGER PRIMARY KEY,
                    Data TEXT
                );
            ";
            command.ExecuteNonQuery();
        }

        public static SqliteConnection GetConnection()
        {
            Initialize();
            var conn = new SqliteConnection(ConnectionString);
            conn.Open();
            return conn;
        }

        private static void MigrateOldData()
        {
            try
            {
                using var connection = new SqliteConnection(ConnectionString);
                connection.Open();

                // Migrate Medals
                var medalsFile = Path.Combine("data", "medals.json");
                if (File.Exists(medalsFile))
                {
                    LogUtil.LogInfo("DatabaseHelper", "Migrating old medals.json to SQLite...");
                    var dict = JsonSerializer.Deserialize<Dictionary<ulong, MedalStorage.MedalDetails>>(File.ReadAllText(medalsFile), SerializerOptions);
                    if (dict != null)
                    {
                        using var transaction = connection.BeginTransaction();
                        using var cmd = connection.CreateCommand();
                        cmd.Transaction = transaction;
                        cmd.CommandText = @"
                            INSERT OR IGNORE INTO Medals (TrainerID, Username, TradeCount, Medals) 
                            VALUES (@id, @user, @count, @medals)";
                        var pId = cmd.Parameters.Add("@id", SqliteType.Integer);
                        var pUser = cmd.Parameters.Add("@user", SqliteType.Text);
                        var pCount = cmd.Parameters.Add("@count", SqliteType.Integer);
                        var pMedals = cmd.Parameters.Add("@medals", SqliteType.Integer);

                        foreach (var kvp in dict)
                        {
                            pId.Value = (long)kvp.Key;
                            pUser.Value = kvp.Value.Username ?? (object)DBNull.Value;
                            pCount.Value = kvp.Value.TradeCount;
                            pMedals.Value = kvp.Value.Medals;
                            cmd.ExecuteNonQuery();
                        }
                        transaction.Commit();
                    }
                    File.Move(medalsFile, medalsFile + ".bak");
                    LogUtil.LogInfo("DatabaseHelper", "Medals migration complete. Old file backed up.");
                }

                // Migrate Trade Codes
                var dataDir = new DirectoryInfo("data");
                if (dataDir.Exists)
                {
                    foreach (var file in dataDir.GetFiles("tradecodes_*.json"))
                    {
                        LogUtil.LogInfo("DatabaseHelper", $"Migrating {file.Name} to SQLite...");
                        var dict = JsonSerializer.Deserialize<Dictionary<ulong, TradeCodeStorage.TradeCodeDetails>>(File.ReadAllText(file.FullName), SerializerOptions);
                        if (dict != null)
                        {
                            using var transaction = connection.BeginTransaction();
                            using var cmd = connection.CreateCommand();
                            cmd.Transaction = transaction;
                            
                            var selCmd = connection.CreateCommand();
                            selCmd.Transaction = transaction;
                            selCmd.CommandText = "SELECT Data FROM TradeCodes WHERE TrainerID = @id";
                            var pSelId = selCmd.Parameters.Add("@id", SqliteType.Integer);

                            cmd.CommandText = "INSERT OR REPLACE INTO TradeCodes (TrainerID, Data) VALUES (@id, @data)";
                            var pId = cmd.Parameters.Add("@id", SqliteType.Integer);
                            var pData = cmd.Parameters.Add("@data", SqliteType.Text);

                            foreach (var kvp in dict)
                            {
                                pSelId.Value = (long)kvp.Key;
                                string? existingData = selCmd.ExecuteScalar() as string;
                                TradeCodeStorage.TradeCodeDetails finalDetails = kvp.Value;
                                
                                if (!string.IsNullOrEmpty(existingData))
                                {
                                    var existing = JsonSerializer.Deserialize<TradeCodeStorage.TradeCodeDetails>(existingData, SerializerOptions);
                                    if (existing != null)
                                    {
                                        if (finalDetails.OT == null) finalDetails.OT = existing.OT;
                                        if (finalDetails.TID == 0) finalDetails.TID = existing.TID;
                                        if (finalDetails.SID == 0) finalDetails.SID = existing.SID;
                                        
                                        finalDetails.Code_SV ??= existing.Code_SV;
                                        finalDetails.Code_SWSH ??= existing.Code_SWSH;
                                        finalDetails.Code_BDSP ??= existing.Code_BDSP;
                                        finalDetails.Code_LA ??= existing.Code_LA;
                                        finalDetails.Code_PLZA ??= existing.Code_PLZA;
                                        finalDetails.Code_LGPE ??= existing.Code_LGPE;
                                        
                                        finalDetails.OT_SV ??= existing.OT_SV;
                                        finalDetails.OT_SWSH ??= existing.OT_SWSH;
                                        finalDetails.OT_BDSP ??= existing.OT_BDSP;
                                        finalDetails.OT_LA ??= existing.OT_LA;
                                        finalDetails.OT_PLZA ??= existing.OT_PLZA;
                                        finalDetails.OT_LGPE ??= existing.OT_LGPE;
                                    }
                                }

                                pId.Value = (long)kvp.Key;
                                pData.Value = JsonSerializer.Serialize(finalDetails, SerializerOptions);
                                cmd.ExecuteNonQuery();
                            }
                            transaction.Commit();
                        }
                        File.Move(file.FullName, file.FullName + ".bak");
                        LogUtil.LogInfo("DatabaseHelper", $"{file.Name} migration complete. Old file backed up.");
                    }
                }
            }
            catch (Exception ex)
            {
                LogUtil.LogError("DatabaseHelper", $"Migration failed: {ex}");
            }
        }
    }
}
