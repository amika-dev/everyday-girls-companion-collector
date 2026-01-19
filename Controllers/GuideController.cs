using EverydayGirlsCompanionCollector.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EverydayGirlsCompanionCollector.Controllers
{
    /// <summary>
    /// Displays gameplay tips and hints to users.
    /// </summary>
    [AllowAnonymous]
    public class GuideController : Controller
    {
        private readonly IGameplayTipService _tipService;

        public GuideController(IGameplayTipService tipService)
        {
            ArgumentNullException.ThrowIfNull(tipService);
            _tipService = tipService;
        }

        /// <summary>
        /// Display all gameplay tips.
        /// </summary>
        [HttpGet]
        public IActionResult Index()
        {
            var tips = _tipService.GetAllTips();
            return View(tips);
        }
    }
}
