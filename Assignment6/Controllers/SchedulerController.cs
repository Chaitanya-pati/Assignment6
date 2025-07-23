
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

        [HttpPost]
        public IActionResult SaveBooking(BookingSaveModel bookingSaveModel)
        {
            bool resultSaved = _bookingService.SaveBooking(bookingSaveModel.BookingData, bookingSaveModel.Rooms);
            return Json(resultSaved);
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


        [HttpGet]
        public IActionResult GetBookings(DateTime? startDate = null, DateTime? endDate = null, int? homeId = null)
        {
            try
            {
                var bookings =  _bookingService.GetAllBookings(startDate, endDate, homeId);

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
                    bookingRooms = b.BookingRooms?.Select(br => new
                    {
                        roomId = br.RoomId,
                        bookingId = br.BookingId
                    }).ToList()
                });

                return Json(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

    }


}
    

