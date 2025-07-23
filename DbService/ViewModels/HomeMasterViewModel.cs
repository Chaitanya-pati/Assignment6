using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DbService.Models;

namespace DbService.ViewModels
{
    public class HomeMasterViewModel
    {
        public List<Home> Homes { get; set; }
        public List<Room> Rooms { get; set; }
        public List<Booking> Bookings { get; set; }
        public List<BookingRoom> BookingRooms { get; set; }

       // public List<LayoutElement> LyoutElements { get; set; }

    }
}
