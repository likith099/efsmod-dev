---
services: app-service\web,app-service
platforms: dotnet
author: tiagocostapt
---

# ASP.NET MVC Application with Optional Azure AD Authentication

This is an ASP.NET MVC web application with optional Azure Active Directory authentication, deployed to Azure App Service.

## Features

- **Optional Authentication**: Users can browse the application without signing in
- **Azure AD Integration**: Secure authentication using Azure Active Directory
- **User Profile Management**: View user information from Azure AD claims
- **Responsive Design**: Bootstrap-based responsive UI
- **Authentication-Aware Navigation**: Different UI elements based on authentication status

## Authentication Flow

1. **Anonymous Access**: Home page and public content accessible without authentication
2. **Optional Sign In**: Users can choose to sign in via the navigation bar
3. **Protected Resources**: Profile page requires authentication
4. **Seamless Integration**: Uses Azure App Service authentication with Azure AD

## Key Components

### Controllers
- `HomeController`: Public pages (Home, About, Contact)
- `AccountController`: Authentication handling (Login, Logout, Profile)

### Authentication Features
- Custom authentication module for Azure App Service integration
- Claims-based identity with user information from Azure AD
- Optional authorization attributes for protected pages
- Automatic redirect to login for protected resources

## Deployment and Configuration

1. Deploy this application to Azure App Service
2. Configure Azure App Service Authentication (see [AUTHENTICATION_SETUP.md](AUTHENTICATION_SETUP.md))
3. Set up Azure AD App Registration
4. Configure authentication to allow anonymous requests

## EFSM-side SSO Provisioning Flow

This app now implements the EFSM side for a two-tenant SSO flow (FLWINS ➜ EFSM):

- FLWINS collects user info and calls EFSM to provision the user
- EFSM optionally invites the user as a B2B guest via Microsoft Graph
- EFSM returns an SSO URL back to FLWINS
- FLWINS presents a link; when the user clicks it, they land at EFSM `SR/Start` already signed in

### API: POST `/Provision/Create`

Request body (JSON):

```
{
	"email": "user@example.com",
	"displayName": "User Name",
	"redirectPath": "/SR/Start"
}
```

Response:

```
{
	"status": "ok",
	"ssoUrl": "https://<efsm-app>/.auth/login/aad?post_login_redirect_url=https%3A%2F%2F<efsm-app>%2FSR%2FStart",
	"message": "Graph invite result: <Status or 'skipped'>",
	"graphResult": { /* Graph API response or raw text */ }
}
```

Notes:
- If Graph environment variables are not set, the endpoint still returns a working `ssoUrl`
- If Graph is configured, EFSM sends a B2B invitation for the user’s email in the EFSM tenant

### Configure Microsoft Graph (optional but recommended)

Set these App Settings in EFSM App Service (or local environment):
- `EFSM_GRAPH_TENANT_ID` = EFSM Azure AD tenant ID
- `EFSM_GRAPH_CLIENT_ID` = App registration client ID (EFSM tenant)
- `EFSM_GRAPH_CLIENT_SECRET` = Client secret

Grant the app these application permissions and admin consent in EFSM tenant:
- Microsoft Graph ➜ `User.Invite.All`

### Post-login Landing Page

- EFSM provides `GET /SR/Start` which welcomes the user when authenticated
- If the user isn’t authenticated, it offers a Sign In button that returns to `/SR/Start`

## Important Configuration

The key to optional authentication is configuring Azure App Service to **"Allow anonymous requests (no action)"** rather than requiring authentication for all requests. See the authentication setup guide for detailed instructions. 

## License

See [LICENSE](LICENSE).
