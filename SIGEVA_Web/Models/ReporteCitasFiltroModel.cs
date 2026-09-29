namespace SIGEVA_Web.Models
{
    public class ReporteCitasFiltroModel
    {
        public DateTime? FechaInicio { get; set; }
        public DateTime? FechaFin { get; set; }
        public string EstadoCita { get; set; } = string.Empty;
        public List<CitaModel> Resultados { get; set; } = new();
    }
}
