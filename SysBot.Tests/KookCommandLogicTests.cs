using FluentAssertions;
using PKHeX.Core;
using PKHeX.Core.AutoMod;
using SysBot.Pokemon;
using SysBot.Pokemon.Helpers;
using SysBot.Pokemon.Kook;
using System.Threading.Tasks;
using Xunit;

namespace SysBot.Tests;

public class KookCommandLogicTests
{
    static KookCommandLogicTests() => AutoLegalityWrapper.EnsureInitialized(new Pokemon.LegalitySettings());

    [Fact]
    public void TestKookSettingsInitialization()
    {
        // Simple test to ensure KookSettings can be instantiated and has expected defaults
        var settings = new KookSettings();
        
        settings.Token.Should().BeEmpty();
        settings.CommandPrefix.Should().Be("$");
        settings.MessageDeletionEnabled.Should().BeTrue();
        settings.BotStatusAnnouncement.Should().BeTrue();
    }
}
