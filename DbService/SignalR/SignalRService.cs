using Microsoft.AspNetCore.SignalR;
using DbService.Models;
namespace DbService.SignalR
{
    public class SignalRService : Hub
    {
        public async Task BindAirBnbBooking(List<Booking> Bookings )
        {
            await Clients.All.SendAsync("BindBookingFromAirbnb", Bookings);
        }
    }
}
