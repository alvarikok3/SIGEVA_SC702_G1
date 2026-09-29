using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SIGEVA_API.Interfaces;
using SIGEVA_API.Models;

namespace SIGEVA_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MantenimientoController(IMantenimientoData _mantenimiento) : ControllerBase
    {
        #region Consultar

        [Authorize]
        [HttpGet]
        [Route("ConsultarMantenimientos")]
        public IActionResult ConsultarMantenimientos()
        {
            return Ok(_mantenimiento.ConsultarMantenimientos());
        }

        [Authorize]
        [HttpGet]
        [Route("ConsultarMantenimientoPorId/{idMantenimiento}")]
        public IActionResult ConsultarMantenimientoPorId(int idMantenimiento)
        {
            var mantenimiento = _mantenimiento.ConsultarMantenimientoPorId(idMantenimiento);
            if (mantenimiento != null)
                return Ok(mantenimiento);

            return NotFound("Mantenimiento no encontrado.");
        }

        #endregion

        #region Registrar

        [Authorize]
        [HttpPost]
        [Route("RegistrarMantenimiento")]
        public IActionResult RegistrarMantenimiento(MantenimientoModel model)
        {
            var resultado = _mantenimiento.RegistrarMantenimiento(model);
            if (resultado > 0)
                return Ok("Mantenimiento registrado correctamente.");

            return BadRequest("No se pudo registrar el mantenimiento.");
        }

        #endregion

        #region Actualizar

        [Authorize]
        [HttpPut]
        [Route("ActualizarMantenimiento")]
        public IActionResult ActualizarMantenimiento(MantenimientoModel model)
        {
            var resultado = _mantenimiento.ActualizarMantenimiento(model);
            if (resultado > 0)
                return Ok("Mantenimiento actualizado correctamente.");

            return BadRequest("No se pudo actualizar el mantenimiento.");
        }

        [Authorize]
        [HttpPut]
        [Route("CambiarEstadoMantenimiento/{idMantenimiento}/{estado}")]
        public IActionResult CambiarEstadoMantenimiento(int idMantenimiento, bool estado)
        {
            var resultado = _mantenimiento.CambiarEstadoMantenimiento(idMantenimiento, estado);
            if (resultado > 0)
                return Ok("Estado del mantenimiento actualizado correctamente.");

            return BadRequest("No se pudo actualizar el estado del mantenimiento.");
        }

        #endregion

        #region Reporte

        [Authorize]
        [HttpPost]
        [Route("ReporteMantenimientos")]
        public IActionResult ReporteMantenimientos(ReporteMantenimientoModel filtro)
        {
            return Ok(_mantenimiento.ReporteMantenimientos(filtro));
        }

        #endregion
    }
}
