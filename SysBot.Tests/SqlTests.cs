using FluentAssertions;
using SysBot.Pokemon;
using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace SysBot.Tests;

public class SqlTests
{
    [Fact]
    public void TestConnection()
    {
        var settings = new DatabaseSettings();
        DatabaseService.Initialize(settings);
        DatabaseService.UseRemoteDb.Should().BeTrue();
    }

    [Fact]
    public void TestConnectionDetailed()
    {
        var settings = new DatabaseSettings();
        // We can't easily capture the log from DatabaseService because it's static and uses LogUtil
        // but we can try to connect ourselves using the same logic if we want, 
        // OR we can just rely on the fact that Initialize failed.
        
        // Let's try to connect manually to see the exception
        var connectionString = GetConnectionStringManually(settings);
        using var conn = new MySqlConnector.MySqlConnection(connectionString);
        try
        {
            conn.Open();
            conn.State.Should().Be(System.Data.ConnectionState.Open);
        }
        catch (System.Exception ex)
        {
            throw new System.Exception($"Connection failed: {ex.Message}", ex);
        }
    }

    private string GetConnectionStringManually(DatabaseSettings settings)
    {
        string db = "genacnhc_dudebot";
        string user = "genacnhc_dudebot";
        string pass = "Nodna@9087";
        string host = settings.DatabaseHost;
        
        if (host == "127.0.0.1" || string.IsNullOrEmpty(host))
        {
            host = "144.208.125.199";
        }
        
        return $"Server={host};Port={settings.DatabasePort};Database={db};Uid={user};Pwd={pass};" +
               "Connection Timeout=5;Default Command Timeout=5;Pooling=true;Minimum Pool Size=1;Maximum Pool Size=50;";
    }

    [Fact]
    public void TestSaveAndGetUser()
    {
        var settings = new DatabaseSettings();
        DatabaseService.Initialize(settings);
        if (!DatabaseService.UseRemoteDb) return;

        ulong testId = 1234567890123456789;
        var details = new TradeCodeStorage.TradeCodeDetails
        {
            TradeCount = 5,
            OT_SV = "TestOT",
            TID_SV = 12345,
            SID_SV = 67890
        };

        DatabaseService.SaveUser(testId, details);
        var retrieved = DatabaseService.GetUser(testId);

        retrieved.Should().NotBeNull();
        retrieved!.TradeCount.Should().Be(5);
        retrieved.OT_SV.Should().Be("TestOT");
        retrieved.TID_SV.Should().Be(12345);
        retrieved.SID_SV.Should().Be(67890);

        DatabaseService.DeleteUser(testId).Should().BeTrue();
        DatabaseService.GetUser(testId).Should().BeNull();
    }

    [Fact]
    public async Task TestBotHeartbeatAndStats()
    {
        var settings = new DatabaseSettings();
        DatabaseService.Initialize(settings);
        if (!DatabaseService.UseRemoteDb) return;

        string instanceId = "TestInstance_" + Guid.NewGuid().ToString().Substring(0, 8);
        string botName = "TestBot_Verification";
        string game = "SV";

        await DatabaseService.SendBotHeartbeat(instanceId, botName, game);
        
        var stats = await DatabaseService.GetBotStats();
        stats.Should().NotBeEmpty();
        stats.Any(s => s.InstanceID == instanceId && s.HosterName == botName).Should().BeTrue();
    }

    [Fact]
    public void TestTotalTradesAndLeaderboard()
    {
        var settings = new DatabaseSettings();
        DatabaseService.Initialize(settings);
        if (!DatabaseService.UseRemoteDb) return;

        ulong userA = 1111111111111111111;
        ulong userB = 2222222222222222222;

        var detailsA = new TradeCodeStorage.TradeCodeDetails { TradeCount = 10, TotalTrades = 10, OT_SV = "UserA" };
        var detailsB = new TradeCodeStorage.TradeCodeDetails { TradeCount = 20, TotalTrades = 20, OT_SV = "UserB" };

        DatabaseService.SaveUser(userA, detailsA);
        DatabaseService.SaveUser(userB, detailsB);

        var leaderboard = DatabaseService.GetLeaderboard(5);
        leaderboard.Should().NotBeEmpty();
        
        // Verify UserB is above UserA in leaderboard
        var indexA = leaderboard.FindIndex(u => u.OT_SV == "UserA");
        var indexB = leaderboard.FindIndex(u => u.OT_SV == "UserB");

        if (indexA != -1 && indexB != -1)
        {
            indexB.Should().BeLessThan(indexA);
        }

        // Verify TotalTrades retrieval
        var retrievedA = DatabaseService.GetUser(userA);
        retrievedA!.TotalTrades.Should().Be(10);

        DatabaseService.DeleteUser(userA);
        DatabaseService.DeleteUser(userB);
    }
}
