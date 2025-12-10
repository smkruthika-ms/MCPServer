# Start MCP Server Script
Set-Location c:\Users\krsm\source\repos\MCPServer\mcp-server-ts
Write-Host "`nStarting MCP Server..." -ForegroundColor Cyan
Write-Host "Location: $(Get-Location)" -ForegroundColor Yellow
Write-Host "Running: node dist\index.js`n" -ForegroundColor Yellow
node dist\index.js
