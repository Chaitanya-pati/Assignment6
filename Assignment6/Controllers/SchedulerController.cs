
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
namespace Assignment6.Controllers
{
    public class SchedulerController : Controller
    {
        private readonly HttpClient _httpClient;
        private readonly IBookingService _bookingService;
        //private readonly DbContextOptions<Assignment6Context> _dbconnection;

        public SchedulerController(IBookingService bookingService)//,string conn)
        {
            //_dbconnection = new DbContextOptionsBuilder<Assignment6Context>().UseSqlServer(conn).Options;
            _bookingService = bookingService;
            _httpClient = new HttpClient();
            //GetWebsiteIcal();
        }
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Scheduler()
        {
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
        // [HttpGet]
        //public async Task<IActionResult> GetBookings(DateTime start, DateTime end)
        //{
        //    try
        //    {
        //    https://www.airbnb.co.in/calendar/ical/1022148698208402787.ics?s=456490de8497d82e9ab85927a6ce1f4d
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
            string conn = "server = localhost; database = Assignment6; trusted_connection = true; multipleactiveresultsets = true; TrustServerCertificate = True;";
            using (var db = new Assignment6Context(new DbContextOptionsBuilder<Assignment6Context>().UseSqlServer(conn).Options))
            {
                var model = new BookingViewModel
                {
                    BookingDateFrom = date,
                    BookingDateTo = date.AddDays(1),
                    AvailableHomes = db.Homes.ToList()
                };

                return PartialView("_AddBookingForm", model);
            }
        }

        //[HttpPost]
        //public IActionResult SaveBooking(BookingSaveModel bookingSaveModel)
        //{
        //    bool resultSaved = _bookingService.SaveBooking(bookingSaveModel.BookingData, bookingSaveModel.Rooms);
        //    return Json(resultSaved);
        //}

        [HttpPost]
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

                bool resultSaved = _bookingService.SaveBookingAlternative(bookingSaveModel.BookingData, bookingSaveModel.Rooms);
                return Json(resultSaved);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // In your SchedulerController class
[HttpGet]
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

