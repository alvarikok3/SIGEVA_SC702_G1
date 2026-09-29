namespace SIGEVA_API.Models
{
    public class CambiarContrasenaModel
    {
        public int IdUsuario { get; set; }

        public string ContrasenaActual { get; set; } = string.Empty;

        public string ContrasenaNueva { get; set; } = string.Empty;
    }
}