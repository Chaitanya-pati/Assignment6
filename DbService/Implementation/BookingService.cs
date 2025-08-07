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

namespace DbService.Implementation
{
    public class BookingService : IBookingService
    {
        private readonly DbContextOptions<Assignment6Context> _dbconnection;

        public BookingService(string conn)
        {
            _dbconnection = new DbContextOptionsBuilder<Assignment6Context>().UseSqlServer(conn).Options;
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
                layouts = db.LayoutElements.Where(e => e.HomeId == homeId).ToList();
            }
            return layouts;
        }

        public bool SaveBooking(Booking bookingData, List<Room> rooms)
        {
            var isSaved = false;
            using (var db = new Assignment6Context(_dbconnection))
            {
                try
                {
                    // Set check -in time to 12:00 PM(noon)
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

        public bool CheckBookings(DateTime startDate, DateTime endDate, int homeId)
        {
            using (var db = new Assignment6Context(_dbconnection))
            {
                return db.Bookings.Any(x =>
                       x.HomeId == homeId &&
                       x.BookingDateFrom <= endDate &&
                        x.BookingDateTo >= startDate &&
                        x.PaymentStatus != "Paid"
                 );
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

                    // REMOVED: Default date filtering - now gets ALL bookings unless specifically filtered
                    // Only apply date filters if both startDate and endDate are provided
                    if (startDate.HasValue && endDate.HasValue)
                    {
                        query = query.Where(b =>
                            b.BookingDateTo >= startDate.Value &&
                            b.BookingDateFrom <= endDate.Value);
                    }

                    if (homeId.HasValue)
                    {
                        query = query.Where(b => b.HomeId == homeId.Value);
                    }

                    return query.OrderBy(b => b.BookingDateFrom).ToList();
                }
            }
            catch (Exception ex)
            {
                // You could log the error here
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
    }
}
