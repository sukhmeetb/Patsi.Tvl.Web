using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using Tvl.BusinessLayer.Interfaces.Login;
using Tvl.BusinessLayer.Interfaces.Security;
using Tvl.Models.Login;

namespace TVLWeb.Controllers
{
    public class LoginController : Controller
    {
        private readonly IValidateUserCredentials _validateUserCredentials;
        private readonly IGetSecurityIdentity _getSecurityIdentity;
        private readonly IGetRedirectionInformation _getRedirectionInformation;
        private readonly ISecurityIdentityExpiration _securityIdentityExpiration;
        private readonly ILogger<LoginController> _logger;
        private readonly IHandleChangePassword _handleChangePassword;

        public LoginController(IGetSecurityIdentity getSecurityIdentity, IValidateUserCredentials validateUserCredentials,
            IGetRedirectionInformation getRedirectionInformation, ILogger<LoginController> logger,
            ISecurityIdentityExpiration securityIdentityExpiration, IHandleChangePassword handleChangePassword)
        {
            _getSecurityIdentity = getSecurityIdentity;
            _validateUserCredentials = validateUserCredentials;
            _getRedirectionInformation = getRedirectionInformation;
            _logger = logger;
            _securityIdentityExpiration = securityIdentityExpiration;
            _handleChangePassword = handleChangePassword;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public IActionResult ChangePassword()
        {
            ChangePasswordCredentials credentials = new ChangePasswordCredentials();
            ViewData["Title"] = "Change Password";

            return View(credentials);
        }

        [HttpGet]
        public IActionResult Login()
        {
            _securityIdentityExpiration.ExpireSecurityIdentity(HttpContext);
            Credentials credentials = new Credentials();
            return View(credentials);
        }

        [HttpPost]
        public async Task<IActionResult> Login(Credentials credentials)
        {
            _logger.LogTrace(ActionMethodLabel.EntryLogMessageTemplate,
                ActionMethodLabel.LoginPost);
            try
            {
                if (ModelState.IsValid)
                {
                    credentials.CredentialVerificationResponse = await _validateUserCredentials
                        .ValidateCredentials(credentials);

                    if (credentials.CredentialVerificationResponse != null
                       && string.IsNullOrWhiteSpace(credentials.CredentialVerificationResponse.ErrorMessage))
                    {
                        await _getSecurityIdentity.CreateSecurityIdentity(credentials.Username,
                        credentials.CredentialVerificationResponse.RoleClaims,
                        credentials.CredentialVerificationResponse.UserClaims, HttpContext);

                        RedirectionInformationModel redirectionInformation = _getRedirectionInformation.GetRedirectionInfo(credentials.CredentialVerificationResponse);
                        TempData["Email"] = credentials.Username;

                        return RedirectToAction(redirectionInformation.ActionMethodName,
                            redirectionInformation.ControllerName);
                    }
                }
                ModelState.Clear();
            }

            catch (Exception exceptionMessage)
            {
                _logger.LogError($"Login Failed: " +
                            $"{exceptionMessage.Message} {exceptionMessage.StackTrace}");
            }

            return View(credentials);
        }
        /// <summary>
        /// Renders the change password view for the user to change the temporary password 
        /// provided by the system
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> ChangePassword(ChangePasswordCredentials changePasswordCredentials)
        {
            try
            {
                //changePasswordCredentials.Email = TempData.Peek("Email").ToString();

                changePasswordCredentials = await _handleChangePassword.PasswordHandler(
                                    changePasswordCredentials);

                if (changePasswordCredentials.IsPasswordChanged)
                {
                    return RedirectToAction("Login");
                }

                ViewData["Title"] = "Change Password";
            }

            catch (Exception exceptionMessage)
            {
                _logger.LogError($"Change Password Failed: " +
                        $"{exceptionMessage.Message} {exceptionMessage.StackTrace}");
            }


            return View(changePasswordCredentials);
        }

        /// <summary>
        /// Signs the user out of the TVL application
        /// by clearing the cookie storing the users Identity 
        /// information and redirecting the user to the login
        /// screen
        /// </summary>
        /// <returns>Redirects to the TVL login screen</returns>
        public IActionResult Signout()
        {
            HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login");
        }

        /// <summary>
        /// This method is to expire the current session
        /// </summary>
        public void SessionEndingRequest()
        {
            _securityIdentityExpiration.ExpireSecurityIdentity(HttpContext);
        }

        ///<summary>
        /// 'Enumeration' class provides text that is written to the logging infrastructure.
        /// </summary>
        private static class ActionMethodLabel
        {
            internal const string LoginPost = "Login Post";
            internal const string EntryLogMessageTemplate = "Entered {0} action method";
        }
    }
}
