using System.ComponentModel.DataAnnotations;
using FincaNova.Web.Domain.Common;

namespace FincaNova.Web.Domain.Labores;

/// <summary>
/// Persona que ejecuta labores agrícolas o recolección. No necesariamente es un
/// usuario del sistema (RF-14, HU-04, HU-05).
/// </summary>
public class Colaborador : AuditableEntity
{
    [Required, StringLength(120)]
    public string Nombre { get; set; } = string.Empty;

    /// <summary>Cédula u otra identificación. Única: no se permiten duplicados (HU-04).</summary>
    [Required, StringLength(30)]
    public string Identificacion { get; set; } = string.Empty;

    [StringLength(30)]
    public string? Telefono { get; set; }

    [StringLength(60)]
    public string? Cargo { get; set; }

    [Range(0, 10_000_000)]
    public decimal TarifaJornada { get; set; }

    [Range(0, 1_000_000)]
    public decimal TarifaHora { get; set; }

    /// <summary>Pago por cajuela recolectada, usado en el cálculo de planilla de cosecha (HU-06).</summary>
    [Range(0, 1_000_000)]
    public decimal TarifaCajuela { get; set; }

    public EstadoColaborador Estado { get; set; } = EstadoColaborador.Activo;

    public ICollection<LaborColaborador> Labores { get; set; } = new List<LaborColaborador>();
}
