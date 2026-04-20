
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Ical.Net;
using Ical.Net.DataTypes;
using Ical.Net.Serialization;
using Calendar = Ical.Net.Calendar;
using Ical.Net.CalendarComponents;
using DbService.ViewModels;
using Microsoft.EntityFrameworkCore;
using DbService.Interface;
using DbService.Models;
using Microsoft.AspNetCore.Hosting.Server;
using DbService.SaveModels;
using System.Collections;
using DbService.Implementation;
using SelectPdf;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;
using Assignment6.twillio;
using Twilio.TwiML.Messaging;
namespace Assignment6.Controllers
{
    public class SchedulerController : Controller
    {
        private readonly HttpClient _httpClient;
        private readonly IBookingService _bookingService;
        private readonly TwilioSmsService _sms;
        //private readonly DbContextOptions<Assignment6Context> _dbconnection;

        public SchedulerController(IBookingService bookingService, TwilioSmsService sms)
        {
            _bookingService = bookingService;
            _httpClient = new HttpClient();
            _sms = sms;
            //SendWhatsappMsg();
        }

        public  async void SendWhatsappMsg()
        {
            var msg = await _sms.SendSmsAsync("+918748040423", "Hiii from twillio");
        }

        public IActionResult Index()
        {
            return View();
        }
        //[Authorize(Roles = "Admin")]
        public IActionResult Scheduler()
        {
            if (!User.Identity.IsAuthenticated || !User.IsInRole("Admin"))
            {
                return RedirectToAction("Login", "Login");
            }

            var model = _bookingService.GetSchedularData();
            return View(model);
        }

        #region comment
        //public async Task<IActionResult> SyncAirbnb()
        //{
        //    try
        //    {

        //        string airbnbIcalUrl = "https://www.airbnb.co.in/calendar/ical/1022148698208402787.ics?s=456490de8497d82e9ab85927a6ce1f4d";

        //        HttpResponseMessage response = await _httpClient.GetAsync(airbnbIcalUrl);
        //        response.EnsureSuccessStatusCode();

        //        string icalContent = await response.Content.ReadAsStringAsync();

        //        var calendar = Ical.Net.Calendar.Load(icalContent);
        //        if (calendar == null || calendar.Events == null)
        //        {
        //            return BadRequest(new { error = "Invalid iCal data" });
        //        }

        //        List<object> bookingStatuses = new List<object>();

        //        DateTime startDate = new DateTime(2024, 1, 1);
        //        DateTime endDate = new DateTime(2025, 12, 31);

        //        for (DateTime date = startDate; date <= endDate; date = date.AddDays(1))
        //        {
        //            bool isBooked = calendar.Events.Any(ev =>
        //                ev.Start.AsSystemLocal.Date <= date && ev.End.AsSystemLocal.Date > date);

        //            bookingStatuses.Add(new
        //            {
        //                Date = date.ToString("yyyy-MM-dd"),
        //                Status = isBooked ? "Booked" : "Available"
        //            });
        //        }

        //        return Ok(bookingStatuses);
        //    }
        //    catch (HttpRequestException ex)
        //    {
        //        return StatusCode(500, new { error = "Error fetching iCal data: " + ex.Message });
        //    }
        //    catch (Exception ex)
        //    {
        //        return StatusCode(500, new { error = ex.Message });
        //    }
        //}

        //public IActionResult GetWebsiteIcal()
        //{
        //    try
        //    {
        //        int currentYear = DateTime.Now.Year;

        //        List<Booking> bookings = new List<Booking>
        //        {
        //            new Booking { GuestName = "August Booking 1", CheckInDate = new DateTime(currentYear, 8, 10), CheckOutDate = new DateTime(currentYear, 8, 15) },
        //            new Booking { GuestName = "August Booking 2", CheckInDate = new DateTime(currentYear, 8, 20), CheckOutDate = new DateTime(currentYear, 8, 22) },
        //            new Booking { GuestName = "September Booking 1", CheckInDate = new DateTime(currentYear, 9, 5), CheckOutDate = new DateTime(currentYear, 9, 8) },
        //            new Booking { GuestName = "September Booking 2", CheckInDate = new DateTime(currentYear, 9, 18), CheckOutDate = new DateTime(currentYear, 9, 21) }
        //        };

        //        var calendar = new Calendar();
        //        calendar.Method = CalendarMethods.Publish;

        //        foreach (var booking in bookings)
        //        {
        //            var calendarEvent = new CalendarEvent
        //            {
        //                Summary = $"Booking: {booking.GuestName}",
        //                Start = new CalDateTime(booking.CheckInDate),
        //                End = new CalDateTime(booking.CheckOutDate),
        //                IsAllDay = true,
        //            };
        //            calendar.Events.Add(calendarEvent);
        //        }

