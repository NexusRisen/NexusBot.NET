using System;
using MySqlConnector;

class Program
{
    static void Main()
    {
        string connStr = "Server=144.208.125.199;Port=3306;Database=genacnhc_dudebot;Uid=genacnhc_dudebot;Pwd=Nodna@9087;";
        using var conn = new MySqlConnection(connStr);
        conn.Open();
        
        using var cmd = new MySqlCommand("SELECT TrainerID, Username, TotalTrades FROM Users ORDER BY TotalTrades DESC LIMIT 10", conn);
        using (var reader = cmd.ExecuteReader())
        {
            while (reader.Read())
            {
                Console.WriteLine($"ID: {reader.GetUInt64(0)}, Username: {reader.GetString(1)}, Trades: {reader.GetInt32(2)}");
            }
        }
        
        using var cmd2 = new MySqlCommand("SELECT TrainerID, Username, TotalTrades FROM Users WHERE Username = 'Ghost in the Machine'", conn);
        using (var reader2 = cmd2.ExecuteReader())
        {
            while (reader2.Read())
            {
                Console.WriteLine($"Ghost Found - ID: {reader2.GetUInt64(0)}, Username: {reader2.GetString(1)}, Trades: {reader2.GetInt32(2)}");
            }
        }
    }
}
