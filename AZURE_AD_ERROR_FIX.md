# Azure AD Authentication Error Fix Guide

## Error: AADSTS500208
**"The domain is not a valid login domain for the account type."**

This error typically occurs due to tenant configuration issues. Here are the solutions:

## � **APPLIED FIX**

✅ **Updated Authority Configuration:**
Changed from specific tenant to `organizations` authority:

```bash
# OLD (causing error):
NEXT_PUBLIC_AZURE_AD_AUTHORITY=https://login.microsoftonline.com/d935a45c-566b-4041-bd28-aa64949aae1d

# NEW (fixed):
NEXT_PUBLIC_AZURE_AD_AUTHORITY=https://login.microsoftonline.com/organizations
```

✅ **Updated Redirect URIs for current dev server:**
```bash
NEXT_PUBLIC_REDIRECT_URI=http://localhost:3001
NEXT_PUBLIC_POST_LOGOUT_REDIRECT_URI=http://localhost:3001
```

## 🎯 **What This Fix Does**

The `organizations` authority allows authentication for:
- ✅ Work or school accounts from any Azure AD tenant
- ✅ Your specific tenant (d935a45c-566b-4041-bd28-aa64949aae1d)
- ❌ Personal Microsoft accounts (excluded for security)

## 🔍 **Alternative Authority Options**

### Option 1: Common Authority (Most Permissive)
Allows both work/school AND personal Microsoft accounts:
```bash
NEXT_PUBLIC_AZURE_AD_AUTHORITY=https://login.microsoftonline.com/common
```

### Option 2: Organizations Authority (Recommended) ✅
Allows work/school accounts from any organization:
```bash
NEXT_PUBLIC_AZURE_AD_AUTHORITY=https://login.microsoftonline.com/organizations
```

### Option 3: Specific Tenant (Most Restrictive)
Only allows accounts from your specific tenant:
```bash
NEXT_PUBLIC_AZURE_AD_AUTHORITY=https://login.microsoftonline.com/d935a45c-566b-4041-bd28-aa64949aae1d
```

## � **Azure AD App Registration Requirements**

### 1. Supported Account Types
In Azure Portal → App registrations → Your app → Authentication:

For `organizations` authority, select:
- **"Accounts in any organizational directory (Any Azure AD directory - Multitenant)"**

### 2. Redirect URIs
Add these redirect URIs in Azure AD:

**For Development:**
- `http://localhost:3000`
- `http://localhost:3001` ← Currently using this port

**For Production:**
- `https://efsm-webapp-dev-ckbzdncfdpd2e4hg.canadacentral-01.azurewebsites.net`

### 3. API Permissions (Already Configured)
- ✅ `openid`
- ✅ `profile` 
- ✅ `email`
- ✅ `User.Read`

## 🚀 **Testing the Fix**

1. **Navigate to:** http://localhost:3001
2. **Click "Sign In"**
3. **You should now see:** Microsoft login page without the AADSTS500208 error
4. **Use any work/school Microsoft account** to test

## ⚠️ **If Error Persists**

### Check Browser Console
1. Open Developer Tools (F12)
2. Look for additional error messages
3. Clear browser cache and cookies for localhost

### Verify App Registration
1. Go to [Azure Portal](https://portal.azure.com)
2. Azure Active Directory → App registrations
3. Find app: `5b914bda-3dd3-478d-8317-4dd124e9bfa5`
4. Verify "Supported account types" matches your authority choice

### Try Different Authority
If `organizations` doesn't work, try `common`:
```bash
NEXT_PUBLIC_AZURE_AD_AUTHORITY=https://login.microsoftonline.com/common
```

## 📝 **Current Configuration Summary**

- **Client ID**: `5b914bda-3dd3-478d-8317-4dd124e9bfa5`
- **Authority**: `https://login.microsoftonline.com/organizations`
- **Redirect URI**: `http://localhost:3001`
- **Development Server**: Running on port 3001
- **Status**: ✅ Configuration updated and server restarted

---

**Next Step**: Test authentication at http://localhost:3001
