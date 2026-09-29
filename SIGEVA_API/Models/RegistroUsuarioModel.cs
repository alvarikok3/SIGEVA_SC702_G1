namespace SIGEVA_API.Models
{
    public class RegistroUsuarioModel
    {
        public int IdRol { get; set; }
        public string Identificacion { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;
        public string Correo { get; set; } = string.Empty;
        public string Contrasena { get; set; } = string.Empty;
    }
}