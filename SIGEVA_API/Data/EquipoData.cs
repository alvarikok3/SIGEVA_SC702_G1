using System.Data;
using Dapper;
using Microsoft.Data.SqlClient;
using SIGEVA_API.Interfaces;
using SIGEVA_API.Models;

namespace SIGEVA_API.Data
{
    public class EquipoData : IEquipoData
    {
        private readonly string _connection;

        public EquipoData(IConfiguration configuration)
        {
            _connection = configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("No se encontró la cadena de conexión DefaultConnection.");
        }

        public List<EquipoMedicoModel> ConsultarEquipos()
        {
            using var context = new SqlConnection(_connection);
            return context.Query<EquipoMedicoModel>(
                "ConsultarEquipos",
                commandType: CommandType.StoredProcedure).ToList();
        }

        public EquipoMedicoModel? ConsultarEquipoPorId(int idEquipo)
        {
            using var context = new SqlConnection(_connection);
            var parameters = new DynamicParameters();
            parameters.Add("@IdEquipo", idEquipo);
            return context.QueryFirstOrDefault<EquipoMedicoModel>(
                "ConsultarEquipoPorId",
                parameters,
                commandType: CommandType.StoredProcedure);
        }

        public int RegistrarEquipo(EquipoMedicoModel model)
        {
            using var context = new SqlConnection(_connection);
            var parameters = new DynamicParameters();
            parameters.Add("@IdUsuario", model.IdUsuario);
            parameters.Add("@Nombre", model.Nombre);
            parameters.Add("@Descripcion", model.Descripcion);
            parameters.Add("@Marca", model.Marca);
            parameters.Add("@Modelo", model.Modelo);
            parameters.Add("@NumeroSerie", model.NumeroSerie);
            parameters.Add("@FechaAdquisicion", model.FechaAdquisicion);
            return context.ExecuteScalar<int>(
                "RegistrarEquipo",
                parameters,
                commandType: CommandType.StoredProcedure);
        }

        public int ActualizarEquipo(EquipoMedicoModel model)
        {
            using var context = new SqlConnection(_connection);
            var parameters = new DynamicParameters();
            parameters.Add("@IdEquipo", model.IdEquipo);
            parameters.Add("@IdUsuario", model.IdUsuario);
            parameters.Add("@Nombre", model.Nombre);
            parameters.Add("@Descripcion", model.Descripcion);
            parameters.Add("@Marca", model.Marca);
            parameters.Add("@Modelo", model.Modelo);
            parameters.Add("@NumeroSerie", model.NumeroSerie);
            parameters.Add("@FechaAdquisicion", model.FechaAdquisicion);
            return context.ExecuteScalar<int>(
                "ActualizarEquipo",
                parameters,
                commandType: CommandType.StoredProcedure);
        }

        public int CambiarEstadoEquipo(int idEquipo, bool estado)
        {
            using var context = new SqlConnection(_connection);
            var parameters = new DynamicParameters();
            parameters.Add("@IdEquipo", idEquipo);
            parameters.Add("@Estado", estado);
            return context.ExecuteScalar<int>(
                "CambiarEstadoEquipo",
                parameters,
                commandType: CommandType.StoredProcedure);
        }
    }
}
