using System.ComponentModel.DataAnnotations;
using FincaNova.Web.Domain.Common;

namespace FincaNova.Web.Domain.Lotes;

/// <summary>
/// Temporada de cosecha (normalmente noviembre–enero). Organiza la información de
/// producción y finanzas por período. Solo uno puede estar activo y no se
/// permiten solapes de fechas (RF-09, HU-40).
/// </summary>
public class PeriodoProductivo : AuditableEntity
{
    public int FincaId { get; set; }

    public Finca Finca { get; set; } = null!;

    [Required, StringLength(80)]
    public string Nombre { get; set; } = string.Empty;

    [DataType(DataType.Date)]
    public DateTime FechaInicio { get; set; }

    [DataType(DataType.Date)]
    public DateTime FechaFin { get; set; }

    public bool Activo { get; set; }

    [StringLength(400)]
    public string? Observaciones { get; set; }
}
