namespace SIGEVA_API.Models
{
    public class EmailSettings
    {
        public string Servidor { get; set; } = string.Empty;

        public int Puerto { get; set; }

        public string NombreRemitente { get; set; } = string.Empty;

        public string CorreoRemitente { get; set; } = string.Empty;

        public string ContrasenaAplicacion { get; set; } = string.Empty;
    }
}