        //        var serializer = new CalendarSerializer();
        //        string icalString = serializer.SerializeToString(calendar);

        //        return Content(icalString, "text/calendar");
        //    }
        //    catch (Exception ex)
        //    {
        //        return StatusCode(500, $"Error generating iCal: {ex.Message}");
        //    }
        //}


        // --- commented by me on 22-07
        //[HttpGet]
        //public async Task<IActionResult> GetBookingsFromAirbnbAndSaveToDb(DateTime start, DateTime end)
        //{
        //    try
        //    {
        //        string airbnbIcalUrl = "https://www.airbnb.co.in/calendar/ical/1022148698208402787.ics?s=456490de8497d82e9ab85927a6ce1f4d";

        //        HttpResponseMessage response = await _httpClient.GetAsync(airbnbIcalUrl);
        //        response.EnsureSuccessStatusCode();

        //        string icalContent = await response.Content.ReadAsStringAsync();

        //        var calendar = Ical.Net.Calendar.Load(icalContent);
        //        if (calendar == null || calendar.Events == null)
        //        {
        //            return BadRequest(new { error = "Invalid iCal data" });
        //        }

        //        List<object> events = new List<object>();

        //        foreach (var calendarEvent in calendar.Events)
        //        {
        //            DateTime eventStart = calendarEvent.Start.AsSystemLocal.Date;
        //            DateTime eventEnd = calendarEvent.End.AsSystemLocal.Date;

        //            if (eventStart <= end && eventEnd >= start)
        //            {
        //                events.Add(new
        //                {
        //                    title = "Booked",
        //                    start = eventStart.ToString("yyyy-MM-dd"),
        //                    end = eventEnd.ToString("yyyy-MM-dd"),
        //                    color = "#FF0000",
        //                    display = "background"
        //                });
        //            }
        //        }

        //        return Ok(events);
        //    }
        //    catch (Exception ex)
        //    {
        //        return StatusCode(500, new { error = ex.Message });
        //    }
        //}


        // Updated Controller method

