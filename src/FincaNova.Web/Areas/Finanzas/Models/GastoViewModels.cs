using System.ComponentModel.DataAnnotations;
using FincaNova.Web.Domain;

namespace FincaNova.Web.Areas.Finanzas.Models;

public class GastoListItemViewModel
{
    public int Id { get; set; }
    public DateTime Fecha { get; set; }
    public CategoriaGasto Categoria { get; set; }
    public string Descripcion { get; set; } = string.Empty;
    public string? Lote { get; set; }
    public decimal Monto { get; set; }
    public bool TieneComprobante { get; set; }
}

public class GastoFiltroViewModel
{
    public int? LoteId { get; set; }
    public int? PeriodoId { get; set; }
    public CategoriaGasto? Categoria { get; set; }
    public DateTime? Desde { get; set; }
    public DateTime? Hasta { get; set; }

    public IReadOnlyList<GastoListItemViewModel> Resultados { get; set; } = Array.Empty<GastoListItemViewModel>();
    public decimal Total { get; set; }
    public IEnumerable<(int Id, string Texto)> Lotes { get; set; } = Enumerable.Empty<(int, string)>();
    public IEnumerable<(int Id, string Texto)> Periodos { get; set; } = Enumerable.Empty<(int, string)>();
}

public class GastoFormViewModel
{
    public int Id { get; set; }

    [DataType(DataType.Date)]
    [Display(Name = "Fecha")]
    public DateTime Fecha { get; set; } = DateTime.Today;

    [Display(Name = "Categoría")]
    public CategoriaGasto Categoria { get; set; } = CategoriaGasto.Insumos;

    [Required(ErrorMessage = "El monto es obligatorio.")]
    [Range(0.01, 1_000_000_000, ErrorMessage = "El monto debe ser mayor a cero.")]
    [Display(Name = "Monto")]
    public decimal Monto { get; set; }

    [Required(ErrorMessage = "Describa el gasto.")]
    [StringLength(300)]
    [Display(Name = "Descripción")]
    public string Descripcion { get; set; } = string.Empty;

    [StringLength(120)]
    [Display(Name = "Proveedor")]
    public string? Proveedor { get; set; }

    [Display(Name = "Lote asociado (opcional)")]
    public int? LoteId { get; set; }

    [Display(Name = "Período productivo (opcional)")]
    public int? PeriodoProductivoId { get; set; }

    [Display(Name = "Comprobante / factura (PDF, JPG o PNG · máx. 5 MB)")]
    public IFormFile? Comprobante { get; set; }

    public bool EsEdicion => Id != 0;
    public string? ComprobanteActual { get; set; }

    public IEnumerable<(int Id, string Texto)> Lotes { get; set; } = Enumerable.Empty<(int, string)>();
    public IEnumerable<(int Id, string Texto)> Periodos { get; set; } = Enumerable.Empty<(int, string)>();
}
