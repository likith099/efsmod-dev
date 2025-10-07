# Test Azure AD Configuration Script
# Run this after fixing the redirect URIs in Azure AD

Write-Host "Testing Azure AD OIDC Configuration..." -ForegroundColor Green
Write-Host ""

# Your configuration
$tenantId = "4cc02933-c81d-4fe9-9f71-850984769f51"
$clientId = "3ebef085-54a2-428f-861e-ebde0a11ba93"
$redirectUri = "https://localhost:44320/signin-oidc"

Write-Host "Tenant ID: $tenantId" -ForegroundColor Yellow
Write-Host "Client ID: $clientId" -ForegroundColor Yellow
Write-Host "Local Redirect URI: $redirectUri" -ForegroundColor Yellow
Write-Host ""

# Test the authorization endpoint
$authUrl = "https://login.microsoftonline.com/$tenantId/oauth2/v2.0/authorize?" +
           "client_id=$clientId&" +
           "response_type=code&" +
           "redirect_uri=[System.Uri]::EscapeDataString($redirectUri)&" +
           "scope=openid+profile+email&" +
           "response_mode=query"

Write-Host "Authorization URL that will be used:" -ForegroundColor Cyan
Write-Host $authUrl
Write-Host ""

Write-Host "Next Steps:" -ForegroundColor Green
Write-Host "1. Make sure these redirect URIs are added to your Azure AD app registration:" -ForegroundColor White
Write-Host "   - https://localhost:44320/signin-oidc" -ForegroundColor Cyan
Write-Host "   - https://flwins-dev-dshjczeyf7dxeqdz.canadacentral-01.azurewebsites.net/signin-oidc" -ForegroundColor Cyan
Write-Host ""
Write-Host "2. Add your client secret to Web.config" -ForegroundColor White
Write-Host ""
Write-Host "3. Test by going to: https://localhost:44320/Home/Login" -ForegroundColor White

# Check if localhost is running
try {
    $response = Invoke-WebRequest -Uri "https://localhost:44320" -Method HEAD -TimeoutSec 5 -ErrorAction SilentlyContinue
    Write-Host ""
    Write-Host "✅ Local server appears to be running on https://localhost:44320" -ForegroundColor Green
} catch {
    Write-Host ""
    Write-Host "❌ Local server is not running. Start your ASP.NET application first." -ForegroundColor Red
    Write-Host "   Try running the 'Start ASP.NET App with IIS Express' task in VS Code" -ForegroundColor Yellow
}