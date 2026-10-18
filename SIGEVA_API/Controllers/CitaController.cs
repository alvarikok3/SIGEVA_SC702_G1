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
            // HU-CIT-003: Solo el administrador
            // puede consultar la agenda general.

            var idRolClaim = User.FindFirst("IdRol")?.Value;

            if (!int.TryParse(idRolClaim, out int idRol))
            {
                return StatusCode(
                    403,
                    "No se pudo identificar el rol del usuario.");
            }

            if (idRol != 1)
            {
                return StatusCode(
                    403,
                    "No tiene permisos para consultar todas las citas.");
            }

            using var context = new SqlConnection(
                _config["ConnectionStrings:DefaultConnection"]);

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
            // Validar el identificador de la cita.
            if (idCita <= 0)
            {
                return BadRequest("El identificador de la cita no es válido.");
            }

            // Obtener identidad y rol desde el JWT.
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

            // Solo Administrador y Médico.
            if (idRolAutenticado != 1 && idRolAutenticado != 3)
            {
                return StatusCode(
                    403,
                    "No tiene permisos para consultar citas médicas.");
            }

            using var context = new SqlConnection(
                _config["ConnectionStrings:DefaultConnection"]);

            var parameters = new DynamicParameters();
            parameters.Add("@IdCita", idCita);

            var cita = context.QueryFirstOrDefault<CitaModel>(
                "ConsultarCitaPorId",
                parameters,
                commandType: CommandType.StoredProcedure);

            if (cita == null)
            {
                return NotFound("Cita no encontrada.");
            }

            // El médico solo puede consultar sus citas.
            if (idRolAutenticado == 3 &&
                cita.IdUsuario != idUsuarioAutenticado)
            {
                return NotFound("Cita no encontrada.");
            }

            return Ok(cita);
        }



        [Authorize]
        [HttpGet]
        [Route("ConsultarCitasPorFecha")]
        public IActionResult ConsultarCitasPorFecha(
            [FromQuery] DateTime fecha)
        {
            // 1. Validar la fecha.
            if (fecha == default)
            {
                return BadRequest("Debe indicar una fecha válida.");
            }

            // 2. Obtener el usuario y rol autenticados.
            var idUsuarioClaim = User.FindFirst("IdUsuario")?.Value;
            var idRolClaim = User.FindFirst("IdRol")?.Value;

            if (!int.TryParse(idUsuarioClaim, out int idUsuarioAutenticado))
            {
                return Unauthorized(
                    "No se pudo identificar al usuario autenticado.");
            }

            if (!int.TryParse(idRolClaim, out int idRolAutenticado))
            {
                return StatusCode(
                    403,
                    "No se pudo identificar el rol del usuario.");
            }

            // 3. Validar los permisos.
            if (idRolAutenticado != 1 && idRolAutenticado != 3)
            {
                return StatusCode(
                    403,
                    "No tiene permisos para consultar el calendario.");
            }

            // 4. Preparar los parámetros.
            var parameters = new DynamicParameters();

            parameters.Add("@Fecha", fecha.Date, DbType.Date);

            // 5. Ejecutar la consulta.
            using var context = new SqlConnection(
                _config["ConnectionStrings:DefaultConnection"]);

            // El administrador consulta la agenda general.
            if (idRolAutenticado == 1)
            {
                var citas = context.Query<CitaModel>(
                    "dbo.ConsultarCitasPorFecha",
                    parameters,
                    commandType: CommandType.StoredProcedure
                ).ToList();

                return Ok(citas);
            }

            // El médico consulta únicamente su agenda.
            // Utilizamos el procedimiento filtrado que ya probamos.
            parameters.Add(
                "@IdUsuario",
                idUsuarioAutenticado,
                DbType.Int32);

            parameters.Add(
                "@IdPaciente",
                null,
                DbType.Int32);

            var citasMedico = context.Query<CitaModel>(
                "dbo.ConsultarCitasFiltradas",
                parameters,
                commandType: CommandType.StoredProcedure
            ).ToList();

            return Ok(citasMedico);
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



        #region Reporte


        [Authorize]
        [HttpPut]
        [Route("ActualizarEstadoCitaAdministrativa/{idCita}/{estadoNuevo}")]
        public IActionResult ActualizarEstadoCitaAdministrativa(
            int idCita,
            string estadoNuevo)
        {
            // 1. Validar el identificador de la cita.
            if (idCita <= 0)
            {
                return BadRequest(
                    "El identificador de la cita no es válido.");
            }

            // 2. Obtener el usuario autenticado desde el JWT.
            var idUsuarioClaim = User.FindFirst("IdUsuario")?.Value;
            var idRolClaim = User.FindFirst("IdRol")?.Value;

            if (!int.TryParse(
                idUsuarioClaim,
                out int idUsuarioAutenticado))
            {
                return Unauthorized(
                    "No se pudo identificar al usuario autenticado.");
            }

            if (!int.TryParse(
                idRolClaim,
                out int idRolAutenticado))
            {
                return StatusCode(
                    403,
                    "No se pudo identificar el rol del usuario.");
            }

            // 3. Solo el administrador puede cambiar estados.
            if (idRolAutenticado != 1)
            {
                return StatusCode(
                    403,
                    "No tiene permisos para actualizar estados de citas.");
            }

            // 4. Validar el estado solicitado.
            if (estadoNuevo != "Confirmada" &&
                estadoNuevo != "Cancelada")
            {
                return BadRequest(
                    "Estado inválido. Solo se permite confirmar o cancelar citas.");
            }

            // 5. Preparar los parámetros del procedimiento.
            var parameters = new DynamicParameters();

            parameters.Add("@IdCita", idCita, DbType.Int32);

            parameters.Add(
                "@EstadoNuevo",
                estadoNuevo,
                DbType.String,
                size: 20);

            parameters.Add(
                "@IdUsuarioAdministrativo",
                idUsuarioAutenticado,
                DbType.Int32);

            // 6. Ejecutar el procedimiento almacenado.
            using var context = new SqlConnection(
                _config["ConnectionStrings:DefaultConnection"]);

            var resultado = context.ExecuteScalar<int>(
                "dbo.ActualizarEstadoCitaAdministrativa",
                parameters,
                commandType: CommandType.StoredProcedure);

            // 7. Interpretar el resultado.
            switch (resultado)
            {
                case 1:
                    return Ok(
                        "Estado actualizado correctamente y registrado en auditoría.");

                case -1:
                    return NotFound(
                        "La cita médica no existe.");

                case -2:
                    return StatusCode(
                        403,
                        "El usuario no tiene permisos administrativos.");

                case -3:
                    return BadRequest(
                        "El estado solicitado no es válido.");

                case -4:
                    return Conflict(
                        "No se permite cambiar la cita al estado seleccionado.");

                default:
                    return BadRequest(
                        "No fue posible actualizar el estado de la cita.");
            }
        }


        [Authorize]
        [HttpPost]
        [Route("ReporteCitas")]
        public IActionResult ReporteCitas([FromBody] ReporteCitasModel filtro)
        {
            // 1. Validar permisos del usuario autenticado.
            var idRolClaim = User.FindFirst("IdRol")?.Value;

            if (!int.TryParse(idRolClaim, out int idRol))
            {
                return StatusCode(
                    403,
                    "No se pudo identificar el rol del usuario.");
            }

            // Los reportes generales son exclusivos del administrador.
            if (idRol != 1)
            {
                return StatusCode(
                    403,
                    "No tiene permisos para generar reportes generales de citas.");
            }

            // 2. Validar los filtros.
            if (filtro == null)
            {
                return BadRequest("Debe proporcionar los filtros del reporte.");
            }

            if (filtro.FechaInicio > filtro.FechaFin)
            {
                return BadRequest(
                    "La fecha inicial no puede ser posterior a la fecha final.");
            }

            // 3. Preparar los parámetros.
            var parameters = new DynamicParameters();

            parameters.Add("@FechaInicio", filtro.FechaInicio);
            parameters.Add("@FechaFin", filtro.FechaFin);

            parameters.Add(
                "@EstadoCita",
                string.IsNullOrWhiteSpace(filtro.EstadoCita)
                    ? null
                    : filtro.EstadoCita);

            // 4. Ejecutar el procedimiento existente.
            using var context = new SqlConnection(
                _config["ConnectionStrings:DefaultConnection"]);

            var citas = context.Query<CitaModel>(
                "dbo.ReporteCitas",
                parameters,
                commandType: CommandType.StoredProcedure
            ).ToList();

            return Ok(citas);
        }



        [Authorize]
        [HttpGet]
        [Route("ConsultarCitasFiltradas")]
        public IActionResult ConsultarCitasFiltradas(
            [FromQuery] DateTime? fecha,
            [FromQuery] int? idUsuario,
            [FromQuery] int? idPaciente)
        {
            // 1. Validar los parámetros opcionales.
            if (idUsuario.HasValue && idUsuario.Value <= 0)
            {
                return BadRequest("El médico seleccionado no es válido.");
            }

            if (idPaciente.HasValue && idPaciente.Value <= 0)
            {
                return BadRequest("El paciente seleccionado no es válido.");
            }

            // 2. Identificar al usuario autenticado.
            var idUsuarioClaim = User.FindFirst("IdUsuario")?.Value;
            var idRolClaim = User.FindFirst("IdRol")?.Value;

            if (!int.TryParse(idUsuarioClaim, out int idUsuarioAutenticado))
            {
                return Unauthorized(
                    "No se pudo identificar al usuario autenticado.");
            }

            if (!int.TryParse(idRolClaim, out int idRolAutenticado))
            {
                return StatusCode(
                    403,
                    "No se pudo identificar el rol del usuario.");
            }


            // HU-CIT-003: Control de acceso por rol.

            // Administrador = 1
            // Usuario = 2
            // Médico = 3

            if (idRolAutenticado != 1 && idRolAutenticado != 3)
            {
                return StatusCode(
                    403,
                    "No tiene permisos para consultar esta agenda.");
            }

            // El médico solo puede consultar sus propias citas.
            // No se confía en el IdUsuario recibido desde la URL.
            if (idRolAutenticado == 3)
            {
                idUsuario = idUsuarioAutenticado;
            }


            // 4. Preparar los parámetros de SQL Server.
            var parameters = new DynamicParameters();

            parameters.Add("@Fecha", fecha?.Date, DbType.Date);
            parameters.Add("@IdUsuario", idUsuario, DbType.Int32);
            parameters.Add("@IdPaciente", idPaciente, DbType.Int32);

            // 5. Ejecutar el procedimiento almacenado.
            using var context = new SqlConnection(
                _config["ConnectionStrings:DefaultConnection"]);

            var citas = context.Query<CitaModel>(
                "dbo.ConsultarCitasFiltradas",
                parameters,
                commandType: CommandType.StoredProcedure
            ).ToList();

            // 6. Devolver la lista, incluso cuando esté vacía.
            return Ok(citas);
        }


        [Authorize]
        [HttpPut]
        [Route("ActualizarCita")]
        public IActionResult ActualizarCita([FromBody] CitaModel model)
        {
            // 1. Verificar el usuario y su rol desde el JWT.
            var idUsuarioClaim = User.FindFirst("IdUsuario")?.Value;
            var idRolClaim = User.FindFirst("IdRol")?.Value;

            if (!int.TryParse(idUsuarioClaim, out int idUsuario) ||
                !int.TryParse(idRolClaim, out int idRol))
            {
                return Unauthorized("No se pudo validar la sesión.");
            }

            // Solo el administrador puede editar citas.
            if (idRol != 1)
            {
                return StatusCode(
                    403,
                    "No tiene permisos para editar citas médicas.");
            }

            // 2. Validar los datos recibidos.
            if (model == null || model.IdCita <= 0)
            {
                return BadRequest("La cita seleccionada no es válida.");
            }

            if (model.IdPaciente <= 0 || model.IdUsuario <= 0)
            {
                return BadRequest(
                    "Debe seleccionar un paciente y un médico válidos.");
            }

            // 3. Preparar los parámetros.
            var parameters = new DynamicParameters();

            parameters.Add("@IdCita", model.IdCita);
            parameters.Add("@IdPaciente", model.IdPaciente);
            parameters.Add("@IdUsuario", model.IdUsuario);
            parameters.Add("@FechaHora", model.FechaHora);
            parameters.Add("@Observaciones", model.Observaciones);

            // No enviamos EstadoCita: se gestiona por separado.

            // 4. Ejecutar el procedimiento almacenado.
            using var context = new SqlConnection(
                _config["ConnectionStrings:DefaultConnection"]);

            var resultado = context.ExecuteScalar<int>(
                "dbo.ActualizarCita",
                parameters,
                commandType: CommandType.StoredProcedure);

            // 5. Interpretar el resultado.
            return resultado switch
            {
                1 => Ok("Cita médica actualizada correctamente."),

                -1 => Conflict(
                    "La cita no existe o su estado no permite editarla."),

                -2 => BadRequest(
                    "El paciente seleccionado no existe."),

                -3 => BadRequest(
                    "Debe seleccionar un médico activo."),

                -4 => BadRequest(
                    "La fecha y hora deben ser futuras."),

                -5 => Conflict(
                    "El médico ya tiene una cita en ese horario."),

                _ => BadRequest(
                    "No fue posible actualizar la cita médica.")
            };
        }


        #endregion
    }
}
