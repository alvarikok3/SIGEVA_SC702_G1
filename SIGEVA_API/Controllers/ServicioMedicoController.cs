
using Dapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using SIGEVA_API.Models;
using System.Data;

namespace SIGEVA_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ServicioMedicoController : ControllerBase
    {
        private readonly IConfiguration _config;

        public ServicioMedicoController(IConfiguration config)
        {
            _config = config;
        }

        // HU-CAT-002
        // CA-01: Buscar servicios por nombre o especialidad.
        // CA-02: Filtrar servicios por categoría.
        // CA-03: Devolver lista vacía cuando no hay resultados.

        [Authorize]
        [HttpGet("ConsultarCatalogoServicios")]
        public IActionResult ConsultarCatalogoServicios(
            [FromQuery] string? busqueda = null,
            [FromQuery] int? idCategoria = null)
        {
            // Validar la categoría recibida.
            if (idCategoria.HasValue && idCategoria.Value <= 0)
            {
                return BadRequest(
                    "La categoría seleccionada no es válida.");
            }

            // Limitar la longitud de la búsqueda.
            if (busqueda != null && busqueda.Length > 150)
            {
                return BadRequest(
                    "La búsqueda no puede superar los 150 caracteres.");
            }

            var parametros = new DynamicParameters();

            parametros.Add(
                "@Busqueda",
                string.IsNullOrWhiteSpace(busqueda)
                    ? null
                    : busqueda.Trim(),
                DbType.String,
                size: 150);

            parametros.Add(
                "@IdCategoria",
                idCategoria,
                DbType.Int32);

            try
            {
                using var db = new SqlConnection(
                    _config["ConnectionStrings:DefaultConnection"]);

                var servicios = db.Query<ServicioMedicoModel>(
                    "dbo.ConsultarCatalogoServicios",
                    parametros,
                    commandType: CommandType.StoredProcedure
                ).ToList();

                return Ok(servicios);
            }
            catch (SqlException)
            {
                return StatusCode(
                    500,
                    "No fue posible consultar el catálogo de servicios médicos.");
            }
        }

        // HU-CAT-002
        // Obtener categorías activas de servicios médicos.

        [Authorize]
        [HttpGet("ConsultarCategoriasServicios")]
        public IActionResult ConsultarCategoriasServicios()
        {
            try
            {
                using var db = new SqlConnection(
                    _config["ConnectionStrings:DefaultConnection"]);

                var categorias = db.Query(
                    @"SELECT DISTINCT
                          C.IdCategoria,
                          C.Nombre
                      FROM dbo.Categorias AS C
                      INNER JOIN dbo.ServiciosMedicos AS S
                          ON S.IdCategoria = C.IdCategoria
                      WHERE C.Estado = 1
                        AND S.Estado = 1
                      ORDER BY C.Nombre"
                ).ToList();

                return Ok(categorias);
            }
            catch (SqlException)
            {
                return StatusCode(
                    500,
                    "No fue posible consultar las categorías de servicios.");
            }
        }
    }
}
