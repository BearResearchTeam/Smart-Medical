using System.Collections.Generic;

namespace Smart_Medical.KouZiAI.Dtos
{
    public class KouZiAIConfigDto
    {
        public string ApiKey { get; set; } = string.Empty;
        
        public string BaseUrl { get; set; } = string.Empty;
        
        public string AgentId { get; set; } = string.Empty;
        
        public string? DefaultModel { get; set; }
        
        public int TimeoutSeconds { get; set; } = 30;
        
        public int MaxRetries { get; set; } = 3;
        
        public Dictionary<string, string>? Headers { get; set; }
    }
}