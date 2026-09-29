using System.ComponentModel.DataAnnotations;

namespace SIGEVA_API.Models
{
    public class MantenimientoModel
    {
        public int IdMantenimiento { get; set; }

        [Required]
        public int IdEquipo { get; set; }

        public DateTime FechaMantenimiento { get; set; }

        [Required]
        public string TipoMantenimiento { get; set; } = string.Empty;

        public string? Descripcion { get; set; }

        public decimal Costo { get; set; }

        [Required]
        public string Responsable { get; set; } = string.Empty;

        public bool Estado { get; set; }

        public string? NombreEquipo { get; set; }
    }
}
