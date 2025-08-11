using System.Text.Json;
using Ical.Net;
using Ical.Net.CalendarComponents;

public class AirbnbSyncJob
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<AirbnbSyncJob> _logger;
    private static string _lastBookingsHash = ""; // in-memory cache

    private const string IcalUrl = "https://www.airbnb.co.in/calendar/ical/1022148698208402787.ics?s=456490de8497d82e9ab85927a6ce1f4d";

    public AirbnbSyncJob(IHttpClientFactory httpClientFactory, ILogger<AirbnbSyncJob> logger)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    public async Task RunAsync()
    {
        try
        {
            var today = DateTime.Today;
            var nextYear = today.AddYears(1);

            var client = _httpClientFactory.CreateClient();
            var response = await client.GetAsync(IcalUrl);
            response.EnsureSuccessStatusCode();

            var icalData = await response.Content.ReadAsStringAsync();
            var calendar = Calendar.Load(icalData);

            var bookings = calendar.Events
                .OfType<CalendarEvent>()
                .Where(ev => ev.Start.AsSystemLocal.Date >= today &&
                             ev.Start.AsSystemLocal.Date <= nextYear)
                .Select(ev => new
                {
                    Summary = ev.Summary ?? "Booking",
                    Start = ev.Start.AsSystemLocal.Date,
                    End = ev.End.AsSystemLocal.Date
                })
                .OrderBy(b => b.Start)
                .ToList();

            // Serialize to JSON for comparison
            var currentHash = JsonSerializer.Serialize(bookings);

            if (currentHash != _lastBookingsHash)
            {
                _lastBookingsHash = currentHash;

                _logger.LogInformation("New or updated Airbnb bookings detected:");
                foreach (var b in bookings)
                {
                    _logger.LogInformation("Booking: {Summary}, Start: {Start}, End: {End}",
                        b.Summary, b.Start, b.End);
                }

                // Call your DB method here
                // SaveBookingsToDatabase(bookings);
            }
            else
            {
                _logger.LogInformation("No booking changes since last check.");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching Airbnb bookings");
        }
    }
}
