using DbService.Implementation;
using DbService.Interface;
using DbService.Models;
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

        public IActionResult GetRoomsByHomeId(int homeId)
        {
            try
            {
                var model = _webService.GetWebSiteData();
                var rooms = model.Rooms?.Where(r => r.HomeId == homeId).ToList() ?? new List<Room>();
                return Json(rooms.Select(r => new {
                    id = r.Id,
                    name = r.Name,
                    price = r.PricePerDay ?? 0,
                    homeId = r.HomeId,
                    // Determine if room is selectable (only bedrooms)
                    isSelectable = IsSelectableRoom(r.Name)
                }));
            }
            catch (Exception ex)
            {
                return Json(new { error = ex.Message });
            }
        }

        private bool IsSelectableRoom(string roomName)
        {
            if (string.IsNullOrEmpty(roomName)) return false;

            var roomType = roomName.ToLower();

            // Only bedrooms are selectable
            bool isBedroom = roomType.Contains("bedroom") || roomType.Contains("bed") ||
                             roomType.Contains("suite") || roomType.Contains("room");

            // Exclude common areas
            bool isCommonArea = roomType.Contains("hall") || roomType.Contains("balcony") ||
                               roomType.Contains("living") || roomType.Contains("kitchen") ||
                               roomType.Contains("dining") || roomType.Contains("bathroom");

            return isBedroom && !isCommonArea;
        }
    }
}
