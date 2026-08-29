using Microsoft.AspNetCore.Identity;

namespace FincaNova.Web.Domain;

/// <summary>
/// Usuario de la plataforma. Extiende <see cref="IdentityUser"/> con los datos
/// de personal de la finca (HU-45, HU-48, HU-49).
/// </summary>
public class ApplicationUser : IdentityUser
{
    public string Nombre { get; set; } = string.Empty;

    public string Apellidos { get; set; } = string.Empty;

    /// <summary>Permite inactivar el acceso sin borrar el usuario (HU-47, HU-50).</summary>
    public bool Activo { get; set; } = true;

    public DateTime FechaRegistro { get; set; } = DateTime.UtcNow;

    /// <summary>Finca a la que pertenece el usuario. En la v1 siempre es la finca única.</summary>
    public int? FincaId { get; set; }

    public Finca? Finca { get; set; }

    public string NombreCompleto => $"{Nombre} {Apellidos}".Trim();
}
