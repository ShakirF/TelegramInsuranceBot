using Application.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Infrastructure.OpenAI
{
    public class OpenAIService : IOpenAIService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<OpenAIService> _logger;
        private readonly string _apiKey;

        public OpenAIService(HttpClient httpClient, IConfiguration config, ILogger<OpenAIService> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
            _apiKey = config["OpenAI:ApiKey"]!;
        }

        public async Task<string> GetBotReplyAsync(string prompt, CancellationToken cancellationToken = default)
        {
            var request = new
            {
                model = "anthropic/claude-3-haiku", 
                messages = new[]
                {
                new { role = "system", content = "You are a friendly Telegram bot that helps users buy car insurance. Keep responses short, include emojis, use simple language, and guide users step by step. Never send long paragraphs. Don't add extra explanations unless necessary." },
                new { role = "user", content = prompt }
            }
            };

            var httpRequest = new HttpRequestMessage(HttpMethod.Post, "https://openrouter.ai/api/v1/chat/completions")
            {
                Headers =
            {
                { "Authorization", $"Bearer {_apiKey}" },
                { "HTTP-Referer", "https://yourdomain.com" }, // optional
                { "X-Title", "TelegramBot" } // optional
            },
                Content = new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json")
            };

            var response = await _httpClient.SendAsync(httpRequest, cancellationToken);
            var content = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("OpenRouter error {StatusCode}: {Content}", response.StatusCode, content);
                throw new Exception($"OpenRouter API error: {response.StatusCode}");
            }

            var parsed = JsonDocument.Parse(content);
            return parsed.RootElement
                .GetProperty("choices")[0]
                .GetProperty("message")
                .GetProperty("content")
                .GetString() ?? "Sorry, I couldn't understand.";
        }
    }

}
