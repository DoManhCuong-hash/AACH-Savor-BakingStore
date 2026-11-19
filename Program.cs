using System.IO;
using AachSavor.Data;
using AachSavor.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
    });

// Persist DataProtection keys inside the local App_Data folder
var keysFolder = Path.Combine(builder.Environment.ContentRootPath, "App_Data", "DataProtection-Keys");
if (!Directory.Exists(keysFolder))
{
    Directory.CreateDirectory(keysFolder);
}
builder.Services.AddDataProtection()
    .PersistKeysToFileSystem(new DirectoryInfo(keysFolder))
    .SetApplicationName("AachSavor");

// Database context for SQL Server
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") 
    ?? "Server=localhost;Database=AachSavorDB;Trusted_Connection=True;TrustServerCertificate=True;";
builder.Services.AddDbContext<AachSavorDbContext>(options =>
{
    options.UseSqlServer(connectionString);
});

// Register Store Service (handles data layer, business logic, stock, wholesale pricing & auto-persistence)
builder.Services.AddSingleton<IStoreService, StoreService>();

// Cookie Authentication for Admin / Staff roles
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.AccessDeniedPath = "/Account/AccessDenied";
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
        options.SlidingExpiration = true;
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
    options.AddPolicy("StaffOrAdmin", policy => policy.RequireRole("Admin", "Staff"));
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
