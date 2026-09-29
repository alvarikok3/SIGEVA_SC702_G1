namespace SIGEVA_API.Models
{
    public class ReporteMantenimientoModel
    {
        public DateTime? FechaInicio { get; set; }
        public DateTime? FechaFin { get; set; }
        public int? IdEquipo { get; set; }
        public string TipoMantenimiento { get; set; } = string.Empty;
    }
}
