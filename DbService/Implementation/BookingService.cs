using DbService.Interface;
using DbService.Models;
using DbService.ViewModels;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using static System.Runtime.InteropServices.JavaScript.JSType;
using Ical.Net;
using Ical.Net.CalendarComponents;
using Ical.Net.DataTypes;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;
using DbService.SignalR;
namespace DbService.Implementation
{
    public class BookingService : IBookingService
    {
        private readonly DbContextOptions<Assignment6Context> _dbconnection;
        private readonly IServiceProvider _serviceProvider;
        public BookingService(string conn, IServiceProvider serviceProvider )
        {
            _dbconnection = new DbContextOptionsBuilder<Assignment6Context>().UseSqlServer(conn).Options;
            _serviceProvider = serviceProvider;
        }

        public Home GetHomeDrawing(int homeId)
        {
            var home = new Home();
            using (var db = new Assignment6Context(_dbconnection))
            {
                home = db.Homes.FirstOrDefault(x => x.Id == homeId);
                var layouts = db.LayoutElements.Where(e => e.HomeId == homeId).ToList();
            }
            return home;
        }

        public List<LayoutElement> GetLayoutByHomeId(int homeId)
        {
            var layouts = new List<LayoutElement>();
            using (var db = new Assignment6Context(_dbconnection))
            {
                layouts = db.LayoutElements.Include(l=>l.Room)
                    .Where(e => e.HomeId == homeId).ToList();
            }
            return layouts;
        }

        //public bool SaveBooking(Booking bookingData, List<Room> rooms)
        //{
        //    var isSaved = false;
        //    using (var db = new Assignment6Context(_dbconnection))
        //    {
        //        try
        //        {
        //            if(db.Bookings.Any(x =>
        //               x.HomeId == bookingData.HomeId &&
        //               x.BookingDateFrom <= bookingData.BookingDateFrom &&
        //                x.BookingDateTo >= bookingData.BookingDateTo))
        //            {
        //               return false;
        //            }
        //            //if()
        //            // Set check -in time to 12:00 PM(noon)
        //            bookingData.BookingDateFrom = bookingData.BookingDateFrom.Date.AddHours(12);
        //            // Set check-out time to 1:00 PM (13:00)
        //            bookingData.BookingDateTo = bookingData.BookingDateTo.Date.AddHours(13);
        //            bookingData.CreatedAt = DateTime.Now;
        //            db.Bookings.Add(bookingData);
        //            db.SaveChanges();
        //            foreach (var room in rooms)
        //            {
        //                var roomSaveData = new BookingRoom();
        //                roomSaveData.BookingId = bookingData.Id;
        //                roomSaveData.RoomId = room.Id;
        //                db.BookingRooms.Add(roomSaveData);
        //            }

        //            db.SaveChanges();
        //            isSaved = true;
        //        }
        //        catch
        //        {
        //            isSaved = false;
        //        }

        //    }
        //    return isSaved;
        //}

        //public bool CheckBookings(DateTime startDate, DateTime endDate, int homeId)
        //{
        //    using (var db = new Assignment6Context(_dbconnection))
        //    {
        //        // Check for conflicts
        //        // A conflict exists if:
        //        // - New booking starts before existing booking ends AND
        //        // - New booking ends after existing booking starts
        //        // Note: We use < and > (not <= and >=) so checkout day is available

        //        return db.Bookings.Any(x =>
        //               x.HomeId == homeId &&
        //               x.IsBooked == true && // Only check approved bookings
        //               x.BookingDateFrom < endDate &&   // Existing starts before new ends
        //               x.BookingDateTo.AddDays(-1) > startDate      // Existing ends after new starts
        //         );
        //    }
        //}


        public bool CheckBookings(DateTime startDate, DateTime endDate, int homeId)
        {
            using (var db = new Assignment6Context(_dbconnection))
            {
                // Convert to date-only for comparison (removes time component)
                var checkStartDate = startDate.Date;
                var checkEndDate = endDate.Date;

                return db.Bookings.Any(x =>
                       x.HomeId == homeId &&
                       x.IsBooked == true && // Only check approved bookings
                       x.BookingDateFrom.Date < checkEndDate &&           // Existing starts before new ends
                       x.BookingDateTo.Date > checkStartDate              // Existing ends after new starts (checkout day available)
                 );
            }
        }

