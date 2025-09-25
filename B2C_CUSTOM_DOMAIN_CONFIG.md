# Azure AD B2C External ID Configuration for EFSM

## 🎯 **UPDATED FOR CUSTOM DOMAIN: efsmod.ciamlogin.com**

Your application is now configured to use your custom Azure AD B2C External ID domain instead of the standard Microsoft login page.

## ✅ **Configuration Updated**

### Environment Variables (.env.local):
```bash
NEXT_PUBLIC_AZURE_AD_AUTHORITY=https://efsmod.ciamlogin.com/d935a45c-566b-4041-bd28-aa64949aae1d/v2.0
NEXT_PUBLIC_KNOWN_AUTHORITY=efsmod.ciamlogin.com
```

### Key Changes Made:
1. **Authority**: Changed from `login.microsoftonline.com` to `efsmod.ciamlogin.com`
2. **Known Authorities**: Added B2C domain to trusted authorities
3. **Scopes**: Adjusted for B2C compatibility (removed `User.Read`)
4. **Response Mode**: Added query mode for B2C compatibility

## 🔧 **Azure AD B2C App Registration Requirements**

### 1. Redirect URIs in Azure Portal
In your Azure AD B2C → App registrations → Your app (`5b914bda-3dd3-478d-8317-4dd124e9bfa5`):

**Add these redirect URIs:**
- `http://localhost:3001` (for local development)
- `https://efsm-webapp-dev-ckbzdncfdpd2e4hg.canadacentral-01.azurewebsites.net` (for production)

### 2. Platform Configuration
- **Platform Type**: Single-page application (SPA)
- **Implicit Grant**: Enable ID tokens
- **Allow public client flows**: No

### 3. API Permissions
- `openid` ✅
- `profile` ✅ 
- `email` ✅
- Remove `User.Read` (not typically used with B2C)

## 🚀 **Testing Your Custom Login**

1. **Navigate to**: http://localhost:3001
2. **Click "Sign In"**
3. **You should be redirected to**: `https://efsmod.ciamlogin.com/...` (your custom login page)
4. **NOT**: the standard Microsoft login page

## 🔍 **Verification Steps**

### Check the Login URL:
When you click "Sign In", the browser should navigate to a URL starting with:
```
https://efsmod.ciamlogin.com/d935a45c-566b-4041-bd28-aa64949aae1d/oauth2/v2.0/authorize?...
```

### Expected Flow:
1. Click "Sign In" → Redirect to your custom EFSM login page
2. Enter credentials → Custom EFSM authentication experience
3. Success → Return to your app with user profile

## ⚠️ **Troubleshooting**

### If Still Going to Standard Microsoft Login:

1. **Clear Browser Cache**: Hard refresh (Ctrl+F5)
2. **Check Environment Variables**: Restart dev server after changes
3. **Verify B2C Configuration**: Ensure custom domain is properly configured in Azure

### Browser Console Check:
Open Developer Tools and look for:
```javascript
// Should see your custom authority in console
authority: "https://efsmod.ciamlogin.com/d935a45c-566b-4041-bd28-aa64949aae1d/v2.0"
```

## 🎨 **Custom Domain Benefits**

With `efsmod.ciamlogin.com` you get:
- ✅ Branded login experience with EFSM domain
- ✅ Custom UI/UX for authentication
- ✅ Professional appearance for users
- ✅ Seamless integration with your app branding

## 📝 **Current Status**

- **Custom Domain**: `efsmod.ciamlogin.com` ✅
- **Client ID**: `5b914bda-3dd3-478d-8317-4dd124e9bfa5` ✅
- **Tenant ID**: `d935a45c-566b-4041-bd28-aa64949aae1d` ✅
- **Local Dev**: `http://localhost:3001` ✅
- **Production**: `https://efsm-webapp-dev-ckbzdncfdpd2e4hg.canadacentral-01.azurewebsites.net` ✅

---

**Next Step**: Test authentication at http://localhost:3001 - you should now see your custom EFSM login page!