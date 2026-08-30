using System.ComponentModel.DataAnnotations;
using FincaNova.Web.Domain;
using FincaNova.Web.ViewSupport;

namespace FincaNova.Web.Areas.Lotes.Models;

/// <summary>Fila del listado de lotes y micro lotes (HU-31, HU-32, HU-39).</summary>
public class LoteListItemViewModel
{
    public int Id { get; set; }
    public string Codigo { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public TipoLote Tipo { get; set; }
    public string? LotePadre { get; set; }
    public decimal AreaHectareas { get; set; }
    public string? VariedadCafe { get; set; }
    public int? AnioSiembra { get; set; }
    public EstadoLote Estado { get; set; }
    public int MicroLotes { get; set; }
}

/// <summary>Filtros del listado de lotes (HU-32, HU-39).</summary>
public class LoteFiltroViewModel
{
    public string? Q { get; set; }
    public TipoLote? Tipo { get; set; }
    public EstadoLote? Estado { get; set; }
    public IReadOnlyList<LoteListItemViewModel> Resultados { get; set; } = Array.Empty<LoteListItemViewModel>();
    public Dictionary<EstadoLote, int> Conteos { get; set; } = new();
}

/// <summary>Formulario de alta y edición de lote / micro lote (HU-30, HU-33, HU-36, HU-37).</summary>
public class LoteFormViewModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "El código es obligatorio.")]
    [StringLength(30)]
    [RegularExpression(Validaciones.Codigo, ErrorMessage = Validaciones.CodigoMsg)]
    [Display(Name = "Código")]
    public string Codigo { get; set; } = string.Empty;

    [Required(ErrorMessage = "El nombre es obligatorio.")]
    [StringLength(120)]
    [Display(Name = "Nombre")]
    public string Nombre { get; set; } = string.Empty;

    [Display(Name = "Tipo")]
    public TipoLote Tipo { get; set; } = TipoLote.Lote;

    [Display(Name = "Lote al que pertenece")]
    public int? LotePadreId { get; set; }

    [Range(0, 100000, ErrorMessage = "El área no puede ser negativa.")]
    [Display(Name = "Área (hectáreas)")]
    public decimal AreaHectareas { get; set; }

    [StringLength(100)]
    [Display(Name = "Variedad de café")]
    public string? VariedadCafe { get; set; }

    [Range(1900, 2100, ErrorMessage = "El año de siembra no es válido.")]
    [Display(Name = "Año de siembra")]
    public int? AnioSiembra { get; set; }

    [StringLength(200)]
    [Display(Name = "Ubicación / referencia")]
    public string? Ubicacion { get; set; }

    [Display(Name = "Estado")]
    public EstadoLote Estado { get; set; } = EstadoLote.Activo;

    [StringLength(500)]
    [Display(Name = "Observaciones")]
    public string? Observaciones { get; set; }

    public bool EsEdicion => Id != 0;

    public IEnumerable<(int Id, string Texto)> LotesPadre { get; set; } = Enumerable.Empty<(int, string)>();
}

/// <summary>Un evento en la bitácora cronológica del lote (HU-41).</summary>
public class BitacoraItemViewModel
{
    public DateTime Fecha { get; set; }
    public string Tipo { get; set; } = string.Empty;
    public string Icono { get; set; } = "bi-record-circle";
    public string Color { get; set; } = "secondary";
    public string Descripcion { get; set; } = string.Empty;
    public string? Detalle { get; set; }
}

/// <summary>Detalle de un lote con su bitácora (HU-31, HU-41).</summary>
public class LoteDetalleViewModel
{
    public Domain.Lotes.Lote Lote { get; set; } = null!;
    public IReadOnlyList<LoteListItemViewModel> MicroLotes { get; set; } = Array.Empty<LoteListItemViewModel>();
    public IReadOnlyList<BitacoraItemViewModel> Bitacora { get; set; } = Array.Empty<BitacoraItemViewModel>();
    public DateTime? Desde { get; set; }
    public DateTime? Hasta { get; set; }
}
