
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SIGEVA_API.Interfaces;
using SIGEVA_API.Models;
using SIGEVA_API.Services;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace SIGEVA_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsuarioController : ControllerBase
    {
        private readonly IUsuarioData _usuarioData;
        private readonly JwtService _jwtService;
        private readonly IEmailService _emailService;
        private readonly DobleFactorService _dobleFactorService;

        public UsuarioController(
            IUsuarioData usuarioData,
            JwtService jwtService,
            IEmailService emailService,
            DobleFactorService dobleFactorService)
        {
            _usuarioData = usuarioData;
            _jwtService = jwtService;
            _emailService = emailService;
            _dobleFactorService = dobleFactorService;
        }

        // ==========================================
        // Registrar usuario
        // ==========================================

        [HttpPost("RegistrarUsuario")]
        public IActionResult RegistrarUsuario(
            RegistroUsuarioModel model)
        {
            var respuesta = _usuarioData.RegistrarUsuario(model);

            if (respuesta > 0)
            {
                return Ok(
                    "Usuario registrado correctamente.");
            }

            return BadRequest(
                "No se pudo registrar el usuario. " +
                "Verifique que la identificación o el correo " +
                "no estén repetidos.");
        }

        // ==========================================
        // HU-AUT-004: Inicio de sesión
        // ==========================================

        [HttpPost("Login")]
        public async Task<IActionResult> Login(
            LoginModel model)
        {
            var usuario = _usuarioData.ValidarUsuario(model);

            if (usuario == null)
            {
                return Unauthorized(
                    "Acceso rechazado. Verifique sus credenciales " +
                    "y que su cuenta esté activa. " +
                    "Si realizó 5 intentos fallidos, " +
                    "espere 15 minutos.");
            }

            // Si el usuario no tiene 2FA activado,
            // continuar con el inicio de sesión habitual.
            if (!usuario.DobleFactorActivo)
            {
                var token = _jwtService.GenerarToken(usuario);

                return Ok(new
                {
                    RequiereDobleFactor = false,
                    Token = token,
                    usuario.IdUsuario,
                    usuario.IdRol,
                    usuario.Nombre,
                    usuario.Correo
                });
            }

            // Generar código de 6 dígitos.
            string codigo =
                _dobleFactorService.GenerarCodigo();

            // Generar ticket único.
            string ticket =
                _dobleFactorService.GenerarTicket();

            // Registrar código y ticket en SQL Server.
            await _dobleFactorService.RegistrarCodigoAsync(
                usuario.IdUsuario,
                codigo,
                ticket);

            // Preparar correo electrónico.
            string mensajeHtml = $@"
                <h2>Verificación de seguridad - SIGEVA</h2>

                <p>Se solicitó iniciar sesión en su cuenta.</p>

                <p>Su código de verificación es:</p>

                <h2>{codigo}</h2>

                <p>Este código vence en 5 minutos.</p>

                <p>Si usted no solicitó este acceso,
                ignore este mensaje.</p>

                <p>Atentamente,<br>
                Sistema SIGEVA</p>";

            try
            {
                await _emailService.EnviarCorreoAsync(
                    usuario.Correo,
                    "Código de verificación - SIGEVA",
                    mensajeHtml);
            }
            catch (Exception)
            {
                return StatusCode(503, new
                {
                    mensaje =
                        "No fue posible enviar el código " +
                        "de verificación. Intente nuevamente."
                });
            }

            // No generar JWT todavía.
            return Ok(new
            {
                RequiereDobleFactor = true,
                Ticket = ticket,
                Mensaje =
                    "Se envió un código de verificación " +
                    "al correo electrónico registrado."
            });
        }

        // ==========================================
        // HU-AUT-004: Verificar código 2FA
        // ==========================================

        [HttpPost("VerificarCodigo2FA")]
        public async Task<IActionResult> VerificarCodigo2FA(
            [FromBody] VerificarCodigo2FAModel model)
        {
            if (model == null ||
                string.IsNullOrWhiteSpace(model.Ticket) ||
                string.IsNullOrWhiteSpace(model.Codigo))
            {
                return BadRequest(new
                {
                    mensaje =
                        "Debe ingresar el ticket y " +
                        "el código de verificación."
                });
            }

            if (model.Codigo.Length != 6 ||
                !model.Codigo.All(char.IsAsciiDigit))
            {
                return BadRequest(new
                {
                    mensaje =
                        "El código debe contener 6 dígitos."
                });
            }

            // Recuperar usuario asociado al ticket.
            // No confiar en un IdUsuario enviado por el cliente.
            var usuario = await _dobleFactorService
                .ObtenerUsuarioPorTicketAsync(model.Ticket);

            if (usuario == null ||
                !usuario.Estado ||
                !usuario.DobleFactorActivo)
            {
                return Unauthorized(new
                {
                    mensaje =
                        "La solicitud de verificación no es válida."
                });
            }

            // Validar el código mediante SQL Server.
            int resultado = await _dobleFactorService
                .ValidarCodigoAsync(
                    usuario.IdUsuario,
                    model.Codigo,
                    model.Ticket);

            // Código expirado.
            if (resultado == -1)
            {
                return BadRequest(new
                {
                    mensaje =
                        "El código de verificación expiró. " +
                        "Inicie sesión nuevamente para " +
                        "solicitar un código nuevo."
                });
            }

            // Demasiados intentos fallidos.
            if (resultado == -2)
            {
                return BadRequest(new
                {
                    mensaje =
                        "Se superó el número de intentos " +
                        "permitidos. Inicie sesión nuevamente."
                });
            }

            // Código incorrecto, utilizado o ticket inválido.
            if (resultado != 1)
            {
                return BadRequest(new
                {
                    mensaje =
                        "El código de verificación es " +
                        "incorrecto o ya fue utilizado."
                });
            }

            // Generar JWT únicamente después
            // de validar correctamente el segundo factor.
            var token = _jwtService.GenerarToken(usuario);

            return Ok(new
            {
                RequiereDobleFactor = false,
                Token = token,
                usuario.IdUsuario,
                usuario.IdRol,
                usuario.Nombre,
                usuario.Correo,
                Mensaje =
                    "Autenticación de doble factor " +
                    "completada correctamente."
            });
        }

        // ==========================================
        // Recuperación de contraseña
        // ==========================================

        [HttpPost("RecuperarContrasena")]
        public async Task<IActionResult> RecuperarContrasena(
            RecuperarContrasenaModel model)
        {
            if (string.IsNullOrWhiteSpace(model.Correo))
            {
                return BadRequest(new
                {
                    mensaje =
                        "Debe ingresar el correo electrónico."
                });
            }

            var usuario = _usuarioData
                .ConsultarUsuarioPorCorreo(model.Correo);

            if (usuario == null)
            {
                return NotFound(new
                {
                    mensaje =
                        "No existe un usuario registrado " +
                        "con ese correo."
                });
            }

            var contrasenaTemporal =
                $"SIGEVA{Random.Shared.Next(100000, 999999)}";

            var resultado =
                _usuarioData.RestablecerContrasenaTemporal(
                    model.Correo,
                    contrasenaTemporal);

            if (resultado == 0)
            {
                return BadRequest(new
                {
                    mensaje =
                        "No fue posible restablecer la contraseña."
                });
            }

            var mensajeHtml = $@"
                <h2>Recuperación de contraseña</h2>

                <p>Hola, <strong>{usuario.Nombre}</strong>.</p>

                <p>Se solicitó la recuperación de la contraseña
                de su cuenta en el sistema SIGEVA.</p>

                <p>Su contraseña temporal es:</p>

                <h3>{contrasenaTemporal}</h3>

                <p>Inicie sesión utilizando esta contraseña y
                cámbiela posteriormente desde su perfil.</p>

                <p>Si usted no realizó esta solicitud,
                comuníquese con el administrador del sistema.</p>

                <br>

                <p>Atentamente,<br>
                Sistema SIGEVA</p>";

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
                    mensaje =
                        "La contraseña fue restablecida, " +
                        "pero no fue posible enviar " +
                        "el correo electrónico.",
                    detalle = ex.Message,
                    errorInterno = ex.InnerException?.Message
                });
            }

            return Ok(new
            {
                mensaje =
                    "Se envió una contraseña temporal " +
                    "al correo electrónico registrado."
            });
        }

        // ==========================================
        // Consultar roles
        // ==========================================

        [Authorize]
        [HttpGet("ConsultarRoles")]
        public IActionResult ConsultarRoles()
        {
            return Ok(_usuarioData.ConsultarRoles());
        }

        // ==========================================
        // Consultar usuarios
        // ==========================================

        [Authorize]
        [HttpGet("ConsultarUsuarios")]
        public IActionResult ConsultarUsuarios()
        {
            return Ok(_usuarioData.ConsultarUsuarios());
        }

        // ==========================================
        // Consultar usuario por ID
        // ==========================================

        [Authorize]
        [HttpGet("ConsultarUsuarioPorId/{idUsuario}")]
        public IActionResult ConsultarUsuarioPorId(
            int idUsuario)
        {
            var usuario =
                _usuarioData.ConsultarUsuarioPorId(idUsuario);

            if (usuario == null)
            {
                return NotFound(
                    "Usuario no encontrado.");
            }

            return Ok(usuario);
        }

        // ==========================================
        // Actualizar usuario
        // ==========================================

        [Authorize]
        [HttpPut("ActualizarUsuario")]
        public IActionResult ActualizarUsuario(
            UsuarioConsultaModel model)
        {
            var respuesta =
                _usuarioData.ActualizarUsuario(model);

            if (respuesta > 0)
            {
                return Ok(
                    "Usuario actualizado correctamente.");
            }

            return BadRequest(
                "No se pudo actualizar el usuario.");
        }

        // ==========================================
        // Cambiar estado de usuario
        // ==========================================

        [Authorize]
        [HttpPut(
            "CambiarEstadoUsuario/{idUsuario}/{estado}")]
        public IActionResult CambiarEstadoUsuario(
            int idUsuario,
            bool estado)
        {
            var respuesta =
                _usuarioData.CambiarEstadoUsuario(
                    idUsuario,
                    estado);

            if (respuesta > 0)
            {
                return Ok(
                    "Estado del usuario actualizado correctamente.");
            }

            return BadRequest(
                "No se pudo actualizar el estado del usuario.");
        }

        // ==========================================
        // Cambiar contraseña
        // ==========================================

        [Authorize]
        [HttpPut("CambiarContrasena")]
        public IActionResult CambiarContrasena(
            CambiarContrasenaModel model)
        {
            if (string.IsNullOrWhiteSpace(
                    model.ContrasenaActual) ||
                string.IsNullOrWhiteSpace(
                    model.ContrasenaNueva))
            {
                return BadRequest(
                    "Debe indicar la contraseña actual y la nueva.");
            }

            if (model.ContrasenaNueva.Length < 5)
            {
                return BadRequest(
                    "La nueva contraseña debe tener " +
                    "al menos 5 caracteres.");
            }

            var respuesta =
                _usuarioData.CambiarContrasena(model);

            if (respuesta > 0)
            {
                return Ok(
                    "Contraseña actualizada correctamente.");
            }

            return BadRequest(
                "No se pudo cambiar la contraseña. " +
                "Verifique la contraseña actual y " +
                "que el usuario esté activo.");
        }
    }

    // ==========================================
    // Modelo de verificación de doble factor
    // ==========================================

    public class VerificarCodigo2FAModel
    {
        public string Ticket { get; set; } = string.Empty;

        public string Codigo { get; set; } = string.Empty;
    }
}
