using Dapper;
using Microsoft.Data.SqlClient;
using System.Data;

namespace SIGEVA_API.Middleware
{
    /// <summary>
    /// Captura excepciones no controladas, las registra en BitacoraErrores y retorna HTTP 500.
    /// </summary>
    public class ErrorHandlingMiddleware(RequestDelegate next, IConfiguration config)
    {
        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await next(context);
            }
            catch (Exception ex)
            {
                await RegistrarErrorAsync(ex, context.Request.Path);

                context.Response.StatusCode = 500;
                context.Response.ContentType = "application/json";
                await context.Response.WriteAsJsonAsync(new
                {
                    mensaje = "Ocurrió un error interno en el servidor. El incidente ha sido registrado."
                });
            }
        }

        private async Task RegistrarErrorAsync(Exception ex, string origen)
        {
            try
            {
                var connectionString = config["ConnectionStrings:DefaultConnection"];
                using var conn = new SqlConnection(connectionString);

                var mensaje = ex.Message.Length > 500
                    ? ex.Message[..500]
                    : ex.Message;

                var detalle = ex.ToString();

                var origenTrunc = origen.Length > 100
                    ? origen[..100]
                    : origen;

                var parameters = new DynamicParameters();
                parameters.Add("@MensajeError", mensaje);
                parameters.Add("@DetalleError", detalle);
                parameters.Add("@Origen", origenTrunc);

                await conn.ExecuteAsync(
                    "RegistrarError",
                    parameters,
                    commandType: CommandType.StoredProcedure);
            }
            catch
            {
                // Si falla el log no interrumpimos la respuesta al cliente.
            }
        }
    }
}
