# OBO with Managed Identity - Azure Configuration Guide

This guide shows you how to configure **On-Behalf-Of (OBO) authentication using Managed Identity** instead of client secrets. This is the **recommended approach** for production deployments in Azure.

## Why Managed Identity?

✅ **No secrets to manage** - Azure handles credentials automatically  
✅ **Automatic rotation** - No expiration or manual rotation needed  
✅ **Better security** - Secrets never leave Azure  
✅ **Compliance** - Meets security best practices  
✅ **Simplified deployment** - No secret storage required

## Architecture Overview

```
User (Teams/Copilot)
    ↓ [User Token]
MCP Gateway (Azure App Service with Managed Identity)
    ↓ [Managed Identity gets OBO token]
Microsoft Entra ID
    ↓ [Downstream Token]
Downstream MCP Servers
```

## Prerequisites

- Your MCP Gateway must be hosted in Azure (App Service, Container Apps, AKS, etc.)
- You need Owner or User Access Administrator role to assign permissions

## Step-by-Step Configuration

### 1. Enable Managed Identity on Your App Service

#### Option A: User-Assigned Managed Identity (Recommended)

**Create User-Assigned Managed Identity:**
```bash
az identity create \
  --name mcpgateway-identity \
  --resource-group your-resource-group \
  --location eastus
```

