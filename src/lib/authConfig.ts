import { Configuration, RedirectRequest } from "@azure/msal-browser";

// MSAL configuration for Azure AD B2C with EFSM Web App
const msalConfig: Configuration = {
  auth: {
    clientId: process.env.NEXT_PUBLIC_AZURE_AD_CLIENT_ID || "5b914bda-3dd3-478d-8317-4dd124e9bfa5",
    authority: process.env.NEXT_PUBLIC_AZURE_AD_AUTHORITY || "https://efsmod.ciamlogin.com/d935a45c-566b-4041-bd28-aa64949aae1d/v2.0",
    redirectUri: process.env.NEXT_PUBLIC_REDIRECT_URI || 
      (typeof window !== 'undefined' ? window.location.origin : "http://localhost:3001"),
    postLogoutRedirectUri: process.env.NEXT_PUBLIC_POST_LOGOUT_REDIRECT_URI ||
      (typeof window !== 'undefined' ? window.location.origin : "http://localhost:3001"),
    knownAuthorities: [
      process.env.NEXT_PUBLIC_KNOWN_AUTHORITY || "efsmod.ciamlogin.com"
    ],
  },
  cache: {
    cacheLocation: "sessionStorage",
    storeAuthStateInCookie: false,
  },
  system: {
    loggerOptions: {
      loggerCallback: (level: number, message: string, containsPii: boolean) => {
        if (containsPii) {
          return;
        }
        switch (level) {
          case 0: // LogLevel.Error
            console.error(message);
            return;
          case 1: // LogLevel.Warning
            console.warn(message);
            return;
          case 2: // LogLevel.Info
            console.info(message);
            return;
          case 3: // LogLevel.Verbose
            console.debug(message);
            return;
        }
      }
    }
  }
};

// Login request configuration for Azure AD B2C
export const loginRequest = {
  scopes: ["openid", "profile", "email"],
  prompt: "select_account",
  redirectUri: process.env.NEXT_PUBLIC_REDIRECT_URI || 
    (typeof window !== 'undefined' ? window.location.origin : "http://localhost:3001"),
  extraQueryParameters: {
    response_mode: "query"
  }
};

// Configuration for Microsoft Graph API
export const graphConfig = {
  graphMeEndpoint: "https://graph.microsoft.com/v1.0/me",
};

// Azure AD B2C External ID configuration
export const b2cPolicies = {
  names: {
    signUpSignIn: process.env.NEXT_PUBLIC_SIGN_UP_SIGN_IN_POLICY || "B2C_1A_SIGNUP_SIGNIN",
    editProfile: process.env.NEXT_PUBLIC_EDIT_PROFILE_POLICY || "",
  },
  authorities: {
    signUpSignIn: {
      authority: process.env.NEXT_PUBLIC_AZURE_AD_AUTHORITY || "https://efsmod.ciamlogin.com/d935a45c-566b-4041-bd28-aa64949aae1d/v2.0",
    },
  },
  authorityDomain: process.env.NEXT_PUBLIC_AUTHORITY_DOMAIN || "efsmod.ciamlogin.com",
  tenantName: "EFSMOD",
  tenantId: "d935a45c-566b-4041-bd28-aa64949aae1d"
};

// Azure AD tenant information
export const tenantConfig = {
  tenantId: "d935a45c-566b-4041-bd28-aa64949aae1d",
  clientId: "5b914bda-3dd3-478d-8317-4dd124e9bfa5",
  objectId: "cbd5c1f4-2e61-4752-87f2-462b3de4727b",
};

export default msalConfig;