using Microsoft.AspNetCore.Components.Routing;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Randevu_Sistemi_Kuafor.Models;

var builder = WebApplication.CreateBuilder(args);


// Veritabaný baðlantýsýný
builder.Services.AddDbContext<SalonDbContext>(options =>
    options.UseNpgsql("Host=localhost;Port=5432;Database=Randevu_Sistemi_Kuafor_DB;Username=postgres;Password=1234"));


builder.Services.AddIdentity<ApplicationUser, ApplicationRole>()
    .AddEntityFrameworkStores<SalonDbContext>()
     .AddDefaultTokenProviders();

builder.Services.AddControllersWithViews();


var app = builder.Build();

// Roller yoksa, onlarý oluþtur
// Roller veritabanýnda var mý, kontrol et




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

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers(); // Controller'larý haritalama

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();



