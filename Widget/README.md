# MCP Server Widget

This directory contains the ChatGPT widget for your MCP Server. The widget provides an interactive UI for displaying sales data, account information, and other tool outputs.

## 🏗️ Architecture

The widget follows the [OpenAI Apps SDK](https://developers.openai.com/apps-sdk/build/mcp-server) architecture:

1. **MCP Server** (C#/.NET) - Defines tools, handles authentication, returns data
2. **Widget Bundle** (React/TypeScript) - Renders inside ChatGPT's iframe
3. **ChatGPT Model** - Decides when to call tools based on user prompts

## 📁 Structure

```
Widget/
├── src/
│   ├── index.tsx       # React widget component
│   └── styles.css      # Widget styling
├── dist/               # Build output (generated)
│   ├── widget.js
│   └── widget.css
├── package.json        # Dependencies and build scripts
└── tsconfig.json       # TypeScript configuration
```

## 🚀 Quick Start

### 1. Install Dependencies

```bash
cd Widget
npm install
```

### 2. Build the Widget

```bash
npm run build
```

This compiles the React/TypeScript code into bundled JS and CSS files in the `dist/` directory.

### 3. Run the MCP Server

```bash
cd ..
dotnet run --project MCPServer
```

The server automatically loads the widget assets on startup.

### 4. Test with MCP Inspector

Install the MCP Inspector to test your widget locally:

```bash
npx @modelcontextprotocol/inspector
```

Connect to `http://localhost:<port>/mcp` and test your tools.

## 🛠️ Development

### Watch Mode

For active development, run the build in watch mode:

```bash
npm run watch
```

This automatically rebuilds when you change any source files.

### Widget Runtime API

The widget has access to `window.openai` which provides:

#### Data Access
- `toolInput` - Arguments passed to the tool
- `toolOutput` - Structured content from your MCP server
- `toolResponseMetadata` - Additional metadata (not visible to the model)
- `widgetState` - Persistent UI state

#### Actions
- `setWidgetState(state)` - Save UI state
- `callTool(name, args)` - Invoke another MCP tool
- `sendFollowUpMessage({ prompt })` - Ask ChatGPT to send a message
- `openExternal({ href })` - Open a link in the user's browser

#### Context
- `theme` - "light" or "dark"
- `displayMode` - Current display mode
- `locale` - User's locale (e.g., "en-US")

### Example: Reading Tool Output

```typescript
const data = window.openai.toolOutput;
// Data structure from your C# tool response
console.log(data.summary, data.items);
```

### Example: Calling a Tool from the Widget

```typescript
const result = await window.openai.callTool('SummarizeAccountProfile', {
  userPrompt: 'Refresh data',
  context: ''
});
```

## 🔧 Customization

### Modify the Widget UI

Edit `src/index.tsx` to customize:
- Layout and components
- Data visualization
- User interactions
- State management

### Update Styling

Edit `src/styles.css` to change:
- Colors and themes
- Typography
- Spacing and layout
- Responsive behavior

### Add New Features

1. Update the React component in `src/index.tsx`
2. Rebuild with `npm run build`
3. Restart the MCP server
4. The widget automatically picks up changes

## 📦 Widget Template Registration

The widget template is registered in `WidgetResourceService.cs`:

```csharp
public Resource CreateWidgetResource()
{
    return new Resource
    {
        Uri = "ui://widget/sales-dashboard.html",
        Name = "Sales Dashboard Widget",
        MimeType = "text/html+skybridge", // Required for ChatGPT widgets
        Annotations = new Dictionary<string, object>
        {
            ["openai/widgetPrefersBorder"] = true,
            ["openai/widgetDomain"] = "https://chatgpt.com",
            ["openai/widgetDescription"] = "Interactive sales dashboard",
            ["openai/widgetCSP"] = new Dictionary<string, object>
            {
                ["connect_domains"] = new[] { "https://chatgpt.com" },
                ["resource_domains"] = new[] { "https://*.oaistatic.com" }
            }
        }
    };
}
```

### Key Points:
- `mimeType: "text/html+skybridge"` - Signals ChatGPT to treat it as a widget
- `openai/widgetCSP` - Content Security Policy for allowed domains
- `openai/widgetDomain` - Dedicated origin for the widget sandbox

## 🔐 Security

### Widget Sandbox
- Widgets run in a sandboxed iframe
- Cannot access parent window or other widgets
- Can only communicate via `window.openai` API

### CSP Configuration
Add allowed domains to the CSP in `WidgetResourceService.cs`:

```csharp
["openai/widgetCSP"] = new Dictionary<string, object>
{
    ["connect_domains"] = new[] { 
        "https://chatgpt.com",
        "https://your-api.example.com" 
    },
    ["resource_domains"] = new[] { 
        "https://*.oaistatic.com",
        "https://cdn.example.com"
    }
}
```

### Best Practices
- Never embed API keys or secrets in widget code
- Validate all data from `toolOutput` before rendering
- Use your MCP server for authentication and authorization
- Sanitize user inputs before calling tools

## 🐛 Troubleshooting

### Widget doesn't render
- Ensure `mimeType` is `"text/html+skybridge"`
- Check that the build output exists in `dist/`
- Verify the server loads assets (check logs)

### `window.openai` is undefined
- Only available in `text/html+skybridge` templates
- Check browser console for CSP violations
- Ensure widget is loaded via ChatGPT

### Stale bundles
- Clear browser cache
- Change the template URI when deploying breaking changes
- Rebuild with `npm run build`

### Build errors
```bash
# Clear and reinstall dependencies
rm -rf node_modules package-lock.json
npm install
npm run build
```

## 📚 Resources

- [OpenAI Apps SDK Documentation](https://developers.openai.com/apps-sdk)
- [MCP Server Guide](https://developers.openai.com/apps-sdk/build/mcp-server)
- [ChatGPT UI Guide](https://developers.openai.com/apps-sdk/build/chatgpt-ui)
- [Model Context Protocol](https://modelcontextprotocol.io/)

## 🚀 Deployment

### For Development
1. Run locally with ngrok or similar:
   ```bash
   ngrok http <port>
   ```
2. Use the HTTPS URL in ChatGPT developer mode

### For Production
1. Build the widget: `npm run build`
2. Deploy your MCP server to an HTTPS endpoint
3. Ensure widget assets are accessible
4. Configure CSP for production domains
5. Test thoroughly with MCP Inspector

## 💡 Examples

### Display a List

```typescript
const items = window.openai.toolOutput?.items || [];
return items.map(item => (
  <div key={item.id}>
    <h3>{item.title}</h3>
    <p>{item.description}</p>
  </div>
));
```

### Persist UI State

```typescript
const [widgetState, setWidgetState] = useWidgetState({
  selectedTab: 'overview'
});

setWidgetState(prev => ({ ...prev, selectedTab: 'details' }));
```

### Call Another Tool

```typescript
const handleRefresh = async () => {
  const result = await window.openai.callTool('GetAccountNews', {
    userPrompt: 'Latest news',
    context: ''
  });
  console.log('Refreshed:', result);
};
```

## 📝 Next Steps

1. **Customize the UI** - Modify `src/index.tsx` to match your data structure
2. **Add visualizations** - Install chart libraries and render data graphically
3. **Enhance interactions** - Add more buttons, forms, and user controls
4. **Optimize performance** - Lazy load components and optimize bundle size
5. **Add tests** - Write unit and integration tests for your widget

Happy coding! 🎉
