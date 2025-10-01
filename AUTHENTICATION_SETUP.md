# Azure App Service Configuration Instructions

## Important: Configure Authentication to be Optional

To make authentication optional (not forced on all pages), you need to configure your Azure App Service settings:

### Option 1: Azure Portal Configuration
1. Go to your App Service in the Azure Portal
2. Navigate to "Authentication" or "Authentication / Authorization" 
3. Make sure "App Service Authentication" is ON
4. Set "Action to take when request is not authenticated" to **"Allow anonymous requests (no action)"**
5. Configure your Azure Active Directory provider settings
6. Save the configuration

### Option 2: Using Azure CLI
```bash
# Set authentication to allow anonymous requests
az webapp auth update --resource-group <your-resource-group> --name <your-app-name> --action AllowAnonymous

# Configure Azure AD (replace with your values)
az webapp auth microsoft update --resource-group <your-resource-group> --name <your-app-name> \
  --client-id <your-client-id> \
  --client-secret <your-client-secret> \
  --tenant-id <your-tenant-id>
```

### Option 3: ARM Template / Bicep
Add this configuration to your deployment template:
```json
{
  "type": "Microsoft.Web/sites/config",
  "apiVersion": "2022-03-01",
  "name": "[concat(parameters('appName'), '/authSettingsV2')]",
  "properties": {
    "globalValidation": {
      "requireAuthentication": false,
      "unauthenticatedClientAction": "AllowAnonymous"
    },
    "identityProviders": {
      "azureActiveDirectory": {
        "enabled": true,
        "registration": {
          "openIdIssuer": "[concat('https://sts.windows.net/', parameters('tenantId'), '/')]",
          "clientId": "[parameters('clientId')]",
          "clientSecretSettingName": "MICROSOFT_PROVIDER_AUTHENTICATION_SECRET"
        }
      }
    }
  }
}
```

## Current Application Features

With the code changes made, your application now supports:

1. **Optional Authentication**: Users can browse the home page without signing in
2. **Sign In Button**: Navigation bar shows "Sign In" button for anonymous users  
3. **User Menu**: Authenticated users see a dropdown with their name, profile link, and logout
4. **Protected Pages**: Profile page requires authentication and redirects to login if needed
5. **Azure AD Integration**: Uses Azure App Service authentication with Azure AD
6. **User Information**: Profile page displays user details from Azure AD claims

## Testing

1. Deploy these changes to your App Service
2. Configure authentication as described above  
3. Visit your site - you should see the home page without forced login
4. Click "Sign In" to authenticate with Azure AD
5. After login, you'll see the user menu and can access the profile page

## Troubleshooting

If authentication is still forced:
- Double-check the Azure App Service authentication configuration
- Ensure "unauthenticatedClientAction" is set to "AllowAnonymous" 
- Check that your authentication module is properly registered in web.config
- Review App Service logs for any authentication errors