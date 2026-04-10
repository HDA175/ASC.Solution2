using ASC.DataAccess;
using ASC.Solution.Services;
using ASC.Web.Configuration;
using ASC.Web.Data;
using ASC.Web.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using ASC.DataAccess;
var builder = WebApplication.CreateBuilder(args);

// ================== DB ==================
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ??
    throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

// ================== Identity ==================
builder.Services.AddIdentity<IdentityUser, IdentityRole>(options =>
{
    options.User.RequireUniqueEmail = true;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

// ================== AppSettings ==================
builder.Services.Configure<ApplicationSettings>(
    builder.Configuration.GetSection("AppSettings"));

// ================== Services ==================
builder.Services.AddTransient<IEmailSender, AuthMessageSender>();
builder.Services.AddTransient<ISmsSender, AuthMessageSender>();

builder.Services.AddSingleton<IIdentitySeed, IdentitySeed>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages(); // 🔥 thêm dòng này

var app = builder.Build();

// ================== Middleware ==================
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication(); // 🔥 QUAN TRỌNG (thiếu cái này là lỗi login)
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapRazorPages();

// ================== Seed Data ==================
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;

    var seed = services.GetRequiredService<IIdentitySeed>();

    await seed.Seed(
        services.GetRequiredService<UserManager<IdentityUser>>(),
        services.GetRequiredService<RoleManager<IdentityRole>>(),
        services.GetRequiredService<IOptions<ApplicationSettings>>()
    );
}

app.Run();