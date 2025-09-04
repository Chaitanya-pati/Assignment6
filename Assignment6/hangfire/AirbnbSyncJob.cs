using System.Text.Json;
using Ical.Net;
using Ical.Net.CalendarComponents;
using DbService.Interface;
using Microsoft.Extensions.DependencyInjection;

public class AirbnbSyncJob : BackgroundService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<AirbnbSyncJob> _logger;
    private readonly IServiceProvider _serviceProvider;
    private readonly TimeSpan _syncInterval = TimeSpan.FromHours(2); // Sync every 2 hours

    // Store hashes for both properties to detect changes
    private static Dictionary<int, string> _lastBookingsHashes = new Dictionary<int, string>();

    // Your property URLs - matching your controller exactly
    private static readonly Dictionary<int, string> AirbnbUrls = new Dictionary<int, string>
    {
        { 1, "https://www.airbnb.co.in/calendar/ical/1022148698208402787.ics?s=456490de8497d82e9ab85927a6ce1f4d" },
        { 2, "https://www.airbnb.co.in/calendar/ical/792554814965396800.ics?s=becaf9695ca6823e046181b44cf6bf51" }
    };

    public AirbnbSyncJob(IHttpClientFactory httpClientFactory, ILogger<AirbnbSyncJob> logger, IServiceProvider serviceProvider)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
        _serviceProvider = serviceProvider;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("AirbnbSyncJob Background Service started at: {time}", DateTimeOffset.Now);

        // Run initial sync after a short delay
        await Task.Delay(TimeSpan.FromMinutes(2), stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await PerformSyncOperation();

                _logger.LogInformation("Next Airbnb sync scheduled in {interval} hours", _syncInterval.TotalHours);
                await Task.Delay(_syncInterval, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("AirbnbSyncJob Background Service is stopping due to cancellation.");
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in AirbnbSyncJob Background Service");
                await Task.Delay(TimeSpan.FromMinutes(15), stoppingToken);
            }
        }
    }

    private async Task PerformSyncOperation()
    {
        _logger.LogInformation("Starting scheduled Airbnb sync operation at: {time}", DateTimeOffset.Now);

        using var scope = _serviceProvider.CreateScope();
        var bookingService = scope.ServiceProvider.GetRequiredService<IBookingService>();

        var totalSyncedBookings = 0;
        var errors = new List<string>();

        foreach (var airbnbProperty in AirbnbUrls)
        {
            try
            {
                var homeId = airbnbProperty.Key;
                var icalUrl = airbnbProperty.Value;
                var propertyName = homeId == 1 ? "Ground Floor" : "First Floor";

                _logger.LogInformation("Processing property {propertyId} ({propertyName})...", homeId, propertyName);

                // Check for changes first (optional optimization)
                var hasChanges = await CheckForBookingChanges(homeId, icalUrl);

                if (hasChanges)
                {
                    _logger.LogInformation("Changes detected for property {propertyId}, syncing to database...", homeId);

                    // Use the same logic as your controller method
                    var syncedCount = await SyncPropertyBookings(bookingService, homeId, icalUrl);
                    totalSyncedBookings += syncedCount;

                    _logger.LogInformation("Successfully synced {count} bookings for property {propertyId} ({propertyName})",
                        syncedCount, homeId, propertyName);
                }
                else
                {
                    _logger.LogInformation("No changes detected for property {propertyId} ({propertyName})", homeId, propertyName);
                }
            }
            catch (Exception ex)
            {
                var errorMsg = $"Property {airbnbProperty.Key}: {ex.Message}";
                errors.Add(errorMsg);
                _logger.LogError(ex, "Error syncing property {propertyId}", airbnbProperty.Key);
            }
        }

        if (totalSyncedBookings > 0)
        {
            _logger.LogInformation(
                "Background sync completed. Total synced: {totalSynced}, Errors: {errorCount}",
                totalSyncedBookings, errors.Count);
        }
        else if (errors.Any())
        {
            _logger.LogWarning("Background sync completed with {errorCount} errors: {errors}",
                errors.Count, string.Join("; ", errors));
        }
        else
        {
            _logger.LogInformation("Background sync completed - no new bookings found");
        }
    }

    private async Task<int> SyncPropertyBookings(IBookingService bookingService, int homeId, string icalUrl)
    {
        try
        {
            // Use the EXACT same logic as your controller method
            var client = _httpClientFactory.CreateClient();
            client.Timeout = TimeSpan.FromMinutes(2);

            // Fetch iCal data
            HttpResponseMessage response = await client.GetAsync(icalUrl);
            response.EnsureSuccessStatusCode();
            string icalContent = await response.Content.ReadAsStringAsync();

            // Parse iCal content - Calendar.Load returns a single Calendar object
            var calendar = Calendar.Load(icalContent);
            if (calendar?.Events == null || calendar.Events.Count == 0)
                return 0;

            // Get events and convert to CalendarEvent list
            var events = calendar.Events.Cast<CalendarEvent>().ToList();
            if (events.Count == 0)
                return 0;

            // Use your existing SyncAirbnbBookings service method
            var syncedCount = await bookingService.SyncAirbnbBookings(events, homeId);
            return syncedCount;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error syncing bookings for property {homeId}", homeId);
            throw; // Re-throw to be handled by caller
        }
    }

    private async Task<bool> CheckForBookingChanges(int homeId, string icalUrl)
    {
        try
        {
            var client = _httpClientFactory.CreateClient();
            client.Timeout = TimeSpan.FromMinutes(1);

            var response = await client.GetAsync(icalUrl);
            response.EnsureSuccessStatusCode();
            var icalContent = await response.Content.ReadAsStringAsync();

            var calendar = Calendar.Load(icalContent);
            if (calendar?.Events == null || calendar.Events.Count == 0)
                return false;

            // Create a simple hash of the booking data
            var today = DateTime.Today;
            var nextYear = today.AddYears(1);

            var bookings = calendar.Events
                .OfType<CalendarEvent>()
                .Where(ev => {
                    if (ev.Start?.Value == null || ev.End?.Value == null) return false;
                    var startDate = ev.Start.Value.Date;
                    return startDate >= today && startDate <= nextYear;
                })
                .Select(ev => new
                {
                    Summary = ev.Summary ?? "Booking",
                    Start = ev.Start.Value.Date,
                    End = ev.End.Value.Date
                })
                .OrderBy(b => b.Start)
                .ToList();

            var currentHash = JsonSerializer.Serialize(bookings);

            if (!_lastBookingsHashes.ContainsKey(homeId) || _lastBookingsHashes[homeId] != currentHash)
            {
                _lastBookingsHashes[homeId] = currentHash;
                return true;
            }

            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking for booking changes for property {homeId}", homeId);
            // Return true to attempt sync anyway if we can't check for changes
            return true;
        }
    }

    // Keep the original RunAsync method for manual triggering via Hangfire or controller
    public async Task RunAsync()
    {
        _logger.LogInformation("Manual Airbnb sync triggered at: {time}", DateTimeOffset.Now);

        try
        {
            await PerformSyncOperation();
            _logger.LogInformation("Manual Airbnb sync completed successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in manual Airbnb sync");
            throw;
        }
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("AirbnbSyncJob Background Service is stopping...");
        await base.StopAsync(cancellationToken);
    }
}