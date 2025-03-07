using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace GrokBot.Models
{
    public class GrokRequest
    {
        [JsonPropertyName("model")]
        public string Model { get; set; } = "grok-2-1212";

        [JsonPropertyName("messages")]
        public List<GrokMessage> Messages { get; set; } = new List<GrokMessage>();

        [JsonPropertyName("max_tokens")]
        public int MaxTokens { get; set; } = 1024;

        [JsonPropertyName("temperature")]
        public double Temperature { get; set; } = 0.7;

        [JsonPropertyName("top_p")]
        public double TopP { get; set; } = 0.9;
    }

    public class GrokMessage
    {
        [JsonPropertyName("role")]
        public string Role { get; set; } = string.Empty;

        [JsonPropertyName("content")]
        public string Content { get; set; } = string.Empty;
    }

    public class GrokResponse
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;

        [JsonPropertyName("object")]
        public string Object { get; set; } = string.Empty;

        [JsonPropertyName("created")]
        public long Created { get; set; }

        [JsonPropertyName("model")]
        public string Model { get; set; } = string.Empty;

        [JsonPropertyName("choices")]
        public List<GrokChoice> Choices { get; set; } = new List<GrokChoice>();

        [JsonPropertyName("usage")]
        public GrokUsage Usage { get; set; } = new GrokUsage();
    }

    public class GrokChoice
    {
        [JsonPropertyName("message")]
        public GrokMessage Message { get; set; } = new GrokMessage();

        [JsonPropertyName("finish_reason")]
        public string FinishReason { get; set; } = string.Empty;

        [JsonPropertyName("index")]
        public int Index { get; set; }
    }

    public class GrokUsage
    {
        [JsonPropertyName("prompt_tokens")]
        public int PromptTokens { get; set; }

        [JsonPropertyName("completion_tokens")]
        public int CompletionTokens { get; set; }

        [JsonPropertyName("total_tokens")]
        public int TotalTokens { get; set; }
    }
}