        public bool SaveBooking(Booking bookingData, List<Room> rooms)
        {
            var isSaved = false;
            using (var db = new Assignment6Context(_dbconnection))
            {
                try
                {
                    // Convert to date-only for comparison
                    var newStartDate = bookingData.BookingDateFrom.Date;
                    var newEndDate = bookingData.BookingDateTo.Date;

                    // Check for booking conflicts
                    if (db.Bookings.Any(x =>
                       x.HomeId == bookingData.HomeId &&
                       x.IsBooked == true && // Only check approved bookings
                       x.BookingDateFrom.Date < newEndDate &&             // Existing starts before new ends
                       x.BookingDateTo.Date > newStartDate                // Existing ends after new starts
                    ))
                    {
                        return false; // Booking conflict exists
                    }

                    // Set check-in time to 12:00 PM (noon)
                    bookingData.BookingDateFrom = bookingData.BookingDateFrom.Date.AddHours(12);
                    // Set check-out time to 1:00 PM (13:00)
                    bookingData.BookingDateTo = bookingData.BookingDateTo.Date.AddHours(13);
                    bookingData.CreatedAt = DateTime.Now;

                    db.Bookings.Add(bookingData);
                    db.SaveChanges();

                    foreach (var room in rooms)
                    {
                        var roomSaveData = new BookingRoom();
                        roomSaveData.BookingId = bookingData.Id;
                        roomSaveData.RoomId = room.Id;
                        db.BookingRooms.Add(roomSaveData);
                    }

                    db.SaveChanges();
                    isSaved = true;
                }
                catch
                {
                    isSaved = false;
                }
            }
            return isSaved;
        }

        // Alternative approach if the above doesn't work - using your AddDays(-1) logic
        public bool CheckBookingsAlternative(DateTime startDate, DateTime endDate, int homeId)
        {
            using (var db = new Assignment6Context(_dbconnection))
            {
                return db.Bookings.Any(x =>
                       x.HomeId == homeId &&
                       x.IsBooked == true && // Only check approved bookings
                       x.BookingDateFrom.Date < endDate.Date &&                    // Existing starts before new ends
                       x.BookingDateTo.Date.AddDays(-1) >= startDate.Date          // Existing ends after new starts (checkout day available)
                 );
            }
        }


        //airbnb
        // Helper method to safely convert CalDateTime to DateTime


