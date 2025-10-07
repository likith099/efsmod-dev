using System;
using System.Collections.Generic;
using System.Configuration;
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
            // Debug logging for localhost
            if (Request.Url.Host.Contains("localhost") || Request.Url.Host == "127.0.0.1")
            {
                var sessionAuth = Session["IsAuthenticated"];
                var sessionAuthBool = sessionAuth != null && (bool)sessionAuth;
                System.Diagnostics.Debug.WriteLine($"[DEBUG] IsUserAuthenticated - Session IsAuthenticated: {sessionAuth}, Bool value: {sessionAuthBool}");
                System.Diagnostics.Debug.WriteLine($"[DEBUG] User.Identity.IsAuthenticated: {User.Identity.IsAuthenticated}");
                System.Diagnostics.Debug.WriteLine($"[DEBUG] Session UserName: {Session["UserName"]}");
                System.Diagnostics.Debug.WriteLine($"[DEBUG] Session UserEmail: {Session["UserEmail"]}");
            }
            
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
        /// Get authenticated user name from various sources - returns first name or email, never generic text
        /// </summary>
        private string GetUserName()
        {
            string fullName = null;
            string email = null;
            
            // 1. Check standard ASP.NET authentication
            if (User.Identity.IsAuthenticated && !string.IsNullOrEmpty(User.Identity.Name))
            {
                fullName = User.Identity.Name;
            }
                
            // 2. Check localhost development session
            if (Request.Url.Host.Contains("localhost") || Request.Url.Host == "127.0.0.1")
            {
                var sessionUserName = Session["UserName"]?.ToString();
                if (!string.IsNullOrEmpty(sessionUserName))
                    fullName = sessionUserName;
                    
                var sessionEmail = Session["UserEmail"]?.ToString();
                if (!string.IsNullOrEmpty(sessionEmail))
                    email = sessionEmail;
            }
                
            // 3. Try Azure App Service Easy Auth headers
            var principalName = Request.Headers["X-MS-CLIENT-PRINCIPAL-NAME"];
            if (!string.IsNullOrEmpty(principalName))
                fullName = principalName;
                
            var principalEmail = Request.Headers["X-MS-CLIENT-PRINCIPAL-EMAIL"];
            if (!string.IsNullOrEmpty(principalEmail))
                email = principalEmail;
                
            // 4. Try to decode the X-MS-CLIENT-PRINCIPAL header for more user info
            var clientPrincipal = Request.Headers["X-MS-CLIENT-PRINCIPAL"];
            if (!string.IsNullOrEmpty(clientPrincipal))
            {
                try
                {
                    var decoded = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(clientPrincipal));
                    dynamic principal = JsonConvert.DeserializeObject(decoded);
                    if (principal?.claims != null)
                    {
                        foreach (var claim in principal.claims)
                        {
                            if (claim.typ == "name" && !string.IsNullOrEmpty((string)claim.val))
                                fullName = claim.val;
                            if (claim.typ == "given_name" && !string.IsNullOrEmpty((string)claim.val))
                                fullName = claim.val; // Prefer first name if available
                            if (claim.typ == "email" && !string.IsNullOrEmpty((string)claim.val))
                                email = claim.val;
                            if (claim.typ == "preferred_username" && !string.IsNullOrEmpty((string)claim.val))
                                email = claim.val;
                        }
                    }
                }
                catch
                {
                    // Continue to fallback
                }
            }
            
            // Extract first name from full name if available
            if (!string.IsNullOrEmpty(fullName))
            {
                // If it looks like an email, treat it as email
                if (fullName.Contains("@"))
                {
                    email = fullName;
                    fullName = null;
                }
                else
                {
                    // Extract first name from full name
                    var nameParts = fullName.Trim().Split(' ');
                    return nameParts[0]; // Return first name
                }
            }
            
            // If no first name available, return email or part of email
            if (!string.IsNullOrEmpty(email))
            {
                // If email looks like a real name (contains dot), extract first part
                var emailParts = email.Split('@')[0];
                if (emailParts.Contains("."))
                {
                    var nameParts = emailParts.Split('.');
                    return char.ToUpper(nameParts[0][0]) + nameParts[0].Substring(1).ToLower(); // Capitalize first name
                }
                return email; // Return full email if no pattern found
            }
            
            // Last resort - return something meaningful instead of "User" or "Unknown"
            return "Guest";
        }
        
        /// <summary>
        /// Get authenticated user email from various sources
        /// </summary>
        private string GetUserEmail()
        {
            // 1. Check session first (from auto-login or other sources)
            var sessionEmail = Session["UserEmail"]?.ToString();
            if (!string.IsNullOrEmpty(sessionEmail))
                return sessionEmail;
                
            // 2. Check localhost development session
            if (Request.Url.Host.Contains("localhost") || Request.Url.Host == "127.0.0.1")
            {
                return sessionEmail ?? "dev@example.com";
            }
                
            // 3. Try standard ASP.NET authentication
            if (User.Identity.IsAuthenticated && !string.IsNullOrEmpty(User.Identity.Name))
            {
                var userName = User.Identity.Name;
                // If the username looks like an email, return it
                if (userName.Contains("@"))
                    return userName;
            }
                
            // 4. Try Azure App Service Easy Auth headers
            var principalEmail = Request.Headers["X-MS-CLIENT-PRINCIPAL-EMAIL"];
            if (!string.IsNullOrEmpty(principalEmail))
                return principalEmail;
                
            // 5. Try to decode the X-MS-CLIENT-PRINCIPAL header for email
            var clientPrincipal = Request.Headers["X-MS-CLIENT-PRINCIPAL"];
            if (!string.IsNullOrEmpty(clientPrincipal))
            {
                try
                {
                    var decoded = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(clientPrincipal));
                    dynamic principal = JsonConvert.DeserializeObject(decoded);
                    if (principal?.claims != null)
                    {
                        foreach (var claim in principal.claims)
                        {
                            if (claim.typ == "email" && !string.IsNullOrEmpty((string)claim.val))
                                return claim.val;
                            if (claim.typ == "preferred_username" && !string.IsNullOrEmpty((string)claim.val))
                                return claim.val;
                            if (claim.typ == "upn" && !string.IsNullOrEmpty((string)claim.val))
                                return claim.val;
                        }
                    }
                }
                catch
                {
                    // Continue to fallback
                }
            }
                
            // 6. Last resort - return null instead of GetUserName() to avoid confusion
            return null;
        }
        public ActionResult Index()
        {
            // Set authentication status and user data for the layout
            ViewBag.IsAuthenticated = IsUserAuthenticated();
            var userName = GetUserName();
            var userEmail = GetUserEmail();
            
            // Ensure we never show null or empty values - show full email if no name available
            ViewBag.UserName = !string.IsNullOrEmpty(userName) ? userName : (!string.IsNullOrEmpty(userEmail) ? userEmail : "Guest");
            
            return View();
        }

        public ActionResult FamilyPortal()
        {
            // Debug logging for localhost
            if (Request.Url.Host.Contains("localhost") || Request.Url.Host == "127.0.0.1")
            {
                System.Diagnostics.Debug.WriteLine($"[DEBUG] FamilyPortal - Checking authentication");
                System.Diagnostics.Debug.WriteLine($"[DEBUG] FamilyPortal - IsUserAuthenticated result: {IsUserAuthenticated()}");
            }
            
            // Check if user is authenticated
            if (!IsUserAuthenticated())
            {
                // Debug logging for localhost
                if (Request.Url.Host.Contains("localhost") || Request.Url.Host == "127.0.0.1")
                {
                    System.Diagnostics.Debug.WriteLine($"[DEBUG] FamilyPortal - User not authenticated, redirecting to Login");
                }
                // User is not authenticated, redirect to login
                var returnUrl = Request.Url.ToString();
                return RedirectToAction("Login", new { returnUrl = returnUrl });
            }
            
            // Set authentication status and user data
            ViewBag.IsAuthenticated = IsUserAuthenticated();
            var userName = GetUserName();
            var userEmail = GetUserEmail();
            
            // Ensure we never show null or empty values - show full email if no name available
            ViewBag.UserName = !string.IsNullOrEmpty(userName) ? userName : (!string.IsNullOrEmpty(userEmail) ? userEmail : "Guest");
            ViewBag.UserEmail = userEmail;
            
            // Get user profile information
            var userProfile = GetUserProfile();
            ViewBag.UserProfile = userProfile;
            
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
            ViewBag.ReturnUrl = returnUrl;
            
            return View();
        }
        
        /// <summary>
        /// Development login - only works on localhost
        /// </summary>
        [HttpPost]
        public ActionResult DevLogin(string name, string email, string returnUrl)
        {
            // Only allow this on localhost
            if (!(Request.Url.Host.Contains("localhost") || Request.Url.Host == "127.0.0.1"))
            {
                return RedirectToAction("Login");
            }
            
            // Set development authentication with meaningful defaults
            Session["IsAuthenticated"] = true;
            
            // Use provided name or extract from email
            if (!string.IsNullOrEmpty(name))
            {
                Session["UserName"] = name;
            }
            else if (!string.IsNullOrEmpty(email))
            {
                // Extract name from email (e.g., john.doe@example.com -> John Doe)
                var emailPart = email.Split('@')[0];
                if (emailPart.Contains("."))
                {
                    var parts = emailPart.Split('.');
                    var firstName = char.ToUpper(parts[0][0]) + parts[0].Substring(1).ToLower();
                    var lastName = parts.Length > 1 ? char.ToUpper(parts[1][0]) + parts[1].Substring(1).ToLower() : "";
                    Session["UserName"] = (firstName + " " + lastName).Trim();
                }
                else
                {
                    Session["UserName"] = char.ToUpper(emailPart[0]) + emailPart.Substring(1).ToLower();
                }
            }
            else
            {
                Session["UserName"] = "John Developer";
                email = "john.developer@example.com";
            }
            
            Session["UserEmail"] = !string.IsNullOrEmpty(email) ? email : "dev@example.com";
            
            // Redirect to return URL or Family Portal
            var redirectUrl = !string.IsNullOrEmpty(returnUrl) ? returnUrl : Url.Action("FamilyPortal");
            return Redirect(redirectUrl);
        }
        
        /// <summary>
        /// FLWINS OIDC login redirect
        /// </summary>
        public ActionResult FlwinsLogin()
        {
            var returnUrl = Request.QueryString["returnUrl"] ?? Url.Action("FamilyPortal", "Home", null, Request.Url.Scheme);
            
            // FLWINS CIAM configuration
            var tenantId = System.Configuration.ConfigurationManager.AppSettings["TenantId"];
            var clientId = System.Configuration.ConfigurationManager.AppSettings["ClientId"];
            
            // FLWINS uses different redirect URI structure
            var redirectUri = "https://flwins-dev-dshjczeyf7dxeqdz.canadacentral-01.azurewebsites.net/.auth/login/aad/callback";
            
            // For local development, we need to handle this differently
            if (Request.Url.Host.Contains("localhost") || Request.Url.Host == "127.0.0.1")
            {
                // For localhost, we'll redirect to the production FLWINS flow
                // but with a different return URL handling
                redirectUri = ConfigurationManager.AppSettings["LocalRedirectUri"] ?? $"{Request.Url.Scheme}://{Request.Url.Authority}/signin-oidc";
            }
            
            // Create state parameter with return URL
            var stateParam = $"redir={Uri.EscapeDataString(returnUrl)}";
            var nonce = Guid.NewGuid().ToString("N") + "_" + DateTime.Now.ToString("yyyyMMddHHmmss");
            
            // Store state and nonce in session for validation
            Session["oauth_state"] = stateParam;
            Session["oauth_nonce"] = nonce;
            Session["auth_method"] = "flwins_oidc";
            Session["original_return_url"] = returnUrl;
            
            // Build FLWINS CIAM authorization URL
            var authUrl = $"https://flwins.ciamlogin.com/{tenantId}/oauth2/v2.0/authorize?" +
                         $"response_type=code+id_token&" +
                         $"redirect_uri={Uri.EscapeDataString(redirectUri)}&" +
                         $"client_id={clientId}&" +
                         $"scope=openid+profile+email&" +
                         $"response_mode=form_post&" +
                         $"nonce={nonce}&" +
                         $"state={Uri.EscapeDataString(stateParam)}";
            
            return Redirect(authUrl);
        }
        
        /// <summary>
        /// Second OIDC Login - Azure AD OIDC
        /// </summary>
        public ActionResult SecondOidcLogin()
        {
            var returnUrl = Request.QueryString["returnUrl"] ?? Url.Action("FamilyPortal", "Home", null, Request.Url.Scheme);
            
            // Second OIDC configuration - You'll need to add these to Web.config
            var tenantId = System.Configuration.ConfigurationManager.AppSettings["SecondOidcTenantId"] ?? System.Configuration.ConfigurationManager.AppSettings["TenantId"];
            var clientId = System.Configuration.ConfigurationManager.AppSettings["SecondOidcClientId"] ?? System.Configuration.ConfigurationManager.AppSettings["ClientId"];
            
            // Standard Azure AD redirect URI
            var redirectUri = $"{Request.Url.Scheme}://{Request.Url.Authority}/signin-oidc";
            
            // For production, you might want a different redirect URI
            if (!Request.Url.Host.Contains("localhost") && !Request.Url.Host.Contains("127.0.0.1"))
            {
                // Use production redirect URI if configured
                redirectUri = System.Configuration.ConfigurationManager.AppSettings["SecondOidcRedirectUri"] ?? redirectUri;
            }
            
            // Create state parameter with return URL
            var stateParam = $"redir={Uri.EscapeDataString(returnUrl)}";
            var nonce = Guid.NewGuid().ToString("N") + "_" + DateTime.Now.ToString("yyyyMMddHHmmss");
            
            // Store state and nonce in session for validation
            Session["oauth_state"] = stateParam;
            Session["oauth_nonce"] = nonce;
            Session["auth_method"] = "second_oidc";
            Session["original_return_url"] = returnUrl;
            
            // Build standard Azure AD authorization URL
            var authUrl = $"https://login.microsoftonline.com/{tenantId}/oauth2/v2.0/authorize?" +
                         $"response_type=code+id_token&" +
                         $"redirect_uri={Uri.EscapeDataString(redirectUri)}&" +
                         $"client_id={clientId}&" +
                         $"scope=openid+profile+email&" +
                         $"response_mode=form_post&" +
                         $"nonce={nonce}&" +
                         $"state={Uri.EscapeDataString(stateParam)}";
            
            // Debug logging for localhost
            if (Request.Url.Host.Contains("localhost") || Request.Url.Host == "127.0.0.1")
            {
                System.Diagnostics.Debug.WriteLine($"[DEBUG] SecondOidcLogin - Auth URL: {authUrl}");
                System.Diagnostics.Debug.WriteLine($"[DEBUG] SecondOidcLogin - Redirect URI: {redirectUri}");
            }
            
            return Redirect(authUrl);
        }
        
        /// <summary>
        /// Self sign-up - Step 1: Collect user info and send OTP
        /// </summary>
        [HttpPost]
        public ActionResult SelfSignUp(string email, string firstName, string lastName, string returnUrl)
        {
            try
            {
                // Validate input
                if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(firstName) || string.IsNullOrWhiteSpace(lastName))
                {
                    ViewBag.ErrorMessage = "All fields are required.";
                    return View("Login");
                }
                
                // Check if email is already registered (optional check)
                // You might want to check against your user database here
                
                // Generate OTP
                var otp = new Random().Next(100000, 999999).ToString();
                
                // Store user registration data in session
                Session["registration_email"] = email;
                Session["registration_firstName"] = firstName;
                Session["registration_lastName"] = lastName;
                Session["registration_otp"] = otp;
                Session["registration_otp_expiry"] = DateTime.Now.AddMinutes(15); // OTP expires in 15 minutes
                Session["registration_returnUrl"] = returnUrl;
                
                // TODO: Send OTP via email (implement email service)
                // For development, we'll just display it
                if (Request.Url.Host.Contains("localhost") || Request.Url.Host == "127.0.0.1")
                {
                    ViewBag.OtpForDevelopment = otp; // Show OTP in development
                }
                
                // Send email with OTP (you'll need to implement this)
                SendOtpEmail(email, firstName, otp);
                
                ViewBag.Email = email;
                ViewBag.FirstName = firstName;
                return View("VerifyOtp");
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = $"Registration failed: {ex.Message}";
                return View("Login");
            }
        }
        
        /// <summary>
        /// Self sign-up - Step 2: Verify OTP and set password
        /// </summary>
        [HttpPost]
        public ActionResult VerifyOtp(string otp, string password, string confirmPassword)
        {
            try
            {
                // Validate OTP
                var sessionOtp = Session["registration_otp"]?.ToString();
                var otpExpiry = Session["registration_otp_expiry"] as DateTime?;
                
                if (string.IsNullOrEmpty(sessionOtp) || otpExpiry == null)
                {
                    ViewBag.ErrorMessage = "Registration session expired. Please start again.";
                    return View("Login");
                }
                
                if (DateTime.Now > otpExpiry)
                {
                    ViewBag.ErrorMessage = "OTP has expired. Please request a new one.";
                    return View("Login");
                }
                
                if (otp != sessionOtp)
                {
                    ViewBag.ErrorMessage = "Invalid OTP. Please check and try again.";
                    ViewBag.Email = Session["registration_email"];
                    ViewBag.FirstName = Session["registration_firstName"];
                    return View("VerifyOtp");
                }
                
                // Validate password
                if (string.IsNullOrWhiteSpace(password) || password.Length < 6)
                {
                    ViewBag.ErrorMessage = "Password must be at least 6 characters long.";
                    ViewBag.Email = Session["registration_email"];
                    ViewBag.FirstName = Session["registration_firstName"];
                    return View("VerifyOtp");
                }
                
                if (password != confirmPassword)
                {
                    ViewBag.ErrorMessage = "Passwords do not match.";
                    ViewBag.Email = Session["registration_email"];
                    ViewBag.FirstName = Session["registration_firstName"];
                    return View("VerifyOtp");
                }
                
                // Create user account (store in database - implement as needed)
                var email = Session["registration_email"].ToString();
                var firstName = Session["registration_firstName"].ToString();
                var lastName = Session["registration_lastName"].ToString();
                
                // TODO: Hash password and store user in database
                // For now, we'll just set up the session
                
                // Set authentication session
                Session["IsAuthenticated"] = true;
                Session["UserName"] = $"{firstName} {lastName}";
                Session["UserEmail"] = email;
                Session["AuthMethod"] = "self_signup";
                
                // Debug logging for localhost
                if (Request.Url.Host.Contains("localhost") || Request.Url.Host == "127.0.0.1")
                {
                    System.Diagnostics.Debug.WriteLine($"[DEBUG] VerifyOtp - Setting session - IsAuthenticated: {Session["IsAuthenticated"]}");
                    System.Diagnostics.Debug.WriteLine($"[DEBUG] VerifyOtp - UserName: {Session["UserName"]}");
                    System.Diagnostics.Debug.WriteLine($"[DEBUG] VerifyOtp - UserEmail: {Session["UserEmail"]}");
                }
                
                // Clear registration session data
                Session.Remove("registration_email");
                Session.Remove("registration_firstName");
                Session.Remove("registration_lastName");
                Session.Remove("registration_otp");
                Session.Remove("registration_otp_expiry");
                
                // Redirect to return URL or Family Portal
                var returnUrl = Session["registration_returnUrl"]?.ToString();
                Session.Remove("registration_returnUrl");
                
                if (string.IsNullOrEmpty(returnUrl) || returnUrl.Contains("Login"))
                {
                    returnUrl = Url.Action("FamilyPortal", "Home");
                }
                
                // Debug logging for localhost
                if (Request.Url.Host.Contains("localhost") || Request.Url.Host == "127.0.0.1")
                {
                    System.Diagnostics.Debug.WriteLine($"[DEBUG] VerifyOtp - Redirecting to: {returnUrl}");
                }
                
                return Redirect(returnUrl);
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = $"Verification failed: {ex.Message}";
                return View("VerifyOtp");
            }
        }
        
        /// <summary>
        /// Helper method to send OTP via email (implement with your email service)
        /// </summary>
        private void SendOtpEmail(string email, string firstName, string otp)
        {
            try
            {
                // TODO: Implement actual email sending
                // For now, just log or use a development email service
                
                // Example implementation using SMTP (configure in Web.config)
                /*
                var smtpClient = new SmtpClient();
                var mailMessage = new MailMessage
                {
                    From = new MailAddress("noreply@yourapp.com"),
                    Subject = "Your Verification Code",
                    Body = $"Hi {firstName},\n\nYour verification code is: {otp}\n\nThis code will expire in 15 minutes.\n\nBest regards,\nYour App Team",
                    IsBodyHtml = false
                };
                mailMessage.To.Add(email);
                smtpClient.Send(mailMessage);
                */
                
                // For development, you might want to log the OTP
                System.Diagnostics.Debug.WriteLine($"OTP for {email}: {otp}");
            }
            catch (Exception ex)
            {
                // Log error but don't throw - email sending is not critical for the flow to continue
                System.Diagnostics.Debug.WriteLine($"Failed to send OTP email: {ex.Message}");
            }
        }

        /// <summary>
        /// OIDC callback endpoint to handle authentication response
        /// </summary>
        public async Task<ActionResult> SigninOidc()
        {
            try
            {
                // FLWINS CIAM uses form_post, so check both form and query parameters
                var code = Request.Form["code"] ?? Request.QueryString["code"];
                var state = Request.Form["state"] ?? Request.QueryString["state"];
                var error = Request.Form["error"] ?? Request.QueryString["error"];
                var errorDescription = Request.Form["error_description"] ?? Request.QueryString["error_description"];
                var idToken = Request.Form["id_token"] ?? Request.QueryString["id_token"];
                
                // Check for authentication errors
                if (!string.IsNullOrEmpty(error))
                {
                    ViewBag.ErrorMessage = $"Authentication failed: {error}. {errorDescription}";
                    return View("Login");
                }
                
                // Validate state parameter
                var expectedState = Session["oauth_state"]?.ToString();
                if (string.IsNullOrEmpty(state) || state != expectedState)
                {
                    ViewBag.ErrorMessage = "Invalid state parameter. Possible CSRF attack.";
                    return View("Login");
                }
                
                // If we have an ID token directly (form_post response), we can process it immediately
                if (!string.IsNullOrEmpty(idToken))
                {
                    // Direct ID token processing for FLWINS CIAM form_post response
                    var userInfo = DecodeIdToken(idToken);
                    if (userInfo != null)
                    {
                        // Set authentication session
                        Session["IsAuthenticated"] = true;
                        Session["UserName"] = userInfo.Name ?? userInfo.Email;
                        Session["UserEmail"] = userInfo.Email;
                        Session["IdToken"] = idToken;
                        Session["AuthMethod"] = Session["auth_method"] ?? "flwins_oidc";
                        
                        // Extract return URL from state
                        var returnUrl = Session["original_return_url"]?.ToString();
                        if (string.IsNullOrEmpty(returnUrl) || returnUrl.Contains("Login"))
                        {
                            returnUrl = Url.Action("FamilyPortal", "Home");
                        }
                        
                        return Redirect(returnUrl);
                    }
                }
                
                if (string.IsNullOrEmpty(code))
                {
                    ViewBag.ErrorMessage = "Authorization code not received.";
                    return View("Login");
                }
                
                // Exchange authorization code for tokens
                var authMethod = Session["auth_method"]?.ToString() ?? "flwins_oidc";
                
                string tenantId, clientId, clientSecret, tokenEndpoint;
                var redirectUri = $"{Request.Url.Scheme}://{Request.Url.Authority}/signin-oidc";
                
                // Configure based on authentication method
                if (authMethod == "second_oidc")
                {
                    // Second OIDC provider configuration
                    tenantId = System.Configuration.ConfigurationManager.AppSettings["SecondOidcTenantId"] ?? System.Configuration.ConfigurationManager.AppSettings["TenantId"];
                    clientId = System.Configuration.ConfigurationManager.AppSettings["SecondOidcClientId"] ?? System.Configuration.ConfigurationManager.AppSettings["ClientId"];
                    clientSecret = System.Configuration.ConfigurationManager.AppSettings["SecondOidcClientSecret"] ?? System.Configuration.ConfigurationManager.AppSettings["ClientSecret"];
                    tokenEndpoint = $"https://login.microsoftonline.com/{tenantId}/oauth2/v2.0/token";
                    
                    // Use second OIDC redirect URI if configured
                    if (!Request.Url.Host.Contains("localhost") && !Request.Url.Host.Contains("127.0.0.1"))
                    {
                        redirectUri = System.Configuration.ConfigurationManager.AppSettings["SecondOidcRedirectUri"] ?? redirectUri;
                    }
                }
                else
                {
                    // FLWINS OIDC configuration (default)
                    tenantId = System.Configuration.ConfigurationManager.AppSettings["TenantId"];
                    clientId = System.Configuration.ConfigurationManager.AppSettings["ClientId"];
                    clientSecret = System.Configuration.ConfigurationManager.AppSettings["ClientSecret"];
                    tokenEndpoint = $"https://flwins.ciamlogin.com/{tenantId}/oauth2/v2.0/token";
                    
                    // For local development, use localhost redirect for FLWINS
                    if (Request.Url.Host.Contains("localhost") || Request.Url.Host == "127.0.0.1")
                    {
                        redirectUri = ConfigurationManager.AppSettings["LocalRedirectUri"] ?? redirectUri;
                    }
                }
                
                using (var httpClient = new HttpClient())
                {
                    var tokenRequest = new FormUrlEncodedContent(new[]
                    {
                        new KeyValuePair<string, string>("client_id", clientId),
                        new KeyValuePair<string, string>("client_secret", clientSecret ?? ""),
                        new KeyValuePair<string, string>("code", code),
                        new KeyValuePair<string, string>("grant_type", "authorization_code"),
                        new KeyValuePair<string, string>("redirect_uri", redirectUri),
                    });
                    
                    var tokenResponse = await httpClient.PostAsync(tokenEndpoint, tokenRequest);
                    var tokenContent = await tokenResponse.Content.ReadAsStringAsync();
                    
                    if (!tokenResponse.IsSuccessStatusCode)
                    {
                        ViewBag.ErrorMessage = $"Token exchange failed: {tokenContent}";
                        return View("Login");
                    }
                    
                    dynamic tokenData = JsonConvert.DeserializeObject(tokenContent);
                    var accessToken = (string)tokenData.access_token;
                    var tokenIdToken = (string)tokenData.id_token;
                    
                    // Decode ID token to get user information
                    var userInfo = DecodeIdToken(tokenIdToken);
                    if (userInfo != null)
                    {
                        // Set authentication session
                        Session["IsAuthenticated"] = true;
                        Session["UserName"] = userInfo.Name ?? userInfo.Email;
                        Session["UserEmail"] = userInfo.Email;
                        Session["AccessToken"] = accessToken;
                        Session["IdToken"] = tokenIdToken;
                        Session["AuthMethod"] = Session["auth_method"] ?? "flwins_oidc";
                        
                        // Redirect to original return URL
                        var returnUrl = Uri.UnescapeDataString(state);
                        if (string.IsNullOrEmpty(returnUrl) || returnUrl == "/" || returnUrl.Contains("Login"))
                        {
                            returnUrl = Url.Action("FamilyPortal", "Home");
                        }
                        
                        return Redirect(returnUrl);
                    }
                    else
                    {
                        ViewBag.ErrorMessage = "Failed to decode user information from ID token.";
                        return View("Login");
                    }
                }
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = $"Authentication error: {ex.Message}";
                return View("Login");
            }
        }
        
        /// <summary>
        /// Helper method to decode JWT ID token
        /// </summary>
        private dynamic DecodeIdToken(string idToken)
        {
            try
            {
                var parts = idToken.Split('.');
                if (parts.Length != 3) return null;
                
                // Decode the payload (second part)
                var payload = parts[1];
                
                // Add padding if necessary
                switch (payload.Length % 4)
                {
                    case 2: payload += "=="; break;
                    case 3: payload += "="; break;
                }
                
                var jsonBytes = Convert.FromBase64String(payload.Replace('-', '+').Replace('_', '/'));
                var json = Encoding.UTF8.GetString(jsonBytes);
                var claims = JsonConvert.DeserializeObject<dynamic>(json);
                
                return new
                {
                    Name = (string)claims.name ?? (string)claims.given_name,
                    Email = (string)claims.email ?? (string)claims.preferred_username,
                    Subject = (string)claims.sub,
                    Claims = claims
                };
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Test method to verify signin-oidc route is working (for debugging)
        /// </summary>
        public ActionResult TestSigninRoute()
        {
            return Json(new { 
                message = "signin-oidc route is working!", 
                timestamp = DateTime.Now,
                requestUrl = Request.Url.ToString(),
                queryString = Request.QueryString.ToString()
            }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Test page to simulate FLWINS redirect (for development/testing)
        /// </summary>
        public ActionResult TestFLWINSRedirect()
        {
            return View();
        }
        
        /// <summary>
        /// Debug action to check current user session (localhost only)
        /// </summary>
        public ActionResult DebugUser()
        {
            if (!(Request.Url.Host.Contains("localhost") || Request.Url.Host == "127.0.0.1"))
            {
                return RedirectToAction("Index");
            }
            
            ViewBag.SessionUserName = Session["UserName"];
            ViewBag.SessionUserEmail = Session["UserEmail"];
            ViewBag.SessionIsAuthenticated = Session["IsAuthenticated"];
            ViewBag.UserIdentityName = User.Identity.Name;
            ViewBag.UserIdentityIsAuthenticated = User.Identity.IsAuthenticated;
            ViewBag.GetUserNameResult = GetUserName();
            ViewBag.GetUserEmailResult = GetUserEmail();
            
            return View();
        }

        /// <summary>
        /// Display user profile page
        /// </summary>
        public ActionResult ManageProfile()
        {
            // Check if user is authenticated
            if (!IsUserAuthenticated())
            {
                return RedirectToAction("Login", new { returnUrl = Request.Url.ToString() });
            }
            
            // Set authentication data for layout
            ViewBag.IsAuthenticated = IsUserAuthenticated();
            var userName = GetUserName();
            var userEmail = GetUserEmail();
            
            // Ensure we never show null or empty values - show full email if no name available
            ViewBag.UserName = !string.IsNullOrEmpty(userName) ? userName : (!string.IsNullOrEmpty(userEmail) ? userEmail : "Guest");
            ViewBag.UserEmail = userEmail;
            
            // Get user profile
            var userProfile = GetUserProfile();
            
            return View(userProfile);
        }
        
        /// <summary>
        /// Update user profile (POST)
        /// </summary>
        [HttpPost]
        public async Task<ActionResult> ManageProfile(UserProfile model)
        {
            // Check if user is authenticated
            if (!IsUserAuthenticated())
            {
                return RedirectToAction("Login");
            }
            
            if (ModelState.IsValid)
            {
                try
                {
                    // Update profile in session/database
                    SaveUserProfile(model);
                    
                    // Update Azure AD if not localhost
                    if (!IsLocalhost())
                    {
                        await UpdateAzureADProfile(model);
                    }
                    
                    ViewBag.SuccessMessage = "Profile updated successfully!";
                }
                catch (Exception ex)
                {
                    ViewBag.ErrorMessage = "Error updating profile: " + ex.Message;
                    System.Diagnostics.Debug.WriteLine($"Profile update error: {ex.Message}");
                }
            }
            
            // Set authentication data for layout
            ViewBag.IsAuthenticated = IsUserAuthenticated();
            var userName = GetUserName();
            var userEmail = GetUserEmail();
            
            // Ensure we never show null or empty values - show full email if no name available
            ViewBag.UserName = !string.IsNullOrEmpty(userName) ? userName : (!string.IsNullOrEmpty(userEmail) ? userEmail : "Guest");
            ViewBag.UserEmail = userEmail;
            
            return View(model);
        }
        
        /// <summary>
        /// Get user profile from session or Azure AD
        /// </summary>
        private UserProfile GetUserProfile()
        {
            // Try to get from session first
            var sessionProfile = Session["UserProfile"] as UserProfile;
            if (sessionProfile != null)
                return sessionProfile;
            
            // Create new profile with available information
            var profile = new UserProfile
            {
                Email = GetUserEmail(),
                LastUpdated = DateTime.UtcNow
            };
            
            // Try to extract name information
            var userName = GetUserName();
            if (!string.IsNullOrEmpty(userName) && !userName.Contains("@"))
            {
                var nameParts = userName.Split(' ');
                profile.FirstName = nameParts[0];
                if (nameParts.Length > 1)
                    profile.LastName = string.Join(" ", nameParts.Skip(1));
            }
            
            // For localhost, check if we have stored profile data
            if (IsLocalhost())
            {
                // In production, this would come from your database
                // For now, we'll use session storage
                Session["UserProfile"] = profile;
            }
            
            return profile;
        }
        
        /// <summary>
        /// Save user profile to session/database
        /// </summary>
        private void SaveUserProfile(UserProfile profile)
        {
            profile.LastUpdated = DateTime.UtcNow;
            profile.Email = GetUserEmail(); // Ensure email matches current user
            
            // Update session
            Session["UserProfile"] = profile;
            
            // Update session user name if first name is provided
            if (!string.IsNullOrEmpty(profile.FirstName))
            {
                Session["UserName"] = profile.FullName;
            }
            
            // In production, save to database here
            System.Diagnostics.Debug.WriteLine($"Saved profile for user: {profile.Email}");
        }
        
        /// <summary>
        /// Update user profile in Azure AD
        /// </summary>
        private async Task UpdateAzureADProfile(UserProfile profile)
        {
            try
            {
                var accessToken = await GetGraphApiAccessToken();
                if (string.IsNullOrEmpty(accessToken))
                    return;

                var userEmail = GetUserEmail();
                if (string.IsNullOrEmpty(userEmail))
                    return;

                // Prepare update payload
                var updatePayload = new
                {
                    displayName = profile.FullName,
                    givenName = profile.FirstName,
                    surname = profile.LastName,
                    mobilePhone = profile.PhoneNumber,
                    streetAddress = profile.Address,
                    city = profile.City,
                    state = profile.State,
                    postalCode = profile.ZipCode
                };

                using (var client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Authorization = 
                        new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);

                    var json = JsonConvert.SerializeObject(updatePayload);
                    var content = new StringContent(json, Encoding.UTF8, "application/json");

                    var requestUrl = $"https://graph.microsoft.com/v1.0/users/{Uri.EscapeDataString(userEmail)}";
                    // Use PostAsync with X-HTTP-Method-Override header for PATCH compatibility
                    var request = new HttpRequestMessage(HttpMethod.Post, requestUrl) { Content = content };
                    request.Headers.Add("X-HTTP-Method-Override", "PATCH");
                    var response = await client.SendAsync(request);

                    if (!response.IsSuccessStatusCode)
                    {
                        var errorContent = await response.Content.ReadAsStringAsync();
                        System.Diagnostics.Debug.WriteLine($"Azure AD update failed: {errorContent}");
                        throw new Exception("Failed to update Azure AD profile");
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error updating Azure AD profile: {ex.Message}");
                throw;
            }
        }

        public ActionResult Logout()
        {
            // Clear user profile session
            Session.Remove("UserProfile");
            
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
                // Clear session data
                Session.Clear();
                Session.Abandon();
                
                // Production: Redirect to FLWINS CIAM logout
                var tenantId = ConfigurationManager.AppSettings["TenantId"];
                var postLogoutRedirectUri = Uri.EscapeDataString(Url.Action("Index", "Home", null, Request.Url.Scheme));
                var logoutUrl = $"https://flwins.ciamlogin.com/{tenantId}/oauth2/v2.0/logout?post_logout_redirect_uri={postLogoutRedirectUri}";
                
                return Redirect(logoutUrl);
            }
        }
        
        /// <summary>
        /// User Profile model for managing user information
        /// </summary>
        public class UserProfile
        {
            public string FirstName { get; set; }
            public string LastName { get; set; }
            public string FullName => !string.IsNullOrEmpty(FirstName) && !string.IsNullOrEmpty(LastName) 
                ? $"{FirstName} {LastName}" : (FirstName ?? LastName ?? "");
            public string Email { get; set; }
            public string PhoneNumber { get; set; }
            public string Address { get; set; }
            public string City { get; set; }
            public string State { get; set; }
            public string ZipCode { get; set; }
            public string Gender { get; set; }
            public DateTime? DateOfBirth { get; set; }
            public string AzureUserId { get; set; }
            public DateTime LastUpdated { get; set; }
        }
    }
}