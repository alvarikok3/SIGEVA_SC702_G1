namespace SIGEVA_Web.Models
{
    public class CitaModel
    {
        public int IdCita { get; set; }
        public int IdPaciente { get; set; }
        public string NombrePaciente { get; set; } = string.Empty;
        public int IdUsuario { get; set; }
        public string NombreMedico { get; set; } = string.Empty;
        public DateTime FechaHora { get; set; }
        public string EstadoCita { get; set; } = "Programada";
        public string? Observaciones { get; set; }
        public DateTime FechaRegistro { get; set; }
    }
}
