using System.ComponentModel.DataAnnotations;

namespace SIGEVA_API.Models
{
    public class CitaModel
    {
        public int IdCita { get; set; }

        [Required]
        public int IdPaciente { get; set; }

        public string NombrePaciente { get; set; } = string.Empty;

        [Required]
        public int IdUsuario { get; set; }

        public string NombreMedico { get; set; } = string.Empty;

        [Required]
        public DateTime FechaHora { get; set; }

        [Required]
        public string EstadoCita { get; set; } = "Programada";

        public string? Observaciones { get; set; }

        public DateTime FechaRegistro { get; set; }
    }
}
