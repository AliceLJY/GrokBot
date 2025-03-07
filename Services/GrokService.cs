using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using System.Text.Json;
using System.Text;
using GrokBot.Models;

namespace GrokBot.Services
{
    public class GrokService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiUrl;

        public GrokService(HttpClient httpClient)
        {
            _httpClient = httpClient;
            
            // In development, use local API, in production use Render.com API with Docker
#if DEBUG
            _apiUrl = "https://localhost:7001/api/grok/chat";
#else
            _apiUrl = "https://grokbot-backend.onrender.com/api/grok/chat";
#endif
        }

        public async Task<string> GetChatResponseAsync(Chat chat)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync(_apiUrl, chat);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"API Error: {response.StatusCode}, Content: {errorContent}");
                    return $"Error communicating with API: {response.StatusCode} - {errorContent}";
                }

                var responseData = await response.Content.ReadFromJsonAsync<JsonElement>();
                if (responseData.TryGetProperty("response", out var responseContent))
                {
                    return responseContent.GetString() ?? "No response content";
                }
                
                return "Sorry, I couldn't generate a response.";
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error calling API: {ex.Message}");
                return $"Error: {ex.Message}";
            }
        }
    }
}