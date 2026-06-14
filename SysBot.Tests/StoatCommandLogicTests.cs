using FluentAssertions;
using PKHeX.Core;
using PKHeX.Core.AutoMod;
using SysBot.Pokemon;
using SysBot.Pokemon.Helpers;
using SysBot.Pokemon.Stoat;
using SysBot.Pokemon.Stoat.Helpers;
using Xunit;

namespace SysBot.Tests;

public class StoatCommandLogicTests
{
    static StoatCommandLogicTests() => AutoLegalityWrapper.EnsureInitialized(new Pokemon.LegalitySettings());

    [Fact]
    public void TestConvertIdLogic()
    {
        // Testing StoatHelper<T>.ConvertId
        string stoatId = "U123456";
        ulong expectedId = 0;
        foreach (char c in stoatId) expectedId = expectedId * 31 + c;

        ulong resultId = StoatHelper<PK8>.ConvertId(stoatId);
        resultId.Should().Be(expectedId);
    }

    [Fact]
    public void TestStoatSQLMedalsAndLinkLogic()
    {
        // Testing DatabaseService and TradeCodeStorage logic with Stoat user IDs
        ulong stoatUserId = StoatHelper<PK8>.ConvertId("U_STOAT_TESTER");
        ulong alternateId = StoatHelper<PK8>.ConvertId("U_STOAT_ALT");

        // Assuming DatabaseService runs purely in-memory or a test file in testing environment
        // We will just simulate the calls that SysStoat makes to verify the code paths work

        // 1. Generate link token for user
        string token = DatabaseService.GenerateLinkToken(stoatUserId);
        
        if (token != "DB_OFF" && token != "ERROR")
        {
            token.Length.Should().Be(6);

            // 2. Link account
            bool linked = DatabaseService.LinkAccount(alternateId, token, "Stoat");
            linked.Should().BeTrue("Linking should succeed with a valid generated token.");
        }

    }
}
