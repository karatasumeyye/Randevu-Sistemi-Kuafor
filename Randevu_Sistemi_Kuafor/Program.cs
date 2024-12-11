using Microsoft.EntityFrameworkCore;
using Randevu_Sistemi_Kuafor.Models;

var builder = WebApplication.CreateBuilder(args);


// Veritabaný baðlantýsýný
builder.Services.AddDbContext<SalonDbContext>(options =>
    options.UseNpgsql("Host=localhost;Port=5432;Database=Randevu_Sistemi_Kuafor_DB;Username=postgres;Password=1234"));



// Session desteði ekleniyor
builder.Services.AddSession(options =>
{
    options.Cookie.HttpOnly = true; // Güvenlik için cookie yalnýzca HTTP üzerinden eriþilebilir.
    options.Cookie.IsEssential = true; // Cookie'nin kritik olduðunu belirtir.
    options.IdleTimeout = TimeSpan.FromMinutes(30); // Oturum süresinin zaman aþýmý.
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

app.UseSession();  //oturumlarý kullanabilmek için 
app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