**Copy the Client ID (you'll need this):**
```bash
az identity show \
  --name mcpgateway-identity \
  --resource-group your-resource-group \
  --query clientId -o tsv
```

**Assign to App Service:**
```bash
az webapp identity assign \
  --name your-mcpgateway-app \
  --resource-group your-resource-group \
  --identities /subscriptions/{subscription-id}/resourcegroups/{resource-group}/providers/Microsoft.ManagedIdentity/userAssignedIdentities/mcpgateway-identity
```

**Or via Azure Portal:**
1. Go to your App Service → **Identity**
2. Go to **User assigned** tab
3. Click **Add**
4. Select your managed identity → **Add**
5. **Copy the Client ID** (e.g., `99453c85-3919-4a0e-b5e1-691d6986b018`)

#### Option B: System-Assigned Managed Identity (Simpler)

**Via Azure Portal:**
1. Go to your App Service → **Identity**
2. Go to **System assigned** tab
3. Toggle **Status** to **On** → **Save**
4. **Copy the Object (principal) ID**

**Via CLI:**
```bash
az webapp identity assign \
  --name your-mcpgateway-app \
  --resource-group your-resource-group
```

### 2. Configure Your Gateway App Registration

Your gateway app (`5d437e13-722e-4a8d-8f4f-3158cf570e94`) needs additional configuration:

#### A. Add Federated Credentials for Managed Identity

**Via Azure Portal:**
1. Go to **Azure Portal** → **App Registrations**
2. Find your gateway app: `5d437e13-722e-4a8d-8f4f-3158cf570e94`
3. Go to **Certificates & secrets** → **Federated credentials** tab
4. Click **Add credential**
5. Select **Azure resources** as the federated credential scenario
6. Configure:
   - **Subscription**: Your subscription
   - **Resource type**: App Service
   - **Resource**: Your MCP Gateway app
   - **Name**: `mcpgateway-federated-credential`
   - **Audience**: `api://AzureADTokenExchange`
7. Click **Add**

**This tells Azure AD**: "This app registration trusts tokens from this Managed Identity"

**Note**: With Federated Identity Credentials, you **do NOT need** to add API permissions to your gateway app registration. The authorization happens at the downstream server level (Step 3 below).

### 3. Configure Each Downstream Server

For **each** downstream server, you need to authorize your gateway's Managed Identity.

#### Example: WinWire MCP Server (Client ID: `1653c680-5347-4042-9077-cc4e8e63e581`)

**Step 1: Authorize Gateway App Registration**
1. Go to **App Registrations** → **WinWire MCP Server**
2. Go to **Expose an API**
3. Scroll to **Authorized client applications**
4. Click **Add a client application**
5. **Client ID**: `5d437e13-722e-4a8d-8f4f-3158cf570e94` (your gateway app)
6. Check the scope: `access_as_user`
7. Click **Add application**

**Step 2: Grant App Role to Managed Identity (Optional but Recommended)**

If the downstream server requires it:

```bash
# Get the Managed Identity's Object ID
MI_OBJECT_ID=$(az identity show \
  --name mcpgateway-identity \
  --resource-group your-resource-group \
  --query principalId -o tsv)

# Get the downstream app's Service Principal Object ID
DOWNSTREAM_SP_ID=$(az ad sp show \
  --id 1653c680-5347-4042-9077-cc4e8e63e581 \
  --query id -o tsv)

# Assign the Managed Identity to the app (requires Microsoft Graph permissions)
az rest --method POST \
  --uri "https://graph.microsoft.com/v1.0/servicePrincipals/$DOWNSTREAM_SP_ID/appRoleAssignments" \
  --body "{
    \"principalId\": \"$MI_OBJECT_ID\",
    \"resourceId\": \"$DOWNSTREAM_SP_ID\",
    \"appRoleId\": \"00000000-0000-0000-0000-000000000000\"
  }"
```

**Repeat for:**
- ✅ CEHub Copilot (`d88d4ffd-f658-4afd-bda7-4c0e45edd4b8`)
- ✅ RVC MCP (`0f00ba63-4e77-46ad-ad79-f1f9f2b5ffe0`)

### 4. Update Your Configuration

Your `appsettings.json` is already configured with Managed Identity! ✅

```json
{
  "OboConfig": {
    "Enabled": true,
    "ClientId": "5d437e13-722e-4a8d-8f4f-3158cf570e94",
    "UseManagedIdentity": true,
    "ManagedIdentityClientId": "99453c85-3919-4a0e-b5e1-691d6986b018",
    "TenantId": "72f988bf-86f1-41af-91ab-2d7cd011db47",
    "Scopes": ["api://1653c680-5347-4042-9077-cc4e8e63e581/.default"]
  }
}
```

**Configuration Options:**

| Setting | Description | Example |
|---------|-------------|---------|
| `UseManagedIdentity` | Enable Managed Identity auth | `true` |
| `ManagedIdentityClientId` | User-assigned MI Client ID (optional) | `"99453c85-3919-4a0e-b5e1-691d6986b018"` |
| | Leave empty for system-assigned | `null` or `""` |
| `ClientSecret` | Not needed with MI | Can be removed |

### 5. Testing the Configuration

#### Local Development (Without Managed Identity)

For local testing, you can use:
- **Azure CLI authentication**: Run `az login` before running your app
- **Visual Studio authentication**: Sign in to Visual Studio
- **Client Secret fallback**: Add `ClientSecret` for local dev only

The `DefaultAzureCredential` will try these in order:
1. Environment variables (client secret)
2. Managed Identity (in Azure)
3. Azure CLI
4. Visual Studio
5. Azure PowerShell

#### Production (Azure App Service)

1. Deploy your app to Azure App Service
2. The Managed Identity will automatically authenticate
3. Check Application Insights logs for:

```
[OBO] Created confidential client with Managed Identity for WinWireMcpServer
[OBO] Successfully acquired token for WinWireMcpServer
```

## Troubleshooting

### Error: "ManagedIdentityCredential authentication failed"

**Cause**: Managed Identity not enabled or not assigned to app

**Solution**:
1. Verify MI is enabled: App Service → Identity → Verify status
2. Check MI Client ID matches your config
3. Ensure app is deployed (MI only works in Azure, not locally)

### Error: "AADSTS700016: Application not found"

**Cause**: Gateway app registration not properly configured

**Solution**:
1. Verify federated credential is added to gateway app registration
2. Check the credential audience is `api://AzureADTokenExchange`
3. Ensure the resource points to your App Service

### Error: "AADSTS65001: User or administrator has not consented"

**Cause**: Admin consent not granted or downstream server not authorized

**Solution**:
1. Grant admin consent in gateway app → API permissions
2. Add gateway as authorized client in downstream app → Expose an API

### Error: "AADSTS50013: Assertion failed signature validation"

**Cause**: Federated credential not configured correctly

**Solution**:
1. Verify federated credential in gateway app registration
2. Check it points to correct App Service
3. Recreate the federated credential if needed

## Comparison: Managed Identity vs Client Secret

| Feature | Managed Identity | Client Secret |
|---------|-----------------|---------------|
| **Security** | ✅ Excellent | ⚠️ Must be secured |
| **Rotation** | ✅ Automatic | ❌ Manual |
| **Storage** | ✅ No storage needed | ❌ Key Vault required |
| **Setup** | ⚠️ Moderate | ✅ Simple |
| **Local Dev** | ⚠️ Needs fallback | ✅ Works everywhere |
| **Production** | ✅ Recommended | ❌ Not recommended |

## Best Practices

### ✅ Do This

1. **Use User-Assigned Managed Identity** for production
   - Easier to manage across multiple resources
   - Can be assigned to multiple App Services
   - Survives resource deletion

2. **Grant Least Privilege**
   - Only grant access to specific downstream APIs
   - Use specific scopes, not `.default` unless needed

3. **Monitor Access**
   - Enable logging in Application Insights
   - Set up alerts for auth failures

4. **Use Client Secret for Local Dev**
   - Keep development workflow simple
   - Store secret in User Secrets or .env file

### ❌ Avoid This

1. **Don't use Client Secrets in production** (if possible)
2. **Don't skip admin consent** - causes runtime errors
3. **Don't forget to authorize gateway** in downstream apps
4. **Don't use system-assigned MI** across multiple services

## Configuration Checklist

### Gateway App Service
- [x] Managed Identity enabled (user-assigned or system-assigned)
- [x] Managed Identity Client ID copied

### Gateway App Registration (`5d437e13-722e-4a8d-8f4f-3158cf570e94`)
- [ ] Federated credential added for Managed Identity
- [ ] Expose an API with `access_as_user` scope (if not already done)

### Each Downstream Server (WinWire, CEHub, RVC)
- [ ] Gateway app (`5d437e13-722e-4a8d-8f4f-3158cf570e94`) added as authorized client
- [ ] Scope `access_as_user` exposed
- [ ] App registration has service principal

### Configuration Files
- [x] `appsettings.json` updated with Managed Identity settings
- [x] `UseManagedIdentity: true` for enabled servers
- [x] `ManagedIdentityClientId` set (or empty for system-assigned)
- [x] No `ClientSecret` in production config

## Next Steps

1. **Enable Managed Identity** on your App Service (Step 1)
2. **Add Federated Credential** to gateway app registration (Step 2A)
3. **Authorize Gateway** in each downstream server (Step 3)
4. **Deploy and Test** in Azure
5. **Monitor Logs** for successful OBO token acquisition

---

**Your configuration is already set up** in `appsettings.json` with:
- ✅ `UseManagedIdentity: true`
- ✅ `ManagedIdentityClientId: "99453c85-3919-4a0e-b5e1-691d6986b018"`

All you need to do now is configure the Azure resources following steps 1-3 above!
