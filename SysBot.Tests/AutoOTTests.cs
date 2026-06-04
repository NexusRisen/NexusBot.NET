using FluentAssertions;
using PKHeX.Core;
using PKHeX.Core.AutoMod;
using SysBot.Pokemon;
using SysBot.Pokemon.Helpers;
using Xunit;

namespace SysBot.Tests;

public class AutoOTTests
{
    static AutoOTTests() => AutoLegalityWrapper.EnsureInitialized(new Pokemon.LegalitySettings());

    [Theory]
    [InlineData("LongNameTrainer123", LanguageID.English, "LongNameTrai")]
    [InlineData("JapaneseName123", LanguageID.Japanese, "Japane")]
    public void SanitizeOTName_TruncatesCorrectly(string originalName, LanguageID language, string expectedName)
    {
        var result = LanguageHelper.SanitizeOTName(originalName, language);
        result.Should().Be(expectedName);
    }

    [Fact]
    public void TestAutoOTLegality()
    {
        var sav = AutoLegalityWrapper.GetTrainerInfo<PK8>();
        var s = new ShowdownSet("Pikachu\nLevel: 50");
        var template = AutoLegalityWrapper.GetTemplate(s);
        var pk = sav.GetLegal(template, out _);
        
        pk.Should().NotBeNull();
        
        // Simulate AutoOT logic
        string newName = LanguageHelper.SanitizeOTName("Ash", (LanguageID)pk.Language);
        pk!.OriginalTrainerName = newName;
        pk.OriginalTrainerGender = 1;
        pk.TrainerTID7 = 123456;
        pk.TrainerSID7 = 654321;
        
        var trash = pk.OriginalTrainerTrash;
        trash.Clear();
        int maxLength = trash.Length / 2;
        int actualLength = System.Math.Min(newName.Length, maxLength);
        for (int i = 0; i < actualLength; i++)
        {
            char value = newName[i];
            trash[i * 2] = (byte)value;
            trash[(i * 2) + 1] = (byte)(value >> 8);
        }
        if (actualLength < maxLength)
        {
            trash[actualLength * 2] = 0x00;
            trash[(actualLength * 2) + 1] = 0x00;
        }
        
        if (!pk.ChecksumValid)
            pk.RefreshChecksum();

        var la = new LegalityAnalysis(pk);
        if (!la.Valid)
        {
            throw new System.Exception($"Name: '{pk.OriginalTrainerName}', Len: {pk.OriginalTrainerName.Length}, Report: {la.Report()}");
        }
        la.Valid.Should().BeTrue();
    }
}
