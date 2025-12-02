# 🎨 ChatGPT Widget for MCP Server

Your MCP Server now supports **interactive widgets** for ChatGPT using the [OpenAI Apps SDK](https://developers.openai.com/apps-sdk)!

## ✨ What's New

Your server can now return rich, interactive UI components that render inside ChatGPT:

- 📊 **Interactive dashboards** for sales data
- 🎯 **Clickable UI elements** (tabs, buttons, cards)
- 💾 **Persistent state** across interactions
- 🔄 **Widget-initiated tool calls** to refresh data
- 🎨 **Theme-aware** (light/dark mode support)

## 🏗️ Architecture

```
┌─────────────────────────────────────────────────────────────┐
│                         ChatGPT                              │
│  ┌──────────────────────────────────────────────────────┐   │
│  │  Widget Iframe (Sandboxed)                           │   │
│  │  - Renders your React component                      │   │
│  │  - Reads data from window.openai.toolOutput          │   │
│  │  - Can call tools via window.openai.callTool()       │   │
│  └──────────────────────────────────────────────────────┘   │
│                           ↕                                   │
│  ┌──────────────────────────────────────────────────────┐   │
│  │  ChatGPT Model                                        │   │
│  │  - Decides when to call your MCP tools               │   │
│  │  - Narrates the experience using structured data     │   │
│  └──────────────────────────────────────────────────────┘   │
└─────────────────────────────────────────────────────────────┘
                            ↕
┌─────────────────────────────────────────────────────────────┐
│                    Your MCP Server (.NET)                    │
│  - Handles authentication                                    │
│  - Executes tool logic                                       │
│  - Returns structured data + widget metadata                 │
│  - Serves widget template (HTML+JS+CSS)                      │
└─────────────────────────────────────────────────────────────┘
```

## 🚀 Quick Start

### 1. Build the Widget

```powershell
cd Widget
npm install
npm run build
```

This creates `dist/widget.js` and `dist/widget.css`.

### 2. Run the MCP Server

```powershell
cd ..
dotnet run --project MCPServer
```

The server automatically loads the widget assets on startup.

### 3. Test with ChatGPT

1. Expose your server via HTTPS (ngrok for local testing)
2. Create a connector in ChatGPT developer mode
3. Ask ChatGPT to call one of your tools
4. See the interactive widget render!

## 📋 What Was Created

### New Files

```
MCPServer/
├── Widget/                               # New widget directory
│   ├── src/
│   │   ├── index.tsx                     # React widget component
│   │   └── styles.css                    # Widget styles
│   ├── dist/                             # Build output (after npm run build)
│   │   ├── widget.js
│   │   └── widget.css
│   ├── package.json                      # npm configuration
│   ├── tsconfig.json                     # TypeScript config
│   └── README.md                         # Widget documentation
│
├── MCPServer/
│   ├── Services/
│   │   └── WidgetResourceService.cs      # Manages widget template
│   ├── Extensions/
│   │   └── McpWidgetExtensions.cs        # Widget registration helpers
│   └── Models/
│       └── WidgetToolResponse.cs         # Response model for tools
```

### Modified Files

- `MCPServer/Program.cs` - Added widget service registration and initialization

## 🎯 How It Works

### 1. Widget Template Registration

The `WidgetResourceService` serves your widget HTML with the special MIME type `text/html+skybridge`:

```csharp
var resource = new Resource
{
    Uri = "ui://widget/sales-dashboard.html",
    MimeType = "text/html+skybridge", // This tells ChatGPT it's a widget
    // ... widget metadata
};
```

### 2. Tool Returns Widget Metadata

Your tools can specify which widget to use:

```csharp
// Example of returning data with widget metadata
return new 
{
    structuredContent = new { summary = "Account info..." },
    _meta = new 
    {
        outputTemplate = "ui://widget/sales-dashboard.html",
        widgetAccessible = true,
        detailedData = new { /* extra data for widget only */ }
    }
};
```

### 3. Widget Renders in ChatGPT

The widget reads data from `window.openai`:

```typescript
const data = window.openai.toolOutput;
// Display the data in your React component
```

## 🔧 Customizing Your Widget

### Modify the UI

Edit `Widget/src/index.tsx` to customize how data is displayed:

```typescript
const SalesWidget: React.FC = () => {
  const toolOutput = useOpenAI(openai => openai.toolOutput);
  
  return (
    <div>
      <h2>{toolOutput?.title}</h2>
      {/* Your custom UI here */}
    </div>
  );
};
```

### Update Styling

Edit `Widget/src/styles.css` to change colors, layout, etc.

### Rebuild

```powershell
cd Widget
npm run build
```

Then restart your MCP server.

## 🎨 Widget Features

### Data Access

```typescript
// Tool input arguments
const input = window.openai.toolInput;

// Structured content (model and widget can see)
const output = window.openai.toolOutput;

// Metadata (only widget can see)
const metadata = window.openai.toolResponseMetadata;
```

### Persistent State

```typescript
// Save state that persists across rerenders
const [state, setState] = useWidgetState({ selectedTab: 'overview' });

setState(prev => ({ ...prev, selectedTab: 'details' }));
```

### Call Tools from Widget

```typescript
// Refresh data by calling another tool
const result = await window.openai.callTool('GetAccountNews', {
  userPrompt: 'Latest updates',
  context: ''
});
```

### Theme Support

```typescript
// Automatically adapts to ChatGPT's theme
const theme = useOpenAI(openai => openai.theme); // "light" or "dark"
```

## 🔐 Security

### Widget Sandbox
- Widgets run in a sandboxed iframe
- Can only access `window.openai` API
- Cannot access parent window or other data

### Content Security Policy
Configure allowed domains in `WidgetResourceService.cs`:

```csharp
["openai/widgetCSP"] = new Dictionary<string, object>
{
    ["connect_domains"] = new[] { "https://chatgpt.com", "https://your-api.com" },
    ["resource_domains"] = new[] { "https://*.oaistatic.com" }
}
```

### Best Practices
- ✅ Authenticate in your MCP server, not the widget
- ✅ Never embed API keys in widget code
- ✅ Validate all data before rendering
- ✅ Use HTTPS for production

## 📚 Your Existing Tools

Your MCP server already has these tools that can now use widgets:

1. **SummarizeAccountProfile** - Account highlights
2. **GetAccountNews** - News highlights
3. **GetAccountCompetitor** - Competitor analysis
4. **GetAccountTeam** - Team members
5. **GetLeadingPrompts** - Suggested prompts

Each tool can return data that the widget will display interactively!

## 🛠️ Development Workflow

1. **Make changes** to `Widget/src/index.tsx` or `Widget/src/styles.css`
2. **Rebuild**: `npm run build` (or `npm run watch` for auto-rebuild)
3. **Restart** the MCP server
4. **Test** in ChatGPT or MCP Inspector

## 📖 Resources

- **Widget README**: `Widget/README.md` - Detailed widget documentation
- **OpenAI Apps SDK**: https://developers.openai.com/apps-sdk
- **MCP Server Guide**: https://developers.openai.com/apps-sdk/build/mcp-server
- **ChatGPT UI Guide**: https://developers.openai.com/apps-sdk/build/chatgpt-ui

## 🐛 Troubleshooting

### Widget doesn't show
- Check that `Widget/dist/widget.js` exists (run `npm run build`)
- Verify server logs show "Loaded widget JavaScript"
- Ensure MIME type is `text/html+skybridge`

### `window.openai` is undefined
- Widget must be served with `text/html+skybridge` MIME type
- Check browser console for CSP violations

### Changes not appearing
- Rebuild widget: `npm run build`
- Restart MCP server
- Clear browser cache

## 🎉 Next Steps

1. ✅ **Widget structure created**
2. ✅ **Build system configured**
3. ✅ **Widget service integrated**
4. 🔄 **Build the widget**: `cd Widget && npm run build`
5. 🔄 **Test locally**: Run server and use ngrok
6. 🔄 **Customize UI**: Edit `Widget/src/index.tsx` to match your data
7. 🔄 **Deploy**: Push to production HTTPS endpoint

## 💡 Example: Updating a Tool for Widgets

To make a tool return widget-friendly data:

```csharp
[McpServerTool, Description("Get account summary with interactive widget")]
public async Task<object> GetAccountSummaryWithWidget(
    IMcpServer server, 
    string accountId, 
    CancellationToken cancellationToken)
{
    var data = await _service.GetAccountDataAsync(accountId);
    
    return new 
    {
        // Data the model reads and narrates
        structuredContent = new 
        {
            summary = $"Account {accountId} has {data.TeamSize} team members",
            highlights = data.KeyHighlights
        },
        
        // Metadata only for the widget (rich detailed data)
        _meta = new 
        {
            outputTemplate = "ui://widget/sales-dashboard.html",
            widgetAccessible = true,
            widgetDescription = "Interactive account dashboard",
            
            // Detailed data the widget can use
            accountDetails = data,
            teamMembers = data.Team,
            recentActivity = data.Activity
        }
    };
}
```

---

**Need help?** Check `Widget/README.md` for detailed widget documentation!
