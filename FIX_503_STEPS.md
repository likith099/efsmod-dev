# 🚨 503 Service Unavailable - Quick Fix Steps

## ✅ **I've Just Triggered a Deployment**
- Pushed empty commit to trigger GitHub Actions
- Check deployment status at: `https://github.com/likith099/efsmod-dev/actions`

## 🔍 **Immediate Steps to Fix 503 Error**

### Step 1: Check GitHub Actions (PRIORITY)
1. **Go to**: `https://github.com/likith099/efsmod-dev/actions`
2. **Look for**: "Build and deploy Node.js app to Azure Web App - EFSM-webapp-dev"
3. **Check**: If the workflow is running or failed
4. **If failed**: Click on the failed run to see error details

### Step 2: Add Missing GitHub Secrets
**Go to**: `https://github.com/likith099/efsmod-dev/settings/secrets/actions`

**Click "New repository secret" and add each of these:**

| Secret Name | Secret Value |
|-------------|--------------|
| `NEXT_PUBLIC_AZURE_AD_CLIENT_ID` | `5b914bda-3dd3-478d-8317-4dd124e9bfa5` |
| `NEXT_PUBLIC_AZURE_AD_TENANT_ID` | `d935a45c-566b-4041-bd28-aa64949aae1d` |
| `NEXT_PUBLIC_AZURE_AD_AUTHORITY` | `https://efsmod.ciamlogin.com/d935a45c-566b-4041-bd28-aa64949aae1d/v2.0` |
| `NEXT_PUBLIC_REDIRECT_URI` | `https://efsm-webapp-dev-ckbzdncfdpd2e4hg.canadacentral-01.azurewebsites.net` |
| `NEXT_PUBLIC_POST_LOGOUT_REDIRECT_URI` | `https://efsm-webapp-dev-ckbzdncfdpd2e4hg.canadacentral-01.azurewebsites.net` |
| `NEXT_PUBLIC_KNOWN_AUTHORITY` | `efsmod.ciamlogin.com` |

### Step 3: Manual App Service Configuration (If GitHub Actions Fails)
1. **Azure Portal** → **App Services** → **EFSM-webapp-dev**
2. **Configuration** → **Application settings**
3. **Add these settings**:
   ```
   NEXT_PUBLIC_AZURE_AD_CLIENT_ID = 5b914bda-3dd3-478d-8317-4dd124e9bfa5
   NEXT_PUBLIC_AZURE_AD_AUTHORITY = https://efsmod.ciamlogin.com/d935a45c-566b-4041-bd28-aa64949aae1d/v2.0
   NEXT_PUBLIC_REDIRECT_URI = https://efsm-webapp-dev-ckbzdncfdpd2e4hg.canadacentral-01.azurewebsites.net
   NEXT_PUBLIC_KNOWN_AUTHORITY = efsmod.ciamlogin.com
   NODE_ENV = production
   WEBSITE_NODE_DEFAULT_VERSION = ~22
   ```
4. **Save** and wait for restart

### Step 4: Check App Service Status
1. **Azure Portal** → **App Services** → **EFSM-webapp-dev**
2. **Overview** → Check if status is "Running"
3. **Deployment Center** → Check deployment history
4. **Log stream** → Monitor for errors

## 🎯 **Most Likely Causes**

1. **Missing GitHub Secrets** ← Most common
2. **Failed GitHub Actions deployment**
3. **App Service not configured**
4. **Build failures during deployment**

## ⏰ **Wait Times**
- **GitHub Actions**: 3-5 minutes for deployment
- **App Service restart**: 1-2 minutes after configuration changes
- **DNS propagation**: Usually immediate for Azure domains

## 🧪 **Test After Fix**
Once fixed, test:
1. **App loads**: `https://efsm-webapp-dev-ckbzdncfdpd2e4hg.canadacentral-01.azurewebsites.net`
2. **Sign in works**: Redirects to `https://efsmod.ciamlogin.com/...`
3. **Authentication completes**: Returns to app with profile

---

## 🚀 **Current Status**
- ✅ Code pushed to GitHub
- ✅ Deployment triggered
- ⏳ Waiting for GitHub Actions to complete
- 🔄 **Action Required**: Add GitHub secrets and monitor deployment

**Check GitHub Actions first, then add secrets if the deployment failed!** 🔍