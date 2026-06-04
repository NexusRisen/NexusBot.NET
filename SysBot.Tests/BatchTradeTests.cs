using FluentAssertions;
using PKHeX.Core;
using PKHeX.Core.AutoMod;
using SysBot.Pokemon;
using SysBot.Pokemon.Helpers;
using Xunit;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SysBot.Tests;

public class BatchTradeTests
{
    static BatchTradeTests() => AutoLegalityWrapper.EnsureInitialized(new Pokemon.LegalitySettings());

    [Fact]
    public void ParseBatchTradeContent_SplitsOnDelimiters()
    {
        string content = "Pikachu\nLevel: 1\n---\nEevee\nLevel: 1\n—-\nDitto\nLevel: 1";
        var result = TradeModuleHelpers.ParseBatchTradeContent(content);

        result.Should().HaveCount(3);
        result[0].Should().Contain("Pikachu");
        result[1].Should().Contain("Eevee");
        result[2].Should().Contain("Ditto");
    }

    [Theory]
    [InlineData("Scale: XXXS", ".Scale=0")]
    [InlineData("Scale: XXXL", ".Scale=255")]
    [InlineData("Met Date: 20230101", ".MetDate=20230101")]
    [InlineData("Egg Met Date: 20230101", ".EggMonth=1")]
    [InlineData("Egg Met Date: 20230101", ".EggDay=1")]
    [InlineData("Egg Met Date: 20230101", ".EggYear=23")]
    public void NormalizeBatchCommands_ConvertsCorrectly(string input, string expected)
    {
        var result = BatchNormalizer.NormalizeBatchCommands(input);
        result.Should().Contain(expected);
    }

    [Fact]
    public void NormalizeBatchCommands_AlcremieToppingInjection()
    {
        string input = "Alcremie-Ruby-Cream-Strawberry";
        var result = BatchNormalizer.NormalizeBatchCommands(input);
        
        result.Should().Contain("Alcremie-Ruby-Cream");
        result.Should().Contain(".FormArgument=0");
    }

    [Fact]
    public void CanGenerateBatch()
    {
        string batchContent = @"
Pikachu
Level: 50
Met Date: 20240101
---
Eevee
Level: 10
Scale: XXXS
---
Ditto
Level: 100
";
        // Follow the bot's flow: Normalize -> Split -> Generate
        var normalizedContent = BatchNormalizer.NormalizeBatchCommands(batchContent);
        var sets = TradeModuleHelpers.ParseBatchTradeContent(normalizedContent);
        
        sets.Should().HaveCount(3);

        var sav = AutoLegalityWrapper.GetTrainerInfo<PK8>();
        foreach (var setStr in sets)
        {
            var s = new ShowdownSet(setStr);
            var template = AutoLegalityWrapper.GetTemplate(s);
            var pk = (PK8)sav.GetLegal(template, out _)!;
            
            pk.Should().NotBeNull();
            new LegalityAnalysis(pk).Valid.Should().BeTrue();
            
            if (setStr.Contains("Pikachu"))
            {
                // Verify Met Date was applied correctly (normalized to .MetDate=20240101)
                var date = (DateOnly)pk.MetDate;
                int dateValue = date.Year * 10000 + date.Month * 100 + date.Day;
                dateValue.Should().Be(20240101);
            }
            if (setStr.Contains("Eevee"))
            {
                // Scale: XXXS in SV is .Scale=0, in SWSH it maps to HeightScalar
                // Just verify it's legal for now as property names differ between gen 8/9
                new LegalityAnalysis(pk).Valid.Should().BeTrue();
            }
        }
    }
}
