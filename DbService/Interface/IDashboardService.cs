
using DbService.ViewModels;

namespace Assignment6.Services
{
    public interface IDashboardService
    {
        Task<DashboardViewModel> GetDashboardDataAsync();
        Task<List<MonthlyRevenueViewModel>> GetMonthlyRevenueAsync(int year);
        Task<List<BookingStatusViewModel>> GetBookingStatusDistributionAsync();
        Task<List<PropertyPerformanceViewModel>> GetPropertyPerformanceAsync();
    }
}