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
    public class PacienteController(IConfiguration _config) : ControllerBase
    {
        [Authorize]
        [HttpGet]
        [Route("ConsultarPacientes")]
        public IActionResult ConsultarPacientes()
        {
            using var context = new SqlConnection(_config["ConnectionStrings:DefaultConnection"]);

            var response = context.Query<PacienteModel>(
                "ConsultarPacientes",
                commandType: CommandType.StoredProcedure);

            return Ok(response);
        }

        [Authorize]
        [HttpGet]
        [Route("ConsultarPacientePorId/{idPaciente}")]
        public IActionResult ConsultarPacientePorId(int idPaciente)
        {
            using var context = new SqlConnection(_config["ConnectionStrings:DefaultConnection"]);

            var parameters = new DynamicParameters();
            parameters.Add("@IdPaciente", idPaciente);

            var response = context.QueryFirstOrDefault<PacienteModel>(
                "ConsultarPacientePorId",
                parameters,
                commandType: CommandType.StoredProcedure);

            if (response != null)
                return Ok(response);

            return NotFound("Paciente no encontrado.");
        }

        [Authorize]
        [HttpPost]
        [Route("RegistrarPaciente")]
        public IActionResult RegistrarPaciente(PacienteModel model)
        {
            using var context = new SqlConnection(_config["ConnectionStrings:DefaultConnection"]);

            var parameters = new DynamicParameters();
            parameters.Add("@Identificacion", model.Identificacion);
            parameters.Add("@Nombre", model.Nombre);
            parameters.Add("@Telefono", model.Telefono);
            parameters.Add("@Correo", model.Correo);
            parameters.Add("@Direccion", model.Direccion);
            parameters.Add("@FechaNacimiento", model.FechaNacimiento);

            var response = context.ExecuteScalar<int>(
                "RegistrarPaciente",
                parameters,
                commandType: CommandType.StoredProcedure);

            if (response > 0)
                return Ok("Paciente registrado correctamente.");

            return BadRequest("No se pudo registrar el paciente. Verifique que la identificación o el correo no estén repetidos.");
        }

        [Authorize]
        [HttpPut]
        [Route("ActualizarPaciente")]
        public IActionResult ActualizarPaciente(PacienteModel model)
        {
            using var context = new SqlConnection(_config["ConnectionStrings:DefaultConnection"]);

            var parameters = new DynamicParameters();
            parameters.Add("@IdPaciente", model.IdPaciente);
            parameters.Add("@Identificacion", model.Identificacion);
            parameters.Add("@Nombre", model.Nombre);
            parameters.Add("@Telefono", model.Telefono);
            parameters.Add("@Correo", model.Correo);
            parameters.Add("@Direccion", model.Direccion);
            parameters.Add("@FechaNacimiento", model.FechaNacimiento);

            var response = context.ExecuteScalar<int>(
                "ActualizarPaciente",
                parameters,
                commandType: CommandType.StoredProcedure);

            if (response > 0)
                return Ok("Paciente actualizado correctamente.");

            return BadRequest("No se pudo actualizar el paciente. Verifique los datos.");
        }

        [Authorize]
        [HttpPut]
        [Route("CambiarEstadoPaciente/{idPaciente}/{estado}")]
        public IActionResult CambiarEstadoPaciente(int idPaciente, bool estado)
        {
            using var context = new SqlConnection(_config["ConnectionStrings:DefaultConnection"]);

            var parameters = new DynamicParameters();
            parameters.Add("@IdPaciente", idPaciente);
            parameters.Add("@Estado", estado);

            var response = context.ExecuteScalar<int>(
                "CambiarEstadoPaciente",
                parameters,
                commandType: CommandType.StoredProcedure);

            if (response > 0)
                return Ok("Estado del paciente actualizado correctamente.");

            return BadRequest("No se pudo actualizar el estado del paciente.");
        }

        #region Reporte

        [Authorize]
        [HttpPost]
        [Route("ReportePacientes")]
        public IActionResult ReportePacientes([FromBody] ReportePacientesModel filtro)
        {
            using var context = new SqlConnection(_config["ConnectionStrings:DefaultConnection"]);

            var parameters = new DynamicParameters();
            parameters.Add("@Nombre", string.IsNullOrWhiteSpace(filtro.Nombre) ? null : filtro.Nombre);
            parameters.Add("@Estado", filtro.Estado);

            var response = context.Query<PacienteModel>(
                "ReportePacientes",
                parameters,
                commandType: CommandType.StoredProcedure);

            return Ok(response);
        }

        #endregion
    }
}
