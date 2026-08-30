using System.ComponentModel.DataAnnotations;
using FincaNova.Web.Domain.Common;
using FincaNova.Web.Domain.Lotes;

namespace FincaNova.Web.Domain.Enfermedades;

/// <summary>Catálogo de enfermedades y plagas del café (roya, broca, ojo de gallo…).</summary>
public class TipoEnfermedad : AuditableEntity
{
    [Required, StringLength(120)]
    public string Nombre { get; set; } = string.Empty;

    [StringLength(400)]
    public string? Descripcion { get; set; }

    public bool Activo { get; set; } = true;

    public ICollection<RegistroEnfermedad> Registros { get; set; } = new List<RegistroEnfermedad>();
}

/// <summary>
/// Detección de una enfermedad o plaga en un lote, con su seguimiento
/// (RF-16..19, HU-07..10).
/// </summary>
public class RegistroEnfermedad : AuditableEntity
{
    public int LoteId { get; set; }

    public Lote Lote { get; set; } = null!;

    public int TipoEnfermedadId { get; set; }

    public TipoEnfermedad TipoEnfermedad { get; set; } = null!;

    public int? PeriodoProductivoId { get; set; }

    public PeriodoProductivo? PeriodoProductivo { get; set; }

    [DataType(DataType.Date)]
    public DateTime FechaDeteccion { get; set; }

    [StringLength(600)]
    public string? Sintomas { get; set; }

    public EstadoEnfermedad Estado { get; set; } = EstadoEnfermedad.Detectada;

    public ICollection<Tratamiento> Tratamientos { get; set; } = new List<Tratamiento>();
}

/// <summary>Acción o insumo aplicado sobre un brote activo (RF-17, HU-08).</summary>
public class Tratamiento : AuditableEntity
{
    public int RegistroEnfermedadId { get; set; }

    public RegistroEnfermedad RegistroEnfermedad { get; set; } = null!;

    [Required, StringLength(300)]
    public string Descripcion { get; set; } = string.Empty;

    /// <summary>Nombre del producto o insumo aplicado (texto libre mientras no exista inventario).</summary>
    [StringLength(120)]
    public string? ProductoTexto { get; set; }

    /// <summary>Insumo del inventario utilizado; si se indica, se rebaja el stock (HU-24).</summary>
    public int? InsumoId { get; set; }

    public Finanzas.Insumo? Insumo { get; set; }

    [StringLength(80)]
    public string? Dosis { get; set; }

    [Range(0, 1_000_000)]
    public decimal CantidadInsumoUsada { get; set; }

    [DataType(DataType.Date)]
    public DateTime FechaAplicacion { get; set; }

    [StringLength(400)]
    public string? ResultadoObservado { get; set; }
}
