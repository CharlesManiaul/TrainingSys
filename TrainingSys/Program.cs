using Microsoft.AspNetCore.Authentication.Cookies;
using TrainingSys.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme).AddCookie(
);

builder.Services.AddScoped<TrainMaster>();



var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
}
//else
//{
//    app.UseExceptionHandler("/Home/Error");
//    app.UseStatusCodePagesWithReExecute("/Home/StatusCode", "?code={0}");

//}
app.UseAuthentication();

app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
