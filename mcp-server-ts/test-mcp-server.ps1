# Test MCP Server - PowerShell Script
# This script demonstrates how to test the TypeScript MCP server

Write-Host "`n=== Testing MCP Server ===" -ForegroundColor Cyan

# 1. Test Health Endpoint
Write-Host "`n1. Testing Health Endpoint..." -ForegroundColor Yellow
try {
    $health = Invoke-RestMethod -Uri "http://localhost:3000/health" -Method Get
    Write-Host "✅ Health check passed" -ForegroundColor Green
    Write-Host "Status: $($health.status)" -ForegroundColor White
    Write-Host "Widget JS loaded: $($health.widgetAssetsLoaded.javascript)" -ForegroundColor White
    Write-Host "Widget CSS loaded: $($health.widgetAssetsLoaded.css)" -ForegroundColor White
} catch {
    Write-Host "❌ Health check failed: $($_.Exception.Message)" -ForegroundColor Red
    exit 1
}

# 2. Test Root Endpoint
Write-Host "`n2. Testing Root Endpoint..." -ForegroundColor Yellow
try {
    $root = Invoke-RestMethod -Uri "http://localhost:3000/" -Method Get
    Write-Host "✅ Root endpoint passed" -ForegroundColor Green
    Write-Host "Server: $($root.name)" -ForegroundColor White
    Write-Host "Version: $($root.version)" -ForegroundColor White
} catch {
    Write-Host "❌ Root endpoint failed: $($_.Exception.Message)" -ForegroundColor Red
}

# 3. Test MCP List Tools
Write-Host "`n3. Testing MCP List Tools..." -ForegroundColor Yellow
$listToolsBody = @{
    jsonrpc = "2.0"
    method = "tools/list"
    id = 1
} | ConvertTo-Json

try {
    $response = Invoke-RestMethod -Uri "http://localhost:3000/mcp/messages" `
        -Method Post `
        -ContentType "application/json" `
        -Body $listToolsBody
    Write-Host "✅ List tools request sent" -ForegroundColor Green
    Write-Host ($response | ConvertTo-Json -Depth 5) -ForegroundColor White
} catch {
    Write-Host "❌ List tools failed: $($_.Exception.Message)" -ForegroundColor Red
}

# 4. Test MCP List Resources
Write-Host "`n4. Testing MCP List Resources..." -ForegroundColor Yellow
$listResourcesBody = @{
    jsonrpc = "2.0"
    method = "resources/list"
    id = 2
} | ConvertTo-Json

try {
    $response = Invoke-RestMethod -Uri "http://localhost:3000/mcp/messages" `
        -Method Post `
        -ContentType "application/json" `
        -Body $listResourcesBody
    Write-Host "✅ List resources request sent" -ForegroundColor Green
    Write-Host ($response | ConvertTo-Json -Depth 5) -ForegroundColor White
} catch {
    Write-Host "❌ List resources failed: $($_.Exception.Message)" -ForegroundColor Red
}

# 5. Test Widget Tool Call
Write-Host "`n5. Testing Widget Tool Call..." -ForegroundColor Yellow
$toolCallBody = @{
    jsonrpc = "2.0"
    method = "tools/call"
    params = @{
        name = "widget_sales_chat"
        arguments = @{
            plannerKey = "SalesAgentPlanner"
            message = "Show me account summary"
        }
    }
    id = 3
} | ConvertTo-Json -Depth 5

try {
    $response = Invoke-RestMethod -Uri "http://localhost:3000/mcp/messages" `
        -Method Post `
        -ContentType "application/json" `
        -Body $toolCallBody
    Write-Host "✅ Widget tool call sent" -ForegroundColor Green
    Write-Host ($response | ConvertTo-Json -Depth 5) -ForegroundColor White
} catch {
    Write-Host "❌ Widget tool call failed: $($_.Exception.Message)" -ForegroundColor Red
}

Write-Host "`n=== Testing Complete ===" -ForegroundColor Cyan
Write-Host "`nServer is running at: http://localhost:3000" -ForegroundColor Green
Write-Host "SSE endpoint: http://localhost:3000/mcp/sse" -ForegroundColor Green
