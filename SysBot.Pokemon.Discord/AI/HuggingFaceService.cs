using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using SysBot.Base;
using PKHeX.Core;

namespace SysBot.Pokemon.Discord.AI;

public class HuggingFaceService : IDisposable
{
    private readonly string _apiKey;
    private readonly string _model;
    private readonly HttpClient _httpClient;
    private readonly int _maxTokens;
    private readonly float _temperature;
    private readonly float _topP;

    private readonly System.Collections.Concurrent.ConcurrentDictionary<ulong, List<Message>> _chatHistory = new();
    private const int MaxHistoryMessages = 10;
    private readonly SemaphoreSlim _requestSemaphore = new(1, 1);
    private bool _isDisposed;

    public HuggingFaceService(string apiKey, string model, int maxTokens = 800, float temperature = 0.7f, float topP = 0.9f)
    {
        _apiKey = apiKey;
        _model = model;
        _maxTokens = maxTokens;
        _temperature = temperature;
        _topP = topP;
        _httpClient = new HttpClient();
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _apiKey);
    }

    public async Task<string> GetAIResponseAsync(ulong userId, string prompt, string? systemPrompt = null)
    {
        await _requestSemaphore.WaitAsync();
        try
        {
            int maxRetries = 3;
            string lastError = string.Empty;
            
            for (int i = 0; i < maxRetries; i++)
            {
                try
                {
                    var messages = new List<Message>();

                    if (!string.IsNullOrWhiteSpace(systemPrompt))
                    {
                        messages.Add(new Message { Role = "user", Content = systemPrompt });
                        messages.Add(new Message { Role = "assistant", Content = "Understood. I will follow these instructions." });
                    }

                    // Add history
                    if (_chatHistory.TryGetValue(userId, out var history))
                    {
                        lock (history)
                        {
                            messages.AddRange(history);
                        }
                    }

                    // Add current prompt
                    messages.Add(new Message { Role = "user", Content = prompt });

                    var requestBody = new
                    {
                        model = _model,
                        messages = messages.Select(m => new { role = m.Role, content = m.Content }),
                        max_tokens = _maxTokens,
                        temperature = _temperature,
                        top_p = _topP
                    };

                    var json = JsonConvert.SerializeObject(requestBody);
                    var content = new StringContent(json, Encoding.UTF8, "application/json");

                    var response = await _httpClient.PostAsync($"https://router.huggingface.co/hf-inference/models/{_model}/v1/chat/completions", content);

                    if (!response.IsSuccessStatusCode)
                    {
                        var errorContent = await response.Content.ReadAsStringAsync();
                        if (response.StatusCode == System.Net.HttpStatusCode.ServiceUnavailable)
                        {
                            LogUtil.LogInfo("HuggingFaceService", $"Model is loading. Retrying in {5 * (i + 1)} seconds... {errorContent}");
                            await Task.Delay(5000 * (i + 1));
                            continue;
                        }

                        if (response.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
                        {
                            var retryAfter = response.Headers.RetryAfter?.Delta?.TotalSeconds ?? (5 * (i + 1));
                            LogUtil.LogInfo("HuggingFaceService", $"Rate limited (429). Retrying in {retryAfter} seconds... {errorContent}");
                            await Task.Delay(TimeSpan.FromSeconds(retryAfter));
                            continue;
                        }

                        throw new Exception($"HTTP {(int)response.StatusCode} {response.ReasonPhrase}: {errorContent}");
                    }

                    var responseJson = await response.Content.ReadAsStringAsync();
                    var result = JsonConvert.DeserializeObject<ChatCompletionResponse>(responseJson);

                    if (result?.Choices != null && result.Choices.Length > 0)
                    {
                        var responseText = result.Choices[0].Message?.Content;
                        if (string.IsNullOrWhiteSpace(responseText))
                            throw new Exception("Received empty content from Hugging Face API.");

                        // Save to history
                        if (!_chatHistory.ContainsKey(userId))
                            _chatHistory[userId] = new List<Message>();

                        lock (_chatHistory[userId])
                        {
                            _chatHistory[userId].Add(new Message { Role = "user", Content = prompt });
                            _chatHistory[userId].Add(new Message { Role = "assistant", Content = responseText });

                            if (_chatHistory[userId].Count > MaxHistoryMessages * 2)
                            {
                                _chatHistory[userId] = _chatHistory[userId].Skip(_chatHistory[userId].Count - MaxHistoryMessages * 2).ToList();
                            }
                        }

                        return responseText;
                    }

                    throw new Exception("Received empty response or choices from Hugging Face API.");
                }
                catch (Exception ex)
                {
                    lastError = ex.Message;
                    LogUtil.LogInfo("HuggingFaceService", $"Error calling Hugging Face API: {ex.Message}");
                    if (i == maxRetries - 1)
                        break;
                    await Task.Delay(2000 * (i + 1));
                }
            }
            
            return $"AI Error: {lastError}";
        }
        finally
        {
            _requestSemaphore.Release();
        }
    }

    public void ClearHistory(ulong userId) => _chatHistory.TryRemove(userId, out _);

    public void Dispose()
    {
        if (_isDisposed) return;
        _isDisposed = true;
        _httpClient.Dispose();
        _requestSemaphore.Dispose();
        GC.SuppressFinalize(this);
    }

    private class ChatCompletionResponse
    {
        [JsonProperty("choices")]
        public Choice[]? Choices { get; set; }
    }

    private class Choice
    {
        [JsonProperty("message")]
        public Message? Message { get; set; }
    }

    private class Message
    {
        [JsonProperty("role")]
        public string? Role { get; set; }

        [JsonProperty("content")]
        public string? Content { get; set; }
    }
}
