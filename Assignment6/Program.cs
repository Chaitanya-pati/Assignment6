using Assignment6.Services;
using DbService.SignalR;
using DbService.Implementation;
using DbService.Interface;
using Hangfire;
using Hangfire.SqlServer; // ✅ ADDED
using Newtonsoft.Json;
using UserManagement.Lib;
using Assignment6;
using Twilio;
using Assignment6.twillio;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Get connection string ONCE
var connectionString = builder.Configuration.GetConnectionString("Assignment6");

// Register existing services
builder.Services.AddSingleton<IBookingService, BookingService>(sp =>
{
    return new BookingService(connectionString, sp);
});

builder.Services.AddSingleton<IConfigurationService, ConfigurationService>(provide =>
{
    return new ConfigurationService(connectionString);
});

builder.Services.AddSingleton<IWebService, WebService>(provide =>
{
    return new WebService(connectionString);
});

builder.Services.AddSingleton<IDashboardService, DashboardService>(provide =>
{
    return new DashboardService(connectionString);
});

// Register PaymentService
builder.Services.AddSingleton<IPaymentService>(provider =>
{
    var config = provider.GetRequiredService<IConfiguration>();
    return new PaymentService(connectionString, config);
});

builder.Services.Configure<WhatsappService>(builder.Configuration.GetSection("Twilio"));
builder.Services.AddSingleton<TwilioSmsService>();

// Twilio Init (keep as-is or uncomment if needed)
var accountSid = builder.Configuration["Twilio:AccountSid"];
var authToken = builder.Configuration["Twilio:AuthToken"];
//TwilioClient.Init(accountSid, authToken);

builder.Services.AddUserManagementServices(builder.Configuration);

// ✅ UPDATED HANGFIRE CONFIG (IMPORTANT)
builder.Services.AddHangfire(cfg =>
{
    cfg.UseSqlServerStorage(connectionString); // ✅ CHANGED
});

builder.Services.AddHangfireServer();

builder.Services.AddControllersWithViews()
    .AddNewtonsoftJson(options =>
    {
        options.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
    });

builder.Services.AddHttpClient();
builder.Services.AddSignalR();

// Airbnb Job
builder.Services.AddScoped<AirbnbSyncJob>();

var app = builder.Build();

// Hangfire Dashboard
app.UseHangfireDashboard("/hangfire");

// ✅ ENSURE JOB REGISTERS PROPERLY
RecurringJob.AddOrUpdate<AirbnbSyncJob>(
    "sync-airbnb",
    job => job.RunAsync(),
    Cron.Minutely);

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// (No change here intentionally)
app.MapHub<SignalRService>("/signalR");

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Website}/{action=Website}/{id?}");

app.Run();