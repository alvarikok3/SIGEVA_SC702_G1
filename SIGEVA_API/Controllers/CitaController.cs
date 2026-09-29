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
            using var context = new SqlConnection(_config["ConnectionStrings:DefaultConnection"]);

            var parameters = new DynamicParameters();
            parameters.Add("@IdPaciente", model.IdPaciente);
            parameters.Add("@IdUsuario", model.IdUsuario);
            parameters.Add("@FechaHora", model.FechaHora);
            parameters.Add("@EstadoCita", string.IsNullOrWhiteSpace(model.EstadoCita) ? "Programada" : model.EstadoCita);
            parameters.Add("@Observaciones", model.Observaciones);

            var response = context.ExecuteScalar<int>(
                "RegistrarCita",
                parameters,
                commandType: CommandType.StoredProcedure);

            if (response > 0)
                return Ok("Cita registrada correctamente.");

            return BadRequest("No se pudo registrar la cita. Verifique el paciente y el médico.");
        }

        [Authorize]
        [HttpPut]
        [Route("ActualizarCita")]
        public IActionResult ActualizarCita(CitaModel model)
        {
            using var context = new SqlConnection(_config["ConnectionStrings:DefaultConnection"]);

            var parameters = new DynamicParameters();
            parameters.Add("@IdCita", model.IdCita);
            parameters.Add("@IdPaciente", model.IdPaciente);
            parameters.Add("@IdUsuario", model.IdUsuario);
            parameters.Add("@FechaHora", model.FechaHora);
            parameters.Add("@EstadoCita", model.EstadoCita);
            parameters.Add("@Observaciones", model.Observaciones);

            var response = context.ExecuteScalar<int>(
                "ActualizarCita",
                parameters,
                commandType: CommandType.StoredProcedure);

            if (response > 0)
                return Ok("Cita actualizada correctamente.");

            return BadRequest("No se pudo actualizar la cita. Verifique los datos.");
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
