using System.ComponentModel.DataAnnotations;
using FincaNova.Web.Domain;
using FincaNova.Web.Domain.Auditoria;
using FincaNova.Web.Models;

namespace FincaNova.Web.Areas.Seguridad.Models;

/// <summary>Filtros y resultados del visor de la bitácora de auditoría (HU-18, RQNF-013).</summary>
public class AuditoriaFiltroViewModel
{
    [DataType(DataType.Date)]
    [Display(Name = "Desde")]
    public DateTime? Desde { get; set; }

    [DataType(DataType.Date)]
    [Display(Name = "Hasta")]
    public DateTime? Hasta { get; set; }

    [Display(Name = "Usuario")]
    public string? Usuario { get; set; }

    [Display(Name = "Entidad")]
    public string? Entidad { get; set; }

    [Display(Name = "Acción")]
    public AccionAuditoria? Accion { get; set; }

    public int Pagina { get; set; } = 1;

    public PagedResult<AuditLog> Resultados { get; set; } = new();

    public IEnumerable<string> EntidadesDisponibles { get; set; } = Enumerable.Empty<string>();
}
