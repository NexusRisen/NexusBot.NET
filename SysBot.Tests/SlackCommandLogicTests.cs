using FluentAssertions;
using PKHeX.Core;
using PKHeX.Core.AutoMod;
using SysBot.Pokemon;
using SysBot.Pokemon.Slack;
using Xunit;

namespace SysBot.Tests;

public class SlackCommandLogicTests
{
    static SlackCommandLogicTests() => AutoLegalityWrapper.EnsureInitialized(new Pokemon.LegalitySettings());

    [Fact]
    public void TestSlackSettingsInitialization()
    {
        // Simple test to ensure SlackSettings can be instantiated and has expected defaults
        var settings = new SlackSettings();
        
        settings.BotToken.Should().BeEmpty();
        settings.AppLevelToken.Should().BeEmpty();
        settings.CommandPrefix.Should().Be("$");
        settings.BotStatusAnnouncement.Should().BeTrue();
    }
}
