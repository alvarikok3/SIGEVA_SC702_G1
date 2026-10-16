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
    public class CitaController(IConfiguration _config) : ControllerBase
    {
        [Authorize]
        [HttpGet]
        [Route("ConsultarCitas")]
        public IActionResult ConsultarCitas()
        {
            using var context = new SqlConnection(_config["ConnectionStrings:DefaultConnection"]);

            var response = context.Query<CitaModel>(
                "ConsultarCitas",
                commandType: CommandType.StoredProcedure);

            return Ok(response);
        }

        [Authorize]
        [HttpGet]
        [Route("ConsultarCitaPorId/{idCita}")]
        public IActionResult ConsultarCitaPorId(int idCita)
        {
            using var context = new SqlConnection(_config["ConnectionStrings:DefaultConnection"]);

            var parameters = new DynamicParameters();
            parameters.Add("@IdCita", idCita);

            var response = context.QueryFirstOrDefault<CitaModel>(
                "ConsultarCitaPorId",
                parameters,
                commandType: CommandType.StoredProcedure);

            if (response != null)
                return Ok(response);

            return NotFound("Cita no encontrada.");
        }

        [Authorize]
        [HttpGet]
        [Route("ConsultarCitasPorFecha")]
        public IActionResult ConsultarCitasPorFecha([FromQuery] DateTime fecha)
        {
            using var context = new SqlConnection(_config["ConnectionStrings:DefaultConnection"]);

            var parameters = new DynamicParameters();
            parameters.Add("@Fecha", fecha.Date);

            var response = context.Query<CitaModel>(
                "ConsultarCitasPorFecha",
                parameters,
                commandType: CommandType.StoredProcedure);

            return Ok(response);
        }


        [Authorize]
        [HttpPost]
        [Route("RegistrarCita")]
        public IActionResult RegistrarCita(CitaModel model)
        {
            if (model == null)
            {
                return BadRequest("Los datos de la cita son obligatorios.");
            }

            if (model.IdPaciente <= 0)
            {
                return BadRequest("Debe seleccionar un paciente válido.");
            }

            if (model.IdUsuario <= 0)
            {
                return BadRequest("Debe seleccionar un médico o responsable válido.");
            }

            if (model.FechaHora <= DateTime.Now)
            {
                return BadRequest("La fecha y hora de la cita deben ser futuras.");
            }
            // Obtener el usuario que inició sesión desde el JWT.
            var idUsuarioClaim = User.FindFirst("IdUsuario")?.Value;

            // Comprobar que el identificador sea válido.
            if (!int.TryParse(idUsuarioClaim, out int idUsuarioAutenticado))
            {
                return Unauthorized(
                    "No se pudo identificar al usuario autenticado.");
            }

            // Obtener el rol del usuario autenticado.
            var idRolClaim = User.FindFirst("IdRol")?.Value;

            if (!int.TryParse(idRolClaim, out int idRolAutenticado))
            {
                return StatusCode(
                    403,
                    "No se pudo identificar el rol del usuario.");
            }
            using var context = new SqlConnection(
                _config["ConnectionStrings:DefaultConnection"]);

            var parameters = new DynamicParameters();

            parameters.Add("@IdPaciente", model.IdPaciente);
            parameters.Add("@IdUsuario", model.IdUsuario);
            parameters.Add("@FechaHora", model.FechaHora);
            parameters.Add(
                "@EstadoCita",
                string.IsNullOrWhiteSpace(model.EstadoCita)
                    ? "Programada"
                    : model.EstadoCita);
            parameters.Add("@Observaciones", model.Observaciones);

            var response = context.ExecuteScalar<int>(
                "dbo.RegistrarCita",
                parameters,
                commandType: CommandType.StoredProcedure);

            if (response == 1)
            {
                return Ok("Cita registrada correctamente.");
            }

            if (response == -1)
            {
                return Conflict(
                    "El médico ya tiene una cita programada o confirmada en ese horario.");
            }

            return BadRequest(
                "No se pudo registrar la cita. Verifique que el paciente y el médico estén activos.");
        }


        [Authorize]
        [HttpPost]
        [Route("ProgramarCitaAdministrativa")]
        public IActionResult ProgramarCitaAdministrativa(
            [FromBody] CitaModel model)
        {
            // 1. Validar los datos recibidos.
            if (model == null)
            {
                return BadRequest("Los datos de la cita son obligatorios.");
            }

            if (model.IdPaciente <= 0)
            {
                return BadRequest("Debe seleccionar un paciente válido.");
            }

            if (model.IdUsuario <= 0)
            {
                return BadRequest("Debe seleccionar un médico válido.");
            }

            if (model.FechaHora <= DateTime.Now)
            {
                return BadRequest("La fecha y hora deben ser futuras.");
            }

            // 2. Obtener el usuario autenticado desde el JWT.
            var idUsuarioClaim = User.FindFirst("IdUsuario")?.Value;
            var idRolClaim = User.FindFirst("IdRol")?.Value;

            if (!int.TryParse(idUsuarioClaim, out int idUsuarioAutenticado))
            {
                return Unauthorized(
                    "No se pudo identificar al usuario autenticado.");
            }

            if (!int.TryParse(idRolClaim, out int idRolAutenticado))
            {
                return StatusCode(403, "No se pudo identificar el rol.");
            }

            // 3. Verificar permisos administrativos.
            if (idRolAutenticado != 1)
            {
                return StatusCode(
                    403,
                    "No tiene permisos para programar citas médicas.");
            }

            // 4. Conectar con SQL Server.
            using var context = new SqlConnection(
                _config["ConnectionStrings:DefaultConnection"]);

            var parameters = new DynamicParameters();

            parameters.Add("@IdPaciente", model.IdPaciente);
            parameters.Add("@IdUsuario", model.IdUsuario);
            parameters.Add("@IdUsuarioAdministrativo", idUsuarioAutenticado);
            parameters.Add("@FechaHora", model.FechaHora);
            parameters.Add("@Observaciones", model.Observaciones);

            // 5. Ejecutar el procedimiento almacenado.
            var response = context.ExecuteScalar<int>(
                "dbo.ProgramarCitaAdministrativa",
                parameters,
                commandType: CommandType.StoredProcedure);

            // 6. Interpretar el resultado.
            if (response == 1)
            {
                return Ok(
                    "Cita programada correctamente y registrada en auditoría.");
            }

            if (response == -1)
            {
                return Conflict(
                    "El médico ya tiene una cita en ese horario.");
            }

            if (response == -2)
            {
                return StatusCode(
                    403,
                    "El usuario no tiene permisos administrativos.");
            }

            return BadRequest(
                "No se pudo programar la cita. Verifique los datos ingresados.");
        }



        [Authorize]
        [HttpPut]
        [Route("CambiarEstadoCita/{idCita}/{estadoCita}")]
        public IActionResult CambiarEstadoCita(int idCita, string estadoCita)
        {
            using var context = new SqlConnection(_config["ConnectionStrings:DefaultConnection"]);

            var parameters = new DynamicParameters();
            parameters.Add("@IdCita", idCita);
            parameters.Add("@EstadoCita", estadoCita);

            var response = context.ExecuteScalar<int>(
                "CambiarEstadoCita",
                parameters,
                commandType: CommandType.StoredProcedure);

            if (response > 0)
                return Ok("Estado de la cita actualizado correctamente.");

            return BadRequest("No se pudo actualizar el estado de la cita.");
        }

        #region Reporte

        [Authorize]
        [HttpPost]
        [Route("ReporteCitas")]
        public IActionResult ReporteCitas([FromBody] ReporteCitasModel filtro)
        {
            using var context = new SqlConnection(_config["ConnectionStrings:DefaultConnection"]);

            var parameters = new DynamicParameters();
            parameters.Add("@FechaInicio", filtro.FechaInicio);
            parameters.Add("@FechaFin", filtro.FechaFin);
            parameters.Add("@EstadoCita", string.IsNullOrEmpty(filtro.EstadoCita) ? null : filtro.EstadoCita);

            var response = context.Query<CitaModel>(
                "ReporteCitas",
                parameters,
                commandType: CommandType.StoredProcedure);

            return Ok(response);
        }

        #endregion
    }
}
