namespace DbService.ViewModels
{
    public class PaymentResponseViewModel
    {
        public string RazorpayPaymentId { get; set; }
        public string RazorpayOrderId { get; set; }
        public string RazorpaySignature { get; set; }
        public int BookingId { get; set; }
    }
}