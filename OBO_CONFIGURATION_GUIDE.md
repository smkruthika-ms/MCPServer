# On-Behalf-Of (OBO) Flow Configuration Guide

This guide explains how to configure OBO authentication for your MCP Gateway to call downstream MCP servers.

## Overview

The OBO flow allows your gateway to:
1. Receive a token from Teams/Copilot (representing the user)
2. Exchange that token for a new token scoped to each downstream MCP server
3. Call downstream servers with properly scoped tokens

## Architecture

```
User (Teams/Copilot)
    ↓ [User Token]
MCP Gateway
    ↓ [OBO Exchange]
Microsoft Identity Platform
    ↓ [Downstream Token]
Downstream MCP Server
```

## Prerequisites

### 1. Gateway App Registration (Your MCP Server)

Create an app registration in Azure AD for your gateway:

1. Go to **Azure Portal** → **App Registrations** → **New Registration**
2. Name: `MCP-Gateway` (or your preferred name)
3. Supported account types: **Accounts in this organizational directory only**
4. Click **Register**

**Important Details to Save:**
- **Client ID**: Copy this (e.g., `54a6fe0f-d031-4f90-9c54-d607a980122d`)
- **Tenant ID**: Copy this (e.g., `72f988bf-86f1-41af-91ab-2d7cd011db47`)

