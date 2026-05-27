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
        private readonly IConfiguration _configuration;
        private readonly RazorpayClient _razorpayClient;
        private readonly RazorpayClient _razorpayClientNew;
        private readonly string _razorpayKeyId;
        private readonly string _razorpayKeySecret;
        private readonly string _razorpayNewKeyId;
        private readonly string _razorpayNewKeySecret;

        // This constructor should match what Program.cs is calling
        public PaymentService(string connectionString, IConfiguration configuration)
        {
            _dbconnection = new DbContextOptionsBuilder<Assignment6Context>()
                .UseSqlServer(connectionString)
                .Options;

            _configuration = configuration;

            // Initialize Razorpay account 1 (1st Floor - Heritage)
            _razorpayKeyId = configuration["Razorpay:KeyId"];
            _razorpayKeySecret = configuration["Razorpay:KeySecret"];
            _razorpayClient = new RazorpayClient(_razorpayKeyId, _razorpayKeySecret);

            // Initialize Razorpay account 2 (Ground Floor - Classic)
            _razorpayNewKeyId = configuration["Razorpaynew:KeyId"];
            _razorpayNewKeySecret = configuration["Razorpaynew:KeySecret"];
            _razorpayClientNew = new RazorpayClient(_razorpayNewKeyId, _razorpayNewKeySecret);
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

                // Get the appropriate Razorpay credentials based on HomeId
                var (razorpayClient, keyId, keySecret) = GetRazorpayCredentials(booking.HomeId);

                // Charge only 50% as advance payment (minimum ₹1 = 100 paise)
                decimal advanceAmount = Math.Round(booking.Price * 0.5m, 2);
                if (advanceAmount < 1) advanceAmount = 1;

                // Amount in paise (integer, no decimals)
                long amountInPaise = (long)(advanceAmount * 100);

                // Create Razorpay Order with 50% advance
                Dictionary<string, object> options = new Dictionary<string, object>();
                options.Add("amount", amountInPaise);
                options.Add("currency", "INR");
                options.Add("receipt", $"bkg_{bookingId}_{DateTime.Now:yyyyMMddHHmmss}");
                options.Add("payment_capture", 1);

                Order order = razorpayClient.Order.Create(options);
                string orderId = order["id"].ToString();

                // Update booking advance price
                booking.AdvancePrice = (long)advanceAmount;
                db.SaveChanges();

                // Save payment record
                var payment = new Payment
                {
                    BookingId = bookingId,
                    RazorpayOrderId = orderId,
                    Amount = advanceAmount,
                    AmountPaid = advanceAmount,
                    Currency = "INR",
                    Status = "created",
                    IsAdvancePayment = true,
                    IsPartialPayment = true,
                    PaymentMethod = "Online",
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
                    Amount = advanceAmount,
                    Currency = "INR",
                    RazorpayOrderId = orderId,
                    RazorpayKeyId = keyId,
                    BookingDetails = booking
                };
            }
        }

        public bool VerifyPayment(PaymentResponseViewModel paymentResponse)
        {
            try
            {
                // Get HomeId for the booking to determine which Razorpay account to use
                int homeId = GetHomeIdByBookingId(paymentResponse.BookingId);
                var (_, _, keySecret) = GetRazorpayCredentials(homeId);

                // Verify signature
                string signature = GenerateSignature(
                    paymentResponse.RazorpayOrderId,
                    paymentResponse.RazorpayPaymentId,
                    keySecret
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

        private string GenerateSignature(string orderId, string paymentId, string keySecret)
        {
            string payload = $"{orderId}|{paymentId}";

            using (var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(keySecret)))
            {
                var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(payload));
                return BitConverter.ToString(hash).Replace("-", "").ToLower();
            }
        }

        private int GetHomeIdByBookingId(int bookingId)
        {
            using (var db = new Assignment6Context(_dbconnection))
            {
                var booking = db.Bookings.FirstOrDefault(b => b.Id == bookingId);
                if (booking == null)
                {
                    throw new Exception($"Booking with ID {bookingId} not found");
                }
                return booking.HomeId;
            }
        }

        private (RazorpayClient client, string keyId, string keySecret) GetRazorpayCredentials(int homeId)
        {
            // HomeId = 1 (Ground Floor - Classic) -> Razorpaynew
            // HomeId = 2 (1st Floor - Heritage) -> Razorpay
            if (homeId == 1)
            {
                return (_razorpayClientNew, _razorpayNewKeyId, _razorpayNewKeySecret);
            }
            else if (homeId == 2)
            {
                return (_razorpayClient, _razorpayKeyId, _razorpayKeySecret);
            }
            else
            {
                // Default to Razorpay for any other HomeId
                return (_razorpayClient, _razorpayKeyId, _razorpayKeySecret);
            }
        }
    }
}