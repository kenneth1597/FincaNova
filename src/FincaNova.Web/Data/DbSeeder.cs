using FincaNova.Web.Domain;
using FincaNova.Web.Domain.Enfermedades;
using FincaNova.Web.Security;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace FincaNova.Web.Data;

/// <summary>
/// Datos iniciales del sistema: roles, finca de Café Chaperno con su
/// configuración, usuario administrador y catálogo base de enfermedades.
/// Es idempotente: se puede ejecutar en cada arranque sin duplicar datos.
/// </summary>
public static class DbSeeder
{
    public const string AdminEmail = "admin@fincanova.local";
    public const string AdminPassword = "FincaNova2026$";

    public static async Task SeedAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var sp = scope.ServiceProvider;

        var db = sp.GetRequiredService<AppDbContext>();
        var roleManager = sp.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = sp.GetRequiredService<UserManager<ApplicationUser>>();
        var logger = sp.GetRequiredService<ILoggerFactory>().CreateLogger("DbSeeder");

        await db.Database.MigrateAsync();

        // ----- Roles -----
        foreach (var rol in Roles.Todos)
        {
            if (!await roleManager.RoleExistsAsync(rol))
            {
                await roleManager.CreateAsync(new IdentityRole(rol));
                logger.LogInformation("Rol creado: {Rol}", rol);
            }
        }

        // ----- Finca única (v1) -----
        var finca = await db.Fincas.Include(f => f.Configuracion).FirstOrDefaultAsync();
        if (finca is null)
        {
            finca = new Finca
            {
                Nombre = "Café Chaperno",
                Ubicacion = "Poás, Alajuela, Costa Rica",
                Propietario = "Edy Alberto Salazar Quesada",
                Telefono = "83222672",
                Configuracion = new ConfiguracionFinca()
            };
            db.Fincas.Add(finca);
            await db.SaveChangesAsync();
            logger.LogInformation("Finca inicial creada: {Finca}", finca.Nombre);
        }

        // ----- Usuario administrador -----
        var admin = await userManager.FindByEmailAsync(AdminEmail);
        if (admin is null)
        {
            admin = new ApplicationUser
            {
                UserName = AdminEmail,
                Email = AdminEmail,
                EmailConfirmed = true,
                Nombre = "Administrador",
                Apellidos = "FincaNova",
                Activo = true,
                FincaId = finca.Id
            };
            var result = await userManager.CreateAsync(admin, AdminPassword);
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(admin, Roles.Administrador);
                logger.LogInformation("Usuario administrador creado: {Email}", AdminEmail);
            }
            else
            {
                logger.LogError("No se pudo crear el administrador: {Errores}",
                    string.Join("; ", result.Errors.Select(e => e.Description)));
            }
        }

        // ----- Catálogo base de enfermedades y plagas del café -----
        if (!await db.TiposEnfermedad.AnyAsync())
        {
            db.TiposEnfermedad.AddRange(
                new TipoEnfermedad { Nombre = "Roya del café", Descripcion = "Hemileia vastatrix. Manchas amarillas/anaranjadas en el envés de la hoja." },
                new TipoEnfermedad { Nombre = "Broca del café", Descripcion = "Hypothenemus hampei. Perforación del fruto." },
                new TipoEnfermedad { Nombre = "Ojo de gallo", Descripcion = "Mycena citricolor. Lesiones circulares en hojas y frutos." },
                new TipoEnfermedad { Nombre = "Antracnosis", Descripcion = "Colletotrichum spp. Necrosis en frutos y ramas." },
                new TipoEnfermedad { Nombre = "Mancha de hierro", Descripcion = "Cercospora coffeicola. Manchas pardas con halo amarillo." },
                new TipoEnfermedad { Nombre = "Nematodos", Descripcion = "Meloidogyne spp. Daño radicular y clorosis." });
            await db.SaveChangesAsync();
            logger.LogInformation("Catálogo de enfermedades inicial creado.");
        }
    }
}
