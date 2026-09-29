using Microsoft.AspNetCore.Mvc;
using SIGEVA_Web.Models;
using System.Net;
using System.Net.Http.Headers;

namespace SIGEVA_Web.Controllers
{
    public class EquipoController(
        IHttpClientFactory _http,
        IConfiguration _config) : Controller
    {
        private HttpClient CrearCliente()
        {
            var client = _http.CreateClient();
            var token = HttpContext.Session.GetString("Token");
            if (!string.IsNullOrEmpty(token))
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            return client;
        }

        #region Consulta

        [HttpGet]
        public IActionResult Index()
        {
            using var client = CrearCliente();
            var urlApi = _config["Valores:UrlApi"] + "Equipo/ConsultarEquipos";
            var response = client.GetAsync(urlApi).Result;

            if (response.StatusCode == HttpStatusCode.OK)
            {
                var datos = response.Content.ReadFromJsonAsync<List<EquipoMedicoModel>>().Result;
                return View(datos);
            }

            if (response.StatusCode == HttpStatusCode.Unauthorized)
                return RedirectToAction("Index", "Login");

            throw new Exception("Ocurrió un error al consultar los equipos.");
        }

        #endregion

        #region Registrar

        [HttpGet]
        public IActionResult Registrar()
        {
            ViewBag.Usuarios = ObtenerUsuarios();
            return View(new EquipoMedicoModel { FechaAdquisicion = DateTime.Today });
        }

        [HttpPost]
        public IActionResult Registrar(EquipoMedicoModel model)
        {
            using var client = CrearCliente();
            var urlApi = _config["Valores:UrlApi"] + "Equipo/RegistrarEquipo";
            var response = client.PostAsJsonAsync(urlApi, model).Result;

            if (response.StatusCode == HttpStatusCode.OK)
                return RedirectToAction("Index");

            if (response.StatusCode == HttpStatusCode.Unauthorized)
                return RedirectToAction("Index", "Login");

            ViewBag.Mensaje = "No se pudo registrar el equipo. Verifique que el número de serie no esté repetido.";
            ViewBag.Tipo = "danger";
            ViewBag.Usuarios = ObtenerUsuarios();
            return View(model);
        }

        #endregion

        #region Editar

        [HttpGet]
        public IActionResult Editar(int id)
        {
            using var client = CrearCliente();
            var urlApi = _config["Valores:UrlApi"] + "Equipo/ConsultarEquipoPorId/" + id;
            var response = client.GetAsync(urlApi).Result;

            if (response.StatusCode == HttpStatusCode.OK)
            {
                var datos = response.Content.ReadFromJsonAsync<EquipoMedicoModel>().Result;
                ViewBag.Usuarios = ObtenerUsuarios();
                return View(datos);
            }

            if (response.StatusCode == HttpStatusCode.Unauthorized)
                return RedirectToAction("Index", "Login");

            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult Editar(EquipoMedicoModel model)
        {
            using var client = CrearCliente();
            var urlApi = _config["Valores:UrlApi"] + "Equipo/ActualizarEquipo";
            var response = client.PutAsJsonAsync(urlApi, model).Result;

            if (response.StatusCode == HttpStatusCode.OK)
                return RedirectToAction("Index");

            if (response.StatusCode == HttpStatusCode.Unauthorized)
                return RedirectToAction("Index", "Login");

            ViewBag.Mensaje = "No se pudo actualizar el equipo.";
            ViewBag.Tipo = "danger";
            ViewBag.Usuarios = ObtenerUsuarios();
            return View(model);
        }

        #endregion

        #region Estado

        [HttpGet]
        public IActionResult CambiarEstado(int id, bool estado)
        {
            using var client = CrearCliente();
            var urlApi = _config["Valores:UrlApi"] + $"Equipo/CambiarEstadoEquipo/{id}/{estado}";
            var response = client.PutAsync(urlApi, null).Result;

            if (response.StatusCode == HttpStatusCode.OK || response.StatusCode == HttpStatusCode.BadRequest)
                return RedirectToAction("Index");

            if (response.StatusCode == HttpStatusCode.Unauthorized)
                return RedirectToAction("Index", "Login");

            throw new Exception("Ocurrió un error al cambiar el estado del equipo.");
        }

        #endregion

        #region Helpers

        private List<UsuarioListaModel> ObtenerUsuarios()
        {
            using var client = CrearCliente();
            var urlApi = _config["Valores:UrlApi"] + "Usuario/ConsultarUsuarios";
            var response = client.GetAsync(urlApi).Result;
            if (response.StatusCode == HttpStatusCode.OK)
                return response.Content.ReadFromJsonAsync<List<UsuarioListaModel>>().Result ?? new();
            return new();
        }

        #endregion
    }
}
