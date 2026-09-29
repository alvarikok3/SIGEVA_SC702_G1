using System.ComponentModel.DataAnnotations;

namespace SIGEVA_API.Models
{
    public class PacienteModel
    {
        public int IdPaciente { get; set; }

        [Required]
        public string Identificacion { get; set; } = string.Empty;

        [Required]
        public string Nombre { get; set; } = string.Empty;

        [Required]
        public string Telefono { get; set; } = string.Empty;

        [Required]
        public string Correo { get; set; } = string.Empty;

        public string? Direccion { get; set; }

        public DateTime? FechaNacimiento { get; set; }

        public bool Estado { get; set; }

        public DateTime FechaRegistro { get; set; }
    }
}
