# Azure AD App Registration Setup Guide

This guide provides step-by-step instructions for configuring your Azure AD app registration for the EFSM Web App.

## Current App Registration Details

- **Application (client) ID**: `5b914bda-3dd3-478d-8317-4dd124e9bfa5`
- **Object ID**: `cbd5c1f4-2e61-4752-87f2-462b3de4727b`
- **Directory (tenant) ID**: `d935a45c-566b-4041-bd28-aa64949aae1d`
- **App Name**: EFSM Web App (or similar)

## Required Configuration Steps

### 1. Authentication Configuration

1. Navigate to [Azure Portal](https://portal.azure.com)
2. Go to **Azure Active Directory** → **App registrations**
3. Find your app with Client ID: `5b914bda-3dd3-478d-8317-4dd124e9bfa5`
4. Click on **Authentication** in the left menu

#### Add Redirect URIs
Under **Platform configurations**, add a **Single-page application** platform with these redirect URIs:

**Production URLs:**
```
https://efsm-webapp-dev-ckbzdncfdpd2e4hg.canadacentral-01.azurewebsites.net
```

**Development URLs (for local testing):**
```
http://localhost:3000
http://localhost:3001
```

#### Advanced Settings
- ✅ **Access tokens (used for implicit flows)**: Enabled
- ✅ **ID tokens (used for implicit and hybrid flows)**: Enabled
- ✅ **Allow public client flows**: No (keep disabled for security)

### 2. API Permissions

1. Click on **API permissions** in the left menu
2. Ensure the following **Microsoft Graph** delegated permissions are granted:

**Required Permissions:**
- `openid` - Sign users in
- `profile` - View users' basic profile
- `email` - View users' email address
- `User.Read` - Read user profile

#### To Add Permissions:
1. Click **+ Add a permission**
2. Select **Microsoft Graph**
3. Choose **Delegated permissions**
4. Search for and select each required permission
5. Click **Add permissions**
6. Click **Grant admin consent** (if you have admin privileges)

### 3. Token Configuration (Optional but Recommended)

1. Click on **Token configuration** in the left menu
2. Click **+ Add optional claim**
3. Select **ID** token type
4. Add these claims:
   - `email`
   - `family_name` 
   - `given_name`
   - `preferred_username`

### 4. Expose an API (If needed for future API access)

If you plan to create backend APIs that this app will call:

1. Click on **Expose an API**
2. Click **Set** next to Application ID URI
3. Accept the default or customize: `api://5b914bda-3dd3-478d-8317-4dd124e9bfa5`

### 5. App Roles (If needed for role-based access)

For role-based access control:

1. Click on **App roles** in the left menu
2. Click **+ Create app role**
3. Define roles like:
   - **Admin**: Full access
   - **User**: Read access
   - **Viewer**: View-only access

## Verification Checklist

After configuration, verify these settings:

- [ ] **Authentication**: Redirect URIs include both production and development URLs
- [ ] **API Permissions**: All required permissions are granted and consented
- [ ] **Token Configuration**: Optional claims are configured
- [ ] **Overview**: Note down Client ID and Tenant ID match your environment variables

## Testing the Configuration

### Local Development Test
1. Set environment variables in `.env.local`:
   ```bash
   NEXT_PUBLIC_AZURE_AD_CLIENT_ID=5b914bda-3dd3-478d-8317-4dd124e9bfa5
   NEXT_PUBLIC_AZURE_AD_TENANT_ID=d935a45c-566b-4041-bd28-aa64949aae1d
   NEXT_PUBLIC_AZURE_AD_AUTHORITY=https://login.microsoftonline.com/d935a45c-566b-4041-bd28-aa64949aae1d
   NEXT_PUBLIC_REDIRECT_URI=http://localhost:3000
   ```

2. Run the application:
   ```bash
   npm run dev
   ```

3. Navigate to `http://localhost:3000`
4. Click "Sign In" and test the authentication flow

### Production Test
1. Deploy to Azure App Service with production environment variables
2. Navigate to `https://efsm-webapp-dev-ckbzdncfdpd2e4hg.canadacentral-01.azurewebsites.net`
3. Test authentication flow

## Troubleshooting Common Issues

### CORS Errors
**Problem**: "CORS policy" errors during authentication
**Solution**: Ensure redirect URIs are exactly correct in Azure AD

### Invalid Client Error
**Problem**: "Invalid client" error
**Solution**: Verify Client ID matches exactly in both Azure AD and environment variables

### Redirect URI Mismatch
**Problem**: "Redirect URI mismatch" error  
**Solution**: Ensure the redirect URI in Azure AD matches the URL your app is running on

### Permissions Not Granted
**Problem**: Can't access user profile information
**Solution**: Ensure API permissions are granted and admin consent is provided

## Security Best Practices

1. **Principle of Least Privilege**: Only request permissions your app actually needs
2. **Regular Review**: Periodically review and remove unused permissions
3. **Monitor Access**: Use Azure AD sign-in logs to monitor authentication patterns
4. **Secure Storage**: Never store client secrets in client-side code (not needed for SPA)
5. **Token Management**: Implement proper token refresh and cleanup

## Additional Resources

- [Microsoft Authentication Library (MSAL) Documentation](https://docs.microsoft.com/en-us/azure/active-directory/develop/msal-overview)
- [Azure AD App Registration Guide](https://docs.microsoft.com/en-us/azure/active-directory/develop/quickstart-register-app)
- [Single-Page Application Authentication](https://docs.microsoft.com/en-us/azure/active-directory/develop/scenario-spa-overview)

## Support Information

If you encounter issues:
1. Check the browser developer console for specific error messages
2. Review Azure AD sign-in logs in the Azure Portal
3. Verify all configuration matches this guide
4. Test with a simple authentication flow first before adding complex features