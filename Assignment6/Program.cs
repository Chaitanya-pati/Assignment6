//using UserManagement.Lib;

using DbService.Implementation;
using DbService.Interface;
using Newtonsoft.Json;

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

builder.Services.AddControllersWithViews()
    .AddNewtonsoftJson(options =>
    {
        options.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
    });


var app = builder.Build();

// Configure the HTTP request pipeline.
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
    pattern: "{controller=Scheduler}/{action=Scheduler}/{id?}");
   //pattern: "{controller=Login}/{action=Login}/{id?}");

app.Run();
