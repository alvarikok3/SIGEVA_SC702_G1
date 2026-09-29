using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;
using SIGEVA_API.Interfaces;
using SIGEVA_API.Models;
using System.Net.Mail;

namespace SIGEVA_API.Services
{
    public class EmailService : IEmailService
    {
        private readonly EmailSettings _emailSettings;

        public EmailService(IOptions<EmailSettings> emailSettings)
        {
            _emailSettings = emailSettings.Value;
        }

        public async Task EnviarCorreoAsync(
            string destinatario,
            string asunto,
            string mensajeHtml)
        {
            if (string.IsNullOrWhiteSpace(destinatario))
                throw new ArgumentException(
                    "El correo del destinatario es obligatorio.",
                    nameof(destinatario));

            var mensaje = new MimeMessage();

            mensaje.From.Add(new MailboxAddress(
                _emailSettings.NombreRemitente,
                _emailSettings.CorreoRemitente));

            mensaje.To.Add(MailboxAddress.Parse(destinatario));
            mensaje.Subject = asunto;

            var cuerpo = new BodyBuilder
            {
                HtmlBody = mensajeHtml
            };

            mensaje.Body = cuerpo.ToMessageBody();

            using var clienteSmtp = new MailKit.Net.Smtp.SmtpClient();

            await clienteSmtp.ConnectAsync(
                _emailSettings.Servidor,
                _emailSettings.Puerto,
                SecureSocketOptions.StartTls);

            await clienteSmtp.AuthenticateAsync(
                _emailSettings.CorreoRemitente,
                _emailSettings.ContrasenaAplicacion);

            await clienteSmtp.SendAsync(mensaje);
            await clienteSmtp.DisconnectAsync(true);
        }
    }
}