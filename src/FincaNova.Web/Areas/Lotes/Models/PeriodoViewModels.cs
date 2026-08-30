using System.ComponentModel.DataAnnotations;

namespace FincaNova.Web.Areas.Lotes.Models;

/// <summary>Formulario de alta y edición de período productivo (HU-40).</summary>
public class PeriodoFormViewModel : IValidatableObject
{
    public int Id { get; set; }

    [Required(ErrorMessage = "El nombre es obligatorio.")]
    [StringLength(80)]
    [Display(Name = "Nombre")]
    public string Nombre { get; set; } = string.Empty;

    [DataType(DataType.Date)]
    [Display(Name = "Fecha de inicio")]
    public DateTime FechaInicio { get; set; } = DateTime.Today;

    [DataType(DataType.Date)]
    [Display(Name = "Fecha de fin")]
    public DateTime FechaFin { get; set; } = DateTime.Today.AddMonths(3);

    [Display(Name = "Marcar como período activo")]
    public bool Activo { get; set; }

    [StringLength(400)]
    [Display(Name = "Observaciones")]
    public string? Observaciones { get; set; }

    public bool EsEdicion => Id != 0;

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (FechaFin <= FechaInicio)
            yield return new ValidationResult(
                "La fecha de fin debe ser posterior a la fecha de inicio.",
                new[] { nameof(FechaFin) });
    }
}

/// <summary>Fila del listado de períodos productivos.</summary>
public class PeriodoListItemViewModel
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public DateTime FechaInicio { get; set; }
    public DateTime FechaFin { get; set; }
    public bool Activo { get; set; }
    public int Recolecciones { get; set; }
    public int Labores { get; set; }
}
