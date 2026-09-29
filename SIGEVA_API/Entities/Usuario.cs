namespace SIGEVA_API.Entities
{
    public class Usuario
    {
        public int IdUsuario { get; set; }
        public int IdRol { get; set; }
        public string Identificacion { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;
        public string Correo { get; set; } = string.Empty;
        public string Contrasena { get; set; } = string.Empty;
        public bool Estado { get; set; }
        public DateTime FechaRegistro { get; set; }
    }
}