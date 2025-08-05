using DbService.Implementation;
using DbService.Interface;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http;

namespace Assignment6.Controllers
{
    public class WebsiteController : Controller
    {
        private readonly IWebService _webService;

        public WebsiteController(IWebService webService)
        {
            _webService = webService;
        }
        public IActionResult Index()
        {
            return View();
        }
        
        public IActionResult Website()
        {
            var model = _webService.GetWebSiteData();
            return View(model);
        }
    }
}
