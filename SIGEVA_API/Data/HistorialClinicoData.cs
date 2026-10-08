using System.Data;
using Dapper;
using Microsoft.Data.SqlClient;
using SIGEVA_API.Interfaces;
using SIGEVA_API.Models;

namespace SIGEVA_API.Data
{
    public class HistorialClinicoData : IHistorialClinicoData
    {
        private readonly string _connection;

        public HistorialClinicoData(IConfiguration configuration)
        {
            _connection =
                configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException(
                    "No se encontró la conexión DefaultConnection.");
        }

        public bool TienePermisoConsulta(int idUsuario)
        {
            using var context = new SqlConnection(_connection);

            return context.ExecuteScalar<bool>(
                "dbo.ValidarPermisoHistorialClinico",
                new { IdUsuario = idUsuario },
                commandType: CommandType.StoredProcedure);
        }

        public HistorialClinicoModel? ConsultarHistorial(int idPaciente)
        {
            using var context = new SqlConnection(_connection);

            using var resultados = context.QueryMultiple(
                "dbo.ConsultarHistorialClinico",
                new { IdPaciente = idPaciente },
                commandType: CommandType.StoredProcedure);

            var historial =
                resultados.ReadFirstOrDefault<HistorialClinicoModel>();

            var registros = resultados
                .Read<HistorialClinicoRegistroModel>()
                .ToList();

            if (historial == null)
                return null;

            historial.Registros = registros;

            return historial;
        }

        public int RegistrarConsultaAuditoria(
            int idUsuario,
            int idPaciente)
        {
            using var context = new SqlConnection(_connection);

            return context.ExecuteScalar<int>(
                "dbo.RegistrarAuditoria",
                new
                {
                    IdUsuario = idUsuario,
                    Modulo = "Historial clínico",
                    Accion = "Consultar historial",
                    Descripcion =
                        $"Consulta del historial clínico del paciente {idPaciente}.",
                    RegistroAfectado = idPaciente.ToString()
                },
                commandType: CommandType.StoredProcedure);
        }
    }
}