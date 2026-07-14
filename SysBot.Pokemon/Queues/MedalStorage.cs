using System;
using Microsoft.Data.Sqlite;
using SysBot.Base;

namespace SysBot.Pokemon;

public class MedalStorage
{
    private readonly string _game;

    public MedalStorage(string game = "SV")
    {
        _game = game;
    }

    public void AddTrade(ulong trainerID, string username)
    {
        try
        {
            using var connection = DatabaseHelper.GetConnection();
            using var cmd = connection.CreateCommand();
            
            // Increment existing or insert new
            cmd.CommandText = @"
                INSERT INTO Medals (TrainerID, Game, Username, TradeCount, Medals)
                VALUES (@id, @game, @user, 1, 1)
                ON CONFLICT(TrainerID, Game) DO UPDATE SET
                    Username = excluded.Username,
                    TradeCount = TradeCount + 1,
                    Medals = 1 + MIN(20, (TradeCount + 1) / 50)
            ";
            cmd.Parameters.AddWithValue("@id", (long)trainerID);
            cmd.Parameters.AddWithValue("@game", _game);
            cmd.Parameters.AddWithValue("@user", username);
            
            cmd.ExecuteNonQuery();
        }
        catch (Exception ex)
        {
            LogUtil.LogInfo("MedalStorage", $"Error adding trade: {ex.Message}");
        }
    }

    public int GetTradeCount(ulong trainerID)
    {
        try
        {
            using var connection = DatabaseHelper.GetConnection();
            using var cmd = connection.CreateCommand();
            cmd.CommandText = "SELECT TradeCount FROM Medals WHERE TrainerID = @id AND Game = @game";
            cmd.Parameters.AddWithValue("@id", (long)trainerID);
            cmd.Parameters.AddWithValue("@game", _game);
            
            var result = cmd.ExecuteScalar();
            if (result != null && result != DBNull.Value)
            {
                return Convert.ToInt32(result);
            }
        }
        catch (Exception ex)
        {
            LogUtil.LogInfo("MedalStorage", $"Error getting trade count: {ex.Message}");
        }
        return 0;
    }

    public class MedalDetails
    {
        public string? Username { get; set; }
        public int TradeCount { get; set; }
        public int Medals { get; set; }
    }
}
