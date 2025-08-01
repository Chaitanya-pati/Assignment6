
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

            var layouts = _bookingService.GetLayoutByHomeId(homeId);

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

                bool resultSaved = _bookingService.SaveBooking(bookingSaveModel.BookingData, bookingSaveModel.Rooms);
                return Json(resultSaved);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        public bool CheckBookings(DateTime startDate, DateTime endDate, int homeId)
        {
            bool isBooked = _bookingService.CheckBookings(startDate, endDate, homeId);
            return isBooked;
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


        //[HttpGet]
        //public IActionResult GetBookings(DateTime? startDate = null, DateTime? endDate = null, int? homeId = null)
        //{
        //    try
        //    {
        //        var bookings =  _bookingService.GetAllBookings(startDate, endDate, homeId);

        //        // Update the result object in GetBookings method
        //        var result = bookings.Select(b => new
        //        {
        //            id = b.Id,
        //            customerName = b.CustomerName,
        //            customerEmail = b.CustomerEmail,
        //            customerPhone = b.CustomerPhone,
        //            message = b.Message,
        //            bookingDateFrom = b.BookingDateFrom.ToString("yyyy-MM-dd"),
        //            bookingDateTo = b.BookingDateTo.ToString("yyyy-MM-dd"),
        //            homeId = b.HomeId,
        //            paymentStatus = b.PaymentStatus,
        //            price = b.Price,
        //            guestNumbers = b.GuestNumbers,
        //            document = b.Document,
        //            bookingRooms = b.BookingRooms?.Select(br => new
        //            {
        //                roomId = br.RoomId,
        //                bookingId = br.BookingId
        //            }).ToList()
        //        });

        //        return Json(result);
        //    }
        //    catch (Exception ex)
        //    {
        //        return BadRequest(new { error = ex.Message });
        //    }
        //}

        [HttpGet]
        public IActionResult GetBookings(DateTime? startDate = null, DateTime? endDate = null, int? homeId = null)
        {
            try
            {
                var bookings = _bookingService.GetAllBookings(startDate, endDate, homeId);

                var result = bookings.Select(b => new
                {
                    id = b.Id,
                    customerName = b.CustomerName,
                    customerEmail = b.CustomerEmail,
                    customerPhone = b.CustomerPhone,
                    message = b.Message,
                    // CHANGED: Include time information for proper calendar display
                    bookingDateFrom = b.BookingDateFrom.ToString("yyyy-MM-ddTHH:mm:ss"),
                    bookingDateTo = b.BookingDateTo.ToString("yyyy-MM-ddTHH:mm:ss"),
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

                // ADDED: Logging for debugging
                Console.WriteLine($"Returning {result.Count()} bookings");
                foreach (var booking in result.Take(3))
                {
                    Console.WriteLine($"Booking {booking.id}: {booking.bookingDateFrom} to {booking.bookingDateTo}");
                }

                return Json(result);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetBookings: {ex.Message}");
                return BadRequest(new { error = ex.Message });
            }
        }

        // ADD these methods to your SchedulerController class

        //[HttpPost]
        //public IActionResult UpdatePaymentStatus(int bookingId, string paymentStatus)
        //{
        //    try
        //    {
        //        bool updated = _bookingService.UpdatePaymentStatus(bookingId, paymentStatus);

        //        if (updated)
        //        {
        //            // Generate invoice after successful payment update
        //            string invoiceUrl = GenerateInvoice(bookingId);

        //            return Json(new
        //            {
        //                success = true,
        //                message = "Payment status updated successfully!",
        //                invoiceUrl = invoiceUrl
        //            });
        //        }
        //        else
        //        {
        //            return Json(new
        //            {
        //                success = false,
        //                message = "Failed to update payment status."
        //            });
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        return Json(new
        //        {
        //            success = false,
        //            message = "Error: " + ex.Message
        //        });
        //    }
        //}

        // First, install the PDF package via NuGet Package Manager:
        // Install-Package iTextSharp or Install-Package PdfSharpCore
        // For this example, I'll use a simple HTML to PDF approach

        // ADD these using statements at the top of your controller


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
    <title>Invoice - {booking.Id}</title>
    <style>
        body {{ 
            font-family: Arial, sans-serif; 
            margin: 0; 
            padding: 20px; 
            background-color: #fff;
            color: #000;
            line-height: 1.4;
            font-size: 12px;
        }}
        
        .invoice-container {{ 
            max-width: 800px; 
            margin: 0 auto; 
            background: white; 
            border: 1px solid #000;
        }}
        
        .header {{ 
            padding: 20px 30px; 
            border-bottom: 2px solid #000;
        }}
        
        .company-info {{
            float: left;
            width: 60%;
        }}
        
        .invoice-info {{
            float: right;
            width: 35%;
            text-align: right;
        }}
        
        .company-name {{ 
            font-size: 18px; 
            font-weight: bold; 
            margin: 0 0 5px 0;
        }}
        
        .company-details {{
            font-size: 11px;
            color: #666;
        }}
        
        .invoice-title {{ 
            font-size: 24px; 
            font-weight: bold;
            margin: 0 0 10px 0;
        }}
        
        .invoice-number {{ 
            font-size: 14px; 
            margin-bottom: 5px;
        }}
        
        .invoice-date {{ 
            font-size: 12px;
        }}
        
        .clearfix {{
            clear: both;
        }}
        
        .billing-section {{
            padding: 20px 30px;
            border-bottom: 1px solid #ddd;
        }}
        
        .bill-to {{
            float: left;
            width: 45%;
        }}
        
        .ship-to {{
            float: right;
            width: 45%;
        }}
        
        .section-title {{
            font-weight: bold;
            font-size: 12px;
            margin-bottom: 8px;
            text-transform: uppercase;
        }}
        
        .address-block {{
            font-size: 11px;
            line-height: 1.3;
        }}
        
        .items-table {{
            width: 100%;
            border-collapse: collapse;
            margin: 0;
        }}
        
        .items-table th {{
            background-color: #f5f5f5;
            border: 1px solid #ddd;
            padding: 10px 8px;
            text-align: left;
            font-size: 11px;
            font-weight: bold;
        }}
        
        .items-table td {{
            border: 1px solid #ddd;
            padding: 8px;
            font-size: 11px;
            vertical-align: top;
        }}
        
        .items-table .text-right {{
            text-align: right;
        }}
        
        .items-table .text-center {{
            text-align: center;
        }}
        
        .totals-section {{
            float: right;
            width: 300px;
            margin: 20px 30px;
        }}
        
        .totals-table {{
            width: 100%;
            border-collapse: collapse;
        }}
        
        .totals-table td {{
            padding: 5px 10px;
            font-size: 12px;
            border-bottom: 1px solid #eee;
        }}
        
        .totals-table .total-row {{
            font-weight: bold;
            border-top: 2px solid #000;
            border-bottom: 2px solid #000;
        }}
        
        .notes-section {{
            clear: both;
            padding: 20px 30px;
            border-top: 1px solid #ddd;
            font-size: 11px;
        }}
        
        .payment-info {{
            margin-top: 15px;
            padding: 10px;
            background-color: #f9f9f9;
            border: 1px solid #ddd;
        }}
        
        @media print {{
            body {{ margin: 0; padding: 0; }}
            .invoice-container {{ border: none; }}
        }}
    </style>
</head>
<body>
    <div class='invoice-container'>
        <!-- Header -->
        <div class='header'>
            <div class='company-info'>
                <div class='company-name'>VERNEKAR HOMESTAYS</div>
                <div class='company-details'>
                    Premium Hospitality & Accommodation Services<br>
                    Email: info@vernekarhomestays.com<br>
                    Phone: +1 (555) 123-4567
                </div>
            </div>
            <div class='invoice-info'>
                <div class='invoice-title'>INVOICE</div>
                <div class='invoice-number'>Invoice #: {booking.Id:D6}</div>
                <div class='invoice-date'>Date: {DateTime.Now:MM/dd/yyyy}</div>
                <div class='invoice-date'>Due Date: {DateTime.Now:MM/dd/yyyy}</div>
            </div>
            <div class='clearfix'></div>
        </div>

        <!-- Billing Information -->
        <div class='billing-section'>
            <div class='bill-to'>
                <div class='section-title'>Bill To:</div>
                <div class='address-block'>
                    <strong>{booking.CustomerName ?? "N/A"}</strong><br>
                    {booking.CustomerEmail ?? "N/A"}<br>
                    {booking.CustomerPhone ?? "N/A"}
                </div>
            </div>
            <div class='ship-to'>
                <div class='section-title'>Property:</div>
                <div class='address-block'>
                    <strong>{booking.Home?.Name ?? "N/A"}</strong><br>
                    Check-in: {booking.BookingDateFrom:MM/dd/yyyy}<br>
                    Check-out: {booking.BookingDateTo:MM/dd/yyyy}<br>
                    Duration: {(booking.BookingDateTo - booking.BookingDateFrom).Days} night{((booking.BookingDateTo - booking.BookingDateFrom).Days != 1 ? "s" : "")}<br>
                    Guests: {booking.GuestNumbers} person{(booking.GuestNumbers != 1 ? "s" : "")}
                </div>
            </div>
            <div class='clearfix'></div>
        </div>

        <!-- Items Table -->
        <table class='items-table'>
            <thead>
                <tr>
                    <th style='width: 50%'>DESCRIPTION</th>
                    <th style='width: 15%' class='text-center'>QTY</th>
                    <th style='width: 15%' class='text-right'>RATE</th>
                    <th style='width: 20%' class='text-right'>AMOUNT</th>
                </tr>
            </thead>
            <tbody>
                <tr>
                    <td>
                        <strong>Accommodation - {booking.Home?.Name ?? "N/A"}</strong><br>
                        <small>
                            Stay from {booking.BookingDateFrom:MM/dd/yyyy} to {booking.BookingDateTo:MM/dd/yyyy}<br>
                            {booking.BookingRooms?.Count ?? 0} room{((booking.BookingRooms?.Count ?? 0) != 1 ? "s" : "")} × {(booking.BookingDateTo - booking.BookingDateFrom).Days} night{((booking.BookingDateTo - booking.BookingDateFrom).Days != 1 ? "s" : "")}
                        </small>
                    </td>
                    <td class='text-center'>1</td>
                    <td class='text-right'>${booking.Price:F2}</td>
                    <td class='text-right'>${booking.Price:F2}</td>
                </tr>
                <tr>
                    <td colspan='4' style='height: 100px; border-bottom: none;'></td>
                </tr>
            </tbody>
        </table>

        <!-- Totals -->
        <div class='totals-section'>
            <table class='totals-table'>
                <tr>
                    <td>SUBTOTAL:</td>
                    <td class='text-right'>${booking.Price:F2}</td>
                </tr>
                <tr>
                    <td>TAX:</td>
                    <td class='text-right'>$0.00</td>
                </tr>
                <tr class='total-row'>
                    <td>TOTAL:</td>
                    <td class='text-right'>${booking.Price:F2}</td>
                </tr>
            </table>
        </div>

        <!-- Notes and Payment Info -->
        <div class='notes-section'>
            <div class='payment-info'>
                <strong>Payment Information:</strong><br>
                Status: PAID<br>
                Payment Date: {DateTime.Now:MM/dd/yyyy}<br>
                Transaction ID: TXN-{booking.Id}-{DateTime.Now:yyyyMMdd}
            </div>
            
            {(string.IsNullOrEmpty(booking.Message) ? "" : $@"
            <div style='margin-top: 15px;'>
                <strong>Notes:</strong><br>
                {booking.Message}
            </div>")}
            
            <div style='margin-top: 20px; font-size: 10px; color: #666;'>
                Thank you for your business!<br>
                Generated on {DateTime.Now:MM/dd/yyyy} at {DateTime.Now:hh:mm tt}
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


    }


}


