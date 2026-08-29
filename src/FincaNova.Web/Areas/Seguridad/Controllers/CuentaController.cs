using System.Text;
using FincaNova.Web.Areas.Seguridad.Models;
using FincaNova.Web.Domain;
using FincaNova.Web.Services.Auditoria;
using FincaNova.Web.Services.Email;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;

namespace FincaNova.Web.Areas.Seguridad.Controllers;

/// <summary>
/// Inicio y cierre de sesión y recuperación de contraseña
/// (RF-01, RF-02, HU-42, HU-43, HU-44).
/// </summary>
[Area("Seguridad")]
public class CuentaController : Controller
{
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IAuditService _audit;
    private readonly IEmailSender _email;
    private readonly ILogger<CuentaController> _logger;

    public CuentaController(
        SignInManager<ApplicationUser> signInManager,
        UserManager<ApplicationUser> userManager,
        IAuditService audit,
        IEmailSender email,
        ILogger<CuentaController> logger)
    {
        _signInManager = signInManager;
        _userManager = userManager;
        _audit = audit;
        _email = email;
        _logger = logger;
    }

    // ---------- Inicio de sesión ----------
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

    // ---------- Recuperación de contraseña (HU-43) ----------
    [HttpGet]
    [AllowAnonymous]
    public IActionResult EsqueciMiContrasena() => View(new EsqueciMiContrasenaViewModel());

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EsqueciMiContrasena(EsqueciMiContrasenaViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        var user = await _userManager.FindByEmailAsync(model.Email);
        if (user is not null && user.Activo)
        {
            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var tokenCodificado = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));
            var enlace = Url.Action(nameof(RestablecerContrasena), "Cuenta",
                new { area = "Seguridad", email = user.Email, token = tokenCodificado },
                protocol: Request.Scheme)!;

            await _email.EnviarAsync(user.Email!, "FincaNova · Restablecer contraseña", $"""
                <p>Hola {user.Nombre},</p>
                <p>Recibimos una solicitud para restablecer la contraseña de su cuenta en FincaNova.</p>
                <p><a href="{enlace}">Haga clic aquí para definir una nueva contraseña</a></p>
                <p>Este enlace vence en 30 minutos. Si usted no realizó esta solicitud, ignore este mensaje.</p>
                """);

            await _audit.RegistrarAsync(AccionAuditoria.Modificar, nameof(ApplicationUser), user.Id,
                $"Solicitud de restablecimiento de contraseña para {user.Email}");
            _logger.LogInformation("Enlace de restablecimiento generado para {Email}", user.Email);
        }

        // Respuesta genérica: no se revela si el correo existe o no (HU-43 esc. 2).
        return RedirectToAction(nameof(EsqueciMiContrasenaConfirmacion));
    }

    [HttpGet]
    [AllowAnonymous]
    public IActionResult EsqueciMiContrasenaConfirmacion() => View();

    [HttpGet]
    [AllowAnonymous]
    public IActionResult RestablecerContrasena(string? token = null, string? email = null)
    {
        if (string.IsNullOrEmpty(token) || string.IsNullOrEmpty(email))
        {
            TempData["Error"] = "El enlace de restablecimiento no es válido.";
            return RedirectToAction(nameof(Login));
        }

        return View(new RestablecerContrasenaViewModel { Token = token, Email = email });
    }

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RestablecerContrasena(RestablecerContrasenaViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        var user = await _userManager.FindByEmailAsync(model.Email);
        if (user is null)
        {
            // No se revela la inexistencia de la cuenta.
            return RedirectToAction(nameof(RestablecerContrasenaConfirmacion));
        }

        string token;
        try
        {
            token = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(model.Token));
        }
        catch (FormatException)
        {
            ModelState.AddModelError(string.Empty, "El enlace de restablecimiento no es válido o ya expiró.");
            return View(model);
        }

        var resultado = await _userManager.ResetPasswordAsync(user, token, model.Password);
        if (!resultado.Succeeded)
        {
            ModelState.AddModelError(string.Empty,
                "El enlace de restablecimiento no es válido o expiró. Solicite uno nuevo.");
            return View(model);
        }

        await _userManager.SetLockoutEndDateAsync(user, null);
        await _userManager.ResetAccessFailedCountAsync(user);
        await _userManager.UpdateSecurityStampAsync(user);

        await _audit.RegistrarAsync(AccionAuditoria.Modificar, nameof(ApplicationUser), user.Id,
            $"Restablecimiento de contraseña completado para {user.Email}");

        return RedirectToAction(nameof(RestablecerContrasenaConfirmacion));
    }

    [HttpGet]
    [AllowAnonymous]
    public IActionResult RestablecerContrasenaConfirmacion() => View();

    // ---------- helpers ----------
    private IActionResult RedirectToLocal(string? returnUrl)
        => !string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl)
            ? Redirect(returnUrl)
            : RedirectToDashboard();

    private IActionResult RedirectToDashboard()
        => RedirectToAction("Index", "Home", new { area = "" });
}
