import express, { Request, Response } from 'express';
import { Server } from '@modelcontextprotocol/sdk/server/index.js';
import { SSEServerTransport } from '@modelcontextprotocol/sdk/server/sse.js';
import {
  CallToolRequestSchema,
  ListResourcesRequestSchema,
  ListToolsRequestSchema,
  ReadResourceRequestSchema,
} from '@modelcontextprotocol/sdk/types.js';
import * as dotenv from 'dotenv';
import { createWidgetResource } from './services/widgetResourceService.js';
import { SalesAgentApiService } from './services/salesAgentApiService.js';
import { widgetTool } from './tools/widgetTool.js';
import path from 'path';
import { fileURLToPath } from 'url';
import fs from 'fs';

// Load environment variables
dotenv.config();

const __filename = fileURLToPath(import.meta.url);
const __dirname = path.dirname(__filename);

const PORT = process.env.PORT || 3000;
const app = express();

// Middleware
app.use(express.json());

// Initialize services
const salesAgentApiService = new SalesAgentApiService();

// Create MCP Server
const mcpServer = new Server(
  {
    name: 'mcp-server-widget',
    version: '1.0.0',
  },
  {
    capabilities: {
      tools: {},
      resources: {},
    },
  }
);

// Load widget assets
let widgetJs = '';
let widgetCss = '';

const widgetJsPath = path.join(__dirname, '../../Widget/dist/widget.js');
const widgetCssPath = path.join(__dirname, '../../Widget/dist/widget.css');

try {
  if (fs.existsSync(widgetJsPath)) {
    widgetJs = fs.readFileSync(widgetJsPath, 'utf-8');
    console.log('✅ Loaded widget JavaScript');
  } else {
    console.warn('⚠️  Widget JavaScript not found. Run: cd Widget && npm run build');
  }
} catch (error) {
  console.error('❌ Error loading widget JavaScript:', error);
}

try {
  if (fs.existsSync(widgetCssPath)) {
    widgetCss = fs.readFileSync(widgetCssPath, 'utf-8');
    console.log('✅ Loaded widget CSS');
  } else {
    console.warn('⚠️  Widget CSS not found. Run: cd Widget && npm run build');
  }
} catch (error) {
  console.error('❌ Error loading widget CSS:', error);
}

// Register list_resources handler
mcpServer.setRequestHandler(ListResourcesRequestSchema, async () => {
  return {
    resources: [createWidgetResource()],
  };
});

// Register read_resource handler
mcpServer.setRequestHandler(ReadResourceRequestSchema, async (request) => {
  const uri = request.params.uri;

  if (uri === 'ui://widget/sales-dashboard.html') {
    // Return the widget HTML template with embedded JS and CSS
    const widgetHtml = `
<!DOCTYPE html>
<html lang="en">
<head>
  <meta charset="UTF-8">
  <meta name="viewport" content="width=device-width, initial-scale=1.0">
  <title>Sales Dashboard Widget</title>
  <style>${widgetCss}</style>
</head>
<body>
  <div id="widget-root"></div>
  <script type="module">${widgetJs}</script>
</body>
</html>`;

    return {
      contents: [
        {
          uri,
          mimeType: 'text/html+skybridge',
          text: widgetHtml,
        },
      ],
    };
  }

  throw new Error(`Resource not found: ${uri}`);
});

// Register list_tools handler
mcpServer.setRequestHandler(ListToolsRequestSchema, async () => {
  return {
    tools: [widgetTool],
  };
});

// Register call_tool handler
mcpServer.setRequestHandler(CallToolRequestSchema, async (request) => {
  const { name, arguments: args } = request.params;

  if (name === 'widget_sales_chat') {
    const { plannerKey, message, token } = args as {
      plannerKey: string;
      message: string;
      token?: string;
    };

    try {
      // Call the sales agent API
      const result = await salesAgentApiService.chatWithAgent(
        plannerKey,
        message,
        token || ''
      );

      // Return response with widget metadata
      return {
        content: [
          {
            type: 'text',
            text: JSON.stringify({
              structuredContent: {
                summary: `Chat response for planner: ${plannerKey}`,
                message: message,
                result: result,
              },
              _meta: {
                outputTemplate: 'ui://widget/sales-dashboard.html',
                widgetAccessible: true,
                widgetDescription: 'Interactive sales chat response',
                detailedData: {
                  plannerKey,
                  userMessage: message,
                  apiResponse: result,
                  timestamp: new Date().toISOString(),
                },
              },
            }),
          },
        ],
      };
    } catch (error) {
      const errorMessage = error instanceof Error ? error.message : 'Unknown error';
      return {
        content: [
          {
            type: 'text',
            text: JSON.stringify({
              error: true,
              message: `Failed to chat with agent: ${errorMessage}`,
            }),
          },
        ],
        isError: true,
      };
    }
  }

  throw new Error(`Tool not found: ${name}`);
});