        [HttpGet]
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
                    // FIXED: Don't add extra day to checkout date
                    bookingDateFrom = b.BookingDateFrom.ToString("yyyy-MM-dd"),
                    bookingDateTo = b.BookingDateTo.ToString("yyyy-MM-dd"), // Use actual checkout date
                    homeId = b.HomeId,
                    paymentStatus = b.PaymentStatus,
                    price = b.Price,
                    guestNumbers = b.GuestNumbers,
                    document = b.Document,
                    bookingRooms = b.BookingRooms?.Select(br => new
                    {
                        roomId = br.RoomId,
                        bookingId = br.BookingId
                    }).ToList()
                });

                Console.WriteLine($"Returning {result.Count()} approved bookings");
                return Json(result);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetBookings: {ex.Message}");
                return BadRequest(new { error = ex.Message });
            }
        }
        // REPLACE the existing GenerateInvoice method with this updated version


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
            var html = $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <title>Invoice - {booking.Id:D6}</title>
    <style>
        * {{
            margin: 0;
            padding: 0;
            box-sizing: border-box;
        }}

        body {{ 
            font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif; 
            background-color: white;
            color: #333;
            line-height: 1.3;
            font-size: 12px;
            height: 100vh;
            overflow: hidden;
        }}
        
        .invoice-container {{ 
            max-width: 800px; 
            margin: 0 auto; 
            background: white; 
            height: 100vh;
            display: flex;
            flex-direction: column;
        }}
        
        .header {{ 
            background: linear-gradient(135deg, #4a5568 0%, #2d3748 100%);
            color: white;
            padding: 20px;
            text-align: center;
        }}
        
        .company-name {{ 
            font-size: 24px; 
            font-weight: bold; 
            margin-bottom: 4px;
            letter-spacing: 1px;
        }}
        
        .company-tagline {{
            font-size: 12px;
            opacity: 0.9;
        }}

        .invoice-header {{
            background: white;
            padding: 15px 20px;
            border-bottom: 2px solid #e2e8f0;
            display: flex;
            justify-content: space-between;
            align-items: center;
        }}

        .invoice-title {{ 
            font-size: 24px; 
            font-weight: bold;
            color: #2d3748;
        }}

        .invoice-number {{ 
            color: #e53e3e;
            font-size: 16px;
            font-weight: bold;
        }}

        .invoice-date {{
            color: #718096;
            font-size: 12px;
            margin-top: 3px;
        }}

        .content-section {{
            padding: 20px;
            flex: 1;
            display: grid;
            grid-template-columns: 1fr 1fr;
            gap: 20px;
        }}

        .left-column {{
            display: flex;
            flex-direction: column;
            gap: 15px;
        }}

        .right-column {{
            display: flex;
            flex-direction: column;
            gap: 15px;
        }}

        .info-section {{
            background: #4a5568;
            color: white;
            padding: 8px 15px;
            font-weight: 600;
            font-size: 14px;
            margin-bottom: 8px;
        }}

        .info-card {{
            background: #f7fafc;
            padding: 15px;
            border-radius: 6px;
            border-left: 4px solid #4a5568;
            flex: 1;
        }}

        .info-row {{
            display: flex;
            justify-content: space-between;
            margin-bottom: 6px;
        }}

        .info-row:last-child {{
            margin-bottom: 0;
        }}

        .info-label {{
            font-weight: 600;
            color: #4a5568;
            font-size: 11px;
        }}

        .info-value {{
            color: #2d3748;
            font-weight: 500;
        }}

        .payment-status {{
            display: inline-block;
            background: #48bb78;
            color: white;
            padding: 4px 8px;
            border-radius: 12px;
            font-size: 10px;
            font-weight: bold;
            text-transform: uppercase;
        }}

        .total-section {{
            background: linear-gradient(135deg, #4a5568 0%, #2d3748 100%);
            color: white;
            padding: 20px;
            border-radius: 8px;
            text-align: center;
            grid-column: span 2;
            margin-top: 10px;
        }}

        .total-label {{
            font-size: 16px;
            margin-bottom: 8px;
            opacity: 0.9;
        }}

        .total-amount {{
            font-size: 32px;
            font-weight: bold;
        }}

        .footer {{
            background: #f7fafc;
            padding: 12px 20px;
            text-align: center;
            color: #718096;
            border-top: 1px solid #e2e8f0;
        }}

        .footer-title {{
            font-size: 14px;
            color: #2d3748;
            margin-bottom: 4px;
            font-weight: 600;
        }}

        .footer-details {{
            font-size: 10px;
            line-height: 1.2;
        }}

        @media print {{
            body {{ 
                background-color: white;
                margin: 0;
                padding: 0;
                height: auto;
                overflow: visible;
            }}
            .invoice-container {{ 
                height: auto;
                page-break-inside: avoid;
            }}
        }}
    </style>
</head>
<body>
    <div class='invoice-container'>
        <!-- Header -->
        <div class='header'>
            <div class='company-name'>VERNEKAR HOMESTAYS</div>
            <div class='company-tagline'>Premium Hospitality & Accommodation Services</div>
        </div>

        <!-- Invoice Title Section -->
        <div class='invoice-header'>
            <div>
                <div class='invoice-title'>INVOICE</div>
                <div class='invoice-date'>Date: {DateTime.Now:MMMM dd, yyyy}</div>
            </div>
            <div>
                <div class='invoice-number'>#{booking.Id:D6}</div>
            </div>
        </div>

        <!-- Content in Two Columns -->
        <div class='content-section'>
            <!-- Left Column -->
            <div class='left-column'>
                <!-- Customer Information -->
                <div>
                    <div class='info-section'>Customer Information</div>
                    <div class='info-card'>
                        <div class='info-row'>
                            <span class='info-label'>Full Name:</span>
                            <span class='info-value'>{booking.CustomerName ?? "N/A"}</span>
                        </div>
                        <div class='info-row'>
                            <span class='info-label'>Email:</span>
                            <span class='info-value'>{booking.CustomerEmail ?? "N/A"}</span>
                        </div>
                        <div class='info-row'>
                            <span class='info-label'>Phone:</span>
                            <span class='info-value'>{booking.CustomerPhone ?? "N/A"}</span>
                        </div>
                    </div>
                </div>

                <!-- Booking Details -->
                <div>
                    <div class='info-section'>Booking Details</div>
                    <div class='info-card'>
                        <div class='info-row'>
                            <span class='info-label'>Property:</span>
                            <span class='info-value'>{booking.Home?.Name ?? "N/A"}</span>
                        </div>
                        <div class='info-row'>
                            <span class='info-label'>Check-in:</span>
                            <span class='info-value'>{booking.BookingDateFrom:MMM dd, yyyy}</span>
                        </div>
                        <div class='info-row'>
                            <span class='info-label'>Check-out:</span>
                            <span class='info-value'>{booking.BookingDateTo:MMM dd, yyyy}</span>
                        </div>
                        <div class='info-row'>
                            <span class='info-label'>Duration:</span>
                            <span class='info-value'>{(booking.BookingDateTo - booking.BookingDateFrom).Days} night{((booking.BookingDateTo - booking.BookingDateFrom).Days != 1 ? "s" : "")}</span>
                        </div>
                        <div class='info-row'>
                            <span class='info-label'>Rooms:</span>
                            <span class='info-value'>{booking.BookingRooms?.Count ?? 0} room{((booking.BookingRooms?.Count ?? 0) != 1 ? "s" : "")}</span>
                        </div>
                    </div>
                </div>
            </div>

            <!-- Right Column -->
            <div class='right-column'>
                <!-- Payment Information -->
                <div>
                    <div class='info-section'>Payment Information</div>
                    <div class='info-card'>
                        <div class='info-row'>
                            <span class='info-label'>Status:</span>
                            <span class='payment-status'>✓ PAID</span>
                        </div>
                        <div class='info-row'>
                            <span class='info-label'>Payment Date:</span>
                            <span class='info-value'>{DateTime.Now:MMM dd, yyyy}</span>
                        </div>
                        <div class='info-row'>
                            <span class='info-label'>Transaction ID:</span>
                            <span class='info-value'>TXN-{booking.Id}-{DateTime.Now:yyyyMMdd}</span>
                        </div>
                    </div>
                </div>

                {(string.IsNullOrEmpty(booking.Message) ? "" : $@"
                <!-- Notes -->
                <div>
                    <div class='info-section'>Notes</div>
                    <div class='info-card'>
                        <div class='info-value' style='line-height: 1.4;'>{booking.Message}</div>
                    </div>
                </div>")}
            </div>

            <!-- Total Amount (Full Width) -->
            <div class='total-section'>
                <div class='total-label'>Total Amount</div>
                <div class='total-amount'>₹{booking.Price:F2}</div>
            </div>
        </div>

        <!-- Footer -->
        <div class='footer'>
            <div class='footer-title'>Thank you for choosing Vernekar HomeStays!</div>
            <div class='footer-details'>
                Invoice generated on {DateTime.Now:MMM dd, yyyy} at {DateTime.Now:hh:mm tt} | For queries: info@vernekarhomestays.com
            </div>
        </div>
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
                // Create the HTML to PDF converter
                HtmlToPdf converter = new HtmlToPdf();

                // Set converter options
                converter.Options.PdfPageSize = PdfPageSize.A4;
                converter.Options.PdfPageOrientation = PdfPageOrientation.Portrait;

                // Set page margins (in points)
                converter.Options.MarginTop = 40;
                converter.Options.MarginBottom = 40;
                converter.Options.MarginLeft = 40;
                converter.Options.MarginRight = 40;

                // Set web page options
                converter.Options.WebPageWidth = 1024;
                converter.Options.WebPageHeight = 0; // auto height

                // Enable JavaScript (if needed)
                converter.Options.JavaScriptEnabled = false;

                // Set timeout (correct property name for this version)
                //converter.Options.NavigationTimeout = 60; // 60 seconds

                // Set CSS media type to print for better styling
                converter.Options.CssMediaType = HtmlToPdfCssMediaType.Print;

                // Set scaling options
                converter.Options.AutoFitWidth = HtmlToPdfPageFitMode.ShrinkOnly;
                converter.Options.AutoFitHeight = HtmlToPdfPageFitMode.NoAdjustment;

                // Convert HTML string to PDF document
                PdfDocument doc = converter.ConvertHtmlString(htmlContent);

                // Save the PDF document to the specified file path
                doc.Save(filePath);

                // Close the PDF document to free resources
                doc.Close();

                Console.WriteLine($"PDF invoice successfully generated at: {filePath}");
            }
            catch (Exception apiEx)
            {
                Console.WriteLine($"SelectPdf API Error: {apiEx.Message}");
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

                // Get booking details for invoice generation BEFORE deletion
                var booking = _bookingService.GetBookingById(bookingId);

                if (booking == null)
                {
                    return Json(new { success = false, message = "Booking not found" });
                }

                // Generate invoice first
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
                        message = "Booking completed and cleared successfully",
                        invoiceUrl = invoiceUrl
                    });
                }
                else
                {
                    return Json(new
                    {
                        success = false,
                        message = "Failed to complete booking"
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
    }


}


