namespace SIGEVA_Web.Models
{
    public class EquipoMedicoModel
    {
        public int IdEquipo { get; set; }
        public int IdUsuario { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string? Descripcion { get; set; }
        public string Marca { get; set; } = string.Empty;
        public string Modelo { get; set; } = string.Empty;
        public string NumeroSerie { get; set; } = string.Empty;
        public DateTime FechaAdquisicion { get; set; }
        public bool Estado { get; set; }
        public string? NombreUsuario { get; set; }
    }
}
