using DbService.Models;
using DbService.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DbService.Interface
{
    public interface IConfigurationService
    {
        public List<Home> GetHomeMaster();
        public Home SaveHome(Home home);
        public Home UpdateHome(Home home);
        public List<Room> SaveRooms(List<Room> rooms);
        public bool SaveLayoutElement(List<LayoutElement> layoutElements);
        public List<LayoutElement> GetLayout(int homeId);
        public bool UpdateLayoutRoomPrice(int layoutElementId, long newPricePerDay);
        public bool UpdateLayoutRoomPrices(List<LayoutRoomPriceUpdateModel> updates);
    }
}
