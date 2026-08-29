using System.ComponentModel.DataAnnotations;

namespace FincaNova.Web.Areas.Seguridad.Models;

/// <summary>Fila del listado de usuarios (HU-45..50).</summary>
public class UsuarioListItemViewModel
{
    public string Id { get; set; } = string.Empty;
    public string NombreCompleto { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Telefono { get; set; }
    public string Rol { get; set; } = string.Empty;
    public bool Activo { get; set; }
    public bool Bloqueado { get; set; }
    public DateTime FechaRegistro { get; set; }
}

/// <summary>Formulario de alta de usuario (HU-45, HU-48, RF-03).</summary>
public class CrearUsuarioViewModel
{
    [Required(ErrorMessage = "El nombre es obligatorio.")]
    [StringLength(120)]
    [Display(Name = "Nombre")]
    public string Nombre { get; set; } = string.Empty;

    [Required(ErrorMessage = "Los apellidos son obligatorios.")]
    [StringLength(120)]
    [Display(Name = "Apellidos")]
    public string Apellidos { get; set; } = string.Empty;

    [Required(ErrorMessage = "El correo electrónico es obligatorio.")]
    [EmailAddress(ErrorMessage = "El formato del correo no es válido.")]
    [Display(Name = "Correo electrónico")]
    public string Email { get; set; } = string.Empty;

    [Phone(ErrorMessage = "El teléfono no es válido.")]
    [StringLength(30)]
    [Display(Name = "Teléfono")]
    public string? Telefono { get; set; }

    [Required(ErrorMessage = "Debe asignar un rol.")]
    [Display(Name = "Rol")]
    public string Rol { get; set; } = string.Empty;

    [Required(ErrorMessage = "La contraseña es obligatoria.")]
    [StringLength(100, MinimumLength = 8, ErrorMessage = "La contraseña debe tener al menos 8 caracteres.")]
    [DataType(DataType.Password)]
    [Display(Name = "Contraseña inicial")]
    public string Password { get; set; } = string.Empty;

    [DataType(DataType.Password)]
    [Compare(nameof(Password), ErrorMessage = "Las contraseñas no coinciden.")]
    [Display(Name = "Confirmar contraseña")]
    public string ConfirmarPassword { get; set; } = string.Empty;

    public IEnumerable<string> RolesDisponibles { get; set; } = Enumerable.Empty<string>();
}

/// <summary>Formulario de edición de datos de contacto y rol (HU-49).</summary>
public class EditarUsuarioViewModel
{
    public string Id { get; set; } = string.Empty;

    [Display(Name = "Correo electrónico")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "El nombre es obligatorio.")]
    [StringLength(120)]
    [Display(Name = "Nombre")]
    public string Nombre { get; set; } = string.Empty;

    [Required(ErrorMessage = "Los apellidos son obligatorios.")]
    [StringLength(120)]
    [Display(Name = "Apellidos")]
    public string Apellidos { get; set; } = string.Empty;

    [Phone(ErrorMessage = "El teléfono no es válido.")]
    [StringLength(30)]
    [Display(Name = "Teléfono")]
    public string? Telefono { get; set; }

    [Required(ErrorMessage = "Debe asignar un rol.")]
    [Display(Name = "Rol")]
    public string Rol { get; set; } = string.Empty;

    /// <summary>
    /// Requerida solo cuando la edición eleva el rol a Administrador: el sistema
    /// exige la contraseña del administrador actual antes de aplicar el cambio (HU-49 esc. 2).
    /// </summary>
    [DataType(DataType.Password)]
    [Display(Name = "Su contraseña (confirmación de seguridad)")]
    public string? PasswordConfirmacion { get; set; }

    public bool Activo { get; set; }
    public bool EsUsuarioActual { get; set; }
    public string RolOriginal { get; set; } = string.Empty;
    public IEnumerable<string> RolesDisponibles { get; set; } = Enumerable.Empty<string>();
}
