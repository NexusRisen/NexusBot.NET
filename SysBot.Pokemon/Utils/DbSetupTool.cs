using System;
using MySqlConnector;
using System.Collections.Generic;

namespace SysBot.Pokemon;

public static class DbSetupTool
{
    public static void Run()
    {
        string host = "144.208.125.199";
        string db = "genacnhc_dudebot";
        string user = "genacnhc_dudebot";
        string pass = "Nodna@9087";
        uint port = 3306;

        string connStr = $"Server={host};Port={port};Database={db};Uid={user};Pwd={pass};";

        Console.WriteLine("--- DudeBot v6.3.6 Stats Optimization ---");
        
        try
        {
            using var conn = new MySqlConnection(connStr);
            conn.Open();

            // Add TotalTrades as INT for website statistics
            string checkCol = $"SELECT COUNT(*) FROM information_schema.COLUMNS WHERE TABLE_SCHEMA = '{db}' AND TABLE_NAME = 'Users' AND COLUMN_NAME = 'TotalTrades'";
            using var cmd = new MySqlCommand(checkCol, conn);
            if (Convert.ToInt32(cmd.ExecuteScalar()) == 0)
            {
                Console.WriteLine("[...] Adding statistic column: TotalTrades");
                string alterQuery = "ALTER TABLE Users ADD COLUMN TotalTrades INT DEFAULT 0 AFTER MedalCount";
                using var alterCmd = new MySqlCommand(alterQuery, conn);
                alterCmd.ExecuteNonQuery();
                Console.WriteLine("[OK] TotalTrades column added.");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("[ERROR] " + ex.Message);
        }
    }
}
