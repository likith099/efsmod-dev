# EFSM Web App Deployment Script (PowerShell)
# This script configures the Azure App Service with the correct environment variables

param(
    [Parameter(Mandatory=$false)]
    [string]$ResourceGroupName = "your-resource-group-name",
    
    [Parameter(Mandatory=$false)]
    [string]$AppName = "EFSM-webapp-dev",
    
    [Parameter(Mandatory=$false)]
    [string]$SubscriptionId = "your-subscription-id"
)

# Function to write colored output
function Write-ColorOutput($ForegroundColor, $Message) {
    Write-Host $Message -ForegroundColor $ForegroundColor
}

Write-ColorOutput Green "Starting EFSM Web App deployment configuration..."

# Check if Azure CLI is installed
try {
    az --version | Out-Null
    Write-ColorOutput Green "✅ Azure CLI is installed"
} catch {
    Write-ColorOutput Red "❌ Azure CLI is not installed. Please install it first."
    exit 1
}

# Check if logged in to Azure
Write-ColorOutput Yellow "Checking Azure login status..."
$loginCheck = az account show 2>$null
if ($LASTEXITCODE -ne 0) {
    Write-ColorOutput Yellow "Please login to Azure..."
    az login
}

# Set subscription if provided
if ($SubscriptionId -ne "your-subscription-id") {
    Write-ColorOutput Yellow "Setting Azure subscription..."
    az account set --subscription $SubscriptionId
}

# Validate resource group and app service exist
Write-ColorOutput Yellow "Validating Azure resources..."
$appExists = az webapp show --resource-group $ResourceGroupName --name $AppName 2>$null
if ($LASTEXITCODE -ne 0) {
    Write-ColorOutput Red "❌ App Service '$AppName' not found in resource group '$ResourceGroupName'"
    Write-ColorOutput Yellow "Available resource groups:"
    az group list --query "[].name" --output table
    exit 1
}

Write-ColorOutput Green "✅ App Service found: $AppName"

# Configure App Service settings
Write-ColorOutput Yellow "Configuring App Service environment variables..."

$settings = @(
    "NEXT_PUBLIC_AZURE_AD_CLIENT_ID=5b914bda-3dd3-478d-8317-4dd124e9bfa5"
    "NEXT_PUBLIC_AZURE_AD_TENANT_ID=d935a45c-566b-4041-bd28-aa64949aae1d"
    "NEXT_PUBLIC_AZURE_AD_AUTHORITY=https://efsmod.ciamlogin.com/d935a45c-566b-4041-bd28-aa64949aae1d/v2.0"
    "NEXT_PUBLIC_REDIRECT_URI=https://efsm-webapp-dev-ckbzdncfdpd2e4hg.canadacentral-01.azurewebsites.net"
    "NEXT_PUBLIC_POST_LOGOUT_REDIRECT_URI=https://efsm-webapp-dev-ckbzdncfdpd2e4hg.canadacentral-01.azurewebsites.net"
    "NEXT_PUBLIC_KNOWN_AUTHORITY=efsmod.ciamlogin.com"
    "NODE_ENV=production"
    "WEBSITE_NODE_DEFAULT_VERSION=~22"
)

$settingsString = $settings -join " "

$result = Invoke-Expression "az webapp config appsettings set --resource-group '$ResourceGroupName' --name '$AppName' --settings $settingsString"
if ($LASTEXITCODE -eq 0) {
    Write-ColorOutput Green "✅ App Service settings configured successfully!"
} else {
    Write-ColorOutput Red "❌ Failed to configure App Service settings"
    exit 1
}

# Configure database connection string
Write-ColorOutput Yellow "Configuring database connection string..."
az webapp config connection-string set `
    --resource-group $ResourceGroupName `
    --name $AppName `
    --connection-string-type MySql `
    --settings AZURE_MYSQL_CONNECTIONSTRING="Database=efsm-webapp-dev-database;Server=efsm-webapp-dev-server.mysql.database.azure.com;User Id=ayoioxhicv;Password=vovE`$gNIRUVOMkcc"

# Configure startup command
Write-ColorOutput Yellow "Configuring startup command..."
az webapp config set `
    --resource-group $ResourceGroupName `
    --name $AppName `
    --startup-file "server.js"

# Display current configuration
Write-ColorOutput Green "Current App Service configuration:"
az webapp config appsettings list `
    --resource-group $ResourceGroupName `
    --name $AppName `
    --query "[?starts_with(name, 'NEXT_PUBLIC_')]" `
    --output table

Write-ColorOutput Green "✅ Deployment configuration completed!"
Write-ColorOutput Yellow "`nNext steps:"
Write-Host "1. Build your application: npm run build" -ForegroundColor White
Write-Host "2. Deploy using your preferred method (GitHub Actions, Azure DevOps, etc.)" -ForegroundColor White
Write-Host "3. Test the application at: https://efsm-webapp-dev-ckbzdncfdpd2e4hg.canadacentral-01.azurewebsites.net" -ForegroundColor White

Write-ColorOutput Yellow "`nAzure AD Configuration Required:"
Write-Host "- Add redirect URI in Azure AD: https://efsm-webapp-dev-ckbzdncfdpd2e4hg.canadacentral-01.azurewebsites.net" -ForegroundColor White
Write-Host "- Ensure API permissions include: openid, profile, email, User.Read" -ForegroundColor White

Write-ColorOutput Yellow "`nUsage examples:"
Write-Host ".\deploy-azure.ps1" -ForegroundColor Gray
Write-Host ".\deploy-azure.ps1 -ResourceGroupName 'my-resource-group'" -ForegroundColor Gray
Write-Host ".\deploy-azure.ps1 -ResourceGroupName 'my-resource-group' -SubscriptionId 'my-subscription-id'" -ForegroundColor Gray