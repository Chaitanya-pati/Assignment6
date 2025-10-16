using DbService.Models;
using DbService.ViewModels;

namespace DbService.Interface
{
    public interface IPaymentService
    {
        PaymentViewModel CreatePaymentOrder(int bookingId);
        bool VerifyPayment(PaymentResponseViewModel paymentResponse);
        Payment GetPaymentByBookingId(int bookingId);
        bool UpdatePaymentStatus(int bookingId, string razorpayPaymentId, string razorpaySignature, string status);
    }
}