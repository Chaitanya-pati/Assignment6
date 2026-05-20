using DbService.Models;
using DbService.SaveModels;
using DbService.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;
using Ical.Net;
using Ical.Net.CalendarComponents;

namespace DbService.Interface
{
    public interface IBookingService
    {

        public Home GetHomeDrawing(int homeId);
        public List<LayoutElement> GetLayoutByHomeId(int homeId);
        public bool SaveBooking(Booking bookingData,List<Room> rooms);
        public HomeMasterViewModel GetSchedularData();
        public bool CheckBookings(DateTime startDate, DateTime endDate, int homeId);
        public List<Booking> GetBookingsBetween(DateTime startDate, DateTime endDate, int? homeId = null);

        public List<Booking> GetAllBookings(DateTime? startDate = null, DateTime? endDate = null, int? homeId = null);

        bool UpdatePaymentStatus(int bookingId, string paymentStatus);
        Booking GetBookingById(int bookingId);

        bool DeleteBooking(int bookingId);
        bool CompleteBookingWithInvoice(int bookingId);
        bool ApproveBookingRequest(int bookingId);
        bool ConfirmBookingWithPayment(BookingConfirmationModel model);
        public bool CheckBookingsAlternative(DateTime startDate, DateTime endDate, int homeId);
        public bool SaveBookingAlternative(Booking bookingData, List<Room> rooms);
        public BookingViewModel GetBookingForm(DateTime date);

        Task<int> SyncAirbnbBookings(IList<Ical.Net.CalendarComponents.CalendarEvent> calendarEvents, int homeId);
        Task<int> CleanupOldAirbnbBookings(int homeId, DateTime cutoffDate);
        public string GetRoomDetailsByRoomId(int roomId);
    }

}
