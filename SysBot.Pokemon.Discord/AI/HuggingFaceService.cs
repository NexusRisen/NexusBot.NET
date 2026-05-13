using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using SysBot.Base;
using PKHeX.Core;

namespace SysBot.Pokemon.Discord.AI;

public class HuggingFaceService
{
    private readonly string _apiKey;
    private readonly string _model;
    private readonly HttpClient _httpClient;

    public HuggingFaceService(string apiKey, string model)
    {
        _apiKey = apiKey;
        _model = model;
        _httpClient = new HttpClient();
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _apiKey);
    }

    public async Task<string> GetAIResponseAsync(string prompt)
    {
        try
        {
            var requestBody = new
            {
                inputs = prompt,
                parameters = new
                {
                    max_new_tokens = 500,
                    return_full_text = false
                }
            };

            var json = JsonConvert.SerializeObject(requestBody);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync($"https://api-inference.huggingface.co/models/{_model}", content);
            response.EnsureSuccessStatusCode();

            var responseJson = await response.Content.ReadAsStringAsync();
            var result = JsonConvert.DeserializeObject<dynamic[]>(responseJson);

            if (result != null && result.Length > 0)
            {
                return result[0].generated_text ?? string.Empty;
            }
        }
        catch (Exception ex)
        {
            LogUtil.LogInfo("HuggingFaceService", $"Error calling Hugging Face API: {ex.Message}");
        }

        return string.Empty;
    }
}