        // Updated SyncAirbnbBookings method using the helper
        public async Task<int> SyncAirbnbBookings(IList<CalendarEvent> calendarEvents, int homeId)
        {
            var syncedCount = 0;

            using (var db = new Assignment6Context(_dbconnection))
            {
                try
                {
                    // Get all rooms for this home to book all of them
                    var homeRooms = db.Rooms.Where(r => r.HomeId == homeId).ToList();

                    // Get existing Airbnb bookings to avoid duplicates
                    var existingAirbnbBookings = db.Bookings
                        .Where(b => b.HomeId == homeId && b.CustomerName == "Airbnb Guest")
                        .ToList();

                    foreach (var calendarEvent in calendarEvents)
                    {
                        try
                        {
                            // Simple datetime extraction - just use .Value property
                            DateTime eventStartDate;
                            DateTime eventEndDate;

                            // Handle Start date
                            if (calendarEvent.Start?.Value != null)
                            {
                                eventStartDate = calendarEvent.Start.Value.Date;
                            }
                            else
                            {
                                continue; // Skip if we can't get start date
                            }

                            // Handle End date
                            if (calendarEvent.End?.Value != null)
                            {
                                eventEndDate = calendarEvent.End.Value.Date;
                            }
                            else
                            {
                                continue; // Skip if we can't get end date
                            }

                            // Skip if end date is not after start date
                            if (eventEndDate <= eventStartDate)
                                continue;

                            // Check if this booking already exists
                            bool bookingExists = existingAirbnbBookings.Any(b =>
                                b.BookingDateFrom.Date == eventStartDate &&
                                b.BookingDateTo.Date == eventEndDate);

                            if (bookingExists)
                                continue;

                            // Check for conflicts using the SAME logic as your SaveBookingAlternative method
                            bool hasConflict = db.Bookings.Any(x =>
                                x.HomeId == homeId &&
                                x.IsBooked == true && // Only check approved bookings
                                x.BookingDateFrom.Date < eventEndDate &&
                                x.BookingDateTo.Date.AddDays(-1) >= eventStartDate);

                            if (hasConflict)
                                continue;

                            // Get home data for pricing
                            var home = db.Homes.FirstOrDefault(h => h.Id == homeId);
                            int days = (eventEndDate - eventStartDate).Days;
                            long totalPrice = 0;

                            if (home != null)
                            {
                                totalPrice = (long)((home.PricePerDay ?? 0) * days);
                            }

                            // Create new Airbnb booking
                            var newBooking = new Booking
                            {
                                CustomerName = "Airbnb Guest",
                                CustomerEmail = "airbnb@guest.com",
                                CustomerPhone = "N/A",
                                Message = $"Airbnb booking - {calendarEvent.Summary ?? "Reserved"}",
                                BookingDateFrom = eventStartDate.Date.AddHours(12),
                                BookingDateTo = eventEndDate.Date.AddHours(13),
                                HomeId = homeId,
                                PaymentStatus = "pending",
                                CreatedAt = DateTime.Now,
                                Price = 0,
                                GuestNumbers = 1,
                                Document = null,
                                IsBooked = true,
                                CheckOut = false
                            };
                            var hubContext = _serviceProvider.GetRequiredService<IHubContext<SignalRService>>();
                            hubContext.Clients.All.SendAsync("BindBookingFromAirbnb", newBooking);
                            // Add booking to database
                            db.Bookings.Add(newBooking);
                            db.SaveChanges();

                            // Book all rooms for this home
                            foreach (var room in homeRooms)
                            {
                                var bookingRoom = new BookingRoom
                                {
                                    BookingId = newBooking.Id,
                                    RoomId = room.Id
                                };
                                db.BookingRooms.Add(bookingRoom);
                            }

                            db.SaveChanges();
                            syncedCount++;
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Error processing calendar event: {ex.Message}");
                            System.Diagnostics.Debug.WriteLine($"Calendar event error details: {ex}");
                        }
                    }
                }
                catch (Exception ex)
                {
                    throw new Exception($"Failed to sync Airbnb bookings for home {homeId}: {ex.Message}");
                }
            }

            return syncedCount;
        }


        // Optional: Add this method to handle cleanup of old Airbnb bookings
        public async Task<int> CleanupOldAirbnbBookings(int homeId, DateTime cutoffDate)
        {
            var deletedCount = 0;

            using (var db = new Assignment6Context(_dbconnection))
            {
                try
                {
                    // Find old Airbnb bookings that are no longer relevant
                    var oldAirbnbBookings = db.Bookings
                        .Where(b => b.HomeId == homeId &&
                                   b.CustomerName == "Airbnb Guest" &&
                                   b.BookingDateTo < cutoffDate)
                        .ToList();

                    foreach (var booking in oldAirbnbBookings)
                    {
                        // Remove associated booking rooms first
                        var bookingRooms = db.BookingRooms.Where(br => br.BookingId == booking.Id);
                        db.BookingRooms.RemoveRange(bookingRooms);

                        // Remove the booking
                        db.Bookings.Remove(booking);
                        deletedCount++;
                    }

                    db.SaveChanges();
                }
                catch (Exception ex)
                {
                    throw new Exception($"Failed to cleanup old Airbnb bookings: {ex.Message}");
                }
            }

            return deletedCount;
        }

        // Enhanced sync method that also includes cleanup (optional)
        public async Task<int> SyncAirbnbBookingsWithCleanup(IList<Ical.Net.CalendarComponents.CalendarEvent> calendarEvents, int homeId)
        {
            // First cleanup old bookings (older than 30 days)
            var cutoffDate = DateTime.Now.AddDays(-30);
            await CleanupOldAirbnbBookings(homeId, cutoffDate);

            // Then sync new bookings
            return await SyncAirbnbBookings(calendarEvents, homeId);
        }


