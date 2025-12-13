# OBO Flow - Quick Reference

## What is OBO?

**On-Behalf-Of (OBO)** flow allows your gateway to exchange the incoming user token from Teams/Copilot for downstream service-specific tokens.

## When to Use OBO vs OAuthPluginVault?

### Use OBO Flow (Implemented Here) ✅
- You want **centralized authentication** at the gateway
- You need to **control token lifecycle** and caching
- You want **single consent experience** for users
- You're building a **unified API gateway** architecture
- You need to add **security policies** at the gateway level

### Use OAuthPluginVault (Current aiplugin_1.json)
- Each downstream server is **independent**
- Different teams own different services
- You want **simpler gateway code** (just proxy)
- Services already have their own auth infrastructure

## Configuration Steps (TL;DR)

### 1. Create Gateway App Registration
```
Azure Portal → App Registrations → New
- Name: MCP-Gateway
- Copy: Client ID, Tenant ID
- Create: Client Secret (save immediately!)
- Expose API: api://{clientId}
- Add Scope: access_as_user
```

### 2. Create Downstream App Registrations
```
For each downstream server:
- Create app registration
- Copy Client ID
- Expose API: api://{clientId}
- Add Scope: access_as_user
- Authorize Gateway: Add client app (Gateway's Client ID)
```

### 3. Grant Permissions
```
Gateway App Registration:
- API Permissions → Add permission → My APIs
- Select each downstream app
- Check: access_as_user (Delegated)
- IMPORTANT: Click "Grant admin consent"
```

### 4. Update appsettings.json
```json
{
  "AzureAd": {
    "TenantId": "YOUR_TENANT_ID",
    "ClientId": "YOUR_GATEWAY_CLIENT_ID",
    "Audience": "api://YOUR_GATEWAY_CLIENT_ID"
  },
  "McpGateway": {
    "DownstreamServers": [
      {
        "Name": "ServerName",
        "OboConfig": {
          "Enabled": true,
          "ClientId": "YOUR_GATEWAY_CLIENT_ID",
          "ClientSecret": "YOUR_GATEWAY_SECRET",
          "TenantId": "YOUR_TENANT_ID",
          "Scopes": ["api://DOWNSTREAM_CLIENT_ID/.default"]
        }
      }
    ]
  }
}
```

### 5. Store Secrets (Development)
```powershell
cd MCPServer
dotnet user-secrets set "McpGateway:DownstreamServers:0:OboConfig:ClientSecret" "your-secret"
```

## How It Works

```
┌─────────────┐
│ User (Teams)│
└──────┬──────┘
       │ 1. User Token
       ↓
┌─────────────────┐
│  MCP Gateway    │
│  - Validates    │
│  - Extracts     │
└──────┬──────────┘
       │ 2. OBO Exchange
       ↓
┌──────────────────┐
│ Microsoft Entra  │
│ (Azure AD)       │
└──────┬───────────┘
       │ 3. Downstream Token
       ↓
┌─────────────────┐
│ MCP Gateway     │
└──────┬──────────┘
       │ 4. Call with new token
       ↓
┌─────────────────┐
│ Downstream MCP  │
│ Server          │
└─────────────────┘
```

## Code Flow in Your Gateway

1. **Request comes in** with `Authorization: Bearer <token>`
2. **OboTokenService.GetIncomingToken()** extracts token
3. **OboTokenService.GetAccessTokenAsync(serverName, token)** exchanges it
4. **McpGatewayService** adds new token to downstream request
5. **Downstream server** receives properly scoped token

## Key Files

| File | Purpose |
|------|---------|
| `Services/OboTokenService.cs` | Handles token exchange and caching |
| `Gateway/McpGatewayService.cs` | Routes requests with OBO tokens |
| `Program.cs` | Configures authentication and DI |
| `appsettings.json` | OBO configuration per server |

## Token Caching

- Tokens are **cached in-memory** per user per server
- **Automatic expiration** with 5-minute buffer
- **Background cleanup** every 10 minutes
- For production: Consider **Redis** or **distributed cache**

## Logs to Monitor

### Success
```
[OBO] Successfully acquired token for WinWireMcpServer | Scopes=api://xxx/.default
[OBO] Added OBO token to request for WinWireMcpServer
```

### Failure
```
[OBO] Failed to acquire token for WinWireMcpServer | Error=... | ErrorCode=...
```

## Common Errors

| Error | Cause | Solution |
|-------|-------|----------|
| AADSTS65001 | No consent | Grant admin consent in Gateway app |
| AADSTS50013 | Bad signature | Check client secret is correct |
| AADSTS700016 | App not found | Verify downstream Client ID in scopes |
| User consent required | Not pre-authorized | Add Gateway as authorized client in downstream app |

## Testing

### With Real Token from Teams
```
Authorization: Bearer eyJ0eXAiOiJKV1QiLCJhbGci...
```

### With MCP Inspector
```powershell
npx @modelcontextprotocol/inspector
```
Add Authorization header manually.

### Check Token Acquisition
Look for these log entries:
1. Gateway receives request
2. OBO token exchange initiated
3. Token acquired successfully
4. Token added to downstream request
5. Downstream call succeeds

## Migrating from OAuthPluginVault

Your current `aiplugin_1.json` uses:
```json
{
  "auth": {
    "type": "OAuthPluginVault",
    "reference_id": "NzJmOTg4YmYtODZmMS00MWFmLTkxYWItMmQ3Y2QwMTFkYjQ3IyMxNjUzYzY4MC01MzQ3LTQwNDItOTA3Ny1jYzRlOGU2M2U1ODE="
  }
}
```

**To use OBO instead:**
1. Remove `auth` section from runtimes
2. Configure OBO in your gateway (done! ✅)
3. Update aiplugin to point to your gateway
4. Gateway handles all downstream auth

```json
{
  "runtimes": [
    {
      "type": "RemoteMCPServer",
      "spec": {
        "url": "https://your-gateway.azurewebsites.net"
      },
      "auth": {
        "type": "OAuthPluginVault",
        "reference_id": "YOUR_GATEWAY_REFERENCE_ID"
      },
      "run_for_functions": ["call_downstream_tool"]
    }
  ]
}
```

Now **one auth config** for gateway, gateway handles all downstream OBO flows!

## Production Checklist

- [ ] All app registrations created
- [ ] Admin consent granted for all API permissions
- [ ] Client secrets stored in Azure Key Vault
- [ ] Token caching strategy reviewed (consider distributed cache)
- [ ] Monitoring and alerting configured
- [ ] Secret rotation policy documented
- [ ] Test OBO flow for each downstream server
- [ ] Verify token expiration handling
- [ ] Load test token caching under concurrent requests

## Support

For detailed setup instructions, see: [OBO_CONFIGURATION_GUIDE.md](./OBO_CONFIGURATION_GUIDE.md)
