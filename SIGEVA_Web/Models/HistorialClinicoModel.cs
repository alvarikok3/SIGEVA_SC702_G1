namespace SIGEVA_Web.Models
{
    public class HistorialClinicoModel
    {
        public int IdPaciente { get; set; }
        public string NombrePaciente { get; set; } = string.Empty;
        public string Identificacion { get; set; } = string.Empty;

        public List<HistorialClinicoRegistroModel> Registros { get; set; }
            = new List<HistorialClinicoRegistroModel>();
    }
}