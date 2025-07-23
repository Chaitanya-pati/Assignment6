using DbService.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DbService.ViewModels
{
    public class BookingViewModel
    {
        public string CustomerName { get; set; }
        public string CustomerEmail { get; set; }
        public string CustomerPhone { get; set; }
        public string Message { get; set; }
        public DateTime BookingDateFrom { get; set; }
        public DateTime BookingDateTo { get; set; }
        public int HomeId { get; set; }

        public List<Home> AvailableHomes { get; set; }
    }
}
