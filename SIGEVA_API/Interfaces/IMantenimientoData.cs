using SIGEVA_API.Models;

namespace SIGEVA_API.Interfaces
{
    public interface IMantenimientoData
    {
        List<MantenimientoModel> ConsultarMantenimientos();
        MantenimientoModel? ConsultarMantenimientoPorId(int idMantenimiento);
        int RegistrarMantenimiento(MantenimientoModel model);
        int ActualizarMantenimiento(MantenimientoModel model);
        int CambiarEstadoMantenimiento(int idMantenimiento, bool estado);
        List<MantenimientoModel> ReporteMantenimientos(ReporteMantenimientoModel filtro);
    }
}
