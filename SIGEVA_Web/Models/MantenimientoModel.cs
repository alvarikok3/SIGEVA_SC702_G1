namespace SIGEVA_Web.Models
{
    public class MantenimientoModel
    {
        public int IdMantenimiento { get; set; }
        public int IdEquipo { get; set; }
        public DateTime FechaMantenimiento { get; set; }
        public string TipoMantenimiento { get; set; } = string.Empty;
        public string? Descripcion { get; set; }
        public decimal Costo { get; set; }
        public string Responsable { get; set; } = string.Empty;
        public bool Estado { get; set; }
        public string? NombreEquipo { get; set; }
    }
}