        public bool SaveBookingAlternative(Booking bookingData, List<Room> rooms)
        {
            var isSaved = false;
            using (var db = new Assignment6Context(_dbconnection))
            {
                try
                {
                    // Check for booking conflicts using AddDays(-1) approach
                    if (db.Bookings.Any(x =>
                       x.HomeId == bookingData.HomeId &&
                       x.IsBooked == true && // Only check approved bookings
                       x.BookingDateFrom.Date < bookingData.BookingDateTo.Date &&                      // Existing starts before new ends
                       x.BookingDateTo.Date.AddDays(-1) >= bookingData.BookingDateFrom.Date           // Existing ends after new starts
                    ))
                    {
                        return false; // Booking conflict exists
                    }

                    // Set check-in time to 12:00 PM (noon)
                    bookingData.BookingDateFrom = bookingData.BookingDateFrom.Date.AddHours(12);
                    // Set check-out time to 1:00 PM (13:00)
                    bookingData.BookingDateTo = bookingData.BookingDateTo.Date.AddHours(13);
                    bookingData.CreatedAt = DateTime.Now;

                    db.Bookings.Add(bookingData);
                    db.SaveChanges();

                    if (rooms != null)
                    {
                        foreach (var room in rooms)
                        {
                            var roomSaveData = new BookingRoom();
                            roomSaveData.BookingId = bookingData.Id;
                            roomSaveData.RoomId = room.Id;
                            db.BookingRooms.Add(roomSaveData);
                        }
                    }
                    db.SaveChanges();
                    isSaved = true;
                }
                catch
                {
                    isSaved = false;
                }
            }
            return isSaved;
        }
        public HomeMasterViewModel GetSchedularData()
        {
            using (var db = new Assignment6Context(_dbconnection))
            {
                HomeMasterViewModel homeMasterViewModel = new HomeMasterViewModel
                {

                    Bookings = db.Bookings.ToList(),
                    Homes = db.Homes.ToList(),
                    Rooms = db.Rooms.ToList(),
                    BookingRooms = db.BookingRooms.ToList()
                };

                return homeMasterViewModel;
            }
        }


        public List<Booking> GetBookingsBetween(DateTime startDate, DateTime endDate, int? homeId = null)
        {
            using (var db = new Assignment6Context(_dbconnection))
            {
                var query = db.Bookings
                              .Where(b => b.BookingDateFrom <= endDate && b.BookingDateTo >= startDate);

                if (homeId.HasValue)
                {
                    query = query.Where(b => b.HomeId == homeId.Value);
                }

                return query.ToList();
            }
        }


        //public List<Booking> GetAllBookings(DateTime? startDate = null, DateTime? endDate = null, int? homeId = null)
        //{
        //    try
        //    {
        //        using (var db = new Assignment6Context(_dbconnection))
        //        {
        //            var query = db.Bookings
        //            .Include(b => b.BookingRooms)
        //                .ThenInclude(br => br.Room)
        //            .Include(b => b.Home)
        //            .AsQueryable();

        //            // Default to current month's range if not provided
        //            if (!startDate.HasValue || !endDate.HasValue)
        //            {
        //                var now = DateTime.Now;
        //                startDate ??= new DateTime(now.Year, now.Month, 1);
        //                endDate ??= new DateTime(now.Year, now.Month, DateTime.DaysInMonth(now.Year, now.Month));
        //            }

        //            // Apply filters
        //            query = query.Where(b =>
        //                b.BookingDateTo >= startDate.Value &&
        //                b.BookingDateFrom <= endDate.Value);

        //            if (homeId.HasValue)
        //            {
        //                query = query.Where(b => b.HomeId == homeId.Value);
        //            }

