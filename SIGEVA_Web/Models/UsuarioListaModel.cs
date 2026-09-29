namespace SIGEVA_Web.Models
{
    public class UsuarioListaModel
    {
        public int IdUsuario { get; set; }
        public int IdRol { get; set; }
        public string NombreRol { get; set; } = string.Empty;
        public string Identificacion { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;
        public string Correo { get; set; } = string.Empty;
        public bool Estado { get; set; }
    }
}
