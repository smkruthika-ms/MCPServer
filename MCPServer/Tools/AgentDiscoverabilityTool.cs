using Azure.Core;
using Microsoft.Graph;
using ModelContextProtocol.Server;
using System.ComponentModel;
using System.Text.Json;
using WebSearchMCPServer.Tools;

namespace WebSearchMCPServer.Tools;

[McpServerToolType]
    public class AgentDiscoverabilityTool
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<SalesChatPluginTool> _logger;

        public AgentDiscoverabilityTool(IHttpContextAccessor httpContextAccessor, ILogger<SalesChatPluginTool> logger)
        {
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
        }

        [McpServerTool, Description("Get list of agents available")]
        public async Task<string> GetAllAgents(IMcpServer thisServer, string userPrompt, CancellationToken cancellationToken)
        {
            
            return "<Add agent list here>";
        }

        [McpServerTool, Description("Invoke an agent")]
        public async Task<string> InvokeAgent(IMcpServer thisServer, string agentName, CancellationToken cancellationToken)
        {
            return "<Add agent invocation response here>";
        }

    }
