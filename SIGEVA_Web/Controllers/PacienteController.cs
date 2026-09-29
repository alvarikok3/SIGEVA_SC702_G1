using Microsoft.AspNetCore.Mvc;
using SIGEVA_Web.Models;
using System.Net;
using System.Net.Http.Headers;

namespace SIGEVA_Web.Controllers
{
    public class PacienteController(
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
            var urlApi = _config["Valores:UrlApi"] + "Paciente/ConsultarPacientes";
            var response = client.GetAsync(urlApi).Result;

            if (response.StatusCode == HttpStatusCode.OK)
            {
                var datos = response.Content.ReadFromJsonAsync<List<PacienteModel>>().Result;
                return View(datos);
            }

            if (response.StatusCode == HttpStatusCode.Unauthorized)
                return RedirectToAction("Index", "Login");

            throw new Exception("Ocurrió un error al consultar los pacientes.");
        }

        #endregion

        #region Registrar

        [HttpGet]
        public IActionResult Registrar()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Registrar(PacienteModel model)
        {
            using var client = CrearCliente();
            var urlApi = _config["Valores:UrlApi"] + "Paciente/RegistrarPaciente";
            var response = client.PostAsJsonAsync(urlApi, model).Result;

            if (response.StatusCode == HttpStatusCode.OK)
            {
                return RedirectToAction("Index");
            }
            else if (response.StatusCode == HttpStatusCode.BadRequest)
            {
                ViewBag.Mensaje = response.Content.ReadAsStringAsync().Result;
                ViewBag.Tipo = "danger";
                return View(model);
            }

            if (response.StatusCode == HttpStatusCode.Unauthorized)
                return RedirectToAction("Index", "Login");

            throw new Exception("Ocurrió un error al registrar el paciente.");
        }

        #endregion

        #region Editar

        [HttpGet]
        public IActionResult Editar(int id)
        {
            using var client = CrearCliente();
            var urlApi = _config["Valores:UrlApi"] + "Paciente/ConsultarPacientePorId/" + id;
            var response = client.GetAsync(urlApi).Result;

            if (response.StatusCode == HttpStatusCode.OK)
            {
                var datos = response.Content.ReadFromJsonAsync<PacienteModel>().Result;
                return View(datos);
            }

            if (response.StatusCode == HttpStatusCode.Unauthorized)
                return RedirectToAction("Index", "Login");

            throw new Exception("Ocurrió un error al consultar el paciente.");
        }

        [HttpPost]
        public IActionResult Editar(PacienteModel model)
        {
            using var client = CrearCliente();
            var urlApi = _config["Valores:UrlApi"] + "Paciente/ActualizarPaciente";
            var response = client.PutAsJsonAsync(urlApi, model).Result;

            if (response.StatusCode == HttpStatusCode.OK)
            {
                return RedirectToAction("Index");
            }
            else if (response.StatusCode == HttpStatusCode.BadRequest)
            {
                ViewBag.Mensaje = response.Content.ReadAsStringAsync().Result;
                ViewBag.Tipo = "danger";
                return View(model);
            }

            if (response.StatusCode == HttpStatusCode.Unauthorized)
                return RedirectToAction("Index", "Login");

            throw new Exception("Ocurrió un error al actualizar el paciente.");
        }

        #endregion

        #region Estado

        [HttpGet]
        public IActionResult CambiarEstado(int id, bool estado)
        {
            using var client = CrearCliente();
            var urlApi = _config["Valores:UrlApi"] + $"Paciente/CambiarEstadoPaciente/{id}/{estado}";
            var response = client.PutAsync(urlApi, null).Result;

            if (response.StatusCode == HttpStatusCode.OK || response.StatusCode == HttpStatusCode.BadRequest)
                return RedirectToAction("Index");

            if (response.StatusCode == HttpStatusCode.Unauthorized)
                return RedirectToAction("Index", "Login");

            throw new Exception("Ocurrió un error al cambiar el estado del paciente.");
        }

        #endregion

        #region Reporte

        [HttpGet]
        public IActionResult Reporte()
        {
            return View(new ReportePacientesFiltroModel());
        }

        [HttpPost]
        public IActionResult Reporte(ReportePacientesFiltroModel filtro)
        {
            using var client = CrearCliente();
            var urlApi = _config["Valores:UrlApi"] + "Paciente/ReportePacientes";
            var response = client.PostAsJsonAsync(urlApi, new { filtro.Nombre, filtro.Estado }).Result;

            if (response.StatusCode == HttpStatusCode.OK)
            {
                filtro.Resultados = response.Content.ReadFromJsonAsync<List<PacienteModel>>().Result ?? new();
                return View(filtro);
            }

            if (response.StatusCode == HttpStatusCode.Unauthorized)
                return RedirectToAction("Index", "Login");

            throw new Exception("Ocurrió un error al generar el reporte de pacientes.");
        }

        #endregion
    }
}
