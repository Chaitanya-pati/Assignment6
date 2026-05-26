using DbService.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DbService.SaveModels
{
    public class BookingSaveModel
    {
        public Booking BookingData { get; set; }
        public List<Room> Rooms { get; set; }
        public string GuestsJson { get; set; }
    }
}
