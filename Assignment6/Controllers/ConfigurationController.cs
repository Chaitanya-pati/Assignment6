using DbService.Interface;
using Microsoft.AspNetCore.Mvc;
using DbService.Models;
using DbService.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace Assignment6.Controllers
{
    public class ConfigurationController : Controller
    {
        private readonly IConfigurationService _configurationService;

        public ConfigurationController(IConfigurationService configurationService)
        {
            _configurationService = configurationService;
        }
       
        
        public IActionResult Configuration()
        {
            return View();
        }
        
        public IActionResult GetMaster()
        {
           List<Home> model = _configurationService.GetHomeMaster();
           return View(model);
        }

        [HttpPost]
        public int SaveHome(Home homeData)
        {
            Home savedHome = _configurationService.SaveHome(homeData);
            return savedHome.Id;
        }

    
        public List<Room> SaveRooms(List<Room> roomSaveRequests)
        {
            var savedRooms =  _configurationService.SaveRooms(roomSaveRequests);
            return savedRooms;
        } 
        
        //public List<Room> UpdateHome(List<Room> roomSaveRequests)
        //{
        //    var savedRooms =  _configurationService.UpdateHome(roomSaveRequests);
        //    return savedRooms;
        //}

        
        public bool SaveLayoutElements(List<LayoutElement> layoutElements)
        {
            var isSaved =  _configurationService.SaveLayoutElement(layoutElements);
            return isSaved;
        } 
        
        public List<LayoutElement> GetLayoutByhomeId(int homeId)
        {
            var layoutElements =  _configurationService.GetLayout(homeId);
            return layoutElements;
        }


    }
}
