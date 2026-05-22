using DbService.Interface;
using DbService.ViewModels;
using Microsoft.AspNetCore.Mvc;
using DbService.Models;

namespace Assignment6.Controllers
{
    public class PaymentController : Controller
    {
        private readonly IPaymentService _paymentService;
        private readonly IBookingService _bookingService;

        public PaymentController(IPaymentService paymentService, IBookingService bookingService)
        {
            _paymentService = paymentService;
            _bookingService = bookingService;
        }

        [HttpGet]
        public IActionResult InitiatePayment(int bookingId)
        {
            try
            {
                var paymentViewModel = _paymentService.CreatePaymentOrder(bookingId);
                return View(paymentViewModel);
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Failed to initiate payment: {ex.Message}";
                return RedirectToAction("Website", "Website");
            }
        }

        [HttpPost]
        public IActionResult VerifyPayment([FromBody] PaymentResponseViewModel paymentResponse)
        {
            try
            {
                bool isValid = _paymentService.VerifyPayment(paymentResponse);

                if (isValid)
                {
                    return Json(new
                    {
                        success = true,
                        message = "Payment successful! Your booking is confirmed.",
                        bookingId = paymentResponse.BookingId
                    });
                }
                else
                {
                    return Json(new
                    {
                        success = false,
                        message = "Payment verification failed. Please contact support."
                    });
                }
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = $"Error verifying payment: {ex.Message}"
                });
            }
        }

        [HttpGet]
        public IActionResult PaymentSuccess(int bookingId)
        {
            _bookingService.ApproveBookingRequest(bookingId);
            var booking = _bookingService.GetBookingById(bookingId);
            var payment = _paymentService.GetPaymentByBookingId(bookingId);

            ViewBag.Booking = booking;
            ViewBag.Payment = payment;

            return View();
        }

        [HttpGet]
        public IActionResult PaymentFailed(int bookingId)
        {
            // Payment verification failed — delete the unpaid pending booking so it
            // does not stay in the DB as an unresolved IsBooked=false record.
            if (bookingId > 0)
            {
                _bookingService.DeleteBooking(bookingId);
            }

            return View();
        }
    }
}