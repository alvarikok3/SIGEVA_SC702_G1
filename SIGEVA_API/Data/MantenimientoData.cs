using System.Data;
using Dapper;
using Microsoft.Data.SqlClient;
using SIGEVA_API.Interfaces;
using SIGEVA_API.Models;

namespace SIGEVA_API.Data
{
    public class MantenimientoData : IMantenimientoData
    {
        private readonly string _connection;

        public MantenimientoData(IConfiguration configuration)
        {
            _connection = configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("No se encontró la cadena de conexión DefaultConnection.");
        }

        public List<MantenimientoModel> ConsultarMantenimientos()
        {
            using var context = new SqlConnection(_connection);
            return context.Query<MantenimientoModel>(
                "ConsultarMantenimientos",
                commandType: CommandType.StoredProcedure).ToList();
        }

        public MantenimientoModel? ConsultarMantenimientoPorId(int idMantenimiento)
        {
            using var context = new SqlConnection(_connection);
            var parameters = new DynamicParameters();
            parameters.Add("@IdMantenimiento", idMantenimiento);
            return context.QueryFirstOrDefault<MantenimientoModel>(
                "ConsultarMantenimientoPorId",
                parameters,
                commandType: CommandType.StoredProcedure);
        }

        public int RegistrarMantenimiento(MantenimientoModel model)
        {
            using var context = new SqlConnection(_connection);
            var parameters = new DynamicParameters();
            parameters.Add("@IdEquipo", model.IdEquipo);
            parameters.Add("@FechaMantenimiento", model.FechaMantenimiento);
            parameters.Add("@TipoMantenimiento", model.TipoMantenimiento);
            parameters.Add("@Descripcion", model.Descripcion);
            parameters.Add("@Costo", model.Costo);
            parameters.Add("@Responsable", model.Responsable);
            return context.ExecuteScalar<int>(
                "RegistrarMantenimiento",
                parameters,
                commandType: CommandType.StoredProcedure);
        }

        public int ActualizarMantenimiento(MantenimientoModel model)
        {
            using var context = new SqlConnection(_connection);
            var parameters = new DynamicParameters();
            parameters.Add("@IdMantenimiento", model.IdMantenimiento);
            parameters.Add("@IdEquipo", model.IdEquipo);
            parameters.Add("@FechaMantenimiento", model.FechaMantenimiento);
            parameters.Add("@TipoMantenimiento", model.TipoMantenimiento);
            parameters.Add("@Descripcion", model.Descripcion);
            parameters.Add("@Costo", model.Costo);
            parameters.Add("@Responsable", model.Responsable);
            return context.ExecuteScalar<int>(
                "ActualizarMantenimiento",
                parameters,
                commandType: CommandType.StoredProcedure);
        }

        public int CambiarEstadoMantenimiento(int idMantenimiento, bool estado)
        {
            using var context = new SqlConnection(_connection);
            var parameters = new DynamicParameters();
            parameters.Add("@IdMantenimiento", idMantenimiento);
            parameters.Add("@Estado", estado);
            return context.ExecuteScalar<int>(
                "CambiarEstadoMantenimiento",
                parameters,
                commandType: CommandType.StoredProcedure);
        }

        public List<MantenimientoModel> ReporteMantenimientos(ReporteMantenimientoModel filtro)
        {
            using var context = new SqlConnection(_connection);
            var parameters = new DynamicParameters();
            parameters.Add("@FechaInicio", filtro.FechaInicio);
            parameters.Add("@FechaFin", filtro.FechaFin);
            parameters.Add("@IdEquipo", filtro.IdEquipo);
            parameters.Add("@TipoMantenimiento",
                string.IsNullOrEmpty(filtro.TipoMantenimiento) ? null : filtro.TipoMantenimiento);
            return context.Query<MantenimientoModel>(
                "ReporteMantenimientos",
                parameters,
                commandType: CommandType.StoredProcedure).ToList();
        }
    }
}
