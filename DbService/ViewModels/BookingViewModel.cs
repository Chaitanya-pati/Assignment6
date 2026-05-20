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
        public long GuestNumbers { get; set; }
        public string Document { get; set; }
        public string CustomerName { get; set; }
        public string CustomerEmail { get; set; }
        public string CustomerPhone { get; set; }
        public string Message { get; set; }
        public DateTime BookingDateFrom { get; set; }
        public DateTime BookingDateTo { get; set; }
        public int HomeId { get; set; }

        // Guest breakdown
        public int? TotalAdults { get; set; }
        public int? TotalKids { get; set; }
        public int? MaleCount { get; set; }
        public int? FemaleCount { get; set; }

        // Business details
        public string BusinessName { get; set; }
        public string GstNumber { get; set; }

        // Purpose of visit
        public string PurposeOfVisit { get; set; }
        public string PurposeOfVisitOther { get; set; }

        // Payment method
        public string PaymentMethod { get; set; }

        public List<Home> AvailableHomes { get; set; }
    }
}
