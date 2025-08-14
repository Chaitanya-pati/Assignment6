using DbService.Interface;
using Microsoft.AspNetCore.Mvc;
using DbService.Models;
using DbService.ViewModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;

namespace Assignment6.Controllers
{
    public class ConfigurationController : Controller
    {
        private readonly IConfigurationService _configurationService;

        public ConfigurationController(IConfigurationService configurationService)
        {
            _configurationService = configurationService;
        }
        [Authorize(Roles = "Admin")]
        public IActionResult Configuration()
        {
            return View();
        }
        [Authorize(Roles = "Admin")]
        public IActionResult GetMaster()
        {
            List<Home> model = _configurationService.GetHomeMaster();
            return View(model);
        }

        [HttpPost]
        public IActionResult SaveHome(Home homeData)
        {
            try
            {
                if (homeData.Id == 0)
                {
                    homeData.CreatedAt = DateTime.Now;
                }

                Home savedHome = _configurationService.SaveHome(homeData);
                return Json(savedHome.Id);
            }
            catch (Exception ex)
            {
                return Json(new { error = ex.Message });
            }
        }

        [HttpPost]
        public IActionResult UpdateHome(Home homeData)
        {
            try
            {
                Home savedHome = _configurationService.SaveHome(homeData);
                return Json(savedHome.Id);
            }
            catch (Exception ex)
            {
                return Json(new { error = ex.Message });
            }
        }

        [HttpPost]
        public IActionResult SaveRooms(List<Room> roomSaveRequests)
        {
            try
            {
                foreach (var room in roomSaveRequests)
                {
                    room.CreatedAt = DateTime.Now;
                }

                var savedRooms = _configurationService.SaveRooms(roomSaveRequests);
                return Json(savedRooms);
            }
            catch (Exception ex)
            {
                return Json(new { error = ex.Message });
            }
        }

        [HttpPost]
        public IActionResult SaveLayoutElements(List<LayoutElement> layoutElements)
        {
            try
            {
                var isSaved = _configurationService.SaveLayoutElement(layoutElements);
                return Json(isSaved);
            }
            catch (Exception ex)
            {
                return Json(new { error = ex.Message });
            }
        }

        [HttpGet]
        public IActionResult GetLayoutByhomeId(int homeId)
        {
            try
            {
                var layoutElements = _configurationService.GetLayout(homeId);
                return Json(layoutElements);
            }
            catch (Exception ex)
            {
                return Json(new { error = ex.Message });
            }
        }
    }
}