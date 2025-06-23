using Microsoft.AspNetCore.Http;
using System.Collections.Generic;

namespace MCPServer.Services
{
    public class SharedContextService
    {
        private readonly Dictionary<string, string> _contextData = new();

        public void SetContext(HttpContext context)
        {
            foreach (var header in context.Request.Headers)
            {
                _contextData[header.Key] = header.Value.ToString();
            }
        }

        public string? GetContextValue(string key)
        {
            _contextData.TryGetValue(key, out var value);
            return value;
        }

        public Dictionary<string, string> GetAllContextData()
        {
            return new Dictionary<string, string>(_contextData);
        }

        public void SetSessionId(string sessionId)
        {
            _contextData["Session-ID"] = sessionId;
        }

        public string? GetSessionId()
        {
            _contextData.TryGetValue("Session-ID", out var sessionId);
            return sessionId;
        }
    }
}
