using SIGEVA_API.Models;

namespace SIGEVA_API.Interfaces
{
    public interface IHistorialClinicoData
    {
        bool TienePermisoConsulta(int idUsuario);

        HistorialClinicoModel? ConsultarHistorial(int idPaciente);

        int RegistrarConsultaAuditoria(
            int idUsuario,
            int idPaciente);
    }
}