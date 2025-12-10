# 🧪 Testing Guide - MCP Server with Widget Tool

## Prerequisites

1. Server must be built:
   ```powershell
   cd c:\Users\krsm\source\repos\MCPServer\mcp-server-ts
   npm run build
   ```

2. Widget must be built:
   ```powershell
   cd c:\Users\krsm\source\repos\MCPServer\Widget
   npm run build
   ```

## Starting the Server

### Option 1: Run in PowerShell (Recommended for Testing)

```powershell
cd c:\Users\krsm\source\repos\MCPServer\mcp-server-ts
node dist\index.js
```

Keep this terminal open. You should see:
```
✅ Loaded widget JavaScript
✅ Loaded widget CSS
🚀 MCP Server with Widget Tool running on port 3000
📡 MCP endpoint: http://localhost:3000/mcp
💓 Health check: http://localhost:3000/health
🎨 Widget assets loaded: JS=true, CSS=true
```

### Option 2: Use npm start

```powershell
cd c:\Users\krsm\source\repos\MCPServer\mcp-server-ts
npm start
```

## Manual Testing Methods

### Method 1: PowerShell Web Requests

Open a **NEW** PowerShell terminal (keep the server running in the first one):

```powershell
# Test 1: Health Check
Invoke-RestMethod -Uri "http://localhost:3000/health" -Method Get

# Expected Output:
# status         : healthy
# timestamp      : 2025-12-03T10:00:00.000Z
# widgetAssetsLoaded : @{javascript=True; css=True}
```

```powershell
# Test 2: Root Endpoint
Invoke-RestMethod -Uri "http://localhost:3000/" -Method Get

# Expected Output:
# name      : MCP Server with Widget Tool
# version   : 1.0.0
# endpoints : @{mcp=POST /mcp; health=GET /health}
```

```powershell
# Test 3: List MCP Tools
$body = @{
    jsonrpc = "2.0"
    method = "tools/list"
    id = 1
} | ConvertTo-Json

Invoke-RestMethod -Uri "http://localhost:3000/mcp/messages" `
    -Method Post `
    -ContentType "application/json" `
    -Body $body

# Expected: Returns list of tools including widget_sales_chat
```

```powershell
# Test 4: Call Widget Tool
$body = @{
    jsonrpc = "2.0"
    method = "tools/call"
    params = @{
        name = "widget_sales_chat"
        arguments = @{
            plannerKey = "SalesAgentPlanner"
            message = "Show me account summary"
        }
    }
    id = 2
} | ConvertTo-Json -Depth 5

Invoke-RestMethod -Uri "http://localhost:3000/mcp/messages" `
    -Method Post `
    -ContentType "application/json" `
    -Body $body

# Expected: Returns tool response with widget metadata
```

### Method 2: Using the Test Script

Run the automated test script:

```powershell
cd c:\Users\krsm\source\repos\MCPServer\mcp-server-ts
.\test-mcp-server.ps1
```

This will test all endpoints automatically.

### Method 3: Browser Testing

1. Ensure server is running
2. Open browser and navigate to:
   - `http://localhost:3000/` - API info
   - `http://localhost:3000/health` - Health check

## Testing with MCP Inspector

### Install MCP Inspector

```powershell
npx @modelcontextprotocol/inspector
```

### Connect to Server

1. When prompted, enter the SSE endpoint:
   ```
   http://localhost:3000/mcp/sse
   ```

