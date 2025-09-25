# Migration Notice: FLWINS to EFSM

## Overview
This project has been migrated from FLWINS to EFSM (Enterprise Financial Systems Management). The following changes have been made:

## ✅ Updated Components

### Application Name & Branding
- **Old**: FlWins / FLWINS
- **New**: EFSM (Enterprise Financial Systems Management)

### Updated Files
1. **Core Application**:
   - `src/components/ProfileContent.tsx` - Updated branding and welcome messages
   - `src/app/layout.tsx` - Updated page title
   - `src/app/profile/page.tsx` - Updated tenant references
   - `package.json` - Updated project name to `efsm-webapp-dev`

2. **Configuration**:
   - `src/lib/authConfig.ts` - Updated to use new Azure AD app registration
   - `.env.local` & `.env.local.example` - Updated environment variables
   - GitHub workflow renamed to `main_efsm-webapp-dev.yml`

3. **Documentation**:
   - `README.md` - Updated project name and repository references
   - `.github/copilot-instructions.md` - Updated repository URL
   - New comprehensive guides added for EFSM deployment

### Azure AD Configuration Changes
- **Old**: Azure AD B2C with custom domain (flwins.ciamlogin.com)
- **New**: Regular Azure AD with tenant ID `d935a45c-566b-4041-bd28-aa64949aae1d`
- **Client ID**: Updated to `5b914bda-3dd3-478d-8317-4dd124e9bfa5`
- **Redirect URI**: Updated to `https://efsm-webapp-dev-ckbzdncfdpd2e4hg.canadacentral-01.azurewebsites.net`

## ⚠️ Legacy Files
The following files contain outdated FLWINS/B2C configuration and should be referenced only for historical purposes:
- `URGENT_FIX.md`
- `SETUP_INSTRUCTIONS.md` 
- `DEPLOYMENT_GUIDE.md`
- `AZURE_DEPLOYMENT_FIX.md`

## 📋 Current Documentation
For up-to-date setup and deployment instructions, refer to:
- `EFSM_DEPLOYMENT_GUIDE.md` - Complete deployment guide
- `AZURE_AD_SETUP.md` - Azure AD configuration instructions
- `UPDATE_SUMMARY.md` - Summary of recent changes
- `README.md` - Main project documentation

## 🚀 Next Steps
1. Ensure Azure AD app registration is configured with new redirect URIs
2. Update any external references from FLWINS to EFSM
3. Configure production environment variables in Azure App Service
4. Test authentication flow with new configuration

---
**Migration Date**: September 25, 2025
**Status**: Complete ✅