using DbService.Models;
using DbService.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DbService.Interface
{
    public interface IBookingService
    {

        public Home GetHomeDrawing(int homeId);
        public List<LayoutElement> GetLayoutByHomeId(int homeId);
        public bool SaveBooking(Booking bookingData,List<Room> rooms);
        public HomeMasterViewModel GetSchedularData();
        public bool CheckBookings(DateTime startDate, DateTime endDate, int homeId);
        //   public  List<Booking> GetBookingsBetween(DateTime start, DateTime end, int? homeId = null);
        public List<Booking> GetBookingsBetween(DateTime startDate, DateTime endDate, int? homeId = null);

        public List<Booking> GetAllBookings(DateTime? startDate = null, DateTime? endDate = null, int? homeId = null);

        bool UpdatePaymentStatus(int bookingId, string paymentStatus);
        Booking GetBookingById(int bookingId);

        bool DeleteBooking(int bookingId);
        bool CompleteBookingWithInvoice(int bookingId);
        bool ApproveBookingRequest(int bookingId);
    }
}
