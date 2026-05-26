using DbService.Implementation;
using DbService.Interface;
using DbService.Models;
using DbService.SaveModels;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http;
using Assignment6.Services;

namespace Assignment6.Controllers
{
    public class WebsiteController : Controller
    {
        private readonly IWebService _webService;
        private readonly IBookingService _bookingService;
        private readonly IEmailService _emailService;

        public WebsiteController(IWebService webService, IBookingService bookingService, IEmailService emailService)
        {
            _webService = webService;
            _bookingService = bookingService;
            _emailService = emailService;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Website()
        {
            var model = _webService.GetWebSiteData();
            return View(model);
        }

        public IActionResult GetRoomsByHomeId(int homeId)
        {
            try
            {
                var model = _webService.GetWebSiteData();
                var rooms = model.Rooms?.Where(r => r.HomeId == homeId).ToList() ?? new List<Room>();
                return Json(rooms.Select(r => new {
                    id = r.Id,
                    label = r.Name,
                    price = r.PricePerDay ?? 0,
                    homeId = r.HomeId,
                    isSelectable = IsSelectableRoom(r.Name)
                }));
            }
            catch (Exception ex)
            {
                return Json(new { error = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> PayLaterBooking(BookingSaveModel bookingSaveModel, IFormFile Document)
        {
            try
            {
                if (Document != null && Document.Length > 0)
                {
                    string uploadsFolder = Path.Combine("wwwroot", "uploads", "bookings");
                    Directory.CreateDirectory(uploadsFolder);
                    string uniqueFileName = $"{DateTime.Now:yyyyMMdd_HHmmss}_{Document.FileName}";
                    string filePath = Path.Combine(uploadsFolder, uniqueFileName);
                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await Document.CopyToAsync(fileStream);
                    }
                    bookingSaveModel.BookingData.Document = $"/uploads/bookings/{uniqueFileName}";
                }

                // Mark as pay later - not booked yet (pending admin approval)
                bookingSaveModel.BookingData.IsBooked = false;
                bookingSaveModel.BookingData.PaymentStatus = "Pay Later";
                bookingSaveModel.BookingData.PaymentMethod = "Pay Later";

                // Calculate pricing same logic as SchedulerController
                var homeData = _webService.GetWebSiteData();
                var home = homeData.Homes?.FirstOrDefault(h => h.Id == bookingSaveModel.BookingData.HomeId);

                if (home != null)
                {
                    int days = (bookingSaveModel.BookingData.BookingDateTo - bookingSaveModel.BookingData.BookingDateFrom).Days;
                    if (days <= 0) days = 1;

                    var freeRoomKeywords = new[] { "hall", "balcony", "living room", "lounge", "kitchen", "dining room", "corridor" };
                    Func<Room, bool> isChargeable = r =>
                    {
                        var label = (r?.Name ?? string.Empty).ToLowerInvariant();
                        return !freeRoomKeywords.Any(k => label.Contains(k));
                    };

                    var homeRooms = homeData.Rooms?.Where(r => r.HomeId == home.Id).ToList() ?? new List<Room>();
                    var chargeableHomeRooms = homeRooms.Where(isChargeable).ToList();

                    if (bookingSaveModel.Rooms != null && bookingSaveModel.Rooms.Any())
                    {
                        var selectedRoomIds = bookingSaveModel.Rooms.Select(r => r.Id).ToHashSet();
                        var selectedChargeableRooms = chargeableHomeRooms
                            .Where(r => selectedRoomIds.Contains(r.Id))
                            .ToList();

                        bool entirePropertySelected =
                            chargeableHomeRooms.Count > 0 &&
                            selectedChargeableRooms.Count >= chargeableHomeRooms.Count;

                        if (entirePropertySelected)
                            bookingSaveModel.BookingData.Price = (long)((home.PricePerDay ?? 0) * days);
                        else
                        {
                            var dailyRoomTotal = selectedChargeableRooms.Sum(r => r.PricePerDay ?? 0);
                            bookingSaveModel.BookingData.Price = (long)(dailyRoomTotal * days);
                        }
                    }
                    else
                    {
                        bookingSaveModel.BookingData.Price = (long)((home.PricePerDay ?? 0) * days);
                    }
                }

                bool resultSaved = _bookingService.SaveBookingAlternative(bookingSaveModel.BookingData, bookingSaveModel.Rooms);

                if (resultSaved && !string.IsNullOrWhiteSpace(bookingSaveModel.GuestsJson))
                {
                    try
                    {
                        var guests = System.Text.Json.JsonSerializer.Deserialize<List<DbService.Models.BookingGuest>>(
                            bookingSaveModel.GuestsJson,
                            new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                        if (guests?.Count > 0)
                            _bookingService.SaveBookingGuests(bookingSaveModel.BookingData.Id, guests);
                    }
                    catch { }
                }

                // Notify admin of new website booking (fire-and-forget)
                if (resultSaved)
                {
                    var savedBooking = _bookingService.GetBookingById(bookingSaveModel.BookingData.Id);
                    if (savedBooking != null)
                        _ = _emailService.SendNewWebsiteBookingToAdminAsync(savedBooking);
                }

                return Json(new
                {
                    success = resultSaved,
                    message = resultSaved
                        ? "Your booking request has been submitted! Our team will contact you shortly to confirm."
                        : "Failed to submit booking request. Please try again.",
                    bookingId = bookingSaveModel.BookingData.Id
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        private bool IsSelectableRoom(string roomName)
        {
            if (string.IsNullOrEmpty(roomName)) return false;
            var roomType = roomName.ToLower();

            bool isBedroom = roomType.Contains("bedroom") || roomType.Contains("bed") ||
                             roomType.Contains("suite") || roomType.Contains("room");

            bool isCommonArea = roomType.Contains("hall") || roomType.Contains("balcony") ||
                               roomType.Contains("living") || roomType.Contains("kitchen") ||
                               roomType.Contains("dining") || roomType.Contains("bathroom");

            return isBedroom && !isCommonArea;
        }
    }
}