# 🚀 EFSM Web App - Azure App Service Deployment Guide

## Repository: https://github.com/likith099/efsmod-dev.git

This guide will walk you through deploying your EFSM Web App to Azure App Service using GitHub Actions.

## 📋 Pre-Deployment Checklist

### ✅ Current Status
- [x] Application working locally with custom B2C login
- [x] Repository ready: `https://github.com/likith099/efsmod-dev.git`
- [x] Azure App Service: `EFSM-webapp-dev`
- [x] Custom domain: `efsmod.ciamlogin.com`

### 🎯 Target Configuration
- **App Service**: `EFSM-webapp-dev`
- **URL**: `https://efsm-webapp-dev-ckbzdncfdpd2e4hg.canadacentral-01.azurewebsites.net`
- **Repository**: `https://github.com/likith099/efsmod-dev.git`

## 🔧 Step 1: Update Production Environment Variables

### Environment Variables for Azure App Service:
```bash
NEXT_PUBLIC_AZURE_AD_CLIENT_ID=5b914bda-3dd3-478d-8317-4dd124e9bfa5
NEXT_PUBLIC_AZURE_AD_TENANT_ID=d935a45c-566b-4041-bd28-aa64949aae1d
NEXT_PUBLIC_AZURE_AD_AUTHORITY=https://efsmod.ciamlogin.com/d935a45c-566b-4041-bd28-aa64949aae1d/v2.0
NEXT_PUBLIC_REDIRECT_URI=https://efsm-webapp-dev-ckbzdncfdpd2e4hg.canadacentral-01.azurewebsites.net
NEXT_PUBLIC_POST_LOGOUT_REDIRECT_URI=https://efsm-webapp-dev-ckbzdncfdpd2e4hg.canadacentral-01.azurewebsites.net
NEXT_PUBLIC_KNOWN_AUTHORITY=efsmod.ciamlogin.com
NODE_ENV=production
WEBSITE_NODE_DEFAULT_VERSION=~22
```

## 🔗 Step 2: Set Up GitHub Actions Deployment

### Option A: Using Azure Portal (Recommended)

1. **Go to Azure Portal** → **App Services** → **EFSM-webapp-dev**

2. **Navigate to Deployment Center**:
   - Click on **Deployment Center** in the left menu
   - Select **GitHub** as source
   - Authorize GitHub if needed
   - Select Repository: `likith099/efsmod-dev`
   - Branch: `main`
   - Build Provider: **GitHub Actions**

3. **Azure will automatically**:
   - Create workflow file in `.github/workflows/`
   - Set up deployment secrets
   - Configure build and deploy pipeline

### Option B: Manual GitHub Secrets Setup

If you want to configure manually, add these secrets to your GitHub repository:

1. **Go to**: `https://github.com/likith099/efsmod-dev/settings/secrets/actions`

2. **Add Repository Secrets**:
   ```
   AZURE_WEBAPP_PUBLISH_PROFILE=<your-publish-profile-content>
   AZURE_WEBAPP_NAME=EFSM-webapp-dev
   ```

## 📱 Step 3: Configure Azure AD B2C Redirect URIs

### Add Production Redirect URI:

1. **Go to Azure Portal** → **Azure Active Directory** → **App registrations**
2. **Find your app**: `5b914bda-3dd3-478d-8317-4dd124e9bfa5`
3. **Click Authentication**
4. **Add Redirect URI**:
   ```
   https://efsm-webapp-dev-ckbzdncfdpd2e4hg.canadacentral-01.azurewebsites.net
   ```
5. **Platform**: Single-page application (SPA)
6. **Save changes**

## 🛠️ Step 4: Configure App Service Settings

### Using PowerShell (Run the deployment script):
```powershell
.\deploy-azure.ps1 -ResourceGroupName "your-resource-group-name"
```

### Or Manually in Azure Portal:
1. **Go to**: App Services → EFSM-webapp-dev → Configuration → Application settings
2. **Add/Update** each environment variable listed in Step 1
3. **Click Save**

## 🔄 Step 5: Deploy Your Application

### Push to GitHub:
```bash
# If you haven't initialized the repo yet:
git init
git add .
git commit -m "Initial commit - EFSM Web App with B2C authentication"
git branch -M main
git remote add origin https://github.com/likith099/efsmod-dev.git
git push -u origin main

# Or if repo already exists:
git add .
git commit -m "Deploy EFSM Web App to Azure App Service"
git push origin main
```

### GitHub Actions will automatically:
1. ✅ Build your Next.js application
2. ✅ Run tests (if any)
3. ✅ Deploy to Azure App Service
4. ✅ Configure environment variables

## 🧪 Step 6: Test Deployment

### After deployment completes:

1. **Navigate to**: `https://efsm-webapp-dev-ckbzdncfdpd2e4hg.canadacentral-01.azurewebsites.net`

2. **Test Authentication Flow**:
   - Click "Sign In"
   - Should redirect to: `https://efsmod.ciamlogin.com/...`
   - Complete login with your credentials
   - Should return to your app with profile information

3. **Verify**:
   - ✅ Custom EFSM login page appears
   - ✅ Authentication completes successfully
   - ✅ User profile displays correctly
   - ✅ Logout works properly

## 🔍 Troubleshooting

### If Deployment Fails:
1. Check GitHub Actions logs in your repository
2. Verify App Service logs in Azure Portal
3. Ensure all environment variables are set correctly

### If Authentication Fails:
1. Verify redirect URIs in Azure AD B2C
2. Check browser developer console for errors
3. Confirm custom domain configuration

### Common Issues:
- **CORS errors**: Ensure redirect URIs match exactly
- **Authority errors**: Verify B2C authority URL is correct
- **Build errors**: Check Node.js version compatibility

## 📚 Documentation Files Updated

- `AZURE_DEPLOYMENT_GUIDE.md` - This guide
- `deploy-azure.ps1` - PowerShell deployment script
- `B2C_CONFIGURATION_COMPLETE.md` - B2C setup details

## 🎯 Expected Final Result

After successful deployment:
- ✅ **Production URL**: `https://efsm-webapp-dev-ckbzdncfdpd2e4hg.canadacentral-01.azurewebsites.net`
- ✅ **Custom B2C Login**: `https://efsmod.ciamlogin.com/...`
- ✅ **Continuous Deployment**: Automatic deployments on git push
- ✅ **Professional EFSM Branding**: Throughout the application

---

**Ready to deploy!** Follow the steps above to get your EFSM Web App live in Azure! 🚀