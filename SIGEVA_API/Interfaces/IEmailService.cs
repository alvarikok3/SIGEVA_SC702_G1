namespace SIGEVA_API.Interfaces
{
    public interface IEmailService
    {
        Task EnviarCorreoAsync(
            string destinatario,
            string asunto,
            string mensajeHtml);
    }
}