**Configure Client Secret:**
1. Go to **Certificates & secrets** → **New client secret**
2. Description: `MCP-Gateway-OBO-Secret`
3. Expires: Choose expiration (e.g., 24 months)
4. Click **Add**
5. **Copy the secret value immediately** (you won't see it again)

**Expose an API:**
1. Go to **Expose an API**
2. Click **Set** next to Application ID URI → Use default `api://{clientId}` → **Save**
3. Click **Add a scope**
   - Scope name: `access_as_user`
   - Who can consent: **Admins and users**
   - Admin consent display name: `Access MCP Gateway as user`
   - Admin consent description: `Allows the app to access MCP Gateway on behalf of the signed-in user`
   - State: **Enabled**
   - Click **Add scope**

**API Permissions:**
For each downstream server, you need to grant permission:
1. Go to **API permissions** → **Add a permission**
2. Click **My APIs** tab
3. Select the downstream server's app registration
4. Check **Delegated permissions** → Select the scope (e.g., `access_as_user`)
5. Click **Add permissions**
6. **IMPORTANT**: Click **Grant admin consent for {your org}** at the top

### 2. Downstream Server App Registrations

Each downstream MCP server needs its own app registration:

**Example: WinWire MCP Server**

1. Go to **Azure Portal** → **App Registrations** → **New Registration**
2. Name: `WinWire-MCP-Server`
3. Supported account types: **Accounts in this organizational directory only**
4. Click **Register**
5. **Copy Client ID**: e.g., `1653c680-5347-4042-9077-cc4e8e63e581`

**Expose an API:**
1. Go to **Expose an API**
2. Set Application ID URI: `api://1653c680-5347-4042-9077-cc4e8e63e581`
3. Add a scope:
   - Scope name: `access_as_user`
   - Who can consent: **Admins and users**
   - Enable the scope

**Authorized client applications (IMPORTANT for OBO):**
1. Still in **Expose an API** section
2. Click **Add a client application**
3. Client ID: Enter your **Gateway's Client ID** (from step 1)
4. Check the scope `access_as_user`
5. Click **Add application**

This pre-authorizes your gateway to request tokens on behalf of users.

Repeat this process for each downstream server (CEHub, RVC, etc.)

### 3. Update appsettings.json

Update your `appsettings.json` with the correct values:

```json
{
  "AzureAd": {
    "Instance": "https://login.microsoftonline.com/",
    "TenantId": "72f988bf-86f1-41af-91ab-2d7cd011db47",
    "ClientId": "54a6fe0f-d031-4f90-9c54-d607a980122d",
    "Audience": "api://54a6fe0f-d031-4f90-9c54-d607a980122d"
  },
  "McpGateway": {
    "DownstreamServers": [
      {
        "Name": "WinWireMcpServer",
        "BaseUrl": "https://apim-setai-ww-uat.azure-api.net/wwai-uat-api-mcp",
        "McpEndpoint": "/mcp",
        "IsEnabled": true,
        "ToolNames": ["winwireSucessStories"],
        "OboConfig": {
          "Enabled": true,
          "ClientId": "54a6fe0f-d031-4f90-9c54-d607a980122d",
          "ClientSecret": "YOUR_GATEWAY_CLIENT_SECRET_HERE",
          "TenantId": "72f988bf-86f1-41af-91ab-2d7cd011db47",
          "Scopes": [
            "api://1653c680-5347-4042-9077-cc4e8e63e581/.default"
          ]
        }
      }
    ]
  }
}
```

**Important Notes:**
- `ClientId` and `ClientSecret` in `OboConfig` are your **Gateway's** credentials
- `Scopes` array contains the downstream server's API scope
- Use `/.default` scope for OBO flows (requests all pre-consented permissions)

### 4. Store Secrets Securely

**For Development (User Secrets):**
```powershell
cd MCPServer
dotnet user-secrets set "AzureAd:ClientId" "54a6fe0f-d031-4f90-9c54-d607a980122d"
dotnet user-secrets set "McpGateway:DownstreamServers:0:OboConfig:ClientSecret" "your-secret-here"
dotnet user-secrets set "McpGateway:DownstreamServers:1:OboConfig:ClientSecret" "your-secret-here"
```

**For Production (Azure Key Vault or App Configuration):**
```json
{
  "McpGateway": {
    "DownstreamServers": [
      {
        "OboConfig": {
          "ClientSecret": "@Microsoft.KeyVault(SecretUri=https://your-vault.vault.azure.net/secrets/gateway-secret)"
        }
      }
    ]
  }
}
```

## Testing OBO Flow

### 1. Get a Token from Teams/Copilot

When Copilot calls your gateway, it will include a token in the `Authorization` header:
```
Authorization: Bearer eyJ0eXAiOiJKV1QiLCJhbGci...
```

### 2. Verify Token Exchange

Check your logs for OBO flow activity:
```
[OBO] Successfully acquired token for WinWireMcpServer | Scopes=api://1653c680-5347-4042-9077-cc4e8e63e581/.default
[OBO] Added OBO token to request for WinWireMcpServer
```

### 3. Test with MCP Inspector

Use the MCP Inspector to test locally:
```powershell
npx @modelcontextprotocol/inspector
```

You'll need to manually add an Authorization header with a valid token.

## Troubleshooting

### Error: "AADSTS65001: The user or administrator has not consented"

**Solution:** Grant admin consent for API permissions in your Gateway app registration.

### Error: "AADSTS50013: Assertion failed signature validation"

**Solution:** Check that your Gateway's Client Secret is correct and not expired.

### Error: "AADSTS700016: Application not found in the directory"

**Solution:** Verify the downstream server's Client ID in the Scopes configuration.

### Error: "User consent required"

**Solution:** Ensure the downstream server has added your Gateway as an authorized client application (see step 2 above).

## Security Best Practices

1. **Rotate Secrets Regularly**: Set expiration on client secrets and rotate before expiry
2. **Use Managed Identity**: For Azure-hosted services, consider using Managed Identity instead of client secrets
3. **Least Privilege**: Only request the minimum scopes needed
4. **Token Caching**: The OBO service caches tokens for performance - tokens are cached per user per downstream service
5. **Monitor Token Usage**: Check logs regularly for failed token acquisitions

## Configuration Reference

### OboConfiguration Properties

| Property | Type | Required | Description |
|----------|------|----------|-------------|
| `Enabled` | bool | Yes | Enable/disable OBO for this server |
| `ClientId` | string | Yes | Your Gateway's Client ID |
| `ClientSecret` | string | Yes | Your Gateway's Client Secret |
| `TenantId` | string | Yes | Azure AD Tenant ID |
| `Scopes` | string[] | Yes | Downstream API scopes to request |

### Scopes Format

For OBO flows, use one of these formats:

**Option 1: .default scope (recommended)**
```json
"Scopes": ["api://downstream-app-id/.default"]
```
Requests all pre-consented delegated permissions.

**Option 2: Specific scope**
```json
"Scopes": ["api://downstream-app-id/access_as_user"]
```
Requests only the specific scope.

## Example: Complete Configuration for Multiple Servers

```json
{
  "AzureAd": {
    "Instance": "https://login.microsoftonline.com/",
    "TenantId": "72f988bf-86f1-41af-91ab-2d7cd011db47",
    "ClientId": "54a6fe0f-d031-4f90-9c54-d607a980122d",
    "Audience": "api://54a6fe0f-d031-4f90-9c54-d607a980122d"
  },
  "McpGateway": {
    "DownstreamServers": [
      {
        "Name": "WinWireMcpServer",
        "BaseUrl": "https://apim-setai-ww-uat.azure-api.net/wwai-uat-api-mcp",
        "McpEndpoint": "/mcp",
        "IsEnabled": true,
        "ToolNames": ["winwireSucessStories"],
        "OboConfig": {
          "Enabled": true,
          "ClientId": "54a6fe0f-d031-4f90-9c54-d607a980122d",
          "ClientSecret": "SECRET1",
          "TenantId": "72f988bf-86f1-41af-91ab-2d7cd011db47",
          "Scopes": ["api://1653c680-5347-4042-9077-cc4e8e63e581/.default"]
        }
      },
      {
        "Name": "CEHubCopilot",
        "BaseUrl": "https://cehubskcopilot-sit.azurewebsites.net",
        "McpEndpoint": "/",
        "IsEnabled": true,
        "ToolNames": ["customer_engagement_hub_copilot"],
        "OboConfig": {
          "Enabled": true,
          "ClientId": "54a6fe0f-d031-4f90-9c54-d607a980122d",
          "ClientSecret": "SECRET1",
          "TenantId": "72f988bf-86f1-41af-91ab-2d7cd011db47",
          "Scopes": ["api://d88d4ffd-f658-4afd-bda7-4c0e45edd4b8/.default"]
        }
      },
      {
        "Name": "LocalDevServer",
        "BaseUrl": "https://localhost:5001",
        "IsEnabled": false,
        "OboConfig": {
          "Enabled": false
        }
      }
    ]
  }
}
```

## Next Steps

1. Create app registrations for all downstream servers
2. Configure API permissions and authorized clients
3. Update appsettings.json with correct Client IDs and Scopes
4. Store secrets securely (User Secrets for dev, Key Vault for prod)
5. Test OBO flow with each downstream server
6. Monitor logs for successful token exchanges
