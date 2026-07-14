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

        private static readonly System.Collections.Concurrent.ConcurrentDictionary<string, bool> _initializedGames = new();

        public static void Initialize(string game)
        {
            if (_initializedGames.ContainsKey(game)) return;
            _initializedGames.TryAdd(game, true);
            
            if (!Directory.Exists("data"))
                Directory.CreateDirectory("data");

            InitializeSchema(game);
            MigrateOldData(game);
        }

        private static void InitializeSchema(string game)
        {
            using var connection = new SqliteConnection($"Data Source={Path.Combine("data", $"nexusbot_{game}.db")}");
            connection.Open();

            using var command = connection.CreateCommand();
            command.CommandText = @"
                CREATE TABLE IF NOT EXISTS Medals (
                    TrainerID INTEGER,
                    Game TEXT,
                    Username TEXT,
                    TradeCount INTEGER,
                    Medals INTEGER,
                    PRIMARY KEY (TrainerID, Game)
                );

                CREATE TABLE IF NOT EXISTS TradeCodes (
                    TrainerID INTEGER,
                    Game TEXT,
                    Data TEXT,
                    PRIMARY KEY (TrainerID, Game)
                );
            ";
            command.ExecuteNonQuery();
        }

        public static SqliteConnection GetConnection(string game)
        {
            Initialize(game);
            var conn = new SqliteConnection($"Data Source={Path.Combine("data", $"nexusbot_{game}.db")}");
            conn.Open();
            return conn;
        }

        private static void MigrateOldData(string game)
        {
            try
            {
                using var connection = new SqliteConnection($"Data Source={Path.Combine("data", $"nexusbot_{game}.db")}");
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
                            INSERT OR IGNORE INTO Medals (TrainerID, Game, Username, TradeCount, Medals) 
                            VALUES (@id, 'SV', @user, @count, @medals)";
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

                // Migrate Trade Codes for this specific game
                var dataDir = new DirectoryInfo("data");
                if (dataDir.Exists)
                {
                    var file = new FileInfo(Path.Combine("data", $"tradecodes_{game.ToLower()}.json"));
                    if (file.Exists)
                    {
                        LogUtil.LogInfo("DatabaseHelper", $"Migrating {file.Name} (Game: {game}) to SQLite...");
                        var dict = JsonSerializer.Deserialize<Dictionary<ulong, TradeCodeStorage.TradeCodeDetails>>(File.ReadAllText(file.FullName), SerializerOptions);
                        if (dict != null)
                        {
                            using var transaction = connection.BeginTransaction();
                            using var cmd = connection.CreateCommand();
                            cmd.Transaction = transaction;

                            cmd.CommandText = "INSERT OR REPLACE INTO TradeCodes (TrainerID, Game, Data) VALUES (@id, @game, @data)";
                            var pId = cmd.Parameters.Add("@id", SqliteType.Integer);
                            var pGame = cmd.Parameters.Add("@game", SqliteType.Text);
                            var pData = cmd.Parameters.Add("@data", SqliteType.Text);

                            foreach (var kvp in dict)
                            {
                                pId.Value = (long)kvp.Key;
                                pGame.Value = game;
                                pData.Value = JsonSerializer.Serialize(kvp.Value, SerializerOptions);
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
