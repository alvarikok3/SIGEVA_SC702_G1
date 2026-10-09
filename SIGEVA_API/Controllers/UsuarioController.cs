using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SIGEVA_API.Interfaces;
using SIGEVA_API.Models;
using SIGEVA_API.Services;

namespace SIGEVA_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsuarioController : ControllerBase
    {
        private readonly IUsuarioData _usuarioData;
        private readonly JwtService _jwtService;
        private readonly IEmailService _emailService;

        public UsuarioController(
       IUsuarioData usuarioData,
       JwtService jwtService,
       IEmailService emailService)
        {
            _usuarioData = usuarioData;
            _jwtService = jwtService;
            _emailService = emailService;
        }

        [HttpPost]
        [Route("RegistrarUsuario")]
        public IActionResult RegistrarUsuario(RegistroUsuarioModel model)
        {
            var respuesta = _usuarioData.RegistrarUsuario(model);

            if (respuesta > 0)
                return Ok("Usuario registrado correctamente.");

            return BadRequest("No se pudo registrar el usuario. Verifique que la identificación o el correo no estén repetidos.");
        }

        [HttpPost]
        [Route("Login")]
        public IActionResult Login(LoginModel model)
        {
            var usuario = _usuarioData.ValidarUsuario(model);

            if (usuario == null)
            {
                return Unauthorized(
                    "Acceso rechazado. Verifique sus credenciales y que su cuenta esté activa. " +
                    "Si realizó 5 intentos fallidos, espere 15 minutos.");
            }

            var token = _jwtService.GenerarToken(usuario);

            return Ok(new
            {
                Token = token,
                usuario.IdUsuario,
                usuario.IdRol,
                usuario.Nombre,
                usuario.Correo
            });
        }
   

        [HttpPost]
        [Route("RecuperarContrasena")]
        public async Task<IActionResult> RecuperarContrasena(
         RecuperarContrasenaModel model)
        {
            if (string.IsNullOrWhiteSpace(model.Correo))
            {
                return BadRequest(new
                {
                    mensaje = "Debe ingresar el correo electrónico."
                });
            }

            var usuario = _usuarioData.ConsultarUsuarioPorCorreo(model.Correo);

            if (usuario == null)
            {
                return NotFound(new
                {
                    mensaje = "No existe un usuario registrado con ese correo."
                });
            }

            var contrasenaTemporal =
                $"SIGEVA{Random.Shared.Next(100000, 999999)}";

            var resultado = _usuarioData.RestablecerContrasenaTemporal(
                model.Correo,
                contrasenaTemporal);

            if (resultado == 0)
            {
                return BadRequest(new
                {
                    mensaje = "No fue posible restablecer la contraseña."
                });
            }

            var mensajeHtml = $@"
        <h2>Recuperación de contraseña</h2>

        <p>Hola, <strong>{usuario.Nombre}</strong>.</p>

        <p>Se solicitó la recuperación de la contraseña de su cuenta
        en el sistema SIGEVA.</p>

        <p>Su contraseña temporal es:</p>

        <h3>{contrasenaTemporal}</h3>

        <p>Inicie sesión utilizando esta contraseña y cámbiela
        posteriormente desde su perfil.</p>

        <p>Si usted no realizó esta solicitud, comuníquese con el
        administrador del sistema.</p>

        <br>

        <p>Atentamente,<br>
        Sistema SIGEVA</p>
    ";

            try
            {
                await _emailService.EnviarCorreoAsync(
                    model.Correo,
                    "Recuperación de contraseña - SIGEVA",
                    mensajeHtml);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    mensaje = "La contraseña fue restablecida, pero no fue posible enviar el correo electrónico.",
                    detalle = ex.Message,
                    errorInterno = ex.InnerException?.Message
                });
            }

            return Ok(new
            {
                mensaje = "Se envió una contraseña temporal al correo electrónico registrado."
            });
        }

        [Authorize]
        [HttpGet]
        [Route("ConsultarRoles")]
        public IActionResult ConsultarRoles()
        {
            return Ok(_usuarioData.ConsultarRoles());
        }

        [Authorize]
        [HttpGet]
        [Route("ConsultarUsuarios")]
        public IActionResult ConsultarUsuarios()
        {
            return Ok(_usuarioData.ConsultarUsuarios());
        }

        [Authorize]
        [HttpGet]
        [Route("ConsultarUsuarioPorId/{idUsuario}")]
        public IActionResult ConsultarUsuarioPorId(int idUsuario)
        {
            var usuario = _usuarioData.ConsultarUsuarioPorId(idUsuario);

            if (usuario == null)
                return NotFound("Usuario no encontrado.");

            return Ok(usuario);
        }

        [Authorize]
        [HttpPut]
        [Route("ActualizarUsuario")]
        public IActionResult ActualizarUsuario(UsuarioConsultaModel model)
        {
            var respuesta = _usuarioData.ActualizarUsuario(model);

            if (respuesta > 0)
                return Ok("Usuario actualizado correctamente.");

            return BadRequest("No se pudo actualizar el usuario.");
        }

        [Authorize]
        [HttpPut]
        [Route("CambiarEstadoUsuario/{idUsuario}/{estado}")]
        public IActionResult CambiarEstadoUsuario(int idUsuario, bool estado)
        {
            var respuesta = _usuarioData.CambiarEstadoUsuario(idUsuario, estado);

            if (respuesta > 0)
                return Ok("Estado del usuario actualizado correctamente.");

            return BadRequest("No se pudo actualizar el estado del usuario.");
        }

        [Authorize]
        [HttpPut]
        [Route("CambiarContrasena")]
        public IActionResult CambiarContrasena(CambiarContrasenaModel model)
        {
            if (string.IsNullOrWhiteSpace(model.ContrasenaActual) ||
                string.IsNullOrWhiteSpace(model.ContrasenaNueva))
            {
                return BadRequest("Debe indicar la contraseña actual y la nueva.");
            }

            if (model.ContrasenaNueva.Length < 5)
                return BadRequest("La nueva contraseña debe tener al menos 5 caracteres.");

            var respuesta = _usuarioData.CambiarContrasena(model);

            if (respuesta > 0)
                return Ok("Contraseña actualizada correctamente.");

            return BadRequest("No se pudo cambiar la contraseña. Verifique la contraseña actual y que el usuario esté activo.");
        }
    }
}