using aznews.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// DbContext
var connection = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<DataContext>(options => options.UseSqlServer(connection));

// MVC + (dev) runtime compilation + auto anti-forgery cho POST (tùy bật)
builder.Services
    .AddControllersWithViews(/*opts => opts.Filters.Add(new AutoValidateAntiforgeryTokenAttribute())*/ )
#if DEBUG
    .AddRazorRuntimeCompilation()
#endif
    ;

// Session/TempData (nếu cần)
builder.Services.AddSession(o =>
{
    o.IdleTimeout = TimeSpan.FromMinutes(30);
    o.Cookie.HttpOnly = true;
    o.Cookie.IsEssential = true;
});

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseSession();
// app.UseAuthentication(); // khi bạn thêm đăng nhập
app.UseAuthorization();

// Areas trước, default sau
app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
