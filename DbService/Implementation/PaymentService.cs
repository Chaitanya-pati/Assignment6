using DbService.Interface;
using DbService.Models;
using DbService.ViewModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Razorpay.Api;
using System.Security.Cryptography;
using System.Text;
using Payment = DbService.Models.Payment;

namespace DbService.Implementation
{
    public class PaymentService : IPaymentService
    {
        private readonly DbContextOptions<Assignment6Context> _dbconnection;
        private readonly RazorpayClient _razorpayClient;
        private readonly string _razorpayKeyId;
        private readonly string _razorpayKeySecret;

        // This constructor should match what Program.cs is calling
        public PaymentService(string connectionString, IConfiguration configuration)
        {
            _dbconnection = new DbContextOptionsBuilder<Assignment6Context>()
                .UseSqlServer(connectionString)
                .Options;

            _razorpayKeyId = configuration["Razorpay:KeyId"];
            _razorpayKeySecret = configuration["Razorpay:KeySecret"];

            // Create RazorpayClient internally
            _razorpayClient = new RazorpayClient(_razorpayKeyId, _razorpayKeySecret);
        }

        public PaymentViewModel CreatePaymentOrder(int bookingId)
        {
            using (var db = new Assignment6Context(_dbconnection))
            {
                var booking = db.Bookings
                    .Include(b => b.Home)
                    .FirstOrDefault(b => b.Id == bookingId);

                if (booking == null)
                {
                    throw new Exception("Booking not found");
                }

                // Create Razorpay Order
                Dictionary<string, object> options = new Dictionary<string, object>();
                options.Add("amount", booking.Price * 100); // Amount in paise
                options.Add("currency", "INR");
                options.Add("receipt", $"booking_{bookingId}_{DateTime.Now:yyyyMMddHHmmss}");
                options.Add("payment_capture", 1);

                Order order = _razorpayClient.Order.Create(options);
                string orderId = order["id"].ToString();

                // Save payment record
                var payment = new Payment
                {
                    BookingId = bookingId,
                    RazorpayOrderId = orderId,
                    Amount = booking.Price,
                    Currency = "INR",
                    Status = "created",
                    CreatedAt = DateTime.Now
                };

                db.Payments.Add(payment);
                db.SaveChanges();

                return new PaymentViewModel
                {
                    BookingId = bookingId,
                    CustomerName = booking.CustomerName,
                    CustomerEmail = booking.CustomerEmail,
                    CustomerPhone = booking.CustomerPhone,
                    Amount = booking.Price,
                    Currency = "INR",
                    RazorpayOrderId = orderId,
                    RazorpayKeyId = _razorpayKeyId,
                    BookingDetails = booking
                };
            }
        }

        public bool VerifyPayment(PaymentResponseViewModel paymentResponse)
        {
            try
            {
                // Verify signature
                string signature = GenerateSignature(
                    paymentResponse.RazorpayOrderId,
                    paymentResponse.RazorpayPaymentId
                );

                bool isValid = signature == paymentResponse.RazorpaySignature;

                if (isValid)
                {
                    // Update payment and booking status
                    UpdatePaymentStatus(
                        paymentResponse.BookingId,
                        paymentResponse.RazorpayPaymentId,
                        paymentResponse.RazorpaySignature,
                        "success"
                    );
                }

                return isValid;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Payment verification error: {ex.Message}");
                return false;
            }
        }

        public bool UpdatePaymentStatus(int bookingId, string razorpayPaymentId, string razorpaySignature, string status)
        {
            using (var db = new Assignment6Context(_dbconnection))
            {
                var payment = db.Payments.FirstOrDefault(p => p.BookingId == bookingId);

                if (payment != null)
                {
                    payment.RazorpayPaymentId = razorpayPaymentId;
                    payment.RazorpaySignature = razorpaySignature;
                    payment.Status = status;
                    payment.UpdatedAt = DateTime.Now;

                    // Update booking payment status
                    var booking = db.Bookings.FirstOrDefault(b => b.Id == bookingId);
                    if (booking != null)
                    {
                        booking.PaymentStatus = status == "success" ? "paid" : "pending";
                        booking.IsBooked = status == "success";
                    }

                    db.SaveChanges();
                    return true;
                }

                return false;
            }
        }

        public Payment GetPaymentByBookingId(int bookingId)
        {
            using (var db = new Assignment6Context(_dbconnection))
            {
                return db.Payments
                    .Include(p => p.Booking)
                    .FirstOrDefault(p => p.BookingId == bookingId);
            }
        }

        private string GenerateSignature(string orderId, string paymentId)
        {
            string payload = $"{orderId}|{paymentId}";

            using (var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(_razorpayKeySecret)))
            {
                var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(payload));
                return BitConverter.ToString(hash).Replace("-", "").ToLower();
            }
        }
    }
}