// MCP endpoint with SSE transport
app.get('/mcp/sse', async (req: Request, res: Response) => {
  console.log('📥 SSE connection established');

  // Extract token from Authorization header
  const authHeader = req.headers.authorization;
  let token = '';
  if (authHeader?.startsWith('Bearer ')) {
    token = authHeader.substring(7);
    console.log('🔑 Token extracted from Authorization header');
  }

  // Handle client disconnect
  req.on('close', () => {
    console.log('📤 SSE connection closed');
  });

  try {
    // Create a new server instance for this connection
    const sessionServer = new Server(
      {
        name: 'mcp-server-widget',
        version: '1.0.0',
      },
      {
        capabilities: {
          tools: {},
          resources: {},
        },
      }
    );

    // Register handlers for this session
    sessionServer.setRequestHandler(ListResourcesRequestSchema, async () => {
      return {
        resources: [createWidgetResource()],
      };
    });

    sessionServer.setRequestHandler(ReadResourceRequestSchema, async (request) => {
      const uri = request.params.uri;

      if (uri === 'ui://widget/sales-dashboard.html') {
        const widgetHtml = `
<!DOCTYPE html>
<html lang="en">
<head>
  <meta charset="UTF-8">
  <meta name="viewport" content="width=device-width, initial-scale=1.0">
  <title>Sales Dashboard Widget</title>
  <style>${widgetCss}</style>
</head>
<body>
  <div id="widget-root"></div>
  <script type="module">${widgetJs}</script>
</body>
</html>`;

        return {
          contents: [
            {
              uri,
              mimeType: 'text/html+skybridge',
              text: widgetHtml,
            },
          ],
        };
      }

      throw new Error(`Resource not found: ${uri}`);
    });

    sessionServer.setRequestHandler(ListToolsRequestSchema, async () => {
      return {
        tools: [widgetTool],
      };
    });

    sessionServer.setRequestHandler(CallToolRequestSchema, async (request) => {
      const { name, arguments: args } = request.params;

      if (name === 'widget_sales_chat') {
        const { plannerKey, message, token: argToken } = args as {
          plannerKey: string;
          message: string;
          token?: string;
        };

        try {
          const result = await salesAgentApiService.chatWithAgent(
            plannerKey,
            message,
            argToken || token || ''
          );

          return {
            content: [
              {
                type: 'text',
                text: JSON.stringify({
                  structuredContent: {
                    summary: `Chat response for planner: ${plannerKey}`,
                    message: message,
                    result: result,
                  },
                  _meta: {
                    outputTemplate: 'ui://widget/sales-dashboard.html',
                    widgetAccessible: true,
                    widgetDescription: 'Interactive sales chat response',
                    detailedData: {
                      plannerKey,
                      userMessage: message,
                      apiResponse: result,
                      timestamp: new Date().toISOString(),
                    },
                  },
                }),
              },
            ],
          };
        } catch (error) {
          const errorMessage = error instanceof Error ? error.message : 'Unknown error';
          return {
            content: [
              {
                type: 'text',
                text: JSON.stringify({
                  error: true,
                  message: `Failed to chat with agent: ${errorMessage}`,
                }),
              },
            ],
            isError: true,
          };
        }
      }

      throw new Error(`Tool not found: ${name}`);
    });

    const transport = new SSEServerTransport('/mcp/messages', res);
    await sessionServer.connect(transport);
    console.log('✅ MCP server connected via SSE');
  } catch (error) {
    console.error('❌ Error connecting MCP server:', error);
    if (!res.headersSent) {
      res.status(500).json({ error: 'Failed to establish SSE connection' });
    }
  }
});

// MCP POST endpoint for messages
app.post('/mcp/messages', async (req: Request, res: Response) => {
  console.log('📥 Received MCP message:', req.body);
  
  // The SSEServerTransport should handle this
  // Just acknowledge receipt
  res.status(202).send();
});

// Health check endpoint
app.get('/health', (req, res) => {
  res.json({
    status: 'healthy',
    timestamp: new Date().toISOString(),
    widgetAssetsLoaded: {
      javascript: widgetJs.length > 0,
      css: widgetCss.length > 0,
    },
  });
});

// Root endpoint
app.get('/', (req, res) => {
  res.json({
    name: 'MCP Server with Widget Tool',
    version: '1.0.0',
    endpoints: {
      mcp: 'POST /mcp',
      health: 'GET /health',
    },
  });
});

// Start server
app.listen(PORT, () => {
  console.log(`🚀 MCP Server with Widget Tool running on port ${PORT}`);
  console.log(`📡 MCP endpoint: http://localhost:${PORT}/mcp`);
  console.log(`💓 Health check: http://localhost:${PORT}/health`);
  console.log(`🎨 Widget assets loaded: JS=${widgetJs.length > 0}, CSS=${widgetCss.length > 0}`);
});
