# EFSM Web App - Deployment Configuration Guide

This document provides the updated configuration for deploying the EFSM Web App to Azure App Service with the correct Azure AD app registration.

## Current Configuration

### Azure AD App Registration Details
- **Application (client) ID**: `5b914bda-3dd3-478d-8317-4dd124e9bfa5`
- **Object ID**: `cbd5c1f4-2e61-4752-87f2-462b3de4727b`
- **Directory (tenant) ID**: `d935a45c-566b-4041-bd28-aa64949aae1d`

### Azure App Service Details
- **App Name**: `EFSM-webapp-dev`
- **URL**: `https://efsm-webapp-dev-ckbzdncfdpd2e4hg.canadacentral-01.azurewebsites.net`
- **Region**: Canada Central
- **Resource Group**: (Based on naming pattern, likely contains "EFSM" or "webapp-dev")

### Database Configuration
- **Server**: `efsm-webapp-dev-server.mysql.database.azure.com`
- **Database**: `efsm-webapp-dev-database`
- **User**: `ayoioxhicv`
- **Connection String**: Available in publish profile

## Environment Configuration

### For Production Deployment (Azure App Service)
Configure these as **Application Settings** in your Azure App Service:

```bash
NEXT_PUBLIC_AZURE_AD_CLIENT_ID=5b914bda-3dd3-478d-8317-4dd124e9bfa5
NEXT_PUBLIC_AZURE_AD_TENANT_ID=d935a45c-566b-4041-bd28-aa64949aae1d
NEXT_PUBLIC_AZURE_AD_AUTHORITY=https://login.microsoftonline.com/d935a45c-566b-4041-bd28-aa64949aae1d
NEXT_PUBLIC_REDIRECT_URI=https://efsm-webapp-dev-ckbzdncfdpd2e4hg.canadacentral-01.azurewebsites.net
NEXT_PUBLIC_POST_LOGOUT_REDIRECT_URI=https://efsm-webapp-dev-ckbzdncfdpd2e4hg.canadacentral-01.azurewebsites.net
AZURE_MYSQL_CONNECTIONSTRING=Database=efsm-webapp-dev-database;Server=efsm-webapp-dev-server.mysql.database.azure.com;User Id=ayoioxhicv;Password=vovE$gNIRUVOMkck
```

### For Local Development
Copy `.env.local.example` to `.env.local` and use:

```bash
NEXT_PUBLIC_AZURE_AD_CLIENT_ID=5b914bda-3dd3-478d-8317-4dd124e9bfa5
NEXT_PUBLIC_AZURE_AD_TENANT_ID=d935a45c-566b-4041-bd28-aa64949aae1d
NEXT_PUBLIC_AZURE_AD_AUTHORITY=https://login.microsoftonline.com/d935a45c-566b-4041-bd28-aa64949aae1d
NEXT_PUBLIC_REDIRECT_URI=http://localhost:3000
NEXT_PUBLIC_POST_LOGOUT_REDIRECT_URI=http://localhost:3000
```

## Azure AD App Registration Configuration

### Required Redirect URIs
In the Azure Portal → Azure Active Directory → App registrations → Your app (`5b914bda-3dd3-478d-8317-4dd124e9bfa5`):

1. **Authentication** → **Platform configurations** → **Single-page application**:
   - Add redirect URI: `https://efsm-webapp-dev-ckbzdncfdpd2e4hg.canadacentral-01.azurewebsites.net`
   - Add redirect URI: `http://localhost:3000` (for local development)

2. **API permissions**:
   - Microsoft Graph → Delegated permissions:
     - `openid`
     - `profile`
     - `email`
     - `User.Read`

3. **Token configuration** (optional):
   - Add optional claims for `id_token`: `email`, `family_name`, `given_name`

## Deployment Steps

### 1. Azure App Service Configuration
```powershell
# Set environment variables in Azure App Service
az webapp config appsettings set --resource-group <your-resource-group> --name EFSM-webapp-dev --settings \
  NEXT_PUBLIC_AZURE_AD_CLIENT_ID=5b914bda-3dd3-478d-8317-4dd124e9bfa5 \
  NEXT_PUBLIC_AZURE_AD_TENANT_ID=d935a45c-566b-4041-bd28-aa64949aae1d \
  NEXT_PUBLIC_AZURE_AD_AUTHORITY=https://login.microsoftonline.com/d935a45c-566b-4041-bd28-aa64949aae1d \
  NEXT_PUBLIC_REDIRECT_URI=https://efsm-webapp-dev-ckbzdncfdpd2e4hg.canadacentral-01.azurewebsites.net \
  NEXT_PUBLIC_POST_LOGOUT_REDIRECT_URI=https://efsm-webapp-dev-ckbzdncfdpd2e4hg.canadacentral-01.azurewebsites.net
```

### 2. Build and Deploy
```bash
# Install dependencies
npm install

# Build the application
npm run build

# Deploy using the publish profile provided
# Or use GitHub Actions / Azure DevOps pipelines
```

### 3. Verification
After deployment, verify:
1. Navigate to: `https://efsm-webapp-dev-ckbzdncfdpd2e4hg.canadacentral-01.azurewebsites.net`
2. Click "Sign In" - should redirect to Microsoft login
3. Complete authentication flow
4. Verify user profile information is displayed

## Troubleshooting

### Common Issues
1. **CORS errors**: Ensure redirect URIs are properly configured in Azure AD
2. **Authentication failures**: Verify client ID and tenant ID are correct
3. **Token validation errors**: Check authority URL format

### Debug Information
- **Tenant ID**: `d935a45c-566b-4041-bd28-aa64949aae1d`
- **Client ID**: `5b914bda-3dd3-478d-8317-4dd124e9bfa5`
- **Authority**: `https://login.microsoftonline.com/d935a45c-566b-4041-bd28-aa64949aae1d`
- **Graph Endpoint**: `https://graph.microsoft.com/v1.0/me`

## Security Notes
- Never commit actual environment variables to source control
- Use Azure Key Vault for sensitive configuration in production
- Regularly rotate database passwords and client secrets
- Monitor authentication logs in Azure AD