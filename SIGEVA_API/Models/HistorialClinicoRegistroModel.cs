namespace SIGEVA_API.Models
{
    public class HistorialClinicoRegistroModel
    {
        public int IdRegistro { get; set; }

        public int IdPaciente { get; set; }

        public DateTime Fecha { get; set; }

        public string TipoRegistro { get; set; } = string.Empty;

        public string Descripcion { get; set; } = string.Empty;

        public string Profesional { get; set; } = string.Empty;

        public string? Estado { get; set; }
    }
}