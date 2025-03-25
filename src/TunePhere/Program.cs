using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using TunePhere.Models;
using TunePhere.Repository;
using TunePhere.Repository.EFRepository;
using TunePhere.Repository.IMPRepository;
using Microsoft.AspNetCore.Identity;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
var Configuration = builder.Configuration;

// Đăng ký DbContext
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddDefaultIdentity<AppUser>(options => {
    options.SignIn.RequireConfirmedAccount = false; // Không yêu cầu xác nhận email
    options.SignIn.RequireConfirmedEmail = false; // Không yêu cầu xác nhận email
    options.Password.RequireDigit = true;
    options.Password.RequiredLength = 6;
    options.Password.RequireNonAlphanumeric = false; // Không yêu cầu ký tự đặc biệt
    options.Password.RequireUppercase = false; // Không yêu cầu chữ hoa
    options.Password.RequireLowercase = false; // Không yêu cầu chữ thường
}).AddEntityFrameworkStores<AppDbContext>();

// Đăng ký các Repository
builder.Services.AddScoped<ISongRepository, EFSongRepository>();
builder.Services.AddScoped<IPlaylistRepository, EFPlaylistRepository>();
builder.Services.AddScoped<IRemixRepository,EFRemixRepository>();
builder.Services.AddScoped<IUserRepository, EFUserRepository>();

builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

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

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapRazorPages();

app.Run();
