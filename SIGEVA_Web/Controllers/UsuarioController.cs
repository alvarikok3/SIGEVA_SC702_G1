using Microsoft.AspNetCore.Mvc;
using SIGEVA_Web.Models;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace SIGEVA_Web.Controllers
{
    public class UsuarioController(
        IHttpClientFactory _httpClient,
        IConfiguration _config) : Controller
    {
        // Agrega el JWT guardado en sesión a las solicitudes hacia la API
        private HttpClient CrearCliente()
        {
            var client = _httpClient.CreateClient();

            var token = HttpContext.Session.GetString("Token");

            if (!string.IsNullOrWhiteSpace(token))
            {
                client.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", token);
            }

            return client;
        }

        // Comprueba que el usuario haya iniciado sesión
        private bool TieneSesion()
        {
            return !string.IsNullOrWhiteSpace(
                HttpContext.Session.GetString("Token"));
        }


        #region CONSULTAR USUARIOS

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            if (!TieneSesion())
                return RedirectToAction("Index", "Login");

            var client = CrearCliente();

            var urlApi =
                _config["Valores:UrlApi"] + "Usuario/ConsultarUsuarios";

            var response = await client.GetAsync(urlApi);

            if (!response.IsSuccessStatusCode)
            {
                ViewBag.Mensaje = "No fue posible consultar los usuarios.";

                return View(new List<UsuarioListaModel>());
            }

            var usuarios =
                await response.Content
                .ReadFromJsonAsync<List<UsuarioListaModel>>();

            return View(usuarios ?? new List<UsuarioListaModel>());
        }

        #endregion


        #region REGISTRAR USUARIO

        [HttpGet]
        public IActionResult Registrar()
        {
            if (!TieneSesion())
                return RedirectToAction("Index", "Login");

            return View();
        }


        [HttpPost]
        public async Task<IActionResult> Registrar(
            int IdRol,
            string Identificacion,
            string Nombre,
            string Correo,
            string Contrasena)
        {
            if (!TieneSesion())
                return RedirectToAction("Index", "Login");

            var client = CrearCliente();

            var usuario = new
            {
                IdRol,
                Identificacion,
                Nombre,
                Correo,
                Contrasena
            };

            var urlApi =
                _config["Valores:UrlApi"] + "Usuario/RegistrarUsuario";

            var response =
                await client.PostAsJsonAsync(urlApi, usuario);

            if (response.IsSuccessStatusCode)
            {
                TempData["Mensaje"] =
                    "Usuario registrado correctamente.";

                return RedirectToAction("Index");
            }

            ViewBag.Mensaje =
                "No fue posible registrar el usuario. Verifique los datos.";

            return View();
        }

        #endregion


        #region EDITAR USUARIO

        [HttpGet]
        public async Task<IActionResult> Editar(int id)
        {
            if (!TieneSesion())
                return RedirectToAction("Index", "Login");

            var client = CrearCliente();

            var urlApi =
                _config["Valores:UrlApi"] +
                $"Usuario/ConsultarUsuarioPorId/{id}";

            var response =
                await client.GetAsync(urlApi);

            if (!response.IsSuccessStatusCode)
                return RedirectToAction("Index");

            var usuario =
                await response.Content
                .ReadFromJsonAsync<UsuarioListaModel>();

            return View(usuario);
        }


        [HttpPost]
        public async Task<IActionResult> Editar(
            UsuarioListaModel model)
        {
            if (!TieneSesion())
                return RedirectToAction("Index", "Login");

            var client = CrearCliente();

            var urlApi =
                _config["Valores:UrlApi"] +
                "Usuario/ActualizarUsuario";

            var response =
                await client.PutAsJsonAsync(urlApi, model);

            if (response.IsSuccessStatusCode)
            {
                TempData["Mensaje"] =
                    "Usuario actualizado correctamente.";

                return RedirectToAction("Index");
            }

            ViewBag.Mensaje =
                "No fue posible actualizar el usuario.";

            return View(model);
        }

        #endregion


        #region ACTIVAR / DESACTIVAR USUARIO

        [HttpGet]
        public async Task<IActionResult> CambiarEstado(
            int id,
            bool estado)
        {
            if (!TieneSesion())
                return RedirectToAction("Index", "Login");

            var client = CrearCliente();

            var urlApi =
                _config["Valores:UrlApi"] +
                $"Usuario/CambiarEstadoUsuario/{id}/{estado}";

            var response =
                await client.PutAsync(urlApi, null);

            if (response.IsSuccessStatusCode)
            {
                TempData["Mensaje"] =
                    "Estado del usuario actualizado correctamente.";
            }
            else
            {
                TempData["Mensaje"] =
                    "No fue posible cambiar el estado del usuario.";
            }

            return RedirectToAction("Index");
        }

        #endregion
    }
}