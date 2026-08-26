using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using SimpleFlow.Data;
using SimpleFlow.Filters;
using SimpleFlow.Models;
using SimpleFlow.Services;
using SimpleFlow.Services.Inventory;
using System.Globalization;
using Microsoft.AspNetCore.Localization;

var builder = WebApplication.CreateBuilder(args);
builder.Configuration.AddJsonFile(
    "appsettings.Local.json",
    optional: true,
    reloadOnChange: true);

// Add services to the container.
builder.Services.AddControllersWithViews(options =>
{
    options.Filters.Add(new Microsoft.AspNetCore.Mvc.AutoValidateAntiforgeryTokenAttribute());
    options.Filters.Add<DatabaseExceptionFilter>();
});
builder.Services.AddScoped<DatabaseExceptionFilter>();
builder.Services.AddScoped<IDatabaseOperationService, DatabaseOperationService>();
builder.Services.AddScoped<IInventoryPostingService, InventoryPostingService>();
builder.Services.AddAuthorization(options =>
    options.AddPolicy("TwoFactorDisabled", policy => policy.RequireAssertion(_ => false)));
builder.Services.AddRazorPages(options =>
{
    var disabledPages = new[]
    {
        "/Account/Manage/TwoFactorAuthentication",
        "/Account/Manage/EnableAuthenticator",
        "/Account/Manage/Disable2fa",
        "/Account/Manage/GenerateRecoveryCodes",
        "/Account/Manage/ResetAuthenticator"
    };

    foreach (var page in disabledPages)
        options.Conventions.AuthorizeAreaPage("Identity", page, "TwoFactorDisabled");
});
var connectionString = builder.Configuration.GetConnectionString("DefaultConnectionString")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnectionString' is not configured.");

builder.Services.AddDbContext<SimpleFlowContext>(options => options.UseSqlServer(connectionString));
builder.Services.AddDefaultIdentity<ApplicationUser>(options =>
    {
        options.SignIn.RequireConfirmedAccount = false;
        options.Password.RequiredLength = 8;
        options.Password.RequireNonAlphanumeric = false;
    })
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<SimpleFlowContext>();

var app = builder.Build();

var supportedCultures = new[]
{
    new CultureInfo("en-US")
};

app.UseRequestLocalization(new RequestLocalizationOptions
{
    DefaultRequestCulture = new RequestCulture("en-US"),
    SupportedCultures = supportedCultures,
    SupportedUICultures = supportedCultures
});

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.MapRazorPages();


app.Run();

/// <summary>
/// Точка входа веб-приложения, доступная интеграционным тестам.
/// </summary>
public partial class Program;
