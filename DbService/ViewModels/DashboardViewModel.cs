using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DbService.ViewModels
{
    public class DashboardViewModel
    {
        public int TotalBookings { get; set; }
        public decimal TotalRevenue { get; set; }
        public decimal TotalAdvance { get; set; }
        public decimal TotalPending { get; set; }
        public int TotalProperties { get; set; }
        public int TotalRooms { get; set; }
        public int PendingBookings { get; set; }
        public int CompletedBookings { get; set; }
        public decimal AverageBookingValue { get; set; }
        public int TotalGuests { get; set; }
        public List<MonthlyRevenueViewModel> MonthlyRevenue { get; set; } = new();
        public List<BookingStatusViewModel> BookingStatusData { get; set; } = new();
        public List<PropertyPerformanceViewModel> PropertyPerformance { get; set; } = new();
        public List<RecentBookingViewModel> RecentBookings { get; set; } = new();
    }

    public class MonthlyRevenueViewModel
    {
        public string Month { get; set; }
        public decimal Revenue { get; set; }
        public int BookingCount { get; set; }
    }

    public class BookingStatusViewModel
    {
        public string Status { get; set; }
        public int Count { get; set; }
        public decimal Percentage { get; set; }
    }

    public class PropertyPerformanceViewModel
    {
        public string PropertyName { get; set; }
        public int BookingCount { get; set; }
        public decimal Revenue { get; set; }
        public decimal OccupancyRate { get; set; }
    }

    public class RecentBookingViewModel
    {
        public int Id { get; set; }
        public string CustomerName { get; set; }
        public string PropertyName { get; set; }
        public DateTime BookingDate { get; set; }
        public decimal Amount { get; set; }
        public decimal Advance { get; set; }
        public decimal Pending { get; set; }
        public string Status { get; set; }
    }
}
