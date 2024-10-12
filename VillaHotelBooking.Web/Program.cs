using VillaHotelBooking.Infa.Data;
using Microsoft.EntityFrameworkCore;
using VillaHotelBooking.App.Common.Interfaces;
using VillaHotelBooking.Infa.Repository;
using Microsoft.AspNetCore.Identity;
using VillaHotelBooking.Domain.Entities;
using System.Globalization;
using Stripe;
using VillaHotelBooking.App.Services.Interface;
using VillaHotelBooking.App.Services.Implementation;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// add db context
builder.Services.AddDbContext<ApplicationDbContext>(options =>
options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// add unit of work
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

// add dashboard service
builder.Services.AddScoped<IDashboardService, DashboardService>();

// add identity
builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>();

// Over write authorization page example
//builder.Services.ConfigureApplicationCookie(options =>
//{
//    options.AccessDeniedPath = "/Home/AccessDenied";
//    options.LoginPath = "/Account/Login";
//});

// custom password policy
//builder.Services.Configure<IdentityOptions>(options =>
//{
//    options.Password.RequireDigit = false;
//    options.Password.RequireLowercase = false;
//    options.Password.RequireUppercase = false;
//    options.Password.RequireNonAlphanumeric = false;
//    options.Password.RequiredLength = 3;
//});

var app = builder.Build();

// Regiter Strip api key
Stripe.StripeConfiguration.ApiKey = app.Configuration.GetSection("Stripe")["SecretKey"];
//StripeConfiguration.ApiKey = builder.Configuration.GetSection("Stripe:SecretKey").Get<string>();

// Register Syncfusion license
Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense(app.Configuration.GetSection("Syncfusion")["LicenseKey"]);

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
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
