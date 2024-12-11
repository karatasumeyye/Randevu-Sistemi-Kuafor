using Microsoft.EntityFrameworkCore;
using Randevu_Sistemi_Kuafor.Models;

var builder = WebApplication.CreateBuilder(args);


// Veritaban� ba�lant�s�n�
builder.Services.AddDbContext<SalonDbContext>(options =>
    options.UseNpgsql("Host=localhost;Port=5432;Database=Randevu_Sistemi_Kuafor_DB;Username=postgres;Password=1234"));



// Session deste�i ekleniyor
builder.Services.AddSession(options =>
{
    options.Cookie.HttpOnly = true; // G�venlik i�in cookie yaln�zca HTTP �zerinden eri�ilebilir.
    options.Cookie.IsEssential = true; // Cookie'nin kritik oldu�unu belirtir.
    options.IdleTimeout = TimeSpan.FromMinutes(30); // Oturum s�resinin zaman a��m�.
});
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

app.UseSession();  //oturumlar� kullanabilmek i�in 
app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
