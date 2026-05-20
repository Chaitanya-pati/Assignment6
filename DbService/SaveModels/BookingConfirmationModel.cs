using System;

namespace DbService.SaveModels
{
    public class BookingConfirmationModel
    {
        public int BookingId { get; set; }
        public decimal AmountPaid { get; set; }
        public string PaymentMethod { get; set; }
        public decimal? FinalRoomPrice { get; set; }
        public bool IsPartialPayment { get; set; }
        public bool IsAdvancePayment { get; set; }
        public string Notes { get; set; }
    }
}
