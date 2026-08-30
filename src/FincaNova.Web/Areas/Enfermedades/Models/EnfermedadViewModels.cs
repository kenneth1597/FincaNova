using System.ComponentModel.DataAnnotations;
using FincaNova.Web.Domain;

namespace FincaNova.Web.Areas.Enfermedades.Models;

/// <summary>Fila del historial de enfermedades (HU-09).</summary>
public class EnfermedadListItemViewModel
{
    public int Id { get; set; }
    public DateTime FechaDeteccion { get; set; }
    public string Lote { get; set; } = string.Empty;
    public string Enfermedad { get; set; } = string.Empty;
    public EstadoEnfermedad Estado { get; set; }
    public int Tratamientos { get; set; }
    public string? UltimoTratamiento { get; set; }
}

/// <summary>Filtros avanzados del historial (HU-09 esc. 4): lote, fecha, tipo y tratamiento.</summary>
public class EnfermedadFiltroViewModel
{
    public int? LoteId { get; set; }
    public int? TipoEnfermedadId { get; set; }
    public EstadoEnfermedad? Estado { get; set; }
    public DateTime? Desde { get; set; }
    public DateTime? Hasta { get; set; }

    [Display(Name = "Tratamiento contiene")]
    public string? Tratamiento { get; set; }

    public IReadOnlyList<EnfermedadListItemViewModel> Resultados { get; set; } = Array.Empty<EnfermedadListItemViewModel>();
    public IEnumerable<(int Id, string Texto)> Lotes { get; set; } = Enumerable.Empty<(int, string)>();
    public IEnumerable<(int Id, string Texto)> Tipos { get; set; } = Enumerable.Empty<(int, string)>();
}

/// <summary>Formulario de registro y edición de una detección de enfermedad (HU-07).</summary>
public class EnfermedadFormViewModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Debe seleccionar el lote afectado.")]
    [Display(Name = "Lote / micro lote")]
    public int? LoteId { get; set; }

    [Required(ErrorMessage = "Debe seleccionar la enfermedad o plaga detectada.")]
    [Display(Name = "Enfermedad o plaga")]
    public int? TipoEnfermedadId { get; set; }

    [Display(Name = "Período productivo")]
    public int? PeriodoProductivoId { get; set; }

    [DataType(DataType.Date)]
    [Display(Name = "Fecha de detección")]
    public DateTime FechaDeteccion { get; set; } = DateTime.Today;

    [Display(Name = "Estado del seguimiento")]
    public EstadoEnfermedad Estado { get; set; } = EstadoEnfermedad.Detectada;

    [StringLength(600)]
    [Display(Name = "Síntomas observados")]
    public string? Sintomas { get; set; }

    public bool EsEdicion => Id != 0;

    public IEnumerable<(int Id, string Texto)> Lotes { get; set; } = Enumerable.Empty<(int, string)>();
    public IEnumerable<(int Id, string Texto)> Tipos { get; set; } = Enumerable.Empty<(int, string)>();
    public IEnumerable<(int Id, string Texto)> Periodos { get; set; } = Enumerable.Empty<(int, string)>();
}

/// <summary>Detalle de una detección con sus tratamientos (HU-08, HU-09).</summary>
public class EnfermedadDetalleViewModel
{
    public Domain.Enfermedades.RegistroEnfermedad Registro { get; set; } = null!;
    public string Lote { get; set; } = string.Empty;
    public string Enfermedad { get; set; } = string.Empty;
    public string? Periodo { get; set; }
    public IReadOnlyList<Domain.Enfermedades.Tratamiento> Tratamientos { get; set; } = Array.Empty<Domain.Enfermedades.Tratamiento>();
    public bool AlertaRecurrencia { get; set; }
    public int DeteccionesEnVentana { get; set; }
}

/// <summary>Formulario de registro de un tratamiento aplicado (HU-08).</summary>
public class TratamientoFormViewModel
{
    public int RegistroEnfermedadId { get; set; }
    public string Enfermedad { get; set; } = string.Empty;
    public string Lote { get; set; } = string.Empty;

    [Required(ErrorMessage = "Describa la acción o tratamiento aplicado.")]
    [StringLength(300)]
    [Display(Name = "Acción / tratamiento aplicado")]
    public string Descripcion { get; set; } = string.Empty;

    [StringLength(120)]
    [Display(Name = "Producto o insumo utilizado (texto libre)")]
    public string? Producto { get; set; }

    [Display(Name = "…o descontar del inventario")]
    public int? InsumoId { get; set; }

    [Range(0, 1_000_000)]
    [Display(Name = "Cantidad usada del inventario")]
    public decimal CantidadInsumo { get; set; }

    [StringLength(80)]
    [Display(Name = "Dosis")]
    public string? Dosis { get; set; }

    public IEnumerable<(int Id, string Texto)> Insumos { get; set; } = Enumerable.Empty<(int, string)>();

    [DataType(DataType.Date)]
    [Display(Name = "Fecha de aplicación")]
    public DateTime FechaAplicacion { get; set; } = DateTime.Today;

    [StringLength(400)]
    [Display(Name = "Resultado observado")]
    public string? ResultadoObservado { get; set; }
}
