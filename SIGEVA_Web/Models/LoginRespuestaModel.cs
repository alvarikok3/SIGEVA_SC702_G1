namespace SIGEVA_Web.Models
{
    public class LoginRespuestaModel
    {
        public string Token { get; set; } = string.Empty;
        public int IdUsuario { get; set; }
        public int IdRol { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Correo { get; set; } = string.Empty;
    }
}