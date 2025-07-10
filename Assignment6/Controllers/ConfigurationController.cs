using Microsoft.AspNetCore.Mvc;

namespace Assignment6.Controllers
{
    public class ConfigurationController : Controller
    {
        public IActionResult Index()
        {
            return View();
        } 
        
        public IActionResult Configuration()
        {
            return View();
        }


    }
}
