using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using TunePhere.Models;
using TunePhere.Repository;
using TunePhere.Repository.EFRepository;
using TunePhere.Repository.IMPRepository;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

var Configuration = builder.Configuration;

// Đăng ký DbContext
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Cấu hình Identity
builder.Services.AddIdentity<AppUser, IdentityRole>(options => {
    options.SignIn.RequireConfirmedAccount = false;
    options.SignIn.RequireConfirmedEmail = false;
    options.Password.RequireDigit = true;
    options.Password.RequiredLength = 6;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireLowercase = false;
})
.AddEntityFrameworkStores<AppDbContext>()
.AddDefaultUI()
.AddDefaultTokenProviders();

// Cấu hình Cookie
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Identity/Account/Login";
    options.LogoutPath = "/Identity/Account/Logout";
    options.AccessDeniedPath = "/Identity/Account/AccessDenied";
    options.Cookie.Name = "TunePhereCookie";
    options.Cookie.HttpOnly = true;
    options.ExpireTimeSpan = TimeSpan.FromDays(30);
    options.SlidingExpiration = true;
});

// Đăng ký các Repository
builder.Services.AddScoped<ISongRepository, EFSongRepository>();
builder.Services.AddScoped<IPlaylistRepository, EFPlaylistRepository>();
builder.Services.AddScoped<IRemixRepository,EFRemixRepository>();
builder.Services.AddScoped<IUserRepository, EFUserRepository>();
builder.Services.AddScoped<IUserFavoriteSongs, EFUserFavoriteSongs>();
builder.Services.AddScoped<IListeningRoomRepository, EFListeningRoomRepository>();
builder.Services.AddScoped<IListeningRoomParticipantRepository, EFListeningRoomParticipantRepository>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

using (var scope = app.Services.CreateScope())
{
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();

    // Tạo các roles
    string[] roleNames = { "Administrator", "User" ,"Artists"};
    foreach (var roleName in roleNames)
    {
        if (!await roleManager.RoleExistsAsync(roleName))
        {
            await roleManager.CreateAsync(new IdentityRole(roleName));
        }
    }

    // Kiểm tra và tạo tài khoản admin
    var adminEmail = "TunePhereAdmin@gmail.com";
    var adminUser = await userManager.FindByEmailAsync(adminEmail);

    if (adminUser == null)
    {
        var admin = new AppUser
        {
            UserName = adminEmail,
            Email = adminEmail,
            FullName = "Administrator",
            EmailConfirmed = true
        };

        var result = await userManager.CreateAsync(admin, "adTunePhere@123");

        if (result.Succeeded)
        {
            await userManager.AddToRoleAsync(admin, "Administrator");
        }
    }
    else if (!await userManager.IsInRoleAsync(adminUser, "Administrator"))
    {
        await userManager.AddToRoleAsync(adminUser, "Administrator");
    }
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
