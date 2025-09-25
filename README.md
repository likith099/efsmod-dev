# EFSM - React Application with Azure AD Authentication

A modern React application built with Next.js 15 and TypeScript, featuring Microsoft Azure AD authentication for secure login and user management.

## Features

- **Modern React Architecture**: Built with Next.js 15 and React 19
- **Azure AD Authentication**: Secure login/logout using Microsoft Azure Active Directory
- **TypeScript Support**: Full TypeScript implementation for type safety
- **Tailwind CSS**: Modern, responsive UI styling
- **User Profile Management**: Display authenticated user information
- **Responsive Design**: Mobile-first approach with beautiful UI components

## Prerequisites

- **Node.js**: Version 22 LTS or higher
- **npm**: Package manager (included with Node.js)
- **Microsoft Azure Account**: For Azure AD app registration
- **Git**: For version control

## Azure AD Setup

Before running the application, you need to configure an Azure AD App Registration. This application supports both regular Azure AD and Azure AD B2C with custom user flows.

### Option 1: Regular Azure AD Setup

1. **Create App Registration**:
   - Go to [Azure Portal](https://portal.azure.com/)
   - Navigate to "Azure Active Directory" > "App registrations" > "New registration"
   - Enter a name for your application (e.g., "EFSM App")
   - Select "Accounts in this organizational directory only" (single tenant)
   - Set redirect URI to: `http://localhost:3000` (for development)

2. **Configure Authentication**:
   - In your app registration, go to "Authentication"
   - Add platform → "Single-page application (SPA)"
   - Add redirect URI: `http://localhost:3000`
   - Enable "Access tokens" and "ID tokens" under "Implicit grant and hybrid flows"

3. **Get Required Information**:
   - Copy the **Application (client) ID**
   - Copy the **Directory (tenant) ID**
   - The authority URL will be: `https://login.microsoftonline.com/{tenant-id}/v2.0`

### Option 2: Azure AD B2C with User Flows

1. **Create B2C Tenant**:
   - Create an Azure AD B2C tenant if you don't have one
   - Note your tenant name (e.g., `yourcompany.onmicrosoft.com`)

2. **Create User Flow**:
   - In your B2C tenant, go to "User flows"
   - Create a new "Sign up and sign in" user flow
   - Configure the user attributes and claims you want to collect
   - Note the user flow name (e.g., `B2C_1_signupsignin`)

3. **Register Application**:
   - Go to "App registrations" in your B2C tenant
   - Create new registration with SPA platform
   - Set redirect URI to: `http://localhost:3000`
   - Enable "Access tokens" and "ID tokens"

4. **Get Required Information**:
   - Copy the **Application (client) ID**
   - Your authority URL will be: `https://{tenant}.b2clogin.com/{tenant}.onmicrosoft.com/{policy-name}`

## Installation & Setup

1. **Clone the repository**:
   ```bash
   git clone https://github.com/likith099/efsmod-dev.git
   cd efsmod-dev
   ```

2. **Install dependencies**:
   ```bash
   npm install
   ```

3. **Environment Configuration**:
   - Copy `.env.local.example` to `.env.local`
   - Update the environment variables with your Azure AD configuration:

   **For EFSM Web App Configuration (Current):**
   ```env
   NEXT_PUBLIC_AZURE_AD_CLIENT_ID=5b914bda-3dd3-478d-8317-4dd124e9bfa5
   NEXT_PUBLIC_AZURE_AD_AUTHORITY=https://login.microsoftonline.com/d935a45c-566b-4041-bd28-aa64949aae1d
   NEXT_PUBLIC_REDIRECT_URI=http://localhost:3000
   NEXT_PUBLIC_POST_LOGOUT_REDIRECT_URI=http://localhost:3000
   ```

   **For Production (Azure App Service):**
   ```env
   NEXT_PUBLIC_AZURE_AD_CLIENT_ID=5b914bda-3dd3-478d-8317-4dd124e9bfa5
   NEXT_PUBLIC_AZURE_AD_AUTHORITY=https://login.microsoftonline.com/d935a45c-566b-4041-bd28-aa64949aae1d
   NEXT_PUBLIC_REDIRECT_URI=https://efsm-webapp-dev-ckbzdncfdpd2e4hg.canadacentral-01.azurewebsites.net
   NEXT_PUBLIC_POST_LOGOUT_REDIRECT_URI=https://efsm-webapp-dev-ckbzdncfdpd2e4hg.canadacentral-01.azurewebsites.net
   ```

   **For Azure AD B2C with User Flows:**
   ```env
   NEXT_PUBLIC_AZURE_AD_CLIENT_ID=your_application_client_id
   NEXT_PUBLIC_AZURE_AD_AUTHORITY=https://yourtenant.b2clogin.com/yourtenant.onmicrosoft.com/B2C_1_your_user_flow_name
   NEXT_PUBLIC_REDIRECT_URI=http://localhost:3000
   NEXT_PUBLIC_KNOWN_AUTHORITY=yourtenant.b2clogin.com
   NEXT_PUBLIC_SIGN_UP_SIGN_IN_POLICY=B2C_1_your_user_flow_name
   NEXT_PUBLIC_AUTHORITY_DOMAIN=yourtenant.b2clogin.com
   ```

4. **Start the development server**:
   ```bash
   npm run dev
   ```

5. **Open your browser**:
   Navigate to [http://localhost:3000](http://localhost:3000)

## Available Scripts

- `npm run dev` - Start development server
- `npm run build` - Build for production
- `npm run start` - Start production server
- `npm run lint` - Run ESLint

## Project Structure

```
src/
├── app/
│   ├── globals.css          # Global styles
│   ├── layout.tsx           # Root layout with MSAL provider
│   └── page.tsx             # Main page component
├── components/
│   ├── AuthButtons.tsx      # Sign in/out buttons
│   ├── MSALWrapper.tsx      # MSAL provider wrapper
│   └── ProfileContent.tsx   # Main authentication UI
└── lib/
    └── authConfig.ts        # MSAL configuration
```

## Technologies Used

- **Framework**: Next.js 15.5
- **React**: Version 19
- **Authentication**: Microsoft Authentication Library (MSAL)
- **Styling**: Tailwind CSS
- **Language**: TypeScript
- **Build Tool**: Turbopack
- **Node.js**: Version 22 LTS

## Authentication Flow

1. **Unauthenticated Users**: See a welcome screen with "Sign in with Microsoft" button
2. **Authentication Process**: Clicking the sign-in button opens Microsoft's authentication popup
3. **Authenticated Users**: See their profile information with name, email, and sign-out option
4. **Sign Out**: Users can sign out, which clears their session and returns to the welcome screen

## Deployment

### Development
The application is configured for local development with `http://localhost:3000` as the redirect URI.

### Production
1. Update your Azure AD app registration with your production domain
2. Update the `NEXT_PUBLIC_REDIRECT_URI` environment variable
3. Build and deploy using your preferred hosting platform

## Troubleshooting

### Common Issues

1. **"Invalid redirect URI"**: 
   - Ensure the redirect URI in your Azure AD app registration matches your application URL
   - Check that `NEXT_PUBLIC_REDIRECT_URI` environment variable is correct

2. **"Application not found"**:
   - Verify your `NEXT_PUBLIC_AZURE_AD_CLIENT_ID` is correct
   - Ensure the application is enabled in Azure AD

3. **Authentication popup blocked**:
   - Allow popups for your domain in browser settings
   - Consider implementing redirect-based authentication instead

### Environment Variables

Make sure all required environment variables are set in `.env.local`:

```env
NEXT_PUBLIC_AZURE_AD_CLIENT_ID=your_client_id
NEXT_PUBLIC_AZURE_AD_AUTHORITY=https://login.microsoftonline.com/your_tenant_id
NEXT_PUBLIC_REDIRECT_URI=http://localhost:3000
```

## Contributing

1. Fork the repository
2. Create a feature branch: `git checkout -b feature/your-feature-name`
3. Commit your changes: `git commit -m 'Add some feature'`
4. Push to the branch: `git push origin feature/your-feature-name`
5. Open a pull request

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## Support

For support and questions, please open an issue on the [GitHub repository](https://github.com/likith099/efsmod-dev/issues).
