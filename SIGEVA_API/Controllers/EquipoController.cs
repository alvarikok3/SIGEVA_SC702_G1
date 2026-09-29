using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SIGEVA_API.Interfaces;
using SIGEVA_API.Models;

namespace SIGEVA_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EquipoController(IEquipoData _equipo) : ControllerBase
    {
        #region Consultar

        [Authorize]
        [HttpGet]
        [Route("ConsultarEquipos")]
        public IActionResult ConsultarEquipos()
        {
            return Ok(_equipo.ConsultarEquipos());
        }

        [Authorize]
        [HttpGet]
        [Route("ConsultarEquipoPorId/{idEquipo}")]
        public IActionResult ConsultarEquipoPorId(int idEquipo)
        {
            var equipo = _equipo.ConsultarEquipoPorId(idEquipo);
            if (equipo != null)
                return Ok(equipo);

            return NotFound("Equipo no encontrado.");
        }

        #endregion

        #region Registrar

        [Authorize]
        [HttpPost]
        [Route("RegistrarEquipo")]
        public IActionResult RegistrarEquipo(EquipoMedicoModel model)
        {
            var resultado = _equipo.RegistrarEquipo(model);
            if (resultado > 0)
                return Ok("Equipo registrado correctamente.");

            return BadRequest("No se pudo registrar el equipo. Verifique que el número de serie no esté repetido.");
        }

        #endregion

        #region Actualizar

        [Authorize]
        [HttpPut]
        [Route("ActualizarEquipo")]
        public IActionResult ActualizarEquipo(EquipoMedicoModel model)
        {
            var resultado = _equipo.ActualizarEquipo(model);
            if (resultado > 0)
                return Ok("Equipo actualizado correctamente.");

            return BadRequest("No se pudo actualizar el equipo.");
        }

        [Authorize]
        [HttpPut]
        [Route("CambiarEstadoEquipo/{idEquipo}/{estado}")]
        public IActionResult CambiarEstadoEquipo(int idEquipo, bool estado)
        {
            var resultado = _equipo.CambiarEstadoEquipo(idEquipo, estado);
            if (resultado > 0)
                return Ok("Estado del equipo actualizado correctamente.");

            return BadRequest("No se pudo actualizar el estado del equipo.");
        }

        #endregion
    }
}
