using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using OtherDetails.UI.Models;

namespace OtherDetails.UI.Controllers
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
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        public IActionResult TableOthers()
        {
            return View();
        }

        // GET
        public IActionResult EditOthers(string id, int pax, string service)
        {
            ViewData["CustomerId"] = id;
            ViewData["Pax"] = pax;
            ViewData["Service"] = service;
            return View();
        }

        // POST
        [HttpPost]
        public IActionResult EditOthers(IFormCollection form)
        {

            return RedirectToAction("TableOthers");
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