        //            return query.ToList();
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        // You could log the error here
        //        throw new Exception("Error fetching bookings: " + ex.Message, ex);
        //    }
        //}
        public List<Booking> GetAllBookings(DateTime? startDate = null, DateTime? endDate = null, int? homeId = null)
        {
            try
            {
                using (var db = new Assignment6Context(_dbconnection))
                {
                    var query = db.Bookings
                        .Include(b => b.BookingRooms)
                            .ThenInclude(br => br.Room)
                        .Include(b => b.Home)
                        .AsQueryable();

                    // Apply date range filter if both dates are provided (for 6-month chunks)
                    if (startDate.HasValue && endDate.HasValue)
                    {
                        // Filter bookings that overlap with the requested date range
                        // A booking overlaps if: booking_end > range_start AND booking_start < range_end
                        query = query.Where(b =>
                            b.BookingDateTo > startDate.Value &&
                            b.BookingDateFrom < endDate.Value);

                        Console.WriteLine($"Filtering bookings between {startDate.Value:yyyy-MM-dd} and {endDate.Value:yyyy-MM-dd}");
                    }
                    else
                    {
                        // If no date filter provided, optionally limit to reasonable range
                        // to prevent loading years of data at once
                        var defaultStartDate = DateTime.Now.AddMonths(-12);
                        var defaultEndDate = DateTime.Now.AddMonths(12);

                        query = query.Where(b =>
                            b.BookingDateTo > defaultStartDate &&
                            b.BookingDateFrom < defaultEndDate);

                        Console.WriteLine($"No date filter provided. Using default range: {defaultStartDate:yyyy-MM-dd} to {defaultEndDate:yyyy-MM-dd}");
                    }

                    // Apply homeId filter if specified
                    if (homeId.HasValue && homeId.Value > 0)
                    {
                        query = query.Where(b => b.HomeId == homeId.Value);
                        Console.WriteLine($"Filtering for HomeId: {homeId.Value}");
                    }

                    var result = query
                        .OrderBy(b => b.BookingDateFrom)
                        .ToList();

                    Console.WriteLine($"Retrieved {result.Count} bookings from database");

                    return result;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetAllBookings service: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                throw new Exception("Error fetching bookings: " + ex.Message, ex);
            }
        }
        // ADD these methods to your BookingService class

        public bool UpdatePaymentStatus(int bookingId, string paymentStatus)
        {
            try
            {
                using (var db = new Assignment6Context(_dbconnection))
                {
                    var booking = db.Bookings.FirstOrDefault(b => b.Id == bookingId);

                    if (booking == null)
                        return false;

                    booking.PaymentStatus = paymentStatus;
                   // booking.UpdatedAt = DateTime.Now; // Add this field to your Booking model if not exists

                    db.SaveChanges();
                    return true;
                }
            }
            catch (Exception ex)
            {
                // Log error here if you have logging
                return false;
            }
        }

        public Booking GetBookingById(int bookingId)
        {
            using (var db = new Assignment6Context(_dbconnection))
            {
                return db.Bookings
                    .Include(b => b.Home)
                    .Include(b => b.BookingRooms)
                        .ThenInclude(br => br.Room)
                    .FirstOrDefault(b => b.Id == bookingId);
            }
        }

        // DbService/Implementation/BookingService.cs - ADD these methods to your existing BookingService class

        // ADD these methods to your existing BookingService class
        public bool DeleteBooking(int bookingId)
        {
            try
            {
                using (var db = new Assignment6Context(_dbconnection))
                {
                    var booking = db.Bookings.FirstOrDefault(b => b.Id == bookingId);

                    if (booking == null)
                        return false;

                    // Delete related BookingRooms first (foreign key constraint)
                    var bookingRooms = db.BookingRooms.Where(br => br.BookingId == bookingId).ToList();

                    if (bookingRooms.Any())
                    {
                        db.BookingRooms.RemoveRange(bookingRooms);
                    }

                    // Delete the booking
                    db.Bookings.Remove(booking);
                    db.SaveChanges();

                    return true;
                }
            }
            catch (Exception ex)
            {
                // Log error here if you have logging
                Console.WriteLine($"Error deleting booking {bookingId}: {ex.Message}");
                return false;
            }
        }

        public bool CompleteBookingWithInvoice(int bookingId)
        {
            try
            {
                using (var db = new Assignment6Context(_dbconnection))
                {
                    var booking = db.Bookings
                        .Include(b => b.Home)
                        .Include(b => b.BookingRooms)
                        .FirstOrDefault(b => b.Id == bookingId);

                    if (booking == null)
                        return false;

                    // First update payment status to paid
                    booking.PaymentStatus = "Paid";
                    booking.CheckOut = true; // Assuming you want to mark it as checked out
                    db.SaveChanges();

                    // Then delete related BookingRooms
                    //var bookingRooms = db.BookingRooms.Where(br => br.BookingId == bookingId).ToList();

                    //if (bookingRooms.Any())
                    //{
                    //    db.BookingRooms.RemoveRange(bookingRooms);
                    //}

                    //// Finally delete the booking
                    //db.Bookings.Remove(booking);
                    //db.SaveChanges();

                    return true;
                }
            }
            catch (Exception ex)
            {
                // Log error here if you have logging
                Console.WriteLine($"Error completing booking {bookingId}: {ex.Message}");
                return false;
            }
        }

        public bool ApproveBookingRequest(int bookingId)
        {
            try
            {
                using var context = new Assignment6Context(_dbconnection);
                var booking = context.Bookings.FirstOrDefault(b => b.Id == bookingId);

                if (booking != null)
                {
                    booking.IsBooked = true;
                    context.SaveChanges();
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error approving booking request: {ex.Message}");
                return false;
            }
        }

        public bool ConfirmBookingWithPayment(DbService.SaveModels.BookingConfirmationModel model)
        {
            try
            {
                using var context = new Assignment6Context(_dbconnection);
                var booking = context.Bookings.FirstOrDefault(b => b.Id == model.BookingId);

                if (booking == null)
                    return false;

                booking.IsBooked = true;
                booking.PaymentMethod = model.PaymentMethod;

                // Update final price if admin changed it (negotiation)
                if (model.FinalRoomPrice.HasValue && model.FinalRoomPrice.Value > 0)
                    booking.Price = (long)model.FinalRoomPrice.Value;

                // Update advance/payment amount
                booking.AdvancePrice = (long)model.AmountPaid;

                bool isPartial = model.IsPartialPayment || model.IsAdvancePayment;

                if (!isPartial)
                {
                    booking.PaymentStatus = "Paid";
                }
                else
                {
                    booking.PaymentStatus = "Partial";
                }

                // Save a Payment record
                var payment = new Payment
                {
                    BookingId = model.BookingId,
                    Amount = (decimal)booking.Price,
                    AmountPaid = model.AmountPaid,
                    Currency = "INR",
                    Status = isPartial ? "partial" : "paid",
                    PaymentMethod = model.PaymentMethod,
                    IsPartialPayment = isPartial,
                    IsAdvancePayment = isPartial,
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now
                };

                context.Payments.Add(payment);
                context.SaveChanges();
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error confirming booking with payment: {ex.Message}");
                return false;
            }
        }

        public BookingViewModel GetBookingForm(DateTime date)
        {
            var model = new BookingViewModel();
            using (var db = new Assignment6Context(_dbconnection))
            {
                model = new BookingViewModel
                {
                    BookingDateFrom = date,
                    BookingDateTo = date.AddDays(1),
                    AvailableHomes = db.Homes.ToList()
                };
            }
            return model;
        }

        public string GetRoomDetailsByRoomId(int roomId)
        {
            try
            {
                using (var db = new Assignment6Context(_dbconnection))
                {
                    return db.Rooms.Where(x => x.Id == roomId).Select(y => y.Name).FirstOrDefault();
                }
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        public bool SavePaymentDetails(int bookingId, long advanceAmount, string advancePaymentMethod, long? finalAmount, string finalPaymentMethod)
        {
            try
            {
                using (var db = new Assignment6Context(_dbconnection))
                {
                    var booking = db.Bookings.FirstOrDefault(b => b.Id == bookingId);
                    if (booking == null) return false;

                    // Store advance / first payment
                    booking.AdvancePrice = advanceAmount;
                    booking.PaymentMethod = advancePaymentMethod;

                    // Store final / second payment (only when two-part)
                    bool hasFinalPayment = finalAmount.HasValue && finalAmount.Value > 0 && !string.IsNullOrWhiteSpace(finalPaymentMethod);
                    booking.FinalPaymentAmount = hasFinalPayment ? finalAmount : null;
                    booking.FinalPaymentMethod = hasFinalPayment ? finalPaymentMethod : null;

                    long totalPaid = advanceAmount + (hasFinalPayment ? finalAmount!.Value : 0);
                    booking.PaymentStatus = totalPaid >= booking.Price ? "Paid" : "Partial";

                    // Log advance payment record
                    if (advanceAmount > 0 && !string.IsNullOrWhiteSpace(advancePaymentMethod))
                    {
                        // Remove stale advance Payment records for this booking so we don't duplicate
                        var oldAdvance = db.Payments.Where(p => p.BookingId == bookingId && p.IsAdvancePayment).ToList();
                        db.Payments.RemoveRange(oldAdvance);

                        db.Payments.Add(new Payment
                        {
                            BookingId = bookingId,
                            Amount = (decimal)booking.Price,
                            AmountPaid = (decimal)advanceAmount,
                            Currency = "INR",
                            Status = "paid",
                            PaymentMethod = advancePaymentMethod,
                            IsAdvancePayment = true,
                            IsPartialPayment = hasFinalPayment || totalPaid < booking.Price,
                            CreatedAt = DateTime.Now,
                            UpdatedAt = DateTime.Now
                        });
                    }

                    // Log final payment record
                    if (hasFinalPayment)
                    {
                        var oldFinal = db.Payments.Where(p => p.BookingId == bookingId && !p.IsAdvancePayment).ToList();
                        db.Payments.RemoveRange(oldFinal);

                        db.Payments.Add(new Payment
                        {
                            BookingId = bookingId,
                            Amount = (decimal)booking.Price,
                            AmountPaid = (decimal)finalAmount!.Value,
                            Currency = "INR",
                            Status = "paid",
                            PaymentMethod = finalPaymentMethod,
                            IsAdvancePayment = false,
                            IsPartialPayment = false,
                            CreatedAt = DateTime.Now,
                            UpdatedAt = DateTime.Now
                        });
                    }

                    db.SaveChanges();
                    return true;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving payment details for booking {bookingId}: {ex.Message}");
                return false;
            }
        }

        public bool UpdateAdvancePayment(int bookingId, long advanceAmount, string advancePaymentMethod)
        {
            try
            {
                using (var db = new Assignment6Context(_dbconnection))
                {
                    var booking = db.Bookings.FirstOrDefault(b => b.Id == bookingId);
                    if (booking == null) return false;

                    booking.AdvancePrice = advanceAmount;
                    booking.PaymentMethod = advancePaymentMethod;

                    long remaining = booking.Price - advanceAmount;
                    booking.PaymentStatus = remaining <= 0 ? "Paid" : "Partial";

                    db.SaveChanges();
                    return true;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating advance payment for booking {bookingId}: {ex.Message}");
                return false;
            }
        }

        public bool CaptureRemainingPayment(int bookingId, string finalPaymentMethod)
        {
            try
            {
                using (var db = new Assignment6Context(_dbconnection))
                {
                    var booking = db.Bookings.FirstOrDefault(b => b.Id == bookingId);
                    if (booking == null) return false;

                    booking.FinalPaymentMethod = finalPaymentMethod;
                    booking.PaymentStatus = "Paid";

                    // Record the final payment in the Payments table
                    long advancePaid = booking.AdvancePrice ?? 0;
                    long remaining = booking.Price - advancePaid;

                    if (remaining > 0)
                    {
                        var payment = new Payment
                        {
                            BookingId = bookingId,
                            Amount = (decimal)booking.Price,
                            AmountPaid = (decimal)remaining,
                            Currency = "INR",
                            Status = "paid",
                            PaymentMethod = finalPaymentMethod,
                            IsPartialPayment = false,
                            IsAdvancePayment = false,
                            CreatedAt = DateTime.Now,
                            UpdatedAt = DateTime.Now
                        };
                        db.Payments.Add(payment);
                    }

                    db.SaveChanges();
                    return true;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error capturing remaining payment for booking {bookingId}: {ex.Message}");
                return false;
            }
        }
    }
}
