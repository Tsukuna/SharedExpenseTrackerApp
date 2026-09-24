using Microsoft.AspNetCore.Mvc;
using SharedExpenseTrackerApp.WebMvcApp.Helpers;
using SharedExpenseTrackerApp.WebMvcApp.Models;
using System.Diagnostics;

namespace SharedExpenseTrackerApp.WebMvcApp.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            // Redirect based on session state
            if (HttpContext.Session.IsAuthenticated())
                return RedirectToAction("Index", "Groups");

            return RedirectToAction("Login", "Auth");
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
