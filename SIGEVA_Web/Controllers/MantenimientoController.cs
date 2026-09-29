using Microsoft.AspNetCore.Mvc;
using SIGEVA_Web.Models;
using System.Net;
using System.Net.Http.Headers;

namespace SIGEVA_Web.Controllers
{
    public class MantenimientoController(
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
            var urlApi = _config["Valores:UrlApi"] + "Mantenimiento/ConsultarMantenimientos";
            var response = client.GetAsync(urlApi).Result;

            if (response.StatusCode == HttpStatusCode.OK)
            {
                var datos = response.Content.ReadFromJsonAsync<List<MantenimientoModel>>().Result;
                return View(datos);
            }

            if (response.StatusCode == HttpStatusCode.Unauthorized)
                return RedirectToAction("Index", "Login");

            throw new Exception("Ocurrió un error al consultar los mantenimientos.");
        }

        #endregion

        #region Registrar

        [HttpGet]
        public IActionResult Registrar()
        {
            ViewBag.Equipos = ObtenerEquipos();
            return View(new MantenimientoModel { FechaMantenimiento = DateTime.Today });
        }

        [HttpPost]
        public IActionResult Registrar(MantenimientoModel model)
        {
            using var client = CrearCliente();
            var urlApi = _config["Valores:UrlApi"] + "Mantenimiento/RegistrarMantenimiento";
            var response = client.PostAsJsonAsync(urlApi, model).Result;

            if (response.StatusCode == HttpStatusCode.OK)
                return RedirectToAction("Index");

            if (response.StatusCode == HttpStatusCode.Unauthorized)
                return RedirectToAction("Index", "Login");

            ViewBag.Mensaje = response.Content.ReadAsStringAsync().Result.Trim('"');
            ViewBag.Tipo = "danger";
            ViewBag.Equipos = ObtenerEquipos();
            return View(model);
        }

        #endregion

        #region Editar

        [HttpGet]
        public IActionResult Editar(int id)
        {
            using var client = CrearCliente();
            var urlApi = _config["Valores:UrlApi"] + "Mantenimiento/ConsultarMantenimientoPorId/" + id;
            var response = client.GetAsync(urlApi).Result;

            if (response.StatusCode == HttpStatusCode.OK)
            {
                var datos = response.Content.ReadFromJsonAsync<MantenimientoModel>().Result;
                ViewBag.Equipos = ObtenerEquipos();
                return View(datos);
            }

            if (response.StatusCode == HttpStatusCode.Unauthorized)
                return RedirectToAction("Index", "Login");

            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult Editar(MantenimientoModel model)
        {
            using var client = CrearCliente();
            var urlApi = _config["Valores:UrlApi"] + "Mantenimiento/ActualizarMantenimiento";
            var response = client.PutAsJsonAsync(urlApi, model).Result;

            if (response.StatusCode == HttpStatusCode.OK)
                return RedirectToAction("Index");

            if (response.StatusCode == HttpStatusCode.Unauthorized)
                return RedirectToAction("Index", "Login");

            ViewBag.Mensaje = "No se pudo actualizar el mantenimiento.";
            ViewBag.Tipo = "danger";
            ViewBag.Equipos = ObtenerEquipos();
            return View(model);
        }

        #endregion

        #region Estado

        [HttpGet]
        public IActionResult CambiarEstado(int id, bool estado)
        {
            using var client = CrearCliente();
            var urlApi = _config["Valores:UrlApi"] + $"Mantenimiento/CambiarEstadoMantenimiento/{id}/{estado}";
            var response = client.PutAsync(urlApi, null).Result;

            if (response.StatusCode == HttpStatusCode.OK || response.StatusCode == HttpStatusCode.BadRequest)
                return RedirectToAction("Index");

            if (response.StatusCode == HttpStatusCode.Unauthorized)
                return RedirectToAction("Index", "Login");

            throw new Exception("Ocurrió un error al cambiar el estado del mantenimiento.");
        }

        #endregion

        #region Reporte

        [HttpGet]
        public IActionResult Reporte()
        {
            var model = new ReporteFiltroModel
            {
                Equipos = ObtenerEquipos(),
                FechaInicio = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1),
                FechaFin = DateTime.Today
            };
            return View(model);
        }

        [HttpPost]
        public IActionResult Reporte(ReporteFiltroModel filtro)
        {
            using var client = CrearCliente();
            var urlApi = _config["Valores:UrlApi"] + "Mantenimiento/ReporteMantenimientos";
            var response = client.PostAsJsonAsync(urlApi, new
            {
                filtro.FechaInicio,
                filtro.FechaFin,
                filtro.IdEquipo,
                filtro.TipoMantenimiento
            }).Result;

            filtro.Equipos = ObtenerEquipos();

            if (response.StatusCode == HttpStatusCode.OK)
            {
                filtro.Resultados = response.Content.ReadFromJsonAsync<List<MantenimientoModel>>().Result ?? new();
                filtro.TotalCosto = filtro.Resultados.Sum(m => m.Costo);
            }
            else if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                return RedirectToAction("Index", "Login");
            }
            else
            {
                ViewBag.Mensaje = "No se pudo generar el reporte. Verifique los filtros e intente de nuevo.";
                ViewBag.Tipo = "danger";
            }

            return View(filtro);
        }

        #endregion

        #region Helpers

        private List<EquipoMedicoModel> ObtenerEquipos()
        {
            using var client = CrearCliente();
            var urlApi = _config["Valores:UrlApi"] + "Equipo/ConsultarEquipos";
            var response = client.GetAsync(urlApi).Result;
            if (response.StatusCode == HttpStatusCode.OK)
            {
                var todos = response.Content.ReadFromJsonAsync<List<EquipoMedicoModel>>().Result ?? new();
                return todos.Where(e => e.Estado).ToList();
            }
            return new();
        }

        #endregion
    }
}
