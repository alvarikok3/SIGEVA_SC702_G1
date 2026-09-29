namespace SIGEVA_Web.Models
{
    public class ReporteFiltroModel
    {
        public DateTime? FechaInicio { get; set; }
        public DateTime? FechaFin { get; set; }
        public int? IdEquipo { get; set; }
        public string TipoMantenimiento { get; set; } = string.Empty;
        public List<EquipoMedicoModel> Equipos { get; set; } = new();
        public List<MantenimientoModel> Resultados { get; set; } = new();
        public decimal TotalCosto { get; set; }
    }
}
