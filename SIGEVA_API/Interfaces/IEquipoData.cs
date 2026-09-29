using SIGEVA_API.Models;

namespace SIGEVA_API.Interfaces
{
    public interface IEquipoData
    {
        List<EquipoMedicoModel> ConsultarEquipos();
        EquipoMedicoModel? ConsultarEquipoPorId(int idEquipo);
        int RegistrarEquipo(EquipoMedicoModel model);
        int ActualizarEquipo(EquipoMedicoModel model);
        int CambiarEstadoEquipo(int idEquipo, bool estado);
    }
}
