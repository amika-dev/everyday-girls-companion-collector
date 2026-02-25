using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EverydayGirlsCompanionCollector.Controllers
{
    /// <summary>
    /// Placeholder controller for the friends feature (coming soon).
    /// </summary>
    [Authorize]
    public class FriendsController : Controller
    {
        /// <summary>
        /// Friends list placeholder page.
        /// </summary>
        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        /// <summary>
        /// Add friends placeholder page.
        /// </summary>
        [HttpGet]
        public IActionResult Add()
        {
            return View();
        }
    }
}
