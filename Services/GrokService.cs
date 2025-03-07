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
            
            // 从URL获取实际域名
            string renderUrl = GetRenderUrl();
            
            // In development, use local API, in production use dynamic Render.com API
#if DEBUG
            _apiUrl = "https://localhost:7001/api/grok/chat";
#else
            _apiUrl = $"{renderUrl}/api/grok/chat";
#endif

            Console.WriteLine($"Using API URL: {_apiUrl}");
        }

        private string GetRenderUrl()
        {
            // 默认值 
            string defaultUrl = "https://grokbot-backend.onrender.com";
            
            try
            {
                // 可以从窗口位置或任何其他方式确定实际域名
                return defaultUrl;
            }
            catch
            {
                return defaultUrl;
            }
        }

        public async Task<string> GetChatResponseAsync(Chat chat)
        {
            try
            {
                Console.WriteLine($"Sending request to {_apiUrl}");
                
                // 添加超时设置和重试逻辑
                _httpClient.Timeout = TimeSpan.FromSeconds(30);
                
                // 添加错误处理和重试逻辑
                int maxRetries = 3;
                for (int attempt = 1; attempt <= maxRetries; attempt++)
                {
                    try
                    {
                        var response = await _httpClient.PostAsJsonAsync(_apiUrl, chat);

                        Console.WriteLine($"API Response Status: {response.StatusCode}");

                        if (response.IsSuccessStatusCode)
                        {
                            var responseText = await response.Content.ReadAsStringAsync();
                            Console.WriteLine($"API Response: {responseText}");

                            var responseData = JsonSerializer.Deserialize<JsonElement>(responseText);
                            if (responseData.TryGetProperty("response", out var responseContent))
                            {
                                return responseContent.GetString() ?? "No response content";
                            }
                            
                            return "Sorry, I couldn't understand the API response format.";
                        }
                        else if (attempt < maxRetries)
                        {
                            // 如果不是最后一次尝试，等待后重试
                            await Task.Delay(1000 * attempt); // 指数退避
                            Console.WriteLine($"Retrying API call, attempt {attempt + 1}/{maxRetries}");
                            continue;
                        }
                        
                        var errorContent = await response.Content.ReadAsStringAsync();
                        Console.WriteLine($"API Error: {response.StatusCode}, Content: {errorContent}");
                        return $"Error communicating with API: {response.StatusCode}. Please try again later.";
                    }
                    catch (Exception ex) when (attempt < maxRetries)
                    {
                        // 如果不是最后一次尝试，捕获异常并重试
                        Console.WriteLine($"API call attempt {attempt} failed: {ex.Message}");
                        await Task.Delay(1000 * attempt); // 指数退避
                    }
                }
                
                // 如果所有尝试都失败
                return "Sorry, I'm having trouble connecting to the server. Please check your connection and try again.";
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error calling API: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"Inner exception: {ex.InnerException.Message}");
                }
                
                return "Sorry, there was a problem connecting to the chat service. Please try again later.";
            }
        }
    }
}
