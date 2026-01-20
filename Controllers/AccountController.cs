using EverydayGirlsCompanionCollector.Data;
using EverydayGirlsCompanionCollector.Models.Entities;
using EverydayGirlsCompanionCollector.Models.ViewModels;
using EverydayGirlsCompanionCollector.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace EverydayGirlsCompanionCollector.Controllers
{
    /// <summary>
    /// Handles user authentication: registration, login, and logout.
    /// </summary>
    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ApplicationDbContext _context;
        private readonly IGameplayTipService _tipService;

        public AccountController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ApplicationDbContext context,
            IGameplayTipService tipService)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _context = context;
            _tipService = tipService;
        }

        /// <summary>
        /// Display registration form.
        /// </summary>
        [HttpGet]
        public IActionResult Register()
        {
            return View(new RegisterViewModel());
        }

        /// <summary>
        /// Process user registration, create UserDailyState, and auto-login.
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = new ApplicationUser
            {
                UserName = model.Email,
                Email = model.Email
            };

            var result = await _userManager.CreateAsync(user, model.Password);

            if (result.Succeeded)
            {
                // Initialize UserDailyState for new user
                var dailyState = new UserDailyState
                {
                    UserId = user.Id
                };
                _context.UserDailyStates.Add(dailyState);
                await _context.SaveChangesAsync();

                // Auto-login after registration
                await _signInManager.SignInAsync(user, isPersistent: false);

                return RedirectToAction("Index", "Home");
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return View(model);
        }

        /// <summary>
        /// Display login form.
        /// </summary>
        [HttpGet]
        public IActionResult Login()
        {
            var randomTip = _tipService.GetRandomTip();
            ViewData["RandomTip"] = randomTip;
            return View();
        }

        /// <summary>
        /// Process user login.
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(string email, string password, bool rememberMe = false)
        {
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            {
                ModelState.AddModelError(string.Empty, "Could you fill in both your email and password?");
                return View();
            }

            var result = await _signInManager.PasswordSignInAsync(
                email,
                password,
                rememberMe,
                lockoutOnFailure: false);

            if (result.Succeeded)
            {
                return RedirectToAction("Index", "Home");
            }

            ModelState.AddModelError(string.Empty, "Hmm, that doesn't look quite right. Double-check your email and password?");
            return View();
        }

        /// <summary>
        /// Log out current user.
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Login", "Account");
        }
    }
}
