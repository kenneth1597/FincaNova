using ClosedXML.Excel;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace FincaNova.Web.Services.Reportes;

/// <summary>Datos tabulares de un reporte, listos para exportar (HU-26, HU-27).</summary>
public record ReporteTabla(
    string Titulo,
    string Subtitulo,
    IReadOnlyList<string> Columnas,
    IReadOnlyList<IReadOnlyList<string>> Filas,
    IReadOnlyList<string>? Totales = null);

public interface IReporteExportService
{
    byte[] ToPdf(ReporteTabla reporte);
    byte[] ToExcel(ReporteTabla reporte);
}

public class ReporteExportService : IReporteExportService
{
    public byte[] ToPdf(ReporteTabla r)
    {
        var doc = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Margin(30);
                page.Size(PageSizes.A4);
                page.DefaultTextStyle(x => x.FontSize(9).FontFamily("Segoe UI", "Arial"));

                page.Header().Column(col =>
                {
                    col.Item().Text("FincaNova · Café Chaperno").FontSize(8).FontColor(Colors.Grey.Medium);
                    col.Item().Text(r.Titulo).FontSize(15).SemiBold().FontColor("#2E7D32");
                    if (!string.IsNullOrWhiteSpace(r.Subtitulo))
                        col.Item().Text(r.Subtitulo).FontSize(9).FontColor(Colors.Grey.Darken1);
                    col.Item().PaddingTop(4).Text($"Generado el {DateTime.Now:dd/MM/yyyy HH:mm}").FontSize(8).FontColor(Colors.Grey.Medium);
                });

                page.Content().PaddingVertical(10).Table(table =>
                {
                    table.ColumnsDefinition(cols =>
                    {
                        foreach (var _ in r.Columnas) cols.RelativeColumn();
                    });

                    table.Header(header =>
                    {
                        foreach (var c in r.Columnas)
                            header.Cell().Background("#E8F5E9").Padding(4).Text(c).SemiBold();
                    });

                    foreach (var fila in r.Filas)
                        foreach (var celda in fila)
                            table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(4).Text(celda);

                    if (r.Totales is { Count: > 0 })
                        foreach (var t in r.Totales)
                            table.Cell().Background("#F5F7F5").Padding(4).Text(t).SemiBold();
                });

                page.Footer().AlignCenter().Text(x =>
                {
                    x.CurrentPageNumber(); x.Span(" / "); x.TotalPages();
                });
            });
        });

        return doc.GeneratePdf();
    }

    public byte[] ToExcel(ReporteTabla r)
    {
        using var wb = new XLWorkbook();
        var ws = wb.Worksheets.Add("Reporte");

        ws.Cell(1, 1).Value = r.Titulo;
        ws.Cell(1, 1).Style.Font.Bold = true;
        ws.Cell(1, 1).Style.Font.FontSize = 14;
        ws.Cell(2, 1).Value = r.Subtitulo;
        ws.Cell(3, 1).Value = $"Generado el {DateTime.Now:dd/MM/yyyy HH:mm}";

        var headerRow = 5;
        for (var c = 0; c < r.Columnas.Count; c++)
        {
            var cell = ws.Cell(headerRow, c + 1);
            cell.Value = r.Columnas[c];
            cell.Style.Font.Bold = true;
            cell.Style.Fill.BackgroundColor = XLColor.FromHtml("#E8F5E9");
        }

        var row = headerRow + 1;
        foreach (var fila in r.Filas)
        {
            for (var c = 0; c < fila.Count; c++)
                ws.Cell(row, c + 1).Value = fila[c];
            row++;
        }

        if (r.Totales is { Count: > 0 })
        {
            for (var c = 0; c < r.Totales.Count; c++)
            {
                var cell = ws.Cell(row, c + 1);
                cell.Value = r.Totales[c];
                cell.Style.Font.Bold = true;
            }
        }

        ws.Columns().AdjustToContents();
        using var ms = new MemoryStream();
        wb.SaveAs(ms);
        return ms.ToArray();
    }
}
