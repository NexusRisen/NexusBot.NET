using FluentAssertions;
using PKHeX.Core;
using PKHeX.Core.AutoMod;
using SysBot.Pokemon;
using SysBot.Pokemon.Helpers;
using SysBot.Pokemon.Discord;
using Xunit;

namespace SysBot.Tests;

public class DiscordCommandLogicTests
{
    static DiscordCommandLogicTests() => AutoLegalityWrapper.EnsureInitialized(new Pokemon.LegalitySettings());

    [Theory]
    [InlineData("```\nPikachu\nAbility: Static\nLevel: 100\n```")]
    public void TestConvertCommandLogic(string setString)
    {
        // Simulate the logic in ConvertShowdown command
        var content = BatchNormalizer.NormalizeBatchCommands(setString);
        content = ReusableActions.StripCodeBlock(content);
        
        var set = new ShowdownSet(content);
        var sav = AutoLegalityWrapper.GetTrainerInfo<PK8>();
        var template = AutoLegalityWrapper.GetTemplate(set);
        
        var pk = sav.GetLegal(template, out var result);
        
        pk.Should().NotBeNull();
        result.Should().NotBe("Failed");
    }

    [Fact]
    public void TestLegalityCheckCommandLogic()
    {
        // Simulate the logic in LegalityCheck (lc) command
        var pkm = new PK8 { Species = (int)Species.Pikachu, CurrentLevel = 5 };
        // This blank/improperly formatted PKM should be invalid
        
        var la = new LegalityAnalysis(pkm);
        la.Valid.Should().BeFalse();
        
        var report = la.Report(verbose: false);
        report.Should().NotBeEmpty();
    }

    [Fact]
    public void TestLegalizeCommandLogic()
    {
        // Simulate the logic in Legalize command
        var sav = AutoLegalityWrapper.GetTrainerInfo<PK8>();
        var s = new ShowdownSet("Pikachu\nLevel: 5");
        var template = AutoLegalityWrapper.GetTemplate(s);
        var pkm = sav.GetLegal(template, out _);
        
        // Break it intentionally
        if (pkm is PK8 pk8)
        {
            pk8.MetLocation = 0; 
        }
        
        // The raw PKM is illegal
        new LegalityAnalysis(pkm!).Valid.Should().BeFalse();
        
        // Legalize it using ALM extension
        var legal = pkm!.LegalizePokemon();
        
        legal.Should().NotBeNull();
        new LegalityAnalysis(legal!).Valid.Should().BeTrue();
    }
}
