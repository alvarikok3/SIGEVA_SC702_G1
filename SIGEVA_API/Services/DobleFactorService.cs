
using System;
using System.Data;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using SIGEVA_API.Entities;

namespace SIGEVA_API.Services
{
    // HU-AUT-004: Autenticación de doble factor
    public class DobleFactorService
    {
        private readonly string _connection;

        public DobleFactorService(IConfiguration configuration)
        {
            _connection = configuration
                .GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException(
                    "No se encontró la cadena de conexión.");
        }

        // ==========================================
        // Generar código de 6 dígitos
        // ==========================================
        public string GenerarCodigo()
        {
            int numero = RandomNumberGenerator.GetInt32(
                0, 1000000);

            return numero.ToString("D6");
        }

        // ==========================================
        // Generar ticket de autenticación
        // ==========================================
        public string GenerarTicket()
        {
            return Convert.ToHexString(
                RandomNumberGenerator.GetBytes(32));
        }

        // ==========================================
        // Calcular hash SHA-256
        // ==========================================
        private static byte[] CalcularHash(string valor)
        {
            return SHA256.HashData(
                Encoding.UTF8.GetBytes(valor));
        }

        // ==========================================
        // Registrar código en SQL Server
        // ==========================================
        public async Task<int> RegistrarCodigoAsync(
            int idUsuario,
            string codigo,
            string ticket)
        {
            if (idUsuario <= 0)
            {
                throw new ArgumentException(
                    "El usuario no es válido.");
            }

            if (string.IsNullOrWhiteSpace(codigo) ||
                codigo.Length != 6 ||
                !codigo.All(char.IsAsciiDigit))
            {
                throw new ArgumentException(
                    "El código debe contener 6 dígitos.");
            }

            if (string.IsNullOrWhiteSpace(ticket))
            {
                throw new ArgumentException(
                    "El ticket es obligatorio.");
            }

            byte[] codigoHash = CalcularHash(
                ticket + ":" + codigo);

            byte[] ticketHash = CalcularHash(ticket);

            DateTime fechaExpiracion =
                DateTime.Now.AddMinutes(5);

            using var conexion =
                new SqlConnection(_connection);

            var parametros = new
            {
                IdUsuario = idUsuario,
                CodigoHash = codigoHash,
                TicketHash = ticketHash,
                FechaExpiracion = fechaExpiracion
            };

            return await conexion.ExecuteScalarAsync<int>(
                "dbo.RegistrarCodigo2FA",
                parametros,
                commandType: CommandType.StoredProcedure);
        }

        // ==========================================
        // Validar código de doble factor
        // ==========================================
        public async Task<int> ValidarCodigoAsync(
            int idUsuario,
            string codigo,
            string ticket)
        {
            if (idUsuario <= 0)
            {
                throw new ArgumentException(
                    "El usuario no es válido.");
            }

            if (string.IsNullOrWhiteSpace(codigo) ||
                codigo.Length != 6 ||
                !codigo.All(char.IsAsciiDigit))
            {
                throw new ArgumentException(
                    "El código debe contener 6 dígitos.");
            }

            if (string.IsNullOrWhiteSpace(ticket))
            {
                throw new ArgumentException(
                    "El ticket es obligatorio.");
            }

            byte[] codigoHash = CalcularHash(
                ticket + ":" + codigo);

            byte[] ticketHash = CalcularHash(ticket);

            using var conexion =
                new SqlConnection(_connection);

            var parametros = new
            {
                IdUsuario = idUsuario,
                CodigoHash = codigoHash,
                TicketHash = ticketHash
            };

            return await conexion.ExecuteScalarAsync<int>(
                "dbo.ValidarCodigo2FA",
                parametros,
                commandType: CommandType.StoredProcedure);
        }

        // ==========================================
        // Recuperar usuario asociado al ticket
        // ==========================================
        public async Task<Usuario?> ObtenerUsuarioPorTicketAsync(
            string ticket)
        {
            if (string.IsNullOrWhiteSpace(ticket) ||
                ticket.Length != 64 ||
                !ticket.All(Uri.IsHexDigit))
            {
                return null;
            }

            byte[] ticketHash = CalcularHash(ticket);

            const string consulta = @"
                SELECT TOP (1)
                    U.IdUsuario,
                    U.IdRol,
                    U.Identificacion,
                    U.Nombre,
                    U.Correo,
                    U.Estado,
                    U.FechaRegistro,
                    U.DobleFactorActivo
                FROM dbo.Codigos2FA C
                INNER JOIN dbo.Usuarios U
                    ON U.IdUsuario = C.IdUsuario
                WHERE C.TicketHash = @TicketHash
                  AND C.Utilizado = 0
                  AND U.Estado = 1
                  AND U.DobleFactorActivo = 1
                ORDER BY C.FechaCreacion DESC;";

            using var conexion =
                new SqlConnection(_connection);

            return await conexion.QueryFirstOrDefaultAsync<Usuario>(
                consulta,
                new
                {
                    TicketHash = ticketHash
                });
        }
    }
}
