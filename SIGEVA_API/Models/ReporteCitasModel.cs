namespace SIGEVA_API.Models
{
    public class ReporteCitasModel
    {
        public DateTime? FechaInicio { get; set; }
        public DateTime? FechaFin { get; set; }
        public string EstadoCita { get; set; } = string.Empty;
    }
}
