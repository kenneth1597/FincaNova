namespace FincaNova.Web.Models;

/// <summary>Página de resultados para listados con paginación.</summary>
public class PagedResult<T>
{
    public IReadOnlyList<T> Items { get; init; } = Array.Empty<T>();
    public int Pagina { get; init; }
    public int TamanoPagina { get; init; }
    public int TotalRegistros { get; init; }

    public int TotalPaginas => TamanoPagina <= 0 ? 0 : (int)Math.Ceiling(TotalRegistros / (double)TamanoPagina);
    public bool TieneAnterior => Pagina > 1;
    public bool TieneSiguiente => Pagina < TotalPaginas;
}
