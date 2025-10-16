using DbService.Models;

namespace DbService.ViewModels
{
    public class PaymentViewModel
    {
        public int BookingId { get; set; }
        public string CustomerName { get; set; }
        public string CustomerEmail { get; set; }
        public string CustomerPhone { get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; }
        public string RazorpayOrderId { get; set; }
        public string RazorpayKeyId { get; set; }
        public Booking BookingDetails { get; set; }
    }
}