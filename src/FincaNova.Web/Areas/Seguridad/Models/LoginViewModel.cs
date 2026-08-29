using System.ComponentModel.DataAnnotations;

namespace FincaNova.Web.Areas.Seguridad.Models;

/// <summary>Datos del formulario de inicio de sesión (HU-42).</summary>
public class LoginViewModel
{
    [Required(ErrorMessage = "El correo electrónico es obligatorio.")]
    [EmailAddress(ErrorMessage = "El formato del correo no es válido.")]
    [Display(Name = "Correo electrónico")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "La contraseña es obligatoria.")]
    [DataType(DataType.Password)]
    [Display(Name = "Contraseña")]
    public string Password { get; set; } = string.Empty;

    [Display(Name = "Recordarme en este equipo")]
    public bool Recordarme { get; set; }

    public string? ReturnUrl { get; set; }
}
