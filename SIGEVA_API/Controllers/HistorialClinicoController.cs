using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SIGEVA_API.Interfaces;

namespace SIGEVA_API.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class HistorialClinicoController : ControllerBase
    {
        private readonly IHistorialClinicoData _historialData;

        public HistorialClinicoController(
            IHistorialClinicoData historialData)
        {
            _historialData = historialData;
        }

        [HttpGet("Consultar/{idPaciente:int}")]
        [ResponseCache(
            Duration = 0,
            Location = ResponseCacheLocation.None,
            NoStore = true)]
        public IActionResult Consultar(int idPaciente)
        {
            var identificador = User.FindFirst("IdUsuario")?.Value;

            if (!int.TryParse(identificador, out var idUsuario)
                || idUsuario <= 0)
            {
                return Unauthorized(new
                {
                    mensaje = "Debe iniciar sesión nuevamente."
                });
            }

            if (!_historialData.TienePermisoConsulta(idUsuario))
            {
                return StatusCode(StatusCodes.Status403Forbidden, new
                {
                    mensaje =
                        "No tiene permisos suficientes para consultar expedientes clínicos."
                });
            }

            if (idPaciente <= 0)
            {
                return BadRequest(new
                {
                    mensaje = "El identificador del paciente no es válido."
                });
            }

            var historial =
                _historialData.ConsultarHistorial(idPaciente);

            if (historial == null)
            {
                return NotFound(new
                {
                    mensaje = "El paciente no existe."
                });
            }

            var idAuditoria =
                _historialData.RegistrarConsultaAuditoria(
                    idUsuario,
                    idPaciente);

            if (idAuditoria <= 0)
            {
                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    new
                    {
                        mensaje =
                            "No fue posible registrar la consulta en la auditoría."
                    });
            }

            return Ok(historial);
        }
    }
}