2. The inspector will show:
   - Available tools (widget_sales_chat)
   - Available resources (ui://widget/sales-dashboard.html)

3. Test the widget tool:
   - Click on `widget_sales_chat` tool
   - Enter parameters:
     ```json
     {
       "plannerKey": "SalesAgentPlanner",
       "message": "Show me account summary"
     }
     ```
   - Click "Call Tool"
   - View the response with widget metadata

## Testing with ChatGPT (Production-like)

### Step 1: Expose Local Server

Use ngrok to create an HTTPS tunnel:

```powershell
# Install ngrok if not already installed
# Download from: https://ngrok.com/download

# Run ngrok
ngrok http 3000
```

Copy the HTTPS URL (e.g., `https://abc123.ngrok-free.app`)

### Step 2: Configure Custom GPT

1. Go to ChatGPT
2. Create a new Custom GPT or configure existing one
3. Add MCP connector configuration:
   ```
   MCP Server URL: https://abc123.ngrok-free.app/mcp/sse
   ```

### Step 3: Test in ChatGPT

Ask ChatGPT:
```
Use the widget_sales_chat tool to show me account information with planner key "SalesAgentPlanner"
```

ChatGPT should:
1. Call your MCP server
2. Execute the widget_sales_chat tool
3. Display the interactive widget

## Troubleshooting Tests

### Server Not Responding

**Check if server is running:**
```powershell
Get-Process | Where-Object {$_.ProcessName -eq "node"}
```

**Check port 3000:**
```powershell
netstat -ano | findstr :3000
```

**Kill process on port 3000 if needed:**
```powershell
# Find PID from netstat output
taskkill /F /PID <PID>
```

### Connection Refused

1. Verify server is running (see startup messages)
2. Check firewall isn't blocking port 3000
3. Try `http://127.0.0.1:3000/health` instead of `localhost`

### Widget Assets Not Loading

```powershell
# Rebuild widget
cd c:\Users\krsm\source\repos\MCPServer\Widget
npm run build

# Verify files exist
dir dist\

# Restart server
```

### TypeScript Compilation Errors

```powershell
cd c:\Users\krsm\source\repos\MCPServer\mcp-server-ts
npm run clean
npm install
npm run build
```

## Expected Test Results

### Health Check Response
```json
{
  "status": "healthy",
  "timestamp": "2025-12-03T10:00:00.000Z",
  "widgetAssetsLoaded": {
    "javascript": true,
    "css": true
  }
}
```

### List Tools Response
```json
{
  "jsonrpc": "2.0",
  "result": {
    "tools": [
      {
        "name": "widget_sales_chat",
        "description": "Chat with the Sales Agent Plugin API...",
        "inputSchema": {
          "type": "object",
          "properties": {
            "plannerKey": { "type": "string" },
            "message": { "type": "string" },
            "token": { "type": "string" }
          },
          "required": ["plannerKey", "message"]
        }
      }
    ]
  },
  "id": 1
}
```

### Widget Tool Call Response
```json
{
  "jsonrpc": "2.0",
  "result": {
    "content": [
      {
        "type": "text",
        "text": "{\"structuredContent\":{...},\"_meta\":{\"outputTemplate\":\"ui://widget/sales-dashboard.html\",...}}"
      }
    ]
  },
  "id": 2
}
```

## Testing Checklist

- [ ] Server starts without errors
- [ ] Widget JavaScript loaded
- [ ] Widget CSS loaded
- [ ] Health endpoint returns 200 OK
- [ ] Root endpoint returns API info
- [ ] MCP Inspector can connect
- [ ] Can list tools via MCP
- [ ] Can list resources via MCP
- [ ] Can call widget_sales_chat tool
- [ ] Widget resource returns HTML with correct MIME type
- [ ] Authentication headers are accepted (if configured)

## Next Steps After Testing

1. ✅ Local testing complete
2. 🔄 Test with ngrok + ChatGPT
3. 🔄 Deploy to production (Azure/AWS)
4. 🔄 Configure authentication (OBO flow)
5. 🔄 Add monitoring and logging

## Quick Test Commands

```powershell
# Quick health check
(Invoke-WebRequest -Uri http://localhost:3000/health -UseBasicParsing).Content | ConvertFrom-Json

# Quick tool list
@{jsonrpc="2.0";method="tools/list";id=1} | ConvertTo-Json | 
  % {Invoke-RestMethod -Uri http://localhost:3000/mcp/messages -Method Post -ContentType "application/json" -Body $_}

# Quick widget call
@{jsonrpc="2.0";method="tools/call";params=@{name="widget_sales_chat";arguments=@{plannerKey="Test";message="Hello"}};id=2} | ConvertTo-Json -Depth 5 | 
  % {Invoke-RestMethod -Uri http://localhost:3000/mcp/messages -Method Post -ContentType "application/json" -Body $_}
```
