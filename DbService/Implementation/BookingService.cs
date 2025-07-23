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
                        x.BookingDateTo >= startDate
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

                    // Default to current month's range if not provided
                    if (!startDate.HasValue || !endDate.HasValue)
                    {
                        var now = DateTime.Now;
                        startDate ??= new DateTime(now.Year, now.Month, 1);
                        endDate ??= new DateTime(now.Year, now.Month, DateTime.DaysInMonth(now.Year, now.Month));
                    }

                    // Apply filters
                    query = query.Where(b =>
                        b.BookingDateTo >= startDate.Value &&
                        b.BookingDateFrom <= endDate.Value);

                    if (homeId.HasValue)
                    {
                        query = query.Where(b => b.HomeId == homeId.Value);
                    }

                    return query.ToList();
                }
            }
            catch (Exception ex)
            {
                // You could log the error here
                throw new Exception("Error fetching bookings: " + ex.Message, ex);
            }
        }

    }
}
