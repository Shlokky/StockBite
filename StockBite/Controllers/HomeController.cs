using System.Diagnostics;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using StockBite.Data;
using StockBite.Models;
using StockBite.Services;

namespace StockBite.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _dbContext;
        private readonly IRoleContext _roleContext;
        private readonly IConfiguration _configuration;
        private readonly string _demoPassword;

        public HomeController(
            ILogger<HomeController> logger,
            ApplicationDbContext dbContext,
            IRoleContext roleContext,
            IConfiguration configuration)
        {
            _logger = logger;
            _dbContext = dbContext;
            _roleContext = roleContext;
            _configuration = configuration;
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

        public IActionResult ConsumerAccess()
        {
            ViewBag.GoogleEnabled = IsGoogleConfigured();
            return View();
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
            if (!User.Identity?.IsAuthenticated ?? true)
            {
                TempData["ErrorMessage"] = "Google sign-in was not completed.";
                return RedirectToAction(nameof(ConsumerAccess));
            }

            var externalResult = await HttpContext.AuthenticateAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = externalResult.Principal ?? User;

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
    }
}
