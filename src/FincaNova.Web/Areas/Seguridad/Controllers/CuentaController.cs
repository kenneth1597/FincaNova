using FincaNova.Web.Areas.Seguridad.Models;
using FincaNova.Web.Domain;
using FincaNova.Web.Services.Auditoria;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace FincaNova.Web.Areas.Seguridad.Controllers;

/// <summary>
/// Inicio y cierre de sesión de la plataforma (RF-01, RF-02, HU-42, HU-44).
/// </summary>
[Area("Seguridad")]
public class CuentaController : Controller
{
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IAuditService _audit;
    private readonly ILogger<CuentaController> _logger;

    public CuentaController(
        SignInManager<ApplicationUser> signInManager,
        UserManager<ApplicationUser> userManager,
        IAuditService audit,
        ILogger<CuentaController> logger)
    {
        _signInManager = signInManager;
        _userManager = userManager;
        _audit = audit;
        _logger = logger;
    }

    [HttpGet]
    [AllowAnonymous]
    public IActionResult Login(string? returnUrl = null)
    {
        if (User.Identity?.IsAuthenticated == true)
            return RedirectToDashboard();

        return View(new LoginViewModel { ReturnUrl = returnUrl });
    }

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        var user = await _userManager.FindByEmailAsync(model.Email);
        if (user is null)
        {
            ModelState.AddModelError(string.Empty, "Credenciales incorrectas.");
            return View(model);
        }

        if (!user.Activo)
        {
            ModelState.AddModelError(string.Empty, "La cuenta se encuentra deshabilitada. Contacte al administrador.");
            return View(model);
        }

        var result = await _signInManager.PasswordSignInAsync(
            user.UserName!, model.Password, model.Recordarme, lockoutOnFailure: true);

        if (result.Succeeded)
        {
            await _audit.RegistrarAsync(AccionAuditoria.InicioSesion, nameof(ApplicationUser), user.Id,
                $"Inicio de sesión de {user.Email}");
            _logger.LogInformation("Inicio de sesión: {Email}", user.Email);
            return RedirectToLocal(model.ReturnUrl);
        }

        if (result.IsLockedOut)
        {
            ModelState.AddModelError(string.Empty,
                "La cuenta fue bloqueada temporalmente por 15 minutos debido a varios intentos fallidos.");
            return View(model);
        }

        ModelState.AddModelError(string.Empty, "Credenciales incorrectas.");
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        var userId = _userManager.GetUserId(User);
        await _signInManager.SignOutAsync();
        await _audit.RegistrarAsync(AccionAuditoria.CierreSesion, nameof(ApplicationUser), userId, "Cierre de sesión");
        return RedirectToAction(nameof(Login));
    }

    [HttpGet]
    public IActionResult AccesoDenegado() => View();

    private IActionResult RedirectToLocal(string? returnUrl)
        => !string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl)
            ? Redirect(returnUrl)
            : RedirectToDashboard();

    private IActionResult RedirectToDashboard()
        => RedirectToAction("Index", "Home", new { area = "" });
}
