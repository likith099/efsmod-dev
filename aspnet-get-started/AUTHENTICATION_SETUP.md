# Configuration Setup

## Setting up Authentication

This project uses dual OIDC authentication providers:
1. **FLWINS CIAM** - Primary identity provider
2. **Second OIDC Provider** - Additional Azure AD provider

### Configuration Steps

1. **Copy the example configuration:**
   ```
   copy Web.config.example Web.config
   ```

2. **Update Web.config with your secrets:**
   - Replace `YOUR_FLWINS_CLIENT_SECRET_HERE` with your actual FLWINS client secret
   - Replace `YOUR_SECOND_TENANT_ID` with your second OIDC tenant ID
   - Replace `YOUR_SECOND_CLIENT_ID` with your second OIDC client ID
   - Replace `YOUR_SECOND_CLIENT_SECRET` with your second OIDC client secret
   - Replace `YOUR_PRODUCTION_REDIRECT_URI` with your production redirect URI

3. **For the new OIDC provider you configured:**
   - Get the Tenant ID from your Azure AD app registration
   - Get the Client ID from your Azure AD app registration
   - Generate and get the Client Secret from your Azure AD app registration
   - Configure the redirect URI in your Azure AD app registration to match your application

### Security Notes

- **Never commit Web.config with real secrets to git**
- The Web.config file is in .gitignore to prevent accidental commits
- Use environment variables in production deployments
- Keep your client secrets secure and rotate them regularly

### Login Options Available

After configuration, users will see three login options:
1. **Self Sign-up** - Create account with email verification
2. **FLWINS OIDC** - Sign in through FLWINS identity provider
3. **Azure AD OIDC** - Sign in through your second OIDC provider

### Azure AD App Registration Setup

For your second OIDC provider, ensure your Azure AD app registration has:
- **Redirect URIs:** 
  - `https://localhost:44320/signin-oidc` (for development)
  - Your production redirect URI
- **Authentication flow:** Allow implicit flow and authorization code flow
- **API permissions:** OpenID permissions (openid, profile, email)