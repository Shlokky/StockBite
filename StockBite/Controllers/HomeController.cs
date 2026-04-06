using System.Diagnostics;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using StockBite.Data;
using StockBite.Models;
using StockBite.Services;
using StockBite.ViewModels;

namespace StockBite.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _dbContext;
        private readonly IRoleContext _roleContext;
        private readonly IConfiguration _configuration;
        private readonly ConsumerEmailFlowService _consumerEmailFlowService;
        private readonly ConsumerAuthCodeService _consumerAuthCodeService;
        private readonly string _demoPassword;

        public HomeController(
            ILogger<HomeController> logger,
            ApplicationDbContext dbContext,
            IRoleContext roleContext,
            IConfiguration configuration,
            ConsumerEmailFlowService consumerEmailFlowService,
            ConsumerAuthCodeService consumerAuthCodeService)
        {
            _logger = logger;
            _dbContext = dbContext;
            _roleContext = roleContext;
            _configuration = configuration;
            _consumerEmailFlowService = consumerEmailFlowService;
            _consumerAuthCodeService = consumerAuthCodeService;
            _demoPassword = configuration["DemoAuth:Password"] ?? "xyzpass";
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult RoleLogin(string role)
        {
            if (!TryGetSupportedRole(role, out var userRole))
            {
                return RedirectToAction(nameof(Index));
            }

            if (userRole == UserRole.Consumer)
            {
                return RedirectToAction(nameof(ConsumerAccess));
            }

            ViewBag.SelectedRole = userRole;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Login(string role, string password)
        {
            if (!Enum.TryParse(role, true, out UserRole userRole))
            {
                TempData["ErrorMessage"] = "Invalid role selection.";
                return RedirectToAction(nameof(Index));
            }

            if (userRole == UserRole.Public)
            {
                TempData["ErrorMessage"] = "Please select a valid role.";
                return RedirectToAction(nameof(Index));
            }

            if (string.IsNullOrWhiteSpace(password) || password != _demoPassword)
            {
                TempData["ErrorMessage"] = "Invalid password.";
                ResetUserContext();
                return RedirectToAction(nameof(RoleLogin), new { role = userRole.ToString() });
            }

            ResetUserContext();
            _roleContext.SetRole(userRole);
            _roleContext.SetAuthenticated(true);

            return userRole switch
            {
                UserRole.Admin => RedirectToAction("Dashboard", "Admin"),
                UserRole.StockManager => RedirectToAction("Index", "StockManager"),
                UserRole.Consumer => RedirectToAction("Index", "Consumer"),
                _ => RedirectToAction(nameof(Index))
            };
        }

        public IActionResult Privacy()
        {
            return View();
        }

        public IActionResult ConsumerAccess(string? externalError = null, string? email = null, bool showCode = false)
        {
            ViewData["ConsumerAuthMode"] = "Login";
            ViewData["ConsumerAuthShowCode"] = showCode;
            ViewBag.GoogleEnabled = IsGoogleConfigured();
            ViewBag.ExternalError = externalError;
            var model = BuildConsumerAccessModel(email);
            return View(model);
        }

        public IActionResult ConsumerSignUp(string? externalError = null, string? email = null, bool showVerification = false)
        {
            ViewData["ConsumerAuthMode"] = "SignUp";
            ViewData["ConsumerAuthShowVerification"] = showVerification;
            ViewBag.GoogleEnabled = IsGoogleConfigured();
            ViewBag.ExternalError = externalError;
            var model = BuildConsumerAccessModel(email);
            return View(model);
        }

        public IActionResult StartGuestCheckout()
        {
            var guestCode = Guid.NewGuid().ToString("N")[..6].ToUpper();
            var guestConsumer = new Consumer
            {
                Name = $"Guest User {guestCode}",
                GuestCode = guestCode
            };

            _dbContext.Consumers.Add(guestConsumer);
            _dbContext.SaveChanges();

            SignInConsumer(guestConsumer.Id);
            return RedirectToAction("Index", "Consumer");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ContinueGuest(string guestCode)
        {
            guestCode = (guestCode ?? string.Empty).Trim().ToUpper();

            if (string.IsNullOrWhiteSpace(guestCode))
            {
                TempData["ErrorMessage"] = "Enter your tracking code.";
                return RedirectToAction(nameof(ConsumerAccess));
            }

            var guestConsumer = _dbContext.Consumers.FirstOrDefault(c => c.GuestCode == guestCode);
            if (guestConsumer == null)
            {
                TempData["ErrorMessage"] = "Tracking code not found.";
                return RedirectToAction(nameof(ConsumerAccess));
            }

            SignInConsumer(guestConsumer.Id);
            return RedirectToAction("Orders", "Consumer");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SignUpWithEmail(ConsumerAccessViewModel model)
        {
            model.SignUpName = CleanText(model.SignUpName);
            model.SignUpEmail = CleanEmail(model.SignUpEmail);
            model.SignUpPassword = CleanText(model.SignUpPassword);

            if (string.IsNullOrWhiteSpace(model.SignUpName) || string.IsNullOrWhiteSpace(model.SignUpEmail) || string.IsNullOrWhiteSpace(model.SignUpPassword))
            {
                TempData["ErrorMessage"] = "Enter your name, email, and password.";
                return RedirectToAction(nameof(ConsumerSignUp), new { email = model.SignUpEmail });
            }

            var consumer = _dbContext.Consumers.FirstOrDefault(c => c.Email != null && c.Email.ToLower() == model.SignUpEmail.ToLower());
            if (consumer == null)
            {
                consumer = new Consumer
                {
                    Name = model.SignUpName,
                    Email = model.SignUpEmail,
                    Password = model.SignUpPassword,
                    GuestCode = Guid.NewGuid().ToString("N")[..6].ToUpper()
                };

                _dbContext.Consumers.Add(consumer);
            }
            else
            {
                consumer.Name = model.SignUpName;
                consumer.Email = model.SignUpEmail;
                consumer.Password = model.SignUpPassword;
            }

            if (consumer.EmailVerified)
            {
                TempData["SuccessMessage"] = "Email already verified. Use the login page to sign in.";
                return RedirectToAction(nameof(ConsumerAccess), new { email = model.SignUpEmail });
            }

            if (!IsEmailConfigured())
            {
                TempData["ErrorMessage"] = "Verification email is not configured on this server yet. Add SMTP settings in appsettings.json first.";
                return RedirectToAction(nameof(ConsumerSignUp), new { email = model.SignUpEmail });
            }

            var code = _consumerEmailFlowService.GenerateAccessCode();
            _consumerAuthCodeService.SetVerificationCode(consumer, code);
            _dbContext.SaveChanges();
            await _consumerEmailFlowService.SendVerificationCodeAsync(consumer, code);

            TempData["SuccessMessage"] = "Verification code sent to your email.";
            return RedirectToAction(nameof(ConsumerSignUp), new { email = model.SignUpEmail, showVerification = true });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult VerifyEmailSignUp(ConsumerAccessViewModel model)
        {
            model.VerificationEmail = CleanEmail(model.VerificationEmail);
            model.VerificationCode = CleanText(model.VerificationCode);

            var consumer = _dbContext.Consumers.FirstOrDefault(c => c.Email != null && c.Email.ToLower() == model.VerificationEmail.ToLower());
            if (consumer == null)
            {
                TempData["ErrorMessage"] = "Email account not found.";
                return RedirectToAction(nameof(ConsumerSignUp), new { email = model.VerificationEmail, showVerification = true });
            }

            if (!_consumerAuthCodeService.IsVerificationCodeValid(consumer, model.VerificationCode))
            {
                TempData["ErrorMessage"] = "Verification code is invalid or expired.";
                return RedirectToAction(nameof(ConsumerSignUp), new { email = model.VerificationEmail, showVerification = true });
            }

            consumer.EmailVerified = true;
            _consumerAuthCodeService.ClearCode(consumer);
            _dbContext.SaveChanges();
            TempData["SuccessMessage"] = "Email verified successfully. You can sign in now.";
            return RedirectToAction(nameof(ConsumerAccess), new { email = model.VerificationEmail, showCode = false });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> StartEmailLogin(ConsumerAccessViewModel model)
        {
            model.LoginEmail = CleanEmail(model.LoginEmail);
            model.LoginPassword = CleanText(model.LoginPassword);

            var consumer = _dbContext.Consumers.FirstOrDefault(c => c.Email != null && c.Email.ToLower() == model.LoginEmail.ToLower());
            if (consumer == null)
            {
                TempData["ErrorMessage"] = "Email account not found.";
                return RedirectToAction(nameof(ConsumerAccess), new { email = model.LoginEmail });
            }

            if (string.IsNullOrWhiteSpace(model.LoginPassword))
            {
                TempData["ErrorMessage"] = "Enter your password.";
                return RedirectToAction(nameof(ConsumerAccess), new { email = model.LoginEmail });
            }

            if (!string.Equals(consumer.Password, model.LoginPassword, StringComparison.Ordinal))
            {
                TempData["ErrorMessage"] = "Password is incorrect.";
                return RedirectToAction(nameof(ConsumerAccess), new { email = model.LoginEmail });
            }

            if (!consumer.EmailVerified)
            {
                TempData["ErrorMessage"] = "Verify your email first, then sign in.";
                return RedirectToAction(nameof(ConsumerSignUp), new { email = model.LoginEmail, showVerification = true });
            }

            if (!IsEmailConfigured())
            {
                TempData["ErrorMessage"] = "Login email is not configured on this server yet. Add SMTP settings in appsettings.json first.";
                return RedirectToAction(nameof(ConsumerAccess), new { email = model.LoginEmail });
            }

            var code = _consumerEmailFlowService.GenerateAccessCode();
            _consumerAuthCodeService.SetLoginCode(consumer, code);
            _dbContext.SaveChanges();
            await _consumerEmailFlowService.SendLoginCodeAsync(consumer, code);

            TempData["SuccessMessage"] = "Login code sent to your email.";
            return RedirectToAction(nameof(ConsumerAccess), new { email = model.LoginEmail, showCode = true });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult VerifyEmailLogin(ConsumerAccessViewModel model)
        {
            model.LoginEmail = CleanEmail(model.LoginEmail);
            model.LoginCode = CleanText(model.LoginCode);

            var consumer = _dbContext.Consumers.FirstOrDefault(c => c.Email != null && c.Email.ToLower() == model.LoginEmail.ToLower());
            if (consumer == null)
            {
                TempData["ErrorMessage"] = "Email account not found.";
                return RedirectToAction(nameof(ConsumerAccess), new { email = model.LoginEmail, showCode = true });
            }

            if (!_consumerAuthCodeService.IsLoginCodeValid(consumer, model.LoginCode))
            {
                TempData["ErrorMessage"] = "Login code is invalid or expired.";
                return RedirectToAction(nameof(ConsumerAccess), new { email = model.LoginEmail, showCode = true });
            }

            _consumerAuthCodeService.ClearCode(consumer);
            _dbContext.SaveChanges();
            SignInConsumer(consumer.Id);
            TempData["SuccessMessage"] = "Signed in successfully.";
            return RedirectToAction("Index", "Consumer");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult StartExternalLogin()
        {
            if (!IsGoogleConfigured())
            {
                TempData["ErrorMessage"] = "Google sign-in is not configured yet on this server.";
                return RedirectToAction(nameof(ConsumerAccess));
            }

            var properties = new AuthenticationProperties
            {
                RedirectUri = Url.Action(nameof(ExternalLoginCallback))
            };

            return Challenge(properties, "Google");
        }

        public async Task<IActionResult> ExternalLoginCallback()
        {
            var externalResult = await HttpContext.AuthenticateAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = externalResult.Principal ?? User;

            if (principal?.Identity?.IsAuthenticated != true)
            {
                TempData["ErrorMessage"] = "Google sign-in was not completed.";
                return RedirectToAction(nameof(ConsumerAccess));
            }

            var email = principal.FindFirstValue(ClaimTypes.Email) ?? principal.FindFirstValue("email");
            var name = principal.FindFirstValue(ClaimTypes.Name) ?? principal.FindFirstValue("name");

            if (string.IsNullOrWhiteSpace(email))
            {
                await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                TempData["ErrorMessage"] = "Google did not return an email address. Please try again.";
                return RedirectToAction(nameof(ConsumerAccess));
            }

            var consumer = _dbContext.Consumers.FirstOrDefault(c => c.Email != null && c.Email.ToLower() == email.ToLower());
            if (consumer == null)
            {
                consumer = new Consumer
                {
                    Name = string.IsNullOrWhiteSpace(name) ? "Google User" : name,
                    Email = email,
                    EmailVerified = true,
                    GuestCode = Guid.NewGuid().ToString("N")[..6].ToUpper()
                };

                _dbContext.Consumers.Add(consumer);
            }
            else
            {
                if (string.IsNullOrWhiteSpace(consumer.Name) && !string.IsNullOrWhiteSpace(name))
                {
                    consumer.Name = name;
                }

                consumer.Email = email;
                consumer.EmailVerified = true;
            }

            _dbContext.SaveChanges();

            SignInConsumer(consumer.Id);
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            TempData["SuccessMessage"] = "Google sign-in completed successfully.";
            return RedirectToAction("Index", "Consumer");
        }

        public async Task<IActionResult> Logout()
        {
            ResetUserContext();
            HttpContext.Session.Clear();
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction(nameof(Index));
        }

        public IActionResult ConsumerProducts()
        {
            ResetUserContext();

            var products = _dbContext.Products.ToList();
            return View(products);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel
            {
                RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier
            });
        }

        private bool TryGetSupportedRole(string? role, out UserRole userRole)
        {
            if (Enum.TryParse(role, true, out userRole))
            {
                return userRole == UserRole.Admin
                    || userRole == UserRole.StockManager
                    || userRole == UserRole.Consumer;
            }

            userRole = UserRole.Public;
            return false;
        }

        private bool IsGoogleConfigured()
        {
            var clientId = _configuration["Authentication:Google:ClientId"];
            var clientSecret = _configuration["Authentication:Google:ClientSecret"];

            return !string.IsNullOrWhiteSpace(clientId) && !string.IsNullOrWhiteSpace(clientSecret);
        }

        private bool IsEmailConfigured()
        {
            var host = _configuration["Email:Host"];
            var senderEmail = _configuration["Email:SenderEmail"];

            return !string.IsNullOrWhiteSpace(host) && !string.IsNullOrWhiteSpace(senderEmail);
        }

        private void ResetUserContext()
        {
            _roleContext.SetAuthenticated(false);
            _roleContext.SetRole(UserRole.Public);
            _roleContext.SetVendorId(null);
            _roleContext.SetConsumerId(null);
        }

        private void SignInConsumer(int consumerId)
        {
            ResetUserContext();
            HttpContext.Session.Remove("ConsumerCart");
            _roleContext.SetRole(UserRole.Consumer);
            _roleContext.SetAuthenticated(true);
            _roleContext.SetConsumerId(consumerId);
        }

        private static string CleanEmail(string? email)
        {
            return (email ?? string.Empty).Trim().ToLowerInvariant();
        }

        private static ConsumerAccessViewModel BuildConsumerAccessModel(string? email)
        {
            var cleanedEmail = CleanEmail(email);

            return new ConsumerAccessViewModel
            {
                SignUpEmail = cleanedEmail,
                VerificationEmail = cleanedEmail,
                LoginEmail = cleanedEmail
            };
        }

        private static string CleanText(string? value)
        {
            return (value ?? string.Empty).Trim();
        }
    }
}
