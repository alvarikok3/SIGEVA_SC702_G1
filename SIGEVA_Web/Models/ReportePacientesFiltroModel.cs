namespace SIGEVA_Web.Models
{
    public class ReportePacientesFiltroModel
    {
        public string? Nombre { get; set; }
        public bool? Estado { get; set; }
        public List<PacienteModel> Resultados { get; set; } = new();
    }
}
