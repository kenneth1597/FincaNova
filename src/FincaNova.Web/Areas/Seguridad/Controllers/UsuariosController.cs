using FincaNova.Web.Areas.Seguridad.Models;
using FincaNova.Web.Data;
using FincaNova.Web.Domain;
using FincaNova.Web.Security;
using FincaNova.Web.Services.Auditoria;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FincaNova.Web.Areas.Seguridad.Controllers;

/// <summary>
/// Gestión de usuarios y asignación de roles. Solo el Administrador puede
/// acceder (RF-03, RF-04, RF-05, HU-45 a HU-50).
/// </summary>
[Area("Seguridad")]
[Authorize(Roles = Roles.Administrador)]
public class UsuariosController : Controller
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly AppDbContext _db;
    private readonly IAuditService _audit;
    private readonly ILogger<UsuariosController> _logger;

    public UsuariosController(
        UserManager<ApplicationUser> userManager,
        AppDbContext db,
        IAuditService audit,
        ILogger<UsuariosController> logger)
    {
        _userManager = userManager;
        _db = db;
        _audit = audit;
        _logger = logger;
    }

    // ---------- Listado ----------
    [HttpGet]
    public async Task<IActionResult> Index(string? q, string? rol, string? estado)
    {
        var usuarios = await _userManager.Users.AsNoTracking().OrderBy(u => u.Nombre).ToListAsync();

        var filas = new List<UsuarioListItemViewModel>();
        foreach (var u in usuarios)
        {
            var roles = await _userManager.GetRolesAsync(u);
            filas.Add(new UsuarioListItemViewModel
            {
                Id = u.Id,
                NombreCompleto = u.NombreCompleto,
                Email = u.Email ?? "",
                Telefono = u.PhoneNumber,
                Rol = roles.FirstOrDefault() ?? "(sin rol)",
                Activo = u.Activo,
                Bloqueado = u.LockoutEnd is { } fin && fin > DateTimeOffset.UtcNow,
                FechaRegistro = u.FechaRegistro
            });
        }

        if (!string.IsNullOrWhiteSpace(q))
        {
            var term = q.Trim();
            filas = filas.Where(f =>
                f.NombreCompleto.Contains(term, StringComparison.OrdinalIgnoreCase) ||
                f.Email.Contains(term, StringComparison.OrdinalIgnoreCase)).ToList();
        }
        if (!string.IsNullOrWhiteSpace(rol))
            filas = filas.Where(f => f.Rol == rol).ToList();
        if (estado == "activos")
            filas = filas.Where(f => f.Activo).ToList();
        else if (estado == "inactivos")
            filas = filas.Where(f => !f.Activo).ToList();

        ViewBag.Query = q;
        ViewBag.Rol = rol;
        ViewBag.Estado = estado;
        ViewBag.Roles = Roles.Todos;
        return View(filas);
    }

    // ---------- Crear ----------
    [HttpGet]
    public IActionResult Crear() => View(new CrearUsuarioViewModel { RolesDisponibles = Roles.Todos });

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Crear(CrearUsuarioViewModel model)
    {
        model.RolesDisponibles = Roles.Todos;

        if (!Roles.Todos.Contains(model.Rol))
            ModelState.AddModelError(nameof(model.Rol), "Debe asignar un rol válido (Administrador, Caficultor o Trabajador).");

        if (await _userManager.FindByEmailAsync(model.Email) is not null)
            ModelState.AddModelError(nameof(model.Email), "El correo electrónico ingresado ya se encuentra vinculado a una cuenta.");

        if (!ModelState.IsValid)
            return View(model);

        var fincaId = await _db.Fincas.Select(f => (int?)f.Id).FirstOrDefaultAsync();

        var usuario = new ApplicationUser
        {
            UserName = model.Email.Trim(),
            Email = model.Email.Trim(),
            EmailConfirmed = true,
            Nombre = model.Nombre.Trim(),
            Apellidos = model.Apellidos.Trim(),
            PhoneNumber = model.Telefono?.Trim(),
            Activo = true,
            FincaId = fincaId
        };

        var creado = await _userManager.CreateAsync(usuario, model.Password);
        if (!creado.Succeeded)
        {
            AgregarErrores(creado);
            return View(model);
        }

        await _userManager.AddToRoleAsync(usuario, model.Rol);
        await _audit.RegistrarAsync(AccionAuditoria.Crear, nameof(ApplicationUser), usuario.Id,
            $"Alta de usuario {usuario.Email} con rol {model.Rol}",
            valorNuevo: new { usuario.Email, usuario.Nombre, usuario.Apellidos, Rol = model.Rol });

        _logger.LogInformation("Usuario creado: {Email} ({Rol})", usuario.Email, model.Rol);
        TempData["Ok"] = $"Usuario «{usuario.NombreCompleto}» creado correctamente.";
        return RedirectToAction(nameof(Index));
    }

    // ---------- Editar ----------
    [HttpGet]
    public async Task<IActionResult> Editar(string id)
    {
        var usuario = await _userManager.FindByIdAsync(id);
        if (usuario is null) return NotFound();

        var rolActual = (await _userManager.GetRolesAsync(usuario)).FirstOrDefault() ?? "";

        return View(new EditarUsuarioViewModel
        {
            Id = usuario.Id,
            Email = usuario.Email ?? "",
            Nombre = usuario.Nombre,
            Apellidos = usuario.Apellidos,
            Telefono = usuario.PhoneNumber,
            Rol = rolActual,
            RolOriginal = rolActual,
            Activo = usuario.Activo,
            EsUsuarioActual = usuario.Id == _userManager.GetUserId(User),
            RolesDisponibles = Roles.Todos
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Editar(EditarUsuarioViewModel model)
    {
        model.RolesDisponibles = Roles.Todos;

        var usuario = await _userManager.FindByIdAsync(model.Id);
        if (usuario is null) return NotFound();

        var rolActual = (await _userManager.GetRolesAsync(usuario)).FirstOrDefault() ?? "";
        model.Email = usuario.Email ?? "";
        model.RolOriginal = rolActual;
        model.Activo = usuario.Activo;
        model.EsUsuarioActual = usuario.Id == _userManager.GetUserId(User);

        if (!Roles.Todos.Contains(model.Rol))
            ModelState.AddModelError(nameof(model.Rol), "Debe asignar un rol válido.");

        var cambiaRol = !string.Equals(model.Rol, rolActual, StringComparison.Ordinal);

        if (cambiaRol && model.EsUsuarioActual)
            ModelState.AddModelError(nameof(model.Rol), "No puede cambiar su propio rol.");

        // Escalar a Administrador exige la contraseña del administrador actual (HU-49 esc. 2).
        var escalaAAdmin = cambiaRol && model.Rol == Roles.Administrador && rolActual != Roles.Administrador;
        if (escalaAAdmin)
        {
            var actual = await _userManager.GetUserAsync(User);
            if (actual is null || string.IsNullOrEmpty(model.PasswordConfirmacion) ||
                !await _userManager.CheckPasswordAsync(actual, model.PasswordConfirmacion))
            {
                ModelState.AddModelError(nameof(model.PasswordConfirmacion),
                    "Debe confirmar su contraseña para otorgar permisos de Administrador.");
            }
        }

        // No dejar la finca sin ningún administrador activo.
        if (cambiaRol && rolActual == Roles.Administrador && model.Rol != Roles.Administrador)
        {
            if (await ContarAdministradoresActivos(exceptoId: usuario.Id) == 0)
                ModelState.AddModelError(nameof(model.Rol), "Debe existir al menos un Administrador activo en el sistema.");
        }

        if (!ModelState.IsValid)
            return View(model);

        var antes = new { usuario.Nombre, usuario.Apellidos, usuario.PhoneNumber, Rol = rolActual };

        usuario.Nombre = model.Nombre.Trim();
        usuario.Apellidos = model.Apellidos.Trim();
        usuario.PhoneNumber = model.Telefono?.Trim();

        var actualizado = await _userManager.UpdateAsync(usuario);
        if (!actualizado.Succeeded)
        {
            AgregarErrores(actualizado);
            return View(model);
        }

        if (cambiaRol)
        {
            if (!string.IsNullOrEmpty(rolActual))
                await _userManager.RemoveFromRoleAsync(usuario, rolActual);
            await _userManager.AddToRoleAsync(usuario, model.Rol);
            await _userManager.UpdateSecurityStampAsync(usuario); // fuerza refrescar permisos
        }

        await _audit.RegistrarAsync(AccionAuditoria.Modificar, nameof(ApplicationUser), usuario.Id,
            $"Edición de usuario {usuario.Email}",
            valorAnterior: antes,
            valorNuevo: new { usuario.Nombre, usuario.Apellidos, usuario.PhoneNumber, Rol = model.Rol });

        TempData["Ok"] = $"Usuario «{usuario.NombreCompleto}» actualizado.";
        return RedirectToAction(nameof(Index));
    }

    // ---------- Activar / inactivar (HU-47, HU-50) ----------
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CambiarEstado(string id)
    {
        var usuario = await _userManager.FindByIdAsync(id);
        if (usuario is null) return NotFound();

        if (usuario.Id == _userManager.GetUserId(User))
        {
            TempData["Error"] = "No puede inactivar su propia cuenta.";
            return RedirectToAction(nameof(Index));
        }

        var rol = (await _userManager.GetRolesAsync(usuario)).FirstOrDefault();
        if (usuario.Activo && rol == Roles.Administrador &&
            await ContarAdministradoresActivos(exceptoId: usuario.Id) == 0)
        {
            TempData["Error"] = "Debe existir al menos un Administrador activo en el sistema.";
            return RedirectToAction(nameof(Index));
        }

        usuario.Activo = !usuario.Activo;
        await _userManager.UpdateAsync(usuario);
        await _userManager.UpdateSecurityStampAsync(usuario); // invalida sesiones abiertas

        var accion = usuario.Activo ? AccionAuditoria.Activar : AccionAuditoria.Inactivar;
        await _audit.RegistrarAsync(accion, nameof(ApplicationUser), usuario.Id,
            $"{(usuario.Activo ? "Reactivación" : "Inactivación")} de usuario {usuario.Email}");

        TempData["Ok"] = usuario.Activo
            ? $"Se reactivó el acceso de «{usuario.NombreCompleto}»."
            : $"Se revocó el acceso de «{usuario.NombreCompleto}».";
        return RedirectToAction(nameof(Index));
    }

    // ---------- Desbloquear tras intentos fallidos (HU-42 esc. 4) ----------
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Desbloquear(string id)
    {
        var usuario = await _userManager.FindByIdAsync(id);
        if (usuario is null) return NotFound();

        await _userManager.SetLockoutEndDateAsync(usuario, null);
        await _userManager.ResetAccessFailedCountAsync(usuario);
        await _audit.RegistrarAsync(AccionAuditoria.Modificar, nameof(ApplicationUser), usuario.Id,
            $"Desbloqueo manual de la cuenta {usuario.Email}");

        TempData["Ok"] = $"La cuenta de «{usuario.NombreCompleto}» fue desbloqueada.";
        return RedirectToAction(nameof(Index));
    }

    // ---------- helpers ----------
    private async Task<int> ContarAdministradoresActivos(string exceptoId)
    {
        var admins = await _userManager.GetUsersInRoleAsync(Roles.Administrador);
        return admins.Count(a => a.Activo && a.Id != exceptoId);
    }

    private void AgregarErrores(IdentityResult resultado)
    {
        foreach (var error in resultado.Errors)
            ModelState.AddModelError(string.Empty, Traducir(error));
    }

    private static string Traducir(IdentityError e) => e.Code switch
    {
        "DuplicateUserName" or "DuplicateEmail" => "El correo electrónico ya se encuentra registrado.",
        "PasswordTooShort" => "La contraseña es demasiado corta (mínimo 8 caracteres).",
        "PasswordRequiresDigit" => "La contraseña debe incluir al menos un número.",
        "PasswordRequiresUpper" => "La contraseña debe incluir al menos una letra mayúscula.",
        "PasswordRequiresLower" => "La contraseña debe incluir al menos una letra minúscula.",
        _ => e.Description
    };
}
