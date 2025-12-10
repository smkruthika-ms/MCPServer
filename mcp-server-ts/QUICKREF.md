# Quick Reference - MCP TypeScript Server

## 🚀 Quick Start Commands

```powershell
# Navigate to TypeScript server
cd mcp-server-ts

# Install dependencies
npm install

# Build the project
npm run build

# Start the server
npm start

# Or use development mode (auto-restart)
npm run dev
```

## 🧪 Testing Commands

### Test Health Endpoint
```powershell
(Invoke-WebRequest -Uri http://localhost:3000/health).Content | ConvertFrom-Json
```

### Test with MCP Inspector
```powershell
npx @modelcontextprotocol/inspector
# Then connect to: http://localhost:3000/mcp/sse
```

### Expose for ChatGPT Testing
```powershell
ngrok http 3000
# Copy the HTTPS URL and use in ChatGPT
```

## 📁 Important Files

| File | Purpose |
|------|---------|
| `src/index.ts` | Main server with Express and MCP integration |
| `src/tools/widgetTool.ts` | Widget tool definition |
| `src/services/widgetResourceService.ts` | Widget resource registration |
| `src/services/salesAgentApiService.ts` | Sales Agent API client |
| `.env` | Configuration (create from `.env.example`) |

## 🛠️ Common Tasks

### Rebuild After Code Changes
```powershell
npm run build
```

### Clean Build
```powershell
npm run clean
npm run build
```

### Rebuild Widget
```powershell
cd ..\Widget
npm run build
cd ..\mcp-server-ts
```

### Check if Server is Running
```powershell
Invoke-WebRequest -Uri http://localhost:3000/health
```

## 🎯 Tool Usage

### widget_sales_chat Tool

**Input Schema:**
```json
{
  "plannerKey": "SalesAgentPlanner",
  "message": "Show me account summary",
  "token": "optional-bearer-token"
}
```

**Test with curl (if installed):**
```powershell
curl -X POST http://localhost:3000/mcp/messages `
  -H "Content-Type: application/json" `
  -d '{"jsonrpc":"2.0","method":"tools/call","params":{"name":"widget_sales_chat","arguments":{"plannerKey":"SalesAgentPlanner","message":"Show me account summary"}},"id":1}'
```

## 📡 Endpoints

| Method | Path | Description |
|--------|------|-------------|
| GET | `/` | API info |
| GET | `/health` | Health check |
| GET | `/mcp/sse` | SSE endpoint for MCP |
| POST | `/mcp/messages` | Message handling |

## 🔧 Environment Variables

Create `.env` file with:

```env
PORT=3000
TENANT_ID=72f988bf-86f1-41af-91ab-2d7cd011db47
CLIENT_ID=your-client-id
CLIENT_SECRET=your-client-secret
SALES_AGENT_API_URL=https://salescopilotskeusuat.azurewebsites.net/api/Playground
APP_SCOPE=api://your-app-id/.default
```

## 🐛 Troubleshooting

### Port 3000 in use
```powershell
# Find process
netstat -ano | findstr :3000

# Kill process (replace <PID>)
taskkill /F /PID <PID>
```

### Widget not loading
```powershell
# Rebuild widget
cd ..\Widget
npm install
npm run build

# Restart server
cd ..\mcp-server-ts
npm start
```

### TypeScript errors
```powershell
npm install
npm run build
```

## 📚 Documentation

- **Complete Setup**: See `COMPLETE.md`
- **Step-by-Step Guide**: See `SETUP.md`
- **Full Documentation**: See `README.md`
- **Widget Guide**: See `../Widget/README.md`

## ✅ Current Status

Server is **running** at `http://localhost:3000`
- ✅ Widget JavaScript loaded
- ✅ Widget CSS loaded
- ✅ MCP endpoints active
- ✅ Health check working
