using Infraestructura.Data;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Dominio.Entidades;
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllersWithViews();
builder.Services.AddDbContext<PuntoHalconDbContext>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddScoped<PasswordHasher<Usuario>>();
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme).AddCookie(options => { options.LoginPath = "/Acceso/Login"; options.AccessDeniedPath = "/Acceso/Login"; options.ExpireTimeSpan = TimeSpan.FromHours(8); options.SlidingExpiration = true; });
builder.Services.AddAuthorization();
var app = builder.Build();
if (!app.Environment.IsDevelopment()) { app.UseExceptionHandler("/Home/Error"); app.UseHsts(); }
app.UseHttpsRedirection(); app.UseStaticFiles(); app.UseRouting(); app.UseAuthentication(); app.UseAuthorization();
app.MapControllerRoute(name: "default", pattern: "{controller=Acceso}/{action=Login}/{id?}");
using (var scope = app.Services.CreateScope()) { var db = scope.ServiceProvider.GetRequiredService<PuntoHalconDbContext>(); db.Database.EnsureCreated(); }
app.Run();
