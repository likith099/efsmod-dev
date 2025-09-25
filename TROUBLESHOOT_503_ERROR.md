# 🔧 Azure App Service 503 Error - Troubleshooting Guide

## 🚨 **Error**: 503 Service Unavailable
**URL**: `https://efsm-webapp-dev-ckbzdncfdpd2e4hg.canadacentral-01.azurewebsites.net/`

This error typically means your app isn't deployed yet or has deployment issues.

## 🔍 **Immediate Checks**

### 1. Check GitHub Actions Deployment
1. Go to: `https://github.com/likith099/efsmod-dev/actions`
2. Look for workflow runs under "Build and deploy Node.js app to Azure Web App - EFSM-webapp-dev"
3. Check if any deployments have run and their status

### 2. Verify GitHub Secrets
Go to: `https://github.com/likith099/efsmod-dev/settings/secrets/actions`

**Required Application Secrets:**
```
NEXT_PUBLIC_AZURE_AD_CLIENT_ID = 5b914bda-3dd3-478d-8317-4dd124e9bfa5
NEXT_PUBLIC_AZURE_AD_TENANT_ID = d935a45c-566b-4041-bd28-aa64949aae1d
NEXT_PUBLIC_AZURE_AD_AUTHORITY = https://efsmod.ciamlogin.com/d935a45c-566b-4041-bd28-aa64949aae1d/v2.0
NEXT_PUBLIC_REDIRECT_URI = https://efsm-webapp-dev-ckbzdncfdpd2e4hg.canadacentral-01.azurewebsites.net
NEXT_PUBLIC_POST_LOGOUT_REDIRECT_URI = https://efsm-webapp-dev-ckbzdncfdpd2e4hg.canadacentral-01.azurewebsites.net
NEXT_PUBLIC_KNOWN_AUTHORITY = efsmod.ciamlogin.com
```

**Required Azure Service Principal Secrets:**
```
AZUREAPPSERVICE_CLIENTID_F394749E68604AF0837359A72218D64F
AZUREAPPSERVICE_TENANTID_DF334475E8BD4AF990EE014E4C4FCF75
AZUREAPPSERVICE_SUBSCRIPTIONID_39D71AF96C9B4D3B91D945340FE2C4E4
```

### 3. Check Azure App Service Status
1. Go to [Azure Portal](https://portal.azure.com)
2. Navigate to **App Services** → **EFSM-webapp-dev**
3. Check **Overview** page for any errors or stopped status
4. Look at **Deployment Center** for deployment history

## 🚀 **Quick Fixes**

### Option 1: Trigger Manual Deployment
1. Go to your GitHub repository: `https://github.com/likith099/efsmod-dev`
2. Go to **Actions** tab
3. Click on your workflow: "Build and deploy Node.js app to Azure Web App - EFSM-webapp-dev"
4. Click **Run workflow** → **Run workflow** to trigger manual deployment

### Option 2: Force Push to Trigger Deployment
```bash
git commit --allow-empty -m "Trigger deployment"
git push origin main
```

### Option 3: Check Azure App Service Logs
1. Azure Portal → App Services → EFSM-webapp-dev
2. **Development Tools** → **Advanced Tools** → **Go**
3. Click **Log stream** to see real-time logs
4. Or check **Deployment Center** → **Logs**

## 🔧 **Common 503 Causes & Solutions**

### 1. **App Not Deployed**
- **Cause**: GitHub Actions hasn't run or failed
- **Solution**: Check GitHub Actions and trigger deployment

### 2. **Build Failures**
- **Cause**: Missing dependencies or build errors
- **Solution**: Check GitHub Actions logs for build failures

### 3. **Missing Environment Variables**
- **Cause**: App Service doesn't have required environment variables
- **Solution**: Configure App Service settings manually or via PowerShell script

### 4. **Port Configuration Issues**
- **Cause**: App Service expecting different port
- **Solution**: Add `PORT` environment variable or check startup command

## 🛠️ **Manual App Service Configuration**

If GitHub Actions isn't working, configure manually:

### Use PowerShell Script:
```powershell
.\deploy-azure.ps1 -ResourceGroupName "your-resource-group-name"
```

### Or Manual Configuration:
1. Azure Portal → App Services → EFSM-webapp-dev → **Configuration**
2. **Application settings** → Add these variables:
   ```
   NEXT_PUBLIC_AZURE_AD_CLIENT_ID = 5b914bda-3dd3-478d-8317-4dd124e9bfa5
   NEXT_PUBLIC_AZURE_AD_AUTHORITY = https://efsmod.ciamlogin.com/d935a45c-566b-4041-bd28-aa64949aae1d/v2.0
   NEXT_PUBLIC_REDIRECT_URI = https://efsm-webapp-dev-ckbzdncfdpd2e4hg.canadacentral-01.azurewebsites.net
   NEXT_PUBLIC_KNOWN_AUTHORITY = efsmod.ciamlogin.com
   NODE_ENV = production
   WEBSITE_NODE_DEFAULT_VERSION = ~22
   ```
3. **Save** and **restart** the app service

## 📋 **Next Steps**

1. ✅ **Check GitHub Actions**: Verify deployment status
2. ✅ **Add Missing Secrets**: Configure all required GitHub secrets  
3. ✅ **Trigger Deployment**: Manual or automatic deployment
4. ✅ **Monitor Logs**: Check Azure App Service logs
5. ✅ **Test Application**: Once deployed, test authentication flow

## 🎯 **Expected Resolution**

Once properly deployed, your app should:
- ✅ Load at: `https://efsm-webapp-dev-ckbzdncfdpd2e4hg.canadacentral-01.azurewebsites.net`
- ✅ Show EFSM branding
- ✅ Redirect to B2C login: `https://efsmod.ciamlogin.com/...`
- ✅ Complete authentication flow successfully

---

**Start with checking GitHub Actions deployment status first!** 🔍