#nullable disable
using System;

namespace DbService.Models;

public partial class BookingGuest
{
    public int Id { get; set; }
    public int BookingId { get; set; }
    public int GuestOrder { get; set; }
    public string GuestName { get; set; }
    public string ContactNumber { get; set; }
    public string Gender { get; set; }
    public string IdProof { get; set; }
    public DateTime CreatedAt { get; set; }

    public virtual Booking Booking { get; set; }
}
