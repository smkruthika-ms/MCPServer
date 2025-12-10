# MCP Server with Widget Tool (TypeScript SDK)

A Model Context Protocol (MCP) server built with the TypeScript SDK that provides a widget tool for interacting with the Sales Agent Plugin API.

## 🚀 Features

- **TypeScript SDK**: Built using the official `@modelcontextprotocol/sdk`
- **Widget Support**: Interactive ChatGPT widget for displaying sales data
- **Streamable HTTP Transport**: Supports SSE-based streaming responses
- **Authentication**: JWT bearer token support
- **Sales Agent Integration**: Chat with Sales Agent Plugin API

## 📁 Project Structure

```
mcp-server-ts/
├── src/
│   ├── index.ts                          # Main server entry point
│   ├── services/
│   │   ├── widgetResourceService.ts      # Widget resource definition
│   │   └── salesAgentApiService.ts       # Sales Agent API client
│   └── tools/
│       └── widgetTool.ts                 # Widget tool definition
├── dist/                                 # Compiled JavaScript (generated)
├── package.json
├── tsconfig.json
├── .env.example
└── README.md
```

## 🛠️ Setup

### 1. Install Dependencies

```bash
npm install
```

### 2. Configure Environment

Copy `.env.example` to `.env` and configure:

```bash
cp .env.example .env
```

Edit `.env`:

```env
PORT=3000
NODE_ENV=development

# Azure AD Configuration
TENANT_ID=72f988bf-86f1-41af-91ab-2d7cd011db47
CLIENT_ID=your-client-id
CLIENT_SECRET=your-client-secret

# API Configuration
SALES_AGENT_API_URL=https://salescopilotskeusuat.azurewebsites.net/api/Playground
APP_SCOPE=api://your-app-id/.default
```

### 3. Build the Widget

The TypeScript server loads the React widget from the `Widget` directory:

```bash
cd ../Widget
npm install
npm run build
cd ../mcp-server-ts
```

### 4. Build the TypeScript Server

```bash
npm run build
```

### 5. Start the Server

```bash
npm start
```

Or use watch mode for development:

```bash
npm run dev
```

## 📡 API Endpoints

### MCP Endpoint
- **POST** `/mcp` - Main MCP endpoint for handling tool calls and resource requests
- Supports streamable HTTP transport with SSE

### Health Check
- **GET** `/health` - Server health status and widget asset verification

### Root
- **GET** `/` - API information and available endpoints

## 🎨 Widget Tool

The server provides a `widget_sales_chat` tool:

### Input Schema
```json
{
  "plannerKey": "SalesAgentPlanner",
  "message": "Show me account summary",
  "token": "optional-bearer-token"
}
```

### Response Format
```json
{
  "structuredContent": {
    "summary": "Chat response for planner: SalesAgentPlanner",
    "message": "Show me account summary",
    "result": { /* API response */ }
  },
  "_meta": {
    "outputTemplate": "ui://widget/sales-dashboard.html",
    "widgetAccessible": true,
    "widgetDescription": "Interactive sales chat response",
    "detailedData": {
      "plannerKey": "SalesAgentPlanner",
      "userMessage": "Show me account summary",
      "apiResponse": { /* full response */ },
      "timestamp": "2025-12-03T10:00:00.000Z"
    }
  }
}
```

## 🔧 Development

### Watch Mode

```bash
npm run dev
```

This runs TypeScript compiler in watch mode and restarts the server on changes.

### Testing with MCP Inspector

Install the MCP Inspector:

```bash
npx @modelcontextprotocol/inspector
```

Connect to your server:

```
http://localhost:3000/mcp
```

### Manual Testing with curl

List resources:
```bash
curl -X POST http://localhost:3000/mcp \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer YOUR_TOKEN" \
  -d '{"jsonrpc":"2.0","method":"resources/list","id":1}'
```

Call the widget tool:
```bash
curl -X POST http://localhost:3000/mcp \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer YOUR_TOKEN" \
  -d '{
    "jsonrpc":"2.0",
    "method":"tools/call",
    "params":{
      "name":"widget_sales_chat",
      "arguments":{
        "plannerKey":"SalesAgentPlanner",
        "message":"Show me account summary"
      }
    },
    "id":2
  }'
```

## 🔐 Authentication

The server supports JWT bearer tokens:

1. **Client sends token**: Include `Authorization: Bearer <token>` header
2. **Server extracts token**: Token is extracted in the MCP endpoint handler
3. **Token propagation**: Token is passed to the Sales Agent API

### On-Behalf-Of (OBO) Flow

To implement OBO flow for user tokens:

1. Update `salesAgentApiService.ts` to use MSAL or Azure Identity SDK
2. Exchange the incoming user token for a token with appropriate scopes
3. Use the exchanged token when calling downstream APIs

Example (using `@azure/identity`):

```typescript
import { OnBehalfOfCredential } from '@azure/identity';

const credential = new OnBehalfOfCredential({
  tenantId: process.env.TENANT_ID!,
  clientId: process.env.CLIENT_ID!,
  clientSecret: process.env.CLIENT_SECRET!,
  userAssertionToken: userToken,
});

const accessToken = await credential.getToken(process.env.APP_SCOPE!);
```

## 📦 Widget Integration

The server serves the widget HTML template with embedded JavaScript and CSS from the `Widget` directory.

### Widget Resource
- **URI**: `ui://widget/sales-dashboard.html`
- **MIME Type**: `text/html+skybridge` (required for ChatGPT widgets)
- **Content**: Embedded React widget bundle

### Widget Features
- Interactive tabs (Overview, Details, Metadata)
- Theme support (light/dark)
- Persistent state
- Tool calling from widget (e.g., refresh data)

## 🚀 Deployment

### Local Testing with ngrok

```bash
ngrok http 3000
```

Use the HTTPS URL in ChatGPT developer mode.

### Production Deployment

1. Build the project:
   ```bash
   npm run build
   ```

2. Set environment variables on your hosting platform

3. Deploy to Azure App Service, AWS, or any Node.js hosting

4. Ensure HTTPS is enabled

5. Configure CORS if needed

## 📚 Resources

- [MCP TypeScript SDK](https://github.com/modelcontextprotocol/typescript-sdk)
- [OpenAI Apps SDK](https://developers.openai.com/apps-sdk)
- [Model Context Protocol](https://modelcontextprotocol.io/)

## 🐛 Troubleshooting

### Widget not loading
- Ensure `Widget/dist/widget.js` and `widget.css` exist
- Run `cd Widget && npm run build`
- Check server logs for asset loading messages

### Token errors
- Verify `Authorization` header format: `Bearer <token>`
- Check token expiry and audience claims
- Ensure token has correct scopes

### API errors
- Verify `SALES_AGENT_API_URL` in `.env`
- Check API endpoint accessibility
- Review API logs for detailed error messages

## 📝 License

MIT
