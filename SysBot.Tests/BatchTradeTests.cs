using FluentAssertions;
using SysBot.Pokemon.Helpers;
using Xunit;
using System.Collections.Generic;
using System.Linq;

namespace SysBot.Tests;

public class BatchTradeTests
{
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
}
