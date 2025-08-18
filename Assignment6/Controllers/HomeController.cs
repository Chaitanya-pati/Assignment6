using Assignment6.Models;
using Assignment6.Services;
using DbService.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace Assignment6.Controllers
{
    public class HomeController : Controller
    {

        private readonly ILogger<HomeController> _logger;
        private readonly IDashboardService _dashboardService;

        public HomeController(ILogger<HomeController> logger, IDashboardService dashboardService)
        {
            _logger = logger;
            _dashboardService = dashboardService;
        }

            [Authorize(Roles = "Admin")]
            public async Task<IActionResult> Index()
            {
                try
                {
                    var dashboardData = await _dashboardService.GetDashboardDataAsync();
                    return View(dashboardData);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error loading dashboard data");
                    return View(new DashboardViewModel());
                }
            }

            [HttpGet]
            public async Task<IActionResult> GetMonthlyRevenue(int year = 0)
            {
                if (year == 0) year = DateTime.Now.Year;

                var data = await _dashboardService.GetMonthlyRevenueAsync(year);
                return Json(data);
            }

            public IActionResult Privacy()
            {
                return View();
            }

            [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
            public IActionResult Error()
            {
                return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
            }
      }
}
