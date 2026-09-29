using System.Data;
using Dapper;
using Microsoft.Data.SqlClient;
using SIGEVA_API.Entities;
using SIGEVA_API.Interfaces;
using SIGEVA_API.Models;

namespace SIGEVA_API.Data
{
    public class UsuarioData : IUsuarioData
    {
        private readonly string _connection;

        public UsuarioData(IConfiguration configuration)
        {
            _connection = configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException(
                    "No se encontró la cadena de conexión DefaultConnection.");
        }

        public int RegistrarUsuario(RegistroUsuarioModel model)
        {
            using (var context = new SqlConnection(_connection))
            {
                var parametros = new
                {
                    model.IdRol,
                    model.Identificacion,
                    model.Nombre,
                    model.Correo,
                    model.Contrasena
                };

                return context.ExecuteScalar<int>(
                    "RegistrarUsuario",
                    parametros,
                    commandType: CommandType.StoredProcedure);
            }
        }

        public Usuario? ValidarUsuario(LoginModel model)
        {
            using (var context = new SqlConnection(_connection))
            {
                var parametros = new
                {
                    model.Correo,
                    model.Contrasena
                };

                return context.QueryFirstOrDefault<Usuario>(
                    "ValidarUsuario",
                    parametros,
                    commandType: CommandType.StoredProcedure);
            }
        }

        public List<Rol> ConsultarRoles()
        {
            using (var context = new SqlConnection(_connection))
            {
                return context.Query<Rol>(
                    "ConsultarRoles",
                    commandType: CommandType.StoredProcedure
                ).ToList();
            }
        }

        public List<UsuarioConsultaModel> ConsultarUsuarios()
        {
            using (var context = new SqlConnection(_connection))
            {
                return context.Query<UsuarioConsultaModel>(
                    "ConsultarUsuarios",
                    commandType: CommandType.StoredProcedure
                ).ToList();
            }
        }

        public UsuarioConsultaModel? ConsultarUsuarioPorId(int idUsuario)
        {
            using (var context = new SqlConnection(_connection))
            {
                var parametros = new
                {
                    IdUsuario = idUsuario
                };

                return context.QueryFirstOrDefault<UsuarioConsultaModel>(
                    "ConsultarUsuarioPorId",
                    parametros,
                    commandType: CommandType.StoredProcedure);
            }
        }

        public int ActualizarUsuario(UsuarioConsultaModel model)
        {
            using (var context = new SqlConnection(_connection))
            {
                var parametros = new
                {
                    model.IdUsuario,
                    model.IdRol,
                    model.Identificacion,
                    model.Nombre,
                    model.Correo
                };

                return context.ExecuteScalar<int>(
                    "ActualizarUsuario",
                    parametros,
                    commandType: CommandType.StoredProcedure);
            }
        }

        public int CambiarEstadoUsuario(int idUsuario, bool estado)
        {
            using (var context = new SqlConnection(_connection))
            {
                var parametros = new
                {
                    IdUsuario = idUsuario,
                    Estado = estado
                };

                return context.ExecuteScalar<int>(
                    "CambiarEstadoUsuario",
                    parametros,
                    commandType: CommandType.StoredProcedure);
            }
        }

        public int CambiarContrasena(CambiarContrasenaModel model)
        {
            using (var context = new SqlConnection(_connection))
            {
                var parametros = new
                {
                    model.IdUsuario,
                    model.ContrasenaActual,
                    model.ContrasenaNueva
                };

                return context.ExecuteScalar<int>(
                    "CambiarContrasena",
                    parametros,
                    commandType: CommandType.StoredProcedure);
            }
        }

        public UsuarioConsultaModel? ConsultarUsuarioPorCorreo(string correo)
        {
            using (var context = new SqlConnection(_connection))
            {
                var parametros = new
                {
                    Correo = correo
                };

                return context.QueryFirstOrDefault<UsuarioConsultaModel>(
                    "ConsultarUsuarioPorCorreo",
                    parametros,
                    commandType: CommandType.StoredProcedure);
            }
        }

        public int RestablecerContrasenaTemporal(
            string correo,
            string contrasenaTemporal)
        {
            using (var context = new SqlConnection(_connection))
            {
                var parametros = new
                {
                    Correo = correo,
                    ContrasenaTemporal = contrasenaTemporal
                };

                return context.ExecuteScalar<int>(
                    "RestablecerContrasenaTemporal",
                    parametros,
                    commandType: CommandType.StoredProcedure);
            }
        }
    }
}