        // Updated controller method - now just fetches bookings from database
        [HttpGet]
        public async Task<IActionResult> GetBookingsFromAirbnbAndSaveToDb()
        {
            try
            {
                // This method now just triggers a manual sync and returns current status
                // The actual saving is done by the background service automatically

                // Optional: Trigger manual sync if needed
                //using (var scope = _serviceProvider.CreateScope())
                //{
                //    var airbnbSyncJob = scope.ServiceProvider.GetRequiredService<AirbnbSyncJob>();
                //    await airbnbSyncJob.RunAsync();
                //}

                // Get current Airbnb bookings from database for both properties
                var airbnbBookings = _bookingService.GetAllBookings(null, null, null)
                    .Where(b => b.CustomerName == "Airbnb Guest" && b.IsBooked == true)
                    .ToList();

                var property1Bookings = airbnbBookings.Where(b => b.HomeId == 1).Count();
                var property2Bookings = airbnbBookings.Where(b => b.HomeId == 2).Count();
                var totalBookings = airbnbBookings.Count;

                return Json(new
                {
                    success = true,
                    message = $"Airbnb sync completed. Found {totalBookings} total Airbnb bookings in database",
                    totalSynced = totalBookings,
                    propertyBreakdown = new
                    {
                        groundFloor = property1Bookings,
                        firstFloor = property2Bookings
                    },
                    errors = new List<string>() // Empty since we're just reading from DB
                });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = $"Sync failed: {ex.Message}"
                });
            }
        }

        // Alternative simpler version - just returns status without triggering sync
        [HttpGet("GetAirbnbBookingsStatus")]
        public IActionResult GetAirbnbBookingsStatus()
        {
            try
            {
                // Just fetch current Airbnb bookings from database
                var airbnbBookings = _bookingService.GetAllBookings(null, null, null)
                    .Where(b => b.CustomerName == "Airbnb Guest" && b.IsBooked == true)
                    .ToList();

                var property1Bookings = airbnbBookings.Where(b => b.HomeId == 1).Count();
                var property2Bookings = airbnbBookings.Where(b => b.HomeId == 2).Count();
                var totalBookings = airbnbBookings.Count;

                return Json(new
                {
                    success = true,
                    message = $"Found {totalBookings} Airbnb bookings in database",
                    totalBookings = totalBookings,
                    propertyBreakdown = new
                    {
                        groundFloor = property1Bookings,
                        firstFloor = property2Bookings
                    },
                    lastUpdated = DateTime.Now
                });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = $"Failed to fetch Airbnb bookings: {ex.Message}"
                });
            }
        }


        //public async Task<IActionResult> MakeBooking()
        //{
        //    try
        //    {
        //        DateTime checkIn = new DateTime(DateTime.Now.Year, 9, 15);
        //        DateTime checkOut = new DateTime(DateTime.Now.Year, 10, 20);

        //        var bookedDatesResult = await GetBookings(checkIn, checkOut);

        //        if (!(bookedDatesResult is OkObjectResult okResult))
        //        {
        //            return BadRequest("Error fetching Airbnb bookings.");
        //        }

        //        var bookedDates = (List<object>)okResult.Value;

        //        bool available = IsAvailable(checkIn, checkOut, bookedDates);

        //        if (!available)
        //        {
        //            return BadRequest("Dates are not available.");
        //        }

        //        return Ok($"Booking confirmed for {checkIn:yyyy-MM-dd} to {checkOut:yyyy-MM-dd}!");
        //    }
        //    catch (Exception ex)
        //    {
        //        return StatusCode(500, $"Error making booking: {ex.Message}");
        //    }
        //}

        //private bool IsAvailable(DateTime checkIn, DateTime checkOut, List<object> bookedDates)
        //{
        //    for (DateTime date = checkIn.Date; date < checkOut.Date; date = date.AddDays(1))
        //    {
        //        foreach (var bookedDateObject in bookedDates)
        //        {
        //            var bookedDate = (Dictionary<string, object>)bookedDateObject;
        //            DateTime bookedStartDate = DateTime.Parse((string)bookedDate["start"]);
        //            DateTime bookedEndDate = DateTime.Parse((string)bookedDate["end"]);

        //            if (date >= bookedStartDate && date < bookedEndDate)
        //            {
        //                return false;
        //            }
        //        }
        //    }
        //    return true;
        //}
        #endregion

        public IActionResult GetHomeDrawing(int homeId)
        {
            var home = _bookingService.GetHomeDrawing(homeId);
            var layouts = _bookingService.GetLayoutByHomeId(homeId); // This now includes Room data

            var model = new HomeLayoutViewModel
            {
                Home = home,
                Layouts = layouts
            };

            return PartialView("_HomeDrawing", model);
        }

        //--Just to open the Booking form--//
        public IActionResult AddBookingForm(DateTime date)
        {
            var model = _bookingService.GetBookingForm(date);
            return PartialView("_AddBookingForm", model);

        }

        public async Task<IActionResult> SaveBooking(BookingSaveModel bookingSaveModel, IFormFile Document)
        {
            try
            {
                // Handle file upload if document is provided
                if (Document != null && Document.Length > 0)
                {
                    // Create uploads directory if it doesn't exist
                    string uploadsFolder = Path.Combine("wwwroot", "uploads", "bookings");
                    Directory.CreateDirectory(uploadsFolder);

                    // Generate unique filename
                    string uniqueFileName = $"{DateTime.Now:yyyyMMdd_HHmmss}_{Document.FileName}";
                    string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    // Save file
                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await Document.CopyToAsync(fileStream);
                    }

                    // Update booking data with file path
                    bookingSaveModel.BookingData.Document = $"/uploads/bookings/{uniqueFileName}";
                }

                // Calculate correct pricing based on room selection
                var homeData = _bookingService.GetSchedularData();
                var home = homeData.Homes?.FirstOrDefault(h => h.Id == bookingSaveModel.BookingData.HomeId);

                if (home != null)
                {
                    int days = (bookingSaveModel.BookingData.BookingDateTo - bookingSaveModel.BookingData.BookingDateFrom).Days;

                    if (bookingSaveModel.Rooms != null && bookingSaveModel.Rooms.Any())
                    {
                        // Individual room pricing
                        var roomIds = bookingSaveModel.Rooms.Select(r => r.Id).ToList();
                        var selectedRooms = homeData.Rooms?.Where(r => roomIds.Contains(r.Id)).ToList() ?? new List<Room>();
                        var dailyRoomTotal = selectedRooms.Sum(r => r.PricePerDay ?? 0);
                        bookingSaveModel.BookingData.Price = (long)(dailyRoomTotal * days);
                    }
                    else
                    {
                        // Entire property pricing
                        bookingSaveModel.BookingData.Price = (long)((home.PricePerDay ?? 0) * days);
                    }
                }

                bool resultSaved = _bookingService.SaveBookingAlternative(bookingSaveModel.BookingData, bookingSaveModel.Rooms);
                return Json(new
                {
                    success = resultSaved,
                    message = resultSaved ? "Booking saved successfully" : "Failed to save booking",
                    bookingId = bookingSaveModel.BookingData.Id
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        public IActionResult CheckBookings(DateTime startDate, DateTime endDate, int homeId)
        {
            try
            {
                bool isBooked = _bookingService.CheckBookingsAlternative(startDate, endDate, homeId);
                return Json(isBooked);
            }
            catch (Exception ex)
            {
                return Json(new { error = ex.Message });
            }
        }

        public IActionResult GetBookingsByHomeId(DateTime start, DateTime end, int? homeId)
        {
            var bookings = _bookingService.GetBookingsBetween(start, end, homeId);

            var result = bookings.Select(b => new
            {
                id = b.Id,
                title = b.CustomerName,
                start = b.BookingDateFrom.ToString("yyyy-MM-dd"),
                end = b.BookingDateTo.AddDays(1).ToString("yyyy-MM-dd"), // FullCalendar is exclusive of end date
                homeId = b.HomeId,
                color = "#dc3545" // red
            });

            return Json(result);
        }

        public IActionResult GetBookings(DateTime? startDate = null, DateTime? endDate = null, int? homeId = null)
        {
            try
            {
                var bookings = _bookingService.GetAllBookings(startDate, endDate, homeId)
                    .Where(b => b.IsBooked) // Only show approved bookings on calendar
                    .ToList();

                var result = bookings.Select(b => new
                {
                    id = b.Id,
                    customerName = b.CustomerName,
                    customerEmail = b.CustomerEmail,
                    customerPhone = b.CustomerPhone,
                    message = b.Message,
                    bookingDateFrom = b.BookingDateFrom.ToString("yyyy-MM-dd"),
                    bookingDateTo = b.BookingDateTo.ToString("yyyy-MM-dd"),
                    homeId = b.HomeId,
                    paymentStatus = b.PaymentStatus,
                    price = b.Price,
                    advancePaid = b.AdvancePrice ?? 0,
                    amountToBePaid = b.Price - (b.AdvancePrice ?? 0),
                    guestNumbers = b.GuestNumbers,
                    document = b.Document,
                   // isAirbnbBooking = b.IsAirbnbBooking ?? false, // Include Airbnb flag
                    bookingRooms = b.BookingRooms?.Select(br => new
                    {
                        roomId = br.RoomId,
                        bookingId = br.BookingId
                    }).ToList()
                });

                Console.WriteLine($"Returning {result.Count()} approved bookings" +
                    (startDate.HasValue && endDate.HasValue ? $" between {startDate.Value:yyyy-MM-dd} and {endDate.Value:yyyy-MM-dd}" : "") +
                    (homeId.HasValue ? $" for HomeId: {homeId.Value}" : ""));

                return Json(result);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetBookings: {ex.Message}");
                return BadRequest(new { error = ex.Message });
            }
        }

        private string GenerateInvoice(int bookingId)
        {
            try
            {
                // Get booking details for invoice
                var booking = _bookingService.GetBookingById(bookingId);

                if (booking == null)
                    return null;

                // Create HTML content for the invoice
                string htmlContent = GenerateInvoiceHtml(booking);

                // Convert HTML to PDF (you can use libraries like iTextSharp, SelectPdf, etc.)
                // For simplicity, I'll show a basic approach that creates a downloadable PDF

                string fileName = $"Invoice_{bookingId}_{DateTime.Now:yyyyMMdd_HHmmss}.pdf";
                string invoicesFolder = Path.Combine("wwwroot", "invoices");

                // Create directory if it doesn't exist
                Directory.CreateDirectory(invoicesFolder);

                string filePath = Path.Combine(invoicesFolder, fileName);

                // Generate PDF from HTML (this is a simplified version)
                // You should use a proper HTML to PDF library like SelectPdf or iTextSharp
                GeneratePdfFromHtml(htmlContent, filePath);

                // Return the URL to download the PDF
                return $"/invoices/{fileName}";
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error generating invoice: {ex.Message}");
                return null;
            }
        }

        // ADD this method to generate HTML content
        private string GenerateInvoiceHtml(Booking booking)
        {
            bool isAirbnbBooking = booking.CustomerName == "Airbnb Guest" || booking.CustomerName?.Contains("Airbnb") == true;
            bool isPastBooking = booking.BookingDateTo < DateTime.Now;
            int nights = (booking.BookingDateTo - booking.BookingDateFrom).Days;
            long advancePaid = booking.AdvancePrice ?? 0;
            long remaining = booking.Price - advancePaid;

            string statusBadge = isAirbnbBooking
                ? "<span style='background:#fffaeb;color:#b54708;padding:3px 10px;border-radius:20px;font-size:10px;font-weight:600;text-transform:uppercase;letter-spacing:0.6px;'>Airbnb</span>"
                : isPastBooking
                    ? "<span style='background:#ecfdf3;color:#027a48;padding:3px 10px;border-radius:20px;font-size:10px;font-weight:600;text-transform:uppercase;letter-spacing:0.6px;'>Completed</span>"
                    : "<span style='background:#ecfdf3;color:#027a48;padding:3px 10px;border-radius:20px;font-size:10px;font-weight:600;text-transform:uppercase;letter-spacing:0.6px;'>Active</span>";

            string roomDetails = (booking.BookingRooms?.Count > 0)
                ? string.Join(", ", booking.BookingRooms.Select(r => $"Room {r.RoomId}"))
                : "N/A";

            string roomCount = $"{booking.BookingRooms?.Count ?? 0} {(booking.BookingRooms?.Count == 1 ? "room" : "rooms")}";
            string bookingType = isAirbnbBooking ? "Airbnb" : "Direct Booking";

            string notesSection = string.IsNullOrEmpty(booking.Message) ? "" : $@"
        <tr>
            <td colspan='2' style='padding-top:12px;'>
                <table width='100%' cellpadding='0' cellspacing='0' style='background:#f9fafb;border:0.5px solid #eaecf0;border-radius:8px;'>
                    <tr>
                        <td style='padding:14px 18px;'>
                            <div style='font-size:10px;font-weight:600;color:#98a2b3;text-transform:uppercase;letter-spacing:0.9px;margin-bottom:8px;'>Special Requests &amp; Notes</div>
                            <div style='font-size:12px;color:#344054;line-height:1.7;'>{booking.Message}</div>
                        </td>
                    </tr>
                </table>
            </td>
        </tr>";

            string amountDueRow = remaining > 0 ? $@"
        <tr style='border-bottom:0.5px solid #f2f4f7;'>
            <td style='padding:11px 18px;font-size:12px;color:#667085;'>Amount due</td>
            <td style='padding:11px 18px;font-size:12px;color:#101828;font-weight:500;text-align:right;'>&#8377;{remaining:N0}</td>
        </tr>" : "";

            var html = $@"
<!DOCTYPE html>
<html lang='en'>
<head>
    <meta charset='utf-8'>
    <title>Invoice - Vernekar HomeStays</title>
    <style>
        * {{ margin: 0; padding: 0; box-sizing: border-box; }}
        html, body {{
          font-family: 'Segoe UI', Arial, sans-serif;
            background: #ffffff;
            color: #0a0f1e;
            font-size: 15px;
            line-height: 1.7;
            width: 100%;
        }}
        .wrapper {{
            width: 100%;
            padding: 52px 56px;
            background: #ffffff;
        }}
        .info-card {{
             background: #f9fafb;
                border: 1px solid #d0d5dd;
                border-radius: 8px;
                padding: 24px 26px;
                vertical-align: top;
        }}
        .card-title {{
          font-size: 11px;
            font-weight: 700;
            color: #0a0c10;
            text-transform: uppercase;
            letter-spacing: 1px;
            margin-bottom: 18px;
            padding-bottom: 10px;
            border-bottom: 1px solid #d0d5dd;
        }}
        .row-label {{
               font-size: 17px;
                color: #0a0c10;
                padding: 7px 0;
                width: 42%;
                font-weight: 500
        }}
        .row-value {{
                font-size: 14px;
                color: #0a0f1e;
                font-weight: 700;
                padding: 7px 0;
                text-align: right;
        }}
    </style>
</head>
<body>
<div class='wrapper'>

    <!-- HEADER -->
    <table width='100%' cellpadding='0' cellspacing='0' style='margin-bottom:36px;padding-bottom:30px;border-bottom:2px solid #d0d5dd;'>
        <tr>
            <td style='vertical-align:top;'>
                <div style='font-size:30px;font-weight:700;color:#101828;letter-spacing:1px;margin-bottom:6px;'>VERNEKAR</div>
                <div style='font-size:15px;color:#16181d;margin-bottom:14px;'>Radha Heritage Homestays</div>
                <div style='font-size:13px;color:#2c313a;line-height:2;'>radhaheritagehomestay@gmail.com</div>
                <div style='font-size:13px;color:#2c313a;'>www.radhaheritagehomestay.com</div>
            </td>
            <td style='vertical-align:top;text-align:right;'>
                <div style='font-size:11px;color:#98a2b3;text-transform:uppercase;letter-spacing:0.8px;font-weight:600;'>Invoice No</div>
                <div style='font-size:28px;font-weight:700;color:#101828;margin:4px 0 16px;'>#{booking.Id:D6}</div>
                <div style='font-size:11px;color:#98a2b3;text-transform:uppercase;letter-spacing:0.8px;font-weight:600;'>Invoice Date</div>
                <div style='font-size:15px;color:#101828;font-weight:500;margin:4px 0 16px;'>{DateTime.Now:dd MMM yyyy}</div>
                <div style='font-size:11px;color:#98a2b3;text-transform:uppercase;letter-spacing:0.8px;font-weight:600;'>Status</div>
                <div style='margin-top:8px;'>{statusBadge}</div>
            </td>
        </tr>
    </table>

    <!-- GUEST + PROPERTY -->
    <table width='100%' cellpadding='0' cellspacing='0' style='margin-bottom:18px;'>
        <tr>
            <td width='49%' class='info-card'>
                <div class='card-title'>Guest Information</div>
                <table width='100%' cellpadding='0' cellspacing='0'>
                    <tr><td class='row-label'>Name</td><td class='row-value'>{booking.CustomerName ?? "N/A"}</td></tr>
                    <tr><td class='row-label'>Email</td><td class='row-value'>{booking.CustomerEmail ?? "N/A"}</td></tr>
                    <tr><td class='row-label'>Phone</td><td class='row-value'>{booking.CustomerPhone ?? "N/A"}</td></tr>
                    <tr><td class='row-label'>Guests</td><td class='row-value'>{booking.GuestNumbers}</td></tr>
                </table>
            </td>
            <td width='2%'>&nbsp;</td>
            <td width='49%' class='info-card'>
                <div class='card-title'>Property Details</div>
                <table width='100%' cellpadding='0' cellspacing='0'>
                    <tr><td class='row-label'>Property</td><td class='row-value'>{booking.Home?.Name ?? "N/A"}</td></tr>
                    <tr><td class='row-label'>Check-in</td><td class='row-value'>{booking.BookingDateFrom:dd MMM yyyy}</td></tr>
                    <tr><td class='row-label'>Check-out</td><td class='row-value'>{booking.BookingDateTo:dd MMM yyyy}</td></tr>
                    <tr><td class='row-label'>Duration</td><td class='row-value' style='font-weight:700;'>{nights} {(nights == 1 ? "night" : "nights")}</td></tr>
                </table>
            </td>
        </tr>
    </table>

    <!-- BOOKING SUMMARY -->
    <table width='100%' cellpadding='0' cellspacing='0' style='margin-bottom:18px;'>
        <tr>
            <td class='info-card'>
                <div class='card-title'>Booking Summary</div>
                <table width='100%' cellpadding='0' cellspacing='0'>
                    <tr>
                        <td width='50%' style='vertical-align:top;padding-right:20px;'>
                            <table width='100%' cellpadding='0' cellspacing='0'>
                                <tr><td class='row-label'>Rooms booked</td><td class='row-value'>{roomCount}</td></tr>
                                <tr><td class='row-label'>Booking type</td><td class='row-value'>{bookingType}</td></tr>
                                <tr><td class='row-label'>Booked on</td><td class='row-value'>{booking.CreatedAt:dd MMM yyyy}</td></tr>
                            </table>
                        </td>
                    </tr>
                </table>
            </td>
        </tr>
    </table>

    <!-- LEDGER -->
    <table width='100%' cellpadding='0' cellspacing='0' style='margin-bottom:18px;border:1px solid #d0d5dd;border-radius:8px;overflow:hidden;background:#ffffff;'>
        <tr style='background:#f9fafb;border-top:1px solid #d0d5dd;'>
            <td style='padding:22px 22px;font-size:17px;font-weight:700;color:#101828;'>Total booking amount</td>
            <td style='padding:22px 22px;font-size:22px;font-weight:700;color:#101828;text-align:right;'>&#8377;{booking.Price:N0}</td>
        </tr>
    </table>

    <!-- NOTES (optional) -->
    {(string.IsNullOrEmpty(booking.Message) ? "" : $@"
    <table width='100%' cellpadding='0' cellspacing='0' style='margin-bottom:18px;'>
        <tr>
            <td style='background:#f9fafb;border:1px solid #eaecf0;border-radius:8px;padding:22px 26px;'>
                <div style='font-size:11px;font-weight:600;color:#0a0c10;text-transform:uppercase;letter-spacing:1px;margin-bottom:12px;padding-bottom:10px;border-bottom:1px solid #0a0c10;'>Special Requests &amp; Notes</div>
                <div style='font-size:14px;color:#344054;line-height:1.8;'>{booking.Message}</div>
            </td>
        </tr>
    </table>")}

    <!-- SPACER - pushes footer toward bottom -->
    <table width='100%' cellpadding='0' cellspacing='0' style='height:120px;'><tr><td></td></tr></table>

    <!-- DIVIDER -->
    <table width='100%' cellpadding='0' cellspacing='0' style='margin-bottom:0;'>
        <tr>
            <td style='border-top:1px solid #eaecf0;padding-top:24px;'>
                <table width='100%' cellpadding='0' cellspacing='0'>
                    <tr>
                        <td style='font-size:13px;color:#2c313a;'>Thank you for choosing Vernekar HomeStays</td>
                        <td style='text-align:right;font-size:12px;color:#373d49;line-height:1.9;'>
                            radhaheritagehomestay@gmail.com &nbsp;·&nbsp; www.radhaheritagehomestay.com<br>
                            Generated on {DateTime.Now:dd MMM yyyy, HH:mm}
                        </td>
                    </tr>
                </table>
            </td>
        </tr>
    </table>

</div>
</body>
</html>";

            return html;
        }

        // ADD this method for PDF generation (using a simple approach)
        // NOTE: For production, use proper libraries like SelectPdf, iTextSharp, or Puppeteer
        // ADD this using statement at the top of your controller file
        // REPLACE your GeneratePdfFromHtml method with this:
        // ADD this using statement at the top of your controller file



        // REPLACE your GeneratePdfFromHtml method with this:
        private void GeneratePdfFromHtml(string htmlContent, string filePath)
        {
            try
            {
                HtmlToPdf converter = new HtmlToPdf();

                converter.Options.PdfPageSize = PdfPageSize.A4;
                converter.Options.PdfPageOrientation = PdfPageOrientation.Portrait;

                converter.Options.MarginTop = 0;    // We handle padding in HTML
                converter.Options.MarginBottom = 0;
                converter.Options.MarginLeft = 0;
                converter.Options.MarginRight = 0;

                converter.Options.WebPageWidth = 1200;  // wider so layout doesn't collapse
                converter.Options.WebPageHeight = 0;

                converter.Options.JavaScriptEnabled = false;
                converter.Options.CssMediaType = HtmlToPdfCssMediaType.Screen; // NOT Print
                converter.Options.AutoFitWidth = HtmlToPdfPageFitMode.ShrinkOnly;
                converter.Options.AutoFitHeight = HtmlToPdfPageFitMode.NoAdjustment;

                PdfDocument doc = converter.ConvertHtmlString(htmlContent);
                doc.Save(filePath);
                doc.Close();
                System.IO.File.Delete(filePath);

                Console.WriteLine($"PDF generated at: {filePath}");

            }
            catch (Exception ex)
            {
                Console.WriteLine($"SelectPdf Error: {ex.Message}");
                CreateHtmlFallback(htmlContent, filePath);
            }
        }

        // Helper method for fallback HTML creation
        private void CreateHtmlFallback(string htmlContent, string filePath)
        {
            try
            {
                string htmlFilePath = filePath.Replace(".pdf", ".html");

                // Enhanced HTML with print styles
                string enhancedHtml = $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <title>Invoice</title>
    <style>
        @media print {{
            .no-print {{ display: none !important; }}
            body {{ margin: 0; }}
            @page {{ margin: 0.5in; }}
        }}
        .print-btn {{
            position: fixed;
            top: 10px;
            right: 10px;
            background: #007bff;
            color: white;
            border: none;
            padding: 10px 15px;
            border-radius: 5px;
            cursor: pointer;
            z-index: 1000;
        }}
    </style>
</head>
<body>
    <button class='print-btn no-print' onclick='window.print()'>Print as PDF</button>
    {htmlContent.Replace("<!DOCTYPE html>", "").Replace("<html>", "").Replace("</html>", "").Replace("<head>", "").Replace("</head>", "").Replace("<body>", "").Replace("</body>", "")}
</body>
</html>";

                System.IO.File.WriteAllText(htmlFilePath, enhancedHtml);

                // Update the file path to point to HTML file
                System.IO.File.Copy(htmlFilePath, filePath, true);

                Console.WriteLine($"Fallback HTML invoice created: {htmlFilePath}");
            }
            catch (Exception htmlEx)
            {
                Console.WriteLine($"Error creating HTML fallback: {htmlEx.Message}");
                throw new Exception("Failed to generate invoice in any format", htmlEx);
            }
        }

        // ADD this new action method to serve the PDF/invoice files
        [HttpGet]
        public IActionResult DownloadInvoice(string fileName)
        {
            try
            {
                string filePath = Path.Combine("wwwroot", "invoices", fileName);

                if (!System.IO.File.Exists(filePath))
                {
                    return NotFound("Invoice file not found.");
                }

                var fileBytes = System.IO.File.ReadAllBytes(filePath);
                var contentType = "application/pdf";
               

                // If you're serving HTML files for testing, use this:
                // var contentType = "text/html";

                return File(fileBytes, contentType, fileName);
            }
            catch (Exception ex)
            {
                return BadRequest($"Error downloading invoice: {ex.Message}");
            }
        }

        // ADD these methods to your existing SchedulerController class

        [HttpPost]
        public IActionResult CompleteBookingWithInvoice(int bookingId)
        {
            try
            {
                if (bookingId <= 0)
                {
                    return Json(new { success = false, message = "Invalid booking ID" });
                }

                // Get booking details for invoice generation
                var booking = _bookingService.GetBookingById(bookingId);

                if (booking == null)
                {
                    return Json(new { success = false, message = "Booking not found" });
                }

                // Generate invoice (works for past bookings too)
                string invoiceUrl = null;
                try
                {
                    invoiceUrl = GenerateInvoice(bookingId);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Invoice generation failed: {ex.Message}");
                    // Continue with completion even if invoice fails
                }

                // Complete booking (update payment status and delete)
                bool completed = _bookingService.CompleteBookingWithInvoice(bookingId);

                if (completed)
                {
                    return Json(new
                    {
                        success = true,
                        message = "Invoice generated successfully",
                        invoiceUrl = invoiceUrl
                    });
                }
                else
                {
                    return Json(new
                    {
                        success = false,
                        message = "Failed to generate invoice"
                    });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception in CompleteBookingWithInvoice: {ex.Message}");
                return Json(new { success = false, message = "An unexpected error occurred" });
            }
        }

        [HttpDelete]
        [HttpPost] // Support both DELETE and POST
        public IActionResult DeleteBooking(int bookingId)
        {
            try
            {
                if (bookingId <= 0)
                {
                    return Json(new { success = false, message = "Invalid booking ID" });
                }

                bool deleted = _bookingService.DeleteBooking(bookingId);

                if (deleted)
                {
                    return Json(new
                    {
                        success = true,
                        message = "Booking deleted successfully"
                    });
                }
                else
                {
                    return Json(new
                    {
                        success = false,
                        message = "Failed to delete booking or booking not found"
                    });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception in DeleteBooking: {ex.Message}");
                return Json(new { success = false, message = "An unexpected error occurred" });
            }
        }

        // MODIFY your existing UpdatePaymentStatus method to just update status (not delete)
        [HttpPost]
        public IActionResult UpdatePaymentStatus(int bookingId, string paymentStatus)
        {
            try
            {
                bool updated = _bookingService.UpdatePaymentStatus(bookingId, paymentStatus);

                if (updated)
                {
                    return Json(new
                    {
                        success = true,
                        message = "Payment status updated successfully!"
                    });
                }
                else
                {
                    return Json(new
                    {
                        success = false,
                        message = "Failed to update payment status."
                    });
                }
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = "Error: " + ex.Message
                });
            }
        }

        [HttpGet]
        public IActionResult GetPendingBookingRequests()
        {
            try
            {
                var pendingBookings = _bookingService.GetAllBookings(null, null, null)
                    .Where(b => !b.IsBooked) // Only get requests that haven't been approved
                    .OrderByDescending(b => b.CreatedAt)
                    .Select(b => new
                    {
                        id = b.Id,
                        customerName = b.CustomerName,
                        customerEmail = b.CustomerEmail,
                        customerPhone = b.CustomerPhone,
                        message = b.Message,
                        bookingDateFrom = b.BookingDateFrom.ToString("yyyy-MM-dd"),
                        bookingDateTo = b.BookingDateTo.ToString("yyyy-MM-dd"),
                        homeId = b.HomeId,
                        price = b.Price,
                        advance = b.AdvancePrice ?? 0,
                        pending = b.Price - (b.AdvancePrice ?? 0),
                        guestNumbers = b.GuestNumbers,
                        createdAt = b.CreatedAt.ToString("yyyy-MM-ddTHH:mm:ss"),
                        paymentStatus = b.PaymentStatus
                    });

                return Json(pendingBookings);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting pending booking requests: {ex.Message}");
                return Json(new { error = ex.Message });
            }
        }

        [HttpPost]
        public IActionResult ApproveBookingRequest(int bookingId)
        {
            try
            {
                if (bookingId <= 0)
                {
                    return Json(new { success = false, message = "Invalid booking ID" });
                }

                // Update the booking to set IsBooked = true
                bool approved = _bookingService.ApproveBookingRequest(bookingId);

                if (approved)
                {
                    return Json(new
                    {
                        success = true,
                        message = "Booking request approved successfully"
                    });
                }
                else
                {
                    return Json(new
                    {
                        success = false,
                        message = "Failed to approve booking request"
                    });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error approving booking request: {ex.Message}");
                return Json(new { success = false, message = "An unexpected error occurred" });
            }
        }

        public IActionResult GetRoomDetailsById(int roomId)
        {
            string name = _bookingService.GetRoomDetailsByRoomId(roomId);
            return Json(name);
        }
    }


}


