using FluentAssertions;
using SysBot.Pokemon.Discord.AI;
using System;
using System.Threading.Tasks;
using Xunit;

namespace SysBot.Tests;

public class AITests
{
    [Fact]
    public async Task TestHuggingFaceResponse()
    {
        string apiKey = Environment.GetEnvironmentVariable("HUGGINGFACE_API_KEY") ?? string.Empty;
        if (string.IsNullOrWhiteSpace(apiKey))
        {
            // Skip test if no API key is provided
            return;
        }

        string model = "meta-llama/Meta-Llama-3-8B-Instruct";
        using var service = new HuggingFaceService(apiKey, model);

        string prompt = "Hello, can you provide a Pokemon Showdown set for a competitive Pikachu?";
        string systemPrompt = "You are a Pokemon assistant. Wrap Showdown sets in [SHOWDOWN] and [/SHOWDOWN] tags.";

        var response = await service.GetAIResponseAsync(12345, prompt, systemPrompt);

        response.Should().NotBeNullOrEmpty();
        response.Should().Contain("[SHOWDOWN]");
        response.Should().Contain("[/SHOWDOWN]");
        response.Should().Contain("Pikachu");
    }

    [Fact]
    public void TestHistoryManagement()
    {
        string apiKey = "dummy-key";
        string model = "dummy-model";
        using var service = new HuggingFaceService(apiKey, model);

        ulong userId = 12345;
        service.ClearHistory(userId);
        
        // This is mainly to test that ClearHistory doesn't throw and the service initializes
        service.Should().NotBeNull();
    }
}
