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

## Important Configuration

The key to optional authentication is configuring Azure App Service to **"Allow anonymous requests (no action)"** rather than requiring authentication for all requests. See the authentication setup guide for detailed instructions. 

## License

See [LICENSE](LICENSE).
