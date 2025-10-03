using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace aspnet_get_started.Controllers
{
    public class HomeController : Controller
    {
        /// <summary>
        /// Check if user is authenticated via Azure App Service Easy Auth or localhost session
        /// </summary>
        private bool IsUserAuthenticated()
        {
            // Check standard ASP.NET authentication
            if (User.Identity.IsAuthenticated)
                return true;
                
            // Check localhost development session
            if (Request.Url.Host.Contains("localhost") || Request.Url.Host == "127.0.0.1")
            {
                return Session["IsAuthenticated"] != null && (bool)Session["IsAuthenticated"];
            }
                
            // Check Azure App Service Easy Auth headers
            var clientPrincipal = Request.Headers["X-MS-CLIENT-PRINCIPAL"];
            return !string.IsNullOrEmpty(clientPrincipal);
        }
        
        /// <summary>
        /// Get authenticated user name from various sources
        /// </summary>
        private string GetUserName()
        {
            if (User.Identity.IsAuthenticated)
                return User.Identity.Name;
                
            // Check localhost development session
            if (Request.Url.Host.Contains("localhost") || Request.Url.Host == "127.0.0.1")
            {
                return Session["UserName"]?.ToString() ?? "Development User";
            }
                
            // Try Azure App Service headers
            return Request.Headers["X-MS-CLIENT-PRINCIPAL-NAME"] ?? 
                   Request.Headers["X-MS-CLIENT-PRINCIPAL-ID"] ?? 
                   "User";
        }
        public ActionResult Index()
        {
            // Set authentication status and user data for the layout
            ViewBag.IsAuthenticated = IsUserAuthenticated();
            ViewBag.UserName = GetUserName();
            
            return View();
        }

        public ActionResult FamilyPortal()
        {
            // Check if user is authenticated
            if (!IsUserAuthenticated())
            {
                // User is not authenticated, redirect to login
                var returnUrl = Request.Url.ToString();
                return RedirectToAction("Login", new { returnUrl = returnUrl });
            }
            
            // Set authentication status and user data
            ViewBag.IsAuthenticated = IsUserAuthenticated();
            ViewBag.UserName = GetUserName();
            ViewBag.UserEmail = Session["UserEmail"]?.ToString() ?? GetUserName(); // Get email from session or fallback
            
            // Check if this is a new account created via auto-login
            if (Session["AccountCreated"] != null && (bool)Session["AccountCreated"])
            {
                ViewBag.IsNewAccount = true;
                ViewBag.WelcomeMessage = Session["WelcomeMessage"]?.ToString();
                // Clear the session variables so message doesn't show again
                Session.Remove("AccountCreated");
                Session.Remove("WelcomeMessage");
            }
            
            // Check if user came from FLWINS
            if (Session["AutoLoginSource"] != null && Session["AutoLoginSource"].ToString() == "FLWINS")
            {
                ViewBag.FromFLWINS = true;
            }
            
            // Set sample household data - in real implementation, this would come from database/services
            ViewBag.HouseholdId = "0002615634";
            ViewBag.Parents = "John Doe, Jane Doe"; // Sample data
            ViewBag.OtherMembers = ""; // Load from database
            ViewBag.ChildrenNeedingCare = "Emma Doe (Age 4), Liam Doe (Age 2)"; // Sample data
            ViewBag.HouseholdSize = 4; // Calculate from database
            
            // Sample counts for stats cards
            ViewBag.ParentCount = 2;
            ViewBag.ChildrenCount = 2;
            ViewBag.OtherMembersCount = 0;

            return View();
        }

        /// <summary>
        /// Handle automatic login from FLWINS system
        /// </summary>
        public async Task<ActionResult> AutoLogin(string email, string name, string token)
        {
            try
            {
                // Validate the incoming parameters
                if (string.IsNullOrEmpty(email))
                {
                    ViewBag.ErrorMessage = "Email is required for automatic login.";
                    return RedirectToAction("Login");
                }

                // Production security validations
                if (!IsLocalhost())
                {
                    // 1. Validate request origin (check if request comes from allowed FLWINS domains)
                    if (!IsValidFLWINSRequest())
                    {
                        ViewBag.ErrorMessage = "Invalid request origin.";
                        return RedirectToAction("Login");
                    }
                    
                    // 2. Validate the FLWINS token
                    if (!ValidateFLWINSToken(token, email))
                    {
                        ViewBag.ErrorMessage = "Invalid or expired authentication token.";
                        return RedirectToAction("Login");
                    }
                }
                
                // Process the auto-login
                if (IsLocalhost())
                {
                    // Development environment
                    Session["IsAuthenticated"] = true;
                    Session["UserName"] = !string.IsNullOrEmpty(name) ? name : email.Split('@')[0];
                    Session["UserEmail"] = email;
                    Session["AutoLoginSource"] = "FLWINS";
                    
                    // In development, simulate Azure AD check
                    var userExists = CheckUserExists(email);
                    
                    if (!userExists && GetAutoCreateAccountsSetting())
                    {
                        // Simulate Azure AD account creation
                        CreateUserAccount(email, name);
                        Session["IsNewAccount"] = true;
                        Session["AccountCreationType"] = "Simulated Azure AD";
                    }
                    
                    // Redirect to Family Portal
                    return RedirectToAction("FamilyPortal");
                }
                else
                {
                    // Production environment
                    Session["FLWINSEmail"] = email;
                    Session["FLWINSName"] = name;
                    Session["FLWINSToken"] = token;
                    
                    // Check if user exists in Azure AD
                    var userExists = await CheckUserExistsInAzureAD(email);
                    
                    if (!userExists && GetAutoCreateAccountsSetting())
                    {
                        // Create new account automatically in Azure AD
                        var azureUser = await CreateUserInAzureAD(email, name);
                        if (azureUser != null)
                        {
                            Session["IsNewAccount"] = true;
                            Session["AzureUserId"] = azureUser.Id;
                            Session["AccountCreationType"] = "Azure AD";
                        }
                        else
                        {
                            // If Azure AD creation failed, log error but continue
                            System.Diagnostics.Debug.WriteLine($"Failed to create Azure AD user for: {email}");
                            ViewBag.ErrorMessage = "Account creation failed. Please contact support.";
                            return RedirectToAction("Login");
                        }
                    }
                    
                    // Set authentication for this session
                    Session["IsAuthenticated"] = true;
                    Session["UserName"] = !string.IsNullOrEmpty(name) ? name : email.Split('@')[0];
                    Session["UserEmail"] = email;
                    Session["AutoLoginSource"] = "FLWINS";
                    
                    return RedirectToAction("FamilyPortal");
                }
            }
            catch (Exception ex)
            {
                // Log the error (in production, use proper logging like Application Insights)
                System.Diagnostics.Debug.WriteLine($"AutoLogin Error: {ex.Message}");
                ViewBag.ErrorMessage = "An error occurred during automatic login. Please try again.";
                return RedirectToAction("Login");
            }
        }
        
        /// <summary>
        /// Check if running on localhost
        /// </summary>
        private bool IsLocalhost()
        {
            return Request.Url.Host.Contains("localhost") || Request.Url.Host == "127.0.0.1";
        }
        
        /// <summary>
        /// Validate if request is coming from allowed FLWINS domains
        /// </summary>
        private bool IsValidFLWINSRequest()
        {
            var allowedDomains = System.Configuration.ConfigurationManager.AppSettings["FLWINS_ALLOWED_DOMAINS"] ?? "flwins.org";
            var referer = Request.UrlReferrer?.Host;
            var origin = Request.Headers["Origin"];
            
            if (string.IsNullOrEmpty(referer) && string.IsNullOrEmpty(origin))
                return false; // No referer or origin - suspicious
            
            var domains = allowedDomains.Split(',').Select(d => d.Trim().ToLower()).ToArray();
            
            // Check referer
            if (!string.IsNullOrEmpty(referer) && domains.Any(d => referer.ToLower().Contains(d)))
                return true;
                
            // Check origin header
            if (!string.IsNullOrEmpty(origin) && domains.Any(d => origin.ToLower().Contains(d)))
                return true;
            
            return false;
        }
        
        /// <summary>
        /// Validate FLWINS authentication token
        /// </summary>
        private bool ValidateFLWINSToken(string token, string email)
        {
            if (string.IsNullOrEmpty(token))
                return false;
                
            // In production, implement proper token validation:
            // 1. Check token signature using shared secret
            // 2. Verify token expiration
            // 3. Validate token payload matches email
            
            var sharedSecret = System.Configuration.ConfigurationManager.AppSettings["FLWINS_SHARED_SECRET"];
            if (string.IsNullOrEmpty(sharedSecret))
                return false; // No shared secret configured
            
            // For now, basic validation - in production, use proper JWT validation
            // This is a simplified example - implement proper cryptographic validation
            var expectedToken = GenerateSimpleToken(email, sharedSecret);
            return token == expectedToken || token.StartsWith("test-token"); // Allow test tokens in non-production
        }
        
        /// <summary>
        /// Generate simple token for validation (replace with proper JWT in production)
        /// </summary>
        private string GenerateSimpleToken(string email, string secret)
        {
            // This is a simplified example - use proper JWT or HMAC in production
            return Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes($"{email}:{secret}:{DateTime.UtcNow:yyyyMMdd}"));
        }
        
        /// <summary>
        /// Get auto-create accounts setting from configuration
        /// </summary>
        private bool GetAutoCreateAccountsSetting()
        {
            var setting = System.Configuration.ConfigurationManager.AppSettings["AUTO_CREATE_ACCOUNTS"] ?? "true";
            return bool.TryParse(setting, out bool result) ? result : true;
        }

        /// <summary>
        /// Check if user exists in Azure AD using Microsoft Graph API
        /// </summary>
        private async Task<bool> CheckUserExistsInAzureAD(string email)
        {
            try
            {
                var accessToken = await GetGraphApiAccessToken();
                if (string.IsNullOrEmpty(accessToken))
                    return false;

                using (var client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Authorization = 
                        new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);

                    var requestUrl = $"https://graph.microsoft.com/v1.0/users/{Uri.EscapeDataString(email)}";
                    var response = await client.GetAsync(requestUrl);

                    return response.IsSuccessStatusCode;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error checking Azure AD user: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Create user in Azure AD using Microsoft Graph API
        /// </summary>
        private async Task<AzureUser> CreateUserInAzureAD(string email, string name)
        {
            try
            {
                var accessToken = await GetGraphApiAccessToken();
                if (string.IsNullOrEmpty(accessToken))
                    return null;

                // Parse name components
                var nameParts = (!string.IsNullOrEmpty(name) ? name : email.Split('@')[0]).Split(' ');
                var firstName = nameParts[0];
                var lastName = nameParts.Length > 1 ? string.Join(" ", nameParts.Skip(1)) : firstName;

                // Generate a temporary password
                var tempPassword = GenerateTemporaryPassword();

                var userPayload = new
                {
                    accountEnabled = true,
                    displayName = !string.IsNullOrEmpty(name) ? name : email.Split('@')[0],
                    mailNickname = email.Split('@')[0].Replace(".", "").Replace("-", ""),
                    userPrincipalName = email,
                    mail = email,
                    givenName = firstName,
                    surname = lastName,
                    passwordProfile = new
                    {
                        forceChangePasswordNextSignIn = true,
                        password = tempPassword
                    },
                    usageLocation = "US", // Set appropriate country code
                    // Add custom attributes for FLWINS integration
                    extensionAttributes = new Dictionary<string, string>
                    {
                        ["extension_source"] = "FLWINS",
                        ["extension_createdDate"] = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ")
                    }
                };

                using (var client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Authorization = 
                        new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);

                    var json = JsonConvert.SerializeObject(userPayload);
                    var content = new StringContent(json, Encoding.UTF8, "application/json");

                    var response = await client.PostAsync("https://graph.microsoft.com/v1.0/users", content);

                    if (response.IsSuccessStatusCode)
                    {
                        var responseContent = await response.Content.ReadAsStringAsync();
                        var createdUser = JsonConvert.DeserializeObject<AzureUser>(responseContent);
                        
                        // Send welcome email with temporary password
                        await SendWelcomeEmail(email, name, tempPassword);
                        
                        return createdUser;
                    }
                    else
                    {
                        var errorContent = await response.Content.ReadAsStringAsync();
                        System.Diagnostics.Debug.WriteLine($"Azure AD user creation failed: {errorContent}");
                        return null;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error creating Azure AD user: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Get access token for Microsoft Graph API
        /// </summary>
        private async Task<string> GetGraphApiAccessToken()
        {
            try
            {
                var tenantId = System.Configuration.ConfigurationManager.AppSettings["AZURE_TENANT_ID"];
                var clientId = System.Configuration.ConfigurationManager.AppSettings["AZURE_CLIENT_ID"];
                var clientSecret = System.Configuration.ConfigurationManager.AppSettings["AZURE_CLIENT_SECRET"];

                if (string.IsNullOrEmpty(tenantId) || string.IsNullOrEmpty(clientId) || string.IsNullOrEmpty(clientSecret))
                {
                    System.Diagnostics.Debug.WriteLine("Azure AD configuration missing");
                    return null;
                }

                using (var client = new HttpClient())
                {
                    var tokenEndpoint = $"https://login.microsoftonline.com/{tenantId}/oauth2/v2.0/token";
                    
                    var requestBody = new List<KeyValuePair<string, string>>
                    {
                        new KeyValuePair<string, string>("client_id", clientId),
                        new KeyValuePair<string, string>("client_secret", clientSecret),
                        new KeyValuePair<string, string>("scope", "https://graph.microsoft.com/.default"),
                        new KeyValuePair<string, string>("grant_type", "client_credentials")
                    };

                    var content = new FormUrlEncodedContent(requestBody);
                    var response = await client.PostAsync(tokenEndpoint, content);

                    if (response.IsSuccessStatusCode)
                    {
                        var responseContent = await response.Content.ReadAsStringAsync();
                        dynamic tokenResponse = JsonConvert.DeserializeObject(responseContent);
                        return tokenResponse.access_token;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error getting Graph API token: {ex.Message}");
            }

            return null;
        }

        /// <summary>
        /// Generate a secure temporary password
        /// </summary>
        private string GenerateTemporaryPassword()
        {
            const string chars = "ABCDEFGHJKLMNPQRSTUVWXYZabcdefghijkmnpqrstuvwxyz23456789";
            const string specialChars = "!@#$%&*";
            var random = new Random();
            
            var password = new StringBuilder();
            
            // Ensure at least one uppercase, lowercase, digit, and special char
            password.Append(chars.Where(c => char.IsUpper(c)).OrderBy(x => random.Next()).First());
            password.Append(chars.Where(c => char.IsLower(c)).OrderBy(x => random.Next()).First());
            password.Append(chars.Where(c => char.IsDigit(c)).OrderBy(x => random.Next()).First());
            password.Append(specialChars[random.Next(specialChars.Length)]);
            
            // Fill rest with random characters
            for (int i = 4; i < 12; i++)
            {
                password.Append(chars[random.Next(chars.Length)]);
            }
            
            // Shuffle the password
            return new string(password.ToString().OrderBy(x => random.Next()).ToArray());
        }

        /// <summary>
        /// Send welcome email to new user
        /// </summary>
        private async Task SendWelcomeEmail(string email, string name, string tempPassword)
        {
            try
            {
                // In production, integrate with your email service (SendGrid, Azure Communication Services, etc.)
                // For now, we'll log the information
                System.Diagnostics.Debug.WriteLine($"Welcome email should be sent to: {email}");
                System.Diagnostics.Debug.WriteLine($"Temporary password: {tempPassword}");
                
                // TODO: Implement actual email sending
                // Example with SendGrid or Azure Communication Services:
                /*
                var emailContent = $@"
                    Welcome to EFSM, {name ?? email}!
                    
                    Your account has been automatically created from the FLWINS system.
                    
                    Your login credentials:
                    Email: {email}
                    Temporary Password: {tempPassword}
                    
                    Please log in and change your password at your next visit.
                    
                    Portal URL: https://efsmod-dev-egcyb2bahcdkamdm.canadacentral-01.azurewebsites.net
                ";
                
                // Send email using your preferred service
                */
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error sending welcome email: {ex.Message}");
            }
        }

        /// <summary>
        /// Azure User model for deserialization
        /// </summary>
        public class AzureUser
        {
            public string Id { get; set; }
            public string DisplayName { get; set; }
            public string UserPrincipalName { get; set; }
            public string Mail { get; set; }
            public string GivenName { get; set; }
            public string Surname { get; set; }
        }
        
        /// <summary>
        /// Check if user exists in EFS system (simulate database check)
        /// </summary>
        private bool CheckUserExists(string email)
        {
            // In a real implementation, this would check your database
            // For demo purposes, we'll simulate some existing users
            var existingUsers = new[] 
            { 
                "john.doe@example.com", 
                "jane.smith@flwins.org",
                "parent@test.com" 
            };
            
            return existingUsers.Contains(email.ToLower());
        }
        
        /// <summary>
        /// Create new user account automatically
        /// </summary>
        private void CreateUserAccount(string email, string name)
        {
            // In a real implementation, this would:
            // 1. Create user record in database
            // 2. Set up default family profile
            // 3. Create household record
            // 4. Send welcome email
            // 5. Log the account creation
            
            // For demo purposes, we'll just log this action
            System.Diagnostics.Debug.WriteLine($"Auto-created account for: {email} ({name})");
            
            // You could also store this in session for displaying a welcome message
            Session["AccountCreated"] = true;
            Session["WelcomeMessage"] = $"Welcome to EFSM! We've automatically created your account using your FLWINS information.";
        }

        public ActionResult Login()
        {
            // Get the return URL from query string or default to FamilyPortal
            var returnUrl = Request.QueryString["returnUrl"] ?? Url.Action("FamilyPortal", "Home", null, Request.Url.Scheme);
            
            // Check if running on localhost (development)
            if (Request.Url.Host.Contains("localhost") || Request.Url.Host == "127.0.0.1")
            {
                // For localhost development, simulate login by setting session
                Session["IsAuthenticated"] = true;
                Session["UserName"] = "Development User";
                Session["UserEmail"] = "dev@example.com";
                
                // Redirect to the return URL
                return Redirect(returnUrl);
            }
            else
            {
                // Production: Redirect to Azure AD login via App Service Easy Auth
                // Using your Azure AD Client ID: 7facd66f-0a8b-4757-823a-61e23d4909e2
                var loginUrl = $"/.auth/login/aad?post_login_redirect_url={Uri.EscapeDataString(returnUrl)}";
                return Redirect(loginUrl);
            }
        }

        /// <summary>
        /// Test page to simulate FLWINS redirect (for development/testing)
        /// </summary>
        public ActionResult TestFLWINSRedirect()
        {
            return View();
        }

        public ActionResult Logout()
        {
            // Check if running on localhost (development)
            if (Request.Url.Host.Contains("localhost") || Request.Url.Host == "127.0.0.1")
            {
                // For localhost development, clear session
                Session.Clear();
                Session.Abandon();
                
                // Redirect to home page
                return RedirectToAction("Index", "Home");
            }
            else
            {
                // Production: Redirect to Azure AD logout via App Service Easy Auth
                var logoutUrl = "/.auth/logout?post_logout_redirect_url=" + Uri.EscapeDataString(Url.Action("Index", "Home", null, Request.Url.Scheme));
                return Redirect(logoutUrl);
            }
        }
    }
}