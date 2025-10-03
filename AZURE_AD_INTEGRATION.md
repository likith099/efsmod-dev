# Azure AD Integration Configuration Guide

## Overview
This guide configures automatic Azure AD user creation for FLWINS users who don't have existing accounts in your Azure AD tenant.

## Prerequisites
1. Azure AD tenant with administrative access
2. Azure App Registration with Microsoft Graph API permissions
3. Azure App Service hosting the EFSM application

## Step 1: Configure Azure AD App Registration

### 1.1 API Permissions
Add the following Microsoft Graph permissions to your app registration (`7facd66f-0a8b-4757-823a-61e23d4909e2`):

**Application Permissions:**
- `User.ReadWrite.All` - Create and read user profiles
- `Directory.ReadWrite.All` - Read and write directory data
- `Mail.Send` - Send mail as any user (for welcome emails)

**Delegated Permissions:**
- `User.Read` - Sign in and read user profile

### 1.2 Grant Admin Consent
1. Go to Azure Portal → Azure Active Directory → App registrations
2. Find your app registration (`7facd66f-0a8b-4757-823a-61e23d4909e2`)
3. Navigate to **API permissions**
4. Click **Grant admin consent for [Your Tenant]**

### 1.3 Create Client Secret
1. Go to **Certificates & secrets**
2. Click **New client secret**
3. Add description: "EFSM FLWINS Integration"
4. Set expiration: 24 months (recommended)
5. **Save the secret value immediately** - you won't be able to see it again

## Step 2: Azure App Service Configuration

### 2.1 Application Settings
Add these settings in Azure Portal → Your App Service → Configuration → Application settings:

```
FLWINS_ALLOWED_DOMAINS = flwins.org,your-flwins-domain.com
FLWINS_SHARED_SECRET = your-super-secure-shared-secret
AUTO_CREATE_ACCOUNTS = true
SESSION_TIMEOUT = 60
ENVIRONMENT = Production

# Azure AD Configuration
AZURE_TENANT_ID = your-azure-tenant-id
AZURE_CLIENT_ID = 7facd66f-0a8b-4757-823a-61e23d4909e2
AZURE_CLIENT_SECRET = your-client-secret-from-step-1.3

# Email Configuration (optional)
SENDGRID_API_KEY = your-sendgrid-api-key
FROM_EMAIL = noreply@efsm.fldoe.gov
```

### 2.2 Get Your Tenant ID
1. Go to Azure Portal → Azure Active Directory → Properties
2. Copy the **Tenant ID** value
3. Use this value for `AZURE_TENANT_ID`

## Step 3: Security Configuration

### 3.1 Network Security
- Ensure your App Service has proper network security groups
- Consider using Azure Front Door for additional protection
- Enable Azure App Service authentication if needed

### 3.2 Monitoring
Enable Application Insights to monitor:
- User creation attempts
- API call failures
- Authentication flows

## Step 4: Testing the Integration

### 4.1 Test URLs
**Development Test Page:**
```
https://efsmod-dev-egcyb2bahcdkamdm.canadacentral-01.azurewebsites.net/Home/TestFLWINSRedirect
```

**Production AutoLogin Endpoint:**
```
https://efsmod-dev-egcyb2bahcdkamdm.canadacentral-01.azurewebsites.net/Home/AutoLogin?email={email}&name={name}&token={token}
```

### 4.2 Test Scenarios
1. **Existing User**: User exists in Azure AD → Direct login
2. **New User**: User doesn't exist → Automatic Azure AD account creation
3. **Invalid Token**: Should reject and redirect to login

## Step 5: User Experience Flow

### 5.1 New User Flow
1. FLWINS redirects user to EFSM AutoLogin endpoint
2. System validates FLWINS token
3. System checks if user exists in Azure AD
4. If user doesn't exist:
   - Creates new Azure AD user account
   - Generates temporary password
   - Sends welcome email (if configured)
   - Sets user session as authenticated
5. User lands on Family Portal with welcome message

### 5.2 Existing User Flow
1. FLWINS redirects user to EFSM AutoLogin endpoint
2. System validates FLWINS token
3. System finds existing Azure AD user
4. Sets user session as authenticated
5. User lands on Family Portal

## Step 6: Troubleshooting

### 6.1 Common Issues
- **403 Forbidden**: Check API permissions and admin consent
- **Token errors**: Verify client secret and tenant ID
- **User creation fails**: Check Graph API permissions
- **Email not sent**: Verify email service configuration

### 6.2 Monitoring and Logs
- Use Application Insights for detailed logging
- Monitor Azure AD sign-in logs
- Check App Service application logs

## Step 7: Production Checklist

- [ ] Azure AD app registration configured with proper permissions
- [ ] Admin consent granted for all API permissions
- [ ] Client secret created and stored securely
- [ ] All application settings configured in App Service
- [ ] HTTPS enforced on App Service
- [ ] Application Insights enabled
- [ ] Welcome email service configured (optional)
- [ ] FLWINS integration tested with real tokens
- [ ] Security headers configured
- [ ] Monitoring and alerting set up

## Security Considerations

1. **Token Validation**: Always validate FLWINS tokens in production
2. **HTTPS Only**: Ensure all communication uses HTTPS
3. **Secret Rotation**: Regularly rotate client secrets
4. **Monitoring**: Monitor for suspicious login attempts
5. **User Cleanup**: Consider cleanup policies for unused accounts
6. **Data Privacy**: Ensure compliance with data protection regulations

## Contact Information

For support or questions about this integration:
- Technical Support: [Your Support Email]
- FLWINS Team: [FLWINS Contact]
- Azure Support: [Azure Support Channel]