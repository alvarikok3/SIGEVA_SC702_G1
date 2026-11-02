
namespace SIGEVA_API.Models
{
    public class ServicioMedicoModel
    {
        public int IdServicio { get; set; }

        public int IdCategoria { get; set; }

        public string Nombre { get; set; } = string.Empty;

        public string? Descripcion { get; set; }

        public bool Estado { get; set; }

        public string Categoria { get; set; } = string.Empty;
    }
}
