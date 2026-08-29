namespace FincaNova.Web.Services.Email;

/// <summary>Envío de correos de la plataforma (recuperación de contraseña, avisos).</summary>
public interface IEmailSender
{
    Task EnviarAsync(string destinatario, string asunto, string cuerpoHtml, CancellationToken cancellationToken = default);
}
