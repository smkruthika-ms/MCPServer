# MCP Server with Widget Tool - Setup Guide

## Quick Start

Follow these steps to set up and run the TypeScript MCP server with widget support:

### 1. Navigate to the TypeScript Server Directory

```powershell
cd mcp-server-ts
```

### 2. Install Dependencies

```powershell
npm install
```

This installs:
- `@modelcontextprotocol/sdk` - MCP TypeScript SDK
- `express` - Web server framework
- `dotenv` - Environment variable management
- TypeScript and type definitions

### 3. Configure Environment Variables

Copy the example environment file:

```powershell
cp .env.example .env
```

Edit `.env` and set your values:

```env
PORT=3000
NODE_ENV=development

# Azure AD Configuration
TENANT_ID=72f988bf-86f1-41af-91ab-2d7cd011db47
CLIENT_ID=your-client-id-here
CLIENT_SECRET=your-client-secret-here

# API Configuration
SALES_AGENT_API_URL=https://salescopilotskeusuat.azurewebsites.net/api/Playground
APP_SCOPE=api://your-app-id/.default
```

### 4. Build the React Widget

The TypeScript server loads the React widget from the `Widget` directory. Build it first:

```powershell
cd ..\Widget
npm install
npm run build
cd ..\mcp-server-ts
```

This creates `Widget/dist/widget.js` and `widget.css` that the server will load.

### 5. Build the TypeScript Server

```powershell
npm run build
```

This compiles TypeScript to JavaScript in the `dist/` directory.

### 6. Start the Server

```powershell
npm start
```

You should see:

```
🚀 MCP Server with Widget Tool running on port 3000
📡 MCP endpoint: http://localhost:3000/mcp
💓 Health check: http://localhost:3000/health
🎨 Widget assets loaded: JS=true, CSS=true
```

### 7. Verify Server is Running

Test the health endpoint:

```powershell
curl http://localhost:3000/health
```

Expected response:

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

## Development Mode

For active development with auto-reload:

```powershell
npm run dev
```

This runs TypeScript compiler in watch mode and restarts the server on file changes.

## Testing with MCP Inspector

Install and run the MCP Inspector to test your server:

```powershell
npx @modelcontextprotocol/inspector
```

Connect to: `http://localhost:3000/mcp`

## Testing with ChatGPT (Local Development)

### 1. Expose Your Local Server

Use ngrok to create an HTTPS tunnel:

```powershell
ngrok http 3000
```

Copy the HTTPS URL (e.g., `https://abc123.ngrok.io`)

### 2. Create a Custom GPT

1. Go to ChatGPT
2. Create a new Custom GPT
3. Configure the MCP connector with your ngrok URL:
   ```
   https://abc123.ngrok.io/mcp
   ```

### 3. Test the Widget Tool

Ask ChatGPT:
```
Use the widget_sales_chat tool to show me account information
```

The response should include the interactive widget!

## Troubleshooting

### Widget Assets Not Loading

If you see `Widget assets loaded: JS=false, CSS=false`:

1. Build the widget:
   ```powershell
   cd ..\Widget
   npm run build
   ```

2. Verify the files exist:
   ```powershell
   dir ..\Widget\dist\
   ```

3. Restart the TypeScript server

### TypeScript Compilation Errors

Clear and rebuild:

```powershell
npm run clean
npm install
npm run build
```

### Port Already in Use

Change the port in `.env`:

```env
PORT=3001
```

Or kill the process using port 3000:

```powershell
# Find process on port 3000
netstat -ano | findstr :3000

# Kill the process (replace PID)
taskkill /F /PID <PID>
```

### Token Authentication Errors

1. Verify your token is valid and not expired
2. Check the `Authorization` header format: `Bearer <token>`
3. Ensure the token has the correct audience and scopes

## Project Structure

```
mcp-server-ts/
├── src/
│   ├── index.ts                          # Main server
│   ├── services/
│   │   ├── widgetResourceService.ts      # Widget resource
│   │   └── salesAgentApiService.ts       # API client
│   └── tools/
│       └── widgetTool.ts                 # Tool definition
├── dist/                                 # Compiled JS (after build)
├── package.json
├── tsconfig.json
├── .env                                  # Your config (create from .env.example)
├── .env.example                          # Example config
├── .gitignore
├── README.md
└── SETUP.md                              # This file
```

## Next Steps

1. ✅ Server is running
2. ✅ Widget assets are loaded
3. 🔄 Implement authentication (OBO flow, managed identity)
4. 🔄 Customize the widget UI in `Widget/src/index.tsx`
5. 🔄 Add more tools to the MCP server
6. 🔄 Deploy to production (Azure, AWS, etc.)

## Additional Resources

- [MCP TypeScript SDK Documentation](https://github.com/modelcontextprotocol/typescript-sdk)
- [OpenAI Apps SDK](https://developers.openai.com/apps-sdk)
- [Widget Development Guide](../Widget/README.md)
- [Model Context Protocol](https://modelcontextprotocol.io/)

## Support

For issues or questions:
1. Check the troubleshooting section above
2. Review the main README.md
3. Check server logs for error messages
4. Verify all environment variables are set correctly
