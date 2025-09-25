#!/bin/bash

# EFSM Web App Deployment Script
# This script configures the Azure App Service with the correct environment variables

# Variables
RESOURCE_GROUP="your-resource-group-name"  # Update with your resource group
APP_NAME="EFSM-webapp-dev"
SUBSCRIPTION_ID="your-subscription-id"     # Update with your subscription ID

# Colors for output
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
RED='\033[0;31m'
NC='\033[0m' # No Color

echo -e "${GREEN}Starting EFSM Web App deployment configuration...${NC}"

# Check if Azure CLI is installed
if ! command -v az &> /dev/null; then
    echo -e "${RED}Azure CLI is not installed. Please install it first.${NC}"
    exit 1
fi

# Login to Azure (if not already logged in)
echo -e "${YELLOW}Checking Azure login status...${NC}"
az account show > /dev/null 2>&1
if [ $? -ne 0 ]; then
    echo -e "${YELLOW}Please login to Azure...${NC}"
    az login
fi

# Set the subscription
if [ "$SUBSCRIPTION_ID" != "your-subscription-id" ]; then
    echo -e "${YELLOW}Setting Azure subscription...${NC}"
    az account set --subscription "$SUBSCRIPTION_ID"
fi

# Configure App Service settings
echo -e "${YELLOW}Configuring App Service environment variables...${NC}"

az webapp config appsettings set \
    --resource-group "$RESOURCE_GROUP" \
    --name "$APP_NAME" \
    --settings \
        NEXT_PUBLIC_AZURE_AD_CLIENT_ID=5b914bda-3dd3-478d-8317-4dd124e9bfa5 \
        NEXT_PUBLIC_AZURE_AD_TENANT_ID=d935a45c-566b-4041-bd28-aa64949aae1d \
        NEXT_PUBLIC_AZURE_AD_AUTHORITY=https://login.microsoftonline.com/d935a45c-566b-4041-bd28-aa64949aae1d \
        NEXT_PUBLIC_REDIRECT_URI=https://efsm-webapp-dev-ckbzdncfdpd2e4hg.canadacentral-01.azurewebsites.net \
        NEXT_PUBLIC_POST_LOGOUT_REDIRECT_URI=https://efsm-webapp-dev-ckbzdncfdpd2e4hg.canadacentral-01.azurewebsites.net \
        NODE_ENV=production \
        WEBSITE_NODE_DEFAULT_VERSION="~22"

if [ $? -eq 0 ]; then
    echo -e "${GREEN}✅ App Service settings configured successfully!${NC}"
else
    echo -e "${RED}❌ Failed to configure App Service settings${NC}"
    exit 1
fi

# Optional: Configure database connection string (if needed)
echo -e "${YELLOW}Configuring database connection...${NC}"
az webapp config connection-string set \
    --resource-group "$RESOURCE_GROUP" \
    --name "$APP_NAME" \
    --connection-string-type "MySql" \
    --settings AZURE_MYSQL_CONNECTIONSTRING="Database=efsm-webapp-dev-database;Server=efsm-webapp-dev-server.mysql.database.azure.com;User Id=ayoioxhicv;Password=vovE\$gNIRUVOMkcc"

# Configure Node.js settings
echo -e "${YELLOW}Configuring Node.js runtime...${NC}"
az webapp config set \
    --resource-group "$RESOURCE_GROUP" \
    --name "$APP_NAME" \
    --startup-file "server.js"

# Display current configuration
echo -e "${GREEN}Current App Service configuration:${NC}"
az webapp config appsettings list \
    --resource-group "$RESOURCE_GROUP" \
    --name "$APP_NAME" \
    --query "[?starts_with(name, 'NEXT_PUBLIC_')]" \
    --output table

echo -e "${GREEN}✅ Deployment configuration completed!${NC}"
echo -e "${YELLOW}Next steps:${NC}"
echo "1. Build your application: npm run build"
echo "2. Deploy using your preferred method (GitHub Actions, Azure DevOps, etc.)"
echo "3. Test the application at: https://efsm-webapp-dev-ckbzdncfdpd2e4hg.canadacentral-01.azurewebsites.net"
echo ""
echo -e "${YELLOW}Azure AD Configuration Required:${NC}"
echo "- Add redirect URI in Azure AD: https://efsm-webapp-dev-ckbzdncfdpd2e4hg.canadacentral-01.azurewebsites.net"
echo "- Ensure API permissions include: openid, profile, email, User.Read"