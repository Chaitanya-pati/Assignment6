
using Assignment6.Services;
using DbService.Implementation;
using DbService.Interface;
using Hangfire;
using Hangfire.MemoryStorage;
using Newtonsoft.Json;
using UserManagement.Lib;
var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
//builder.Services.AddUserManagementServices(builder.Configuration);
builder.Services.AddSingleton<IBookingService, BookingService>(provide =>
{
    return new BookingService(builder.Configuration.GetConnectionString("Assignment6"));
});
builder.Services.AddSingleton<IConfigurationService, ConfigurationService>(provide =>
{
    return new ConfigurationService(builder.Configuration.GetConnectionString("Assignment6"));
});
builder.Services.AddSingleton<IWebService, WebService>(provide =>
{
    return new WebService(builder.Configuration.GetConnectionString("Assignment6"));
});
builder.Services.AddSingleton<IDashboardService, DashboardService>(provide =>
{
    return new DashboardService(builder.Configuration.GetConnectionString("Assignment6"));
});
builder.Services.AddUserManagementServices(builder.Configuration);
builder.Services.AddHangfire(cfg =>
{
    cfg.UseMemoryStorage();
});
builder.Services.AddHangfireServer();
builder.Services.AddControllersWithViews()
    .AddNewtonsoftJson(options =>
    {
        options.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
    });
builder.Services.AddHttpClient();
builder.Services.AddScoped<AirbnbSyncJob>();
var app = builder.Build();
app.UseHangfireDashboard("/hangfire");

//Schedule Airbnb sync every minute
//RecurringJob.AddOrUpdate<AirbnbSyncJob>(
//    "sync-airbnb",
//   job => job.RunAsync(),
//   Cron.Minutely);

//app.MapGet("/", () => "Hangfire Airbnb Sync Running (No History)...");
//Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Website}/{action=Website}/{id?}");
   //pattern: "{controller=Login}/{action=Login}/{id?}");

app.Run();
