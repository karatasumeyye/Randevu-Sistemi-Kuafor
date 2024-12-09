using Microsoft.EntityFrameworkCore;
using Randevu_Sistemi_Kuafor.Models;

var builder = WebApplication.CreateBuilder(args);


// Veritabaný baðlantýsýný
builder.Services.AddDbContext<SalonDbContext>(options =>
    options.UseNpgsql("Host=localhost;Port=5432;Database=Randevu_Sistemi_Kuafor_DB;Username=postgres;Password=1234"));







// Add services to the container.
builder.Services.AddControllersWithViews();

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
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
