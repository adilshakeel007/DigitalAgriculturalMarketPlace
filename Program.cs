using DigitalAgriculturalMarketPlace.Data;
using Microsoft.EntityFrameworkCore;
using DigitalAgriculturalMarketplace.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Http.Features;
using System.Net;
var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
if (string.IsNullOrWhiteSpace(connectionString))
{
    throw new InvalidOperationException(
        "ConnectionStrings:DefaultConnection must be configured for this environment.");
}

builder.Services.AddControllersWithViews(options =>
    options.Filters.Add(new AutoValidateAntiforgeryTokenAttribute()));
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.Name = ".AgriMarket.Session";
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.SameSite = SameSiteMode.Strict;
    options.Cookie.SecurePolicy = builder.Environment.IsDevelopment()
        ? CookieSecurePolicy.SameAsRequest
        : CookieSecurePolicy.Always;
});
builder.Services.Configure<FormOptions>(options =>
{
    // Allows request overhead while ImageUploadValidator enforces a 5 MB image limit.
    options.MultipartBodyLengthLimit = 6 * 1024 * 1024;
});
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

if (builder.Configuration.GetValue<bool>("ReverseProxy:Enabled"))
{
    builder.Services.Configure<ForwardedHeadersOptions>(options =>
    {
        options.ForwardedHeaders = ForwardedHeaders.XForwardedFor |
                                   ForwardedHeaders.XForwardedProto;
        options.ForwardLimit = 1;
        foreach (var value in builder.Configuration
                     .GetSection("ReverseProxy:KnownProxies").Get<string[]>() ?? [])
        {
            if (IPAddress.TryParse(value, out var address))
            {
                options.KnownProxies.Add(address);
            }
        }
    });
}
builder.Services.AddScoped<InventoryService>();
builder.Services.AddScoped<OrderWorkflowService>();
builder.Services.AddScoped<RfqWorkflowService>();

var app = builder.Build();

if (app.Configuration.GetValue<bool>("ReverseProxy:Enabled"))
{
    app.UseForwardedHeaders();
}

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
app.UseSession();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
