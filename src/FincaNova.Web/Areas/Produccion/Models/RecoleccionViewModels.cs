using System.ComponentModel.DataAnnotations;

namespace FincaNova.Web.Areas.Produccion.Models;

/// <summary>Fila del listado de recolección diaria (HU-11).</summary>
public class RecoleccionListItemViewModel
{
    public int Id { get; set; }
    public DateTime Fecha { get; set; }
    public string Lote { get; set; } = string.Empty;
    public string Colaborador { get; set; } = string.Empty;
    public decimal Cajuelas { get; set; }
    public decimal PesoEstimadoKg { get; set; }
}

/// <summary>Filtros y totales del avance de recolección (HU-11, HU-12).</summary>
public class RecoleccionFiltroViewModel
{
    public int? LoteId { get; set; }
    public int? PeriodoId { get; set; }
    public int? ColaboradorId { get; set; }
    public DateTime? Desde { get; set; }
    public DateTime? Hasta { get; set; }

    public IReadOnlyList<RecoleccionListItemViewModel> Resultados { get; set; } = Array.Empty<RecoleccionListItemViewModel>();

    public decimal TotalCajuelas { get; set; }
    public decimal TotalKg { get; set; }
    public decimal KilogramosPorCajuela { get; set; }
    public string? PeriodoNombre { get; set; }

    public record PorColaborador(string Colaborador, decimal Cajuelas, decimal Kg);
    public IReadOnlyList<PorColaborador> Acumulado { get; set; } = Array.Empty<PorColaborador>();

    public IEnumerable<(int Id, string Texto)> Lotes { get; set; } = Enumerable.Empty<(int, string)>();
    public IEnumerable<(int Id, string Texto)> Periodos { get; set; } = Enumerable.Empty<(int, string)>();
    public IEnumerable<(int Id, string Texto)> Colaboradores { get; set; } = Enumerable.Empty<(int, string)>();
}

/// <summary>Formulario de registro y edición de recolección diaria (HU-11, HU-13).</summary>
public class RecoleccionFormViewModel : IValidatableObject
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Debe seleccionar el lote.")]
    [Display(Name = "Lote / micro lote")]
    public int? LoteId { get; set; }

    [Required(ErrorMessage = "Debe seleccionar el período productivo.")]
    [Display(Name = "Período productivo")]
    public int? PeriodoProductivoId { get; set; }

    [Required(ErrorMessage = "Debe seleccionar el trabajador.")]
    [Display(Name = "Trabajador que recolectó")]
    public int? ColaboradorId { get; set; }

    [DataType(DataType.Date)]
    [Display(Name = "Fecha de recolección")]
    public DateTime Fecha { get; set; } = DateTime.Today;

    [Range(0.0, 100000)]
    [Display(Name = "Cantidad recolectada (cajuelas)")]
    public decimal Cajuelas { get; set; }

    [StringLength(400)]
    [Display(Name = "Observaciones")]
    public string? Observaciones { get; set; }

    public bool EsEdicion => Id != 0;
    public decimal KilogramosPorCajuela { get; set; }
    public decimal PesoEstimadoKg => decimal.Round(Cajuelas * KilogramosPorCajuela, 2);

    public IEnumerable<(int Id, string Texto)> Lotes { get; set; } = Enumerable.Empty<(int, string)>();
    public IEnumerable<(int Id, string Texto)> Periodos { get; set; } = Enumerable.Empty<(int, string)>();
    public IEnumerable<(int Id, string Texto)> Colaboradores { get; set; } = Enumerable.Empty<(int, string)>();

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (Cajuelas <= 0)
            yield return new ValidationResult("La cantidad de cajuelas debe ser mayor que cero.", new[] { nameof(Cajuelas) });
    }
}
