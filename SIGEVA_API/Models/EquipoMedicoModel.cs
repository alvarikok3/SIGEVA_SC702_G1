using System.ComponentModel.DataAnnotations;

namespace SIGEVA_API.Models
{
    public class EquipoMedicoModel
    {
        public int IdEquipo { get; set; }

        [Required]
        public int IdUsuario { get; set; }

        [Required]
        public string Nombre { get; set; } = string.Empty;

        public string? Descripcion { get; set; }

        [Required]
        public string Marca { get; set; } = string.Empty;

        [Required]
        public string Modelo { get; set; } = string.Empty;

        [Required]
        public string NumeroSerie { get; set; } = string.Empty;

        public DateTime FechaAdquisicion { get; set; }

        public bool Estado { get; set; }

        public string? NombreUsuario { get; set; }
    }
}
