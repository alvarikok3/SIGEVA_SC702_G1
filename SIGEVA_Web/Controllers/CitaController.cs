using Microsoft.AspNetCore.Mvc;
using SIGEVA_Web.Models;
using System.Net;
using System.Net.Http.Headers;

namespace SIGEVA_Web.Controllers
{
    public class CitaController(
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

        private void CargarListas()
        {
            using var client = CrearCliente();

            var pacientesResp = client.GetAsync(_config["Valores:UrlApi"] + "Paciente/ConsultarPacientes").Result;
            if (pacientesResp.StatusCode == HttpStatusCode.OK)
            {
                var pacientes = pacientesResp.Content.ReadFromJsonAsync<List<PacienteModel>>().Result ?? new();
                ViewBag.Pacientes = pacientes.Where(p => p.Estado).ToList();
            }
            else
            {
                ViewBag.Pacientes = new List<PacienteModel>();
            }

            var usuariosResp = client.GetAsync(_config["Valores:UrlApi"] + "Usuario/ConsultarUsuarios").Result;
            if (usuariosResp.StatusCode == HttpStatusCode.OK)
            {
                var usuarios = usuariosResp.Content.ReadFromJsonAsync<List<UsuarioListaModel>>().Result ?? new();
                ViewBag.Medicos = usuarios.Where(u => u.Estado).ToList();
            }
            else
            {
                ViewBag.Medicos = new List<UsuarioListaModel>();
            }
        }

        #region Consulta

        [HttpGet]
        public IActionResult Index()
        {
            using var client = CrearCliente();
            var urlApi = _config["Valores:UrlApi"] + "Cita/ConsultarCitas";
            var response = client.GetAsync(urlApi).Result;

            if (response.StatusCode == HttpStatusCode.OK)
            {
                var datos = response.Content.ReadFromJsonAsync<List<CitaModel>>().Result;
                return View(datos);
            }

            if (response.StatusCode == HttpStatusCode.Unauthorized)
                return RedirectToAction("Index", "Login");

            throw new Exception("Ocurrió un error al consultar las citas.");
        }

        #endregion

        #region Registrar

        [HttpGet]
        public IActionResult Registrar()
        {
            CargarListas();
            return View(new CitaModel { FechaHora = DateTime.Now.AddHours(1), EstadoCita = "Programada" });
        }

        [HttpPost]
        public IActionResult Registrar(CitaModel model)
        {
            using var client = CrearCliente();
            var urlApi = _config["Valores:UrlApi"] + "Cita/RegistrarCita";
            var response = client.PostAsJsonAsync(urlApi, model).Result;

            if (response.StatusCode == HttpStatusCode.OK)
            {
                return RedirectToAction("Index");
            }
            else if (response.StatusCode == HttpStatusCode.BadRequest)
            {
                ViewBag.Mensaje = response.Content.ReadAsStringAsync().Result;
                ViewBag.Tipo = "danger";
                CargarListas();
                return View(model);
            }

            if (response.StatusCode == HttpStatusCode.Unauthorized)
                return RedirectToAction("Index", "Login");

            throw new Exception("Ocurrió un error al registrar la cita.");
        }

        #endregion

        #region Editar

        [HttpGet]
        public IActionResult Editar(int id)
        {
            using var client = CrearCliente();
            var urlApi = _config["Valores:UrlApi"] + "Cita/ConsultarCitaPorId/" + id;
            var response = client.GetAsync(urlApi).Result;

            if (response.StatusCode == HttpStatusCode.OK)
            {
                var datos = response.Content.ReadFromJsonAsync<CitaModel>().Result;
                CargarListas();
                return View(datos);
            }

            if (response.StatusCode == HttpStatusCode.Unauthorized)
                return RedirectToAction("Index", "Login");

            throw new Exception("Ocurrió un error al consultar la cita.");
        }

        [HttpPost]
        public IActionResult Editar(CitaModel model)
        {
            using var client = CrearCliente();
            var urlApi = _config["Valores:UrlApi"] + "Cita/ActualizarCita";
            var response = client.PutAsJsonAsync(urlApi, model).Result;

            if (response.StatusCode == HttpStatusCode.OK)
            {
                return RedirectToAction("Index");
            }
            else if (response.StatusCode == HttpStatusCode.BadRequest)
            {
                ViewBag.Mensaje = response.Content.ReadAsStringAsync().Result;
                ViewBag.Tipo = "danger";
                CargarListas();
                return View(model);
            }

            if (response.StatusCode == HttpStatusCode.Unauthorized)
                return RedirectToAction("Index", "Login");

            throw new Exception("Ocurrió un error al actualizar la cita.");
        }

        #endregion

        #region Estado

        [HttpGet]
        public IActionResult CambiarEstado(int id, string estado)
        {
            using var client = CrearCliente();
            var urlApi = _config["Valores:UrlApi"] + $"Cita/CambiarEstadoCita/{id}/{estado}";
            var response = client.PutAsync(urlApi, null).Result;

            if (response.StatusCode == HttpStatusCode.OK || response.StatusCode == HttpStatusCode.BadRequest)
                return RedirectToAction("Index");

            if (response.StatusCode == HttpStatusCode.Unauthorized)
                return RedirectToAction("Index", "Login");

            throw new Exception("Ocurrió un error al cambiar el estado de la cita.");
        }

        #endregion

        #region Reporte

        [HttpGet]
        public IActionResult Reporte()
        {
            var filtro = new ReporteCitasFiltroModel
            {
                FechaInicio = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1),
                FechaFin = DateTime.Today
            };
            return View(filtro);
        }

        [HttpPost]
        public IActionResult Reporte(ReporteCitasFiltroModel filtro)
        {
            using var client = CrearCliente();
            var urlApi = _config["Valores:UrlApi"] + "Cita/ReporteCitas";
            var response = client.PostAsJsonAsync(urlApi, new
            {
                filtro.FechaInicio,
                filtro.FechaFin,
                filtro.EstadoCita
            }).Result;

            if (response.StatusCode == HttpStatusCode.OK)
            {
                filtro.Resultados = response.Content.ReadFromJsonAsync<List<CitaModel>>().Result ?? new();
                return View(filtro);
            }

            if (response.StatusCode == HttpStatusCode.Unauthorized)
                return RedirectToAction("Index", "Login");

            throw new Exception("Ocurrió un error al generar el reporte de citas.");
        }

        #endregion

        #region Calendario

        [HttpGet]
        public IActionResult Calendario(DateTime? fecha)
        {
            var dia = (fecha ?? DateTime.Today).Date;

            using var client = CrearCliente();
            var urlApi = _config["Valores:UrlApi"] + "Cita/ConsultarCitasPorFecha?fecha=" + dia.ToString("yyyy-MM-dd");
            var response = client.GetAsync(urlApi).Result;

            if (response.StatusCode == HttpStatusCode.OK)
            {
                var datos = response.Content.ReadFromJsonAsync<List<CitaModel>>().Result;
                ViewBag.Fecha = dia;
                return View(datos);
            }

            if (response.StatusCode == HttpStatusCode.Unauthorized)
                return RedirectToAction("Index", "Login");

            throw new Exception("Ocurrió un error al consultar el calendario de citas.");
        }

        #endregion
    }
}
