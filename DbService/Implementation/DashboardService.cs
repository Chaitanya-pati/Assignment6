
using DbService.Models;
using DbService.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace Assignment6.Services
{
    public class DashboardService : IDashboardService
    {
        private readonly DbContextOptions<Assignment6Context> _dbconnection;

        public DashboardService(string conn)
        {
            _dbconnection = new DbContextOptionsBuilder<Assignment6Context>().UseSqlServer(conn).Options;
        }

        public async Task<DashboardViewModel> GetDashboardDataAsync()
        {
            using (var db = new Assignment6Context(_dbconnection))
            {
                var currentYear = DateTime.Now.Year;

                var totalBookings = db.Bookings.Where(x=>x.IsBooked ==  true).Count();
                var totalRevenue = db.Bookings
                    .Where(b => b.PaymentStatus.ToLower() == "paid" || b.IsBooked == true)
                    .Sum(b => b.Price);

                var totalProperties = db.Homes.Count();
                var totalRooms = db.Rooms.Count();

                var pendingBookings = db.Bookings
                    .Count(b => b.PaymentStatus.ToLower() == "pending" || b.IsBooked == false);

                var completedBookings = db.Bookings
                    .Count(b => b.PaymentStatus.ToLower() == "paid");

                var totalGuests = db.Bookings
                    .Sum(b => (long?)b.GuestNumbers) ?? 0;

                var averageBookingValue = totalBookings > 0 ? totalRevenue / totalBookings : 0;

                var dashboard = new DashboardViewModel
                {
                    TotalBookings = totalBookings,
                    TotalRevenue = totalRevenue,
                    TotalProperties = totalProperties,
                    TotalRooms = totalRooms,
                    PendingBookings = pendingBookings,
                    CompletedBookings = completedBookings,
                    AverageBookingValue = averageBookingValue,
                    TotalGuests = (int)totalGuests,
                    MonthlyRevenue = GetMonthlyRevenueSync(currentYear),
                    BookingStatusData = GetBookingStatusDistributionSync(),
                    PropertyPerformance = GetPropertyPerformanceSync(),
                    RecentBookings = GetRecentBookingsSync()
                };

                return dashboard;
            }
        }

        public async Task<List<MonthlyRevenueViewModel>> GetMonthlyRevenueAsync(int year)
        {
            return GetMonthlyRevenueSync(year);
        }

        private List<MonthlyRevenueViewModel> GetMonthlyRevenueSync(int year)
        {
            using (var db = new Assignment6Context(_dbconnection))
            {
                var monthlyData = db.Bookings
                    .Where(b => b.BookingDateFrom.Year == year)
                    .GroupBy(b => b.BookingDateFrom.Month)
                    .Select(g => new
                    {
                        Month = g.Key,
                        Revenue = g.Sum(b => b.Price),
                        BookingCount = g.Count()
                    })
                    .ToList();

                // Fill missing months with zero values
                var allMonths = new List<MonthlyRevenueViewModel>();
                for (int i = 1; i <= 12; i++)
                {
                    var existingMonth = monthlyData.FirstOrDefault(m => m.Month == i);
                    allMonths.Add(new MonthlyRevenueViewModel
                    {
                        Month = new DateTime(year, i, 1).ToString("MMM"),
                        Revenue = existingMonth?.Revenue ?? 0,
                        BookingCount = existingMonth?.BookingCount ?? 0
                    });
                }

                return allMonths;
            }
        }

        public async Task<List<BookingStatusViewModel>> GetBookingStatusDistributionAsync()
        {
            return GetBookingStatusDistributionSync();
        }

        private List<BookingStatusViewModel> GetBookingStatusDistributionSync()
        {
            using (var db = new Assignment6Context(_dbconnection))
            {
                var statusData = db.Bookings.Where(x => x.IsBooked == true)
                    .GroupBy(b => b.PaymentStatus)
                    .Select(g => new
                    {
                        Status = g.Key,
                        Count = g.Count()
                    })
                    .ToList();

                var totalBookings = statusData.Sum(s => s.Count);

                return statusData.Select(s => new BookingStatusViewModel
                {
                    Status = s.Status,
                    Count = s.Count,
                    Percentage = totalBookings > 0 ? (decimal)s.Count / totalBookings * 100 : 0
                }).ToList();
            }
        }

        public async Task<List<PropertyPerformanceViewModel>> GetPropertyPerformanceAsync()
        {
            return GetPropertyPerformanceSync();
        }

        private List<PropertyPerformanceViewModel> GetPropertyPerformanceSync()
        {
            using (var db = new Assignment6Context(_dbconnection))
            {
                var propertyData = db.Bookings.Where(x => x.IsBooked == true)
                    .Include(b => b.Home)
                    .GroupBy(b => new { b.HomeId, b.Home.Name })
                    .Select(g => new
                    {
                        PropertyName = g.Key.Name,
                        BookingCount = g.Count(),
                        Revenue = g.Sum(b => b.Price)
                    })
                    .OrderByDescending(p => p.Revenue)
                    .Take(10)
                    .ToList();

                return propertyData.Select(p => new PropertyPerformanceViewModel
                {
                    PropertyName = p.PropertyName,
                    BookingCount = p.BookingCount,
                    Revenue = p.Revenue,
                    OccupancyRate = 0 // Calculate based on your business logic if needed
                }).ToList();
            }
        }

        private List<RecentBookingViewModel> GetRecentBookingsSync()
        {
            using (var db = new Assignment6Context(_dbconnection))
            {
                return db.Bookings.Where(x=>x.IsBooked == true)
                    .Include(b => b.Home)
                    .OrderByDescending(b => b.CreatedAt)
                    .Take(10)
                    .Select(b => new RecentBookingViewModel
                    {
                        Id = b.Id,
                        CustomerName = b.CustomerName,
                        PropertyName = b.Home.Name,
                        BookingDate = b.BookingDateFrom,
                        Amount = b.Price,
                        Status = b.PaymentStatus
                    })
                    .ToList();
            }
        }

        // Additional helper methods for dashboard insights
        public List<MonthlyRevenueViewModel> GetYearlyComparison(int year1, int year2)
        {
            using (var db = new Assignment6Context(_dbconnection))
            {
                var year1Data = GetMonthlyRevenueSync(year1);
                var year2Data = GetMonthlyRevenueSync(year2);

                // You can implement comparison logic here
                return year1Data;
            }
        }

        public decimal GetTotalRevenueByDateRange(DateTime startDate, DateTime endDate)
        {
            using (var db = new Assignment6Context(_dbconnection))
            {
                return db.Bookings.Where(x => x.IsBooked == true)
                    .Where(b => b.BookingDateFrom >= startDate &&
                               b.BookingDateTo <= endDate &&
                               b.PaymentStatus.ToLower() == "paid")
                    .Sum(b => b.Price);
            }
        }

        public int GetBookingCountByProperty(int homeId)
        {
            using (var db = new Assignment6Context(_dbconnection))
            {
                return db.Bookings
                    .Count(b => b.HomeId == homeId);
            }
        }
    }
}