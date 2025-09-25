# 🚀 Quick Deployment Instructions

## Repository: https://github.com/likith099/efsmod-dev.git

### Step 1: Push to GitHub
```bash
# Initialize and push to your repository
git init
git add .
git commit -m "EFSM Web App with B2C authentication ready for deployment"
git branch -M main
git remote add origin https://github.com/likith099/efsmod-dev.git

```

### Step 2: Set Up GitHub Secrets
Go to: `https://github.com/likith099/efsmod-dev/settings/secrets/actions`

Add these secrets:
```
NEXT_PUBLIC_AZURE_AD_CLIENT_ID = 5b914bda-3dd3-478d-8317-4dd124e9bfa5
NEXT_PUBLIC_AZURE_AD_TENANT_ID = d935a45c-566b-4041-bd28-aa64949aae1d
NEXT_PUBLIC_AZURE_AD_AUTHORITY = https://efsmod.ciamlogin.com/d935a45c-566b-4041-bd28-aa64949aae1d/v2.0
NEXT_PUBLIC_REDIRECT_URI = https://efsm-webapp-dev-ckbzdncfdpd2e4hg.canadacentral-01.azurewebsites.net
NEXT_PUBLIC_POST_LOGOUT_REDIRECT_URI = https://efsm-webapp-dev-ckbzdncfdpd2e4hg.canadacentral-01.azurewebsites.net
NEXT_PUBLIC_KNOWN_AUTHORITY = efsmod.ciamlogin.com
```

### Step 3: Configure Azure App Service
#### Option A: Use PowerShell Script (Recommended)
```powershell
.\deploy-azure.ps1 -ResourceGroupName "your-resource-group-name"
```

#### Option B: Manual Configuration
1. Go to Azure Portal → App Services → EFSM-webapp-dev
2. Click Configuration → Application settings
3. Add all the environment variables from Step 2
4. Click Save

### Step 4: Set Up Deployment Center
1. Go to Azure Portal → App Services → EFSM-webapp-dev
2. Click Deployment Center
3. Choose GitHub as source
4. Select repository: `likith099/efsmod-dev`
5. Branch: `main`
6. Build Provider: GitHub Actions
7. Save

### Step 5: Add Azure AD Redirect URI
1. Azure Portal → Azure Active Directory → App registrations
2. Find app: `5b914bda-3dd3-478d-8317-4dd124e9bfa5`
3. Authentication → Add redirect URI:
   ```
   https://efsm-webapp-dev-ckbzdncfdpd2e4hg.canadacentral-01.azurewebsites.net
   ```
4. Platform: Single-page application
5. Save

### Step 6: Test Deployment
After GitHub Actions completes:
1. Visit: `https://efsm-webapp-dev-ckbzdncfdpd2e4hg.canadacentral-01.azurewebsites.net`
2. Click "Sign In"
3. Should redirect to: `https://efsmod.ciamlogin.com/...`
4. Complete authentication
5. Verify profile information displays

## ✅ Current Status
- [x] Local development working with B2C
- [x] Repository configured
- [x] GitHub workflow updated
- [x] Deployment scripts ready
- [ ] Push to GitHub
- [ ] Configure GitHub secrets
- [ ] Set up deployment
- [ ] Test production

**Ready to deploy!** 🎉