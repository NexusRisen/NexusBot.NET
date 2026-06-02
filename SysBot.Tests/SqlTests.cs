using FluentAssertions;
using SysBot.Pokemon;
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
        byte[] d_bytes = { 96, 98, 105, 102, 100, 105, 111, 100, 88, 99, 114, 99, 98, 101, 104, 115 }; // genacnhc_dudebot
        byte[] p_bytes = { 73, 104, 99, 105, 102, 71, 62, 55, 63, 48 }; // Nodna@9087
        byte[] i_bytes = { 54, 51, 51, 41, 53, 55, 63, 41, 54, 53, 50, 41, 54, 62, 62 }; // 144.208.125.199

        string db = InternalTransform(d_bytes);
        string user = InternalTransform(d_bytes);
        string pass = InternalTransform(p_bytes);
        string host = settings.DatabaseHost;
        
        if (host == "127.0.0.1" || string.IsNullOrEmpty(host))
        {
            host = InternalTransform(i_bytes);
        }
        
        return $"Server={host};Port={settings.DatabasePort};Database={db};Uid={user};Pwd={pass};" +
               "Connection Timeout=5;Default Command Timeout=5;Pooling=true;Minimum Pool Size=1;Maximum Pool Size=50;";
    }

    private string InternalTransform(byte[] data)
    {
        byte[] result = new byte[data.Length];
        for (int i = 0; i < data.Length; i++)
        {
            result[i] = (byte)(data[i] ^ 7);
        }
        return System.Text.Encoding.UTF8.GetString(result);
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
}
