using FincaNova.Web.Data;
using FincaNova.Web.Domain;
using FincaNova.Web.Services.Auditoria;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// ----- Base de datos (RQNF-002: SQL Server) -----
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("No se configuró la cadena de conexión 'DefaultConnection'.");

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(connectionString, sql => sql.EnableRetryOnFailure()));

builder.Services.AddHttpContextAccessor();

// ----- Identity: autenticación, roles y políticas de seguridad (RQNF-007, HU-42) -----
builder.Services
    .AddIdentity<ApplicationUser, IdentityRole>(options =>
    {
        options.SignIn.RequireConfirmedAccount = false;

        options.Password.RequiredLength = 8;
        options.Password.RequireNonAlphanumeric = false;
        options.Password.RequireDigit = true;
        options.Password.RequireUppercase = true;
        options.Password.RequireLowercase = true;

        // Bloqueo temporal tras 5 intentos fallidos por 15 minutos (HU-42 esc. 4).
        options.Lockout.MaxFailedAccessAttempts = 5;
        options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
        options.Lockout.AllowedForNewUsers = true;

        options.User.RequireUniqueEmail = true;
    })
    .AddEntityFrameworkStores<AppDbContext>()
    .AddDefaultTokenProviders();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Seguridad/Cuenta/Login";
    options.LogoutPath = "/Seguridad/Cuenta/Logout";
    options.AccessDeniedPath = "/Seguridad/Cuenta/AccesoDenegado";

    // Cierre de sesión automático tras 20 minutos de inactividad (HU-42 esc. 5).
    options.ExpireTimeSpan = TimeSpan.FromMinutes(20);
    options.SlidingExpiration = true;
    options.Cookie.HttpOnly = true;
    options.Cookie.SameSite = SameSiteMode.Lax;
});

// ----- MVC -----
builder.Services.AddControllersWithViews();

// Por defecto todo el sitio exige autenticación; las acciones públicas usan [AllowAnonymous].
builder.Services.AddAuthorization(options =>
{
    options.FallbackPolicy = new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build();
});

// ----- Servicios de la aplicación -----
builder.Services.AddScoped<IAuditService, AuditService>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// ----- Datos iniciales (roles, finca, administrador, catálogos) -----
await DbSeeder.SeedAsync(app.Services);

app.Run();
