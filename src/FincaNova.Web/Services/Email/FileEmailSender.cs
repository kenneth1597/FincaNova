using System.Text;

namespace FincaNova.Web.Services.Email;

/// <summary>
/// Implementación para desarrollo: en lugar de enviar el correo, lo guarda como
/// archivo en <c>App_Data/correos</c> y escribe su contenido en el log. Permite
/// probar el flujo de recuperación de contraseña sin un servidor SMTP.
/// En producción se sustituye por una implementación SMTP real.
/// </summary>
public class FileEmailSender : IEmailSender
{
    private readonly string _carpeta;
    private readonly ILogger<FileEmailSender> _logger;

    public FileEmailSender(IWebHostEnvironment env, ILogger<FileEmailSender> logger)
    {
        _carpeta = Path.Combine(env.ContentRootPath, "App_Data", "correos");
        Directory.CreateDirectory(_carpeta);
        _logger = logger;
    }

    public async Task EnviarAsync(string destinatario, string asunto, string cuerpoHtml, CancellationToken cancellationToken = default)
    {
        var nombre = $"{DateTime.Now:yyyyMMdd-HHmmss}-{destinatario.Replace('@', '_')}.html";
        var ruta = Path.Combine(_carpeta, nombre);

        var contenido = new StringBuilder()
            .AppendLine($"<!-- Para: {destinatario} -->")
            .AppendLine($"<!-- Asunto: {asunto} -->")
            .AppendLine($"<!-- Fecha: {DateTime.Now:u} -->")
            .AppendLine(cuerpoHtml)
            .ToString();

        await File.WriteAllTextAsync(ruta, contenido, cancellationToken);
        _logger.LogInformation("Correo simulado para {Destinatario} ({Asunto}) guardado en {Ruta}", destinatario, asunto, ruta);
    }
}
