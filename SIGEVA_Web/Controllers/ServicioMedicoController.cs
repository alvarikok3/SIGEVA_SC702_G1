
using Microsoft.AspNetCore.Mvc;
using SIGEVA_Web.Models;
using System.Net;
using System.Net.Http.Headers;

namespace SIGEVA_Web.Controllers
{
    public class ServicioMedicoController : Controller
    {
        private readonly IHttpClientFactory _http;
        private readonly IConfiguration _config;

        public ServicioMedicoController(
            IHttpClientFactory http,
            IConfiguration config)
        {
            _http = http;
            _config = config;
        }

        private HttpClient CrearCliente()
        {
            var client = _http.CreateClient();

            var token = HttpContext.Session.GetString("Token");

            if (!string.IsNullOrWhiteSpace(token))
            {
                client.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", token);
            }

            return client;
        }

        // HU-CAT-002
        // Consultar, buscar y filtrar servicios médicos.

        [HttpGet]
        public async Task<IActionResult> Index(
            string? busqueda,
            int? idCategoria)
        {
            var token = HttpContext.Session.GetString("Token");

            if (string.IsNullOrWhiteSpace(token))
            {
                return RedirectToAction("Index", "Login");
            }

            if (idCategoria.HasValue && idCategoria.Value <= 0)
            {
                return BadRequest(
                    "La categoría seleccionada no es válida.");
            }

            if (busqueda != null && busqueda.Length > 150)
            {
                return BadRequest(
                    "La búsqueda no puede superar los 150 caracteres.");
            }

            using var client = CrearCliente();

            var urlBase = _config["Valores:UrlApi"];

            if (string.IsNullOrWhiteSpace(urlBase))
            {
                return StatusCode(
                    500,
                    "No se ha configurado la dirección de la API.");
            }

            // Consultar las categorías disponibles.
            var respuestaCategorias = await client.GetAsync(
                urlBase + "ServicioMedico/ConsultarCategoriasServicios");

            if (respuestaCategorias.StatusCode == HttpStatusCode.Unauthorized)
            {
                return RedirectToAction("Index", "Login");
            }

            if (!respuestaCategorias.IsSuccessStatusCode)
            {
                return StatusCode(
                    (int)respuestaCategorias.StatusCode,
                    "No fue posible consultar las categorías.");
            }

            var categorias = await respuestaCategorias.Content
                .ReadFromJsonAsync<List<CategoriaServicioModel>>()
                ?? new List<CategoriaServicioModel>();

            ViewBag.Categorias = categorias;

            // Construir los filtros.
            var parametros = new List<string>();

            if (!string.IsNullOrWhiteSpace(busqueda))
            {
                parametros.Add(
                    "busqueda=" + Uri.EscapeDataString(busqueda.Trim()));
            }

            if (idCategoria.HasValue)
            {
                parametros.Add(
                    "idCategoria=" + idCategoria.Value);
            }

            var urlServicios =
                urlBase + "ServicioMedico/ConsultarCatalogoServicios";

            if (parametros.Count > 0)
            {
                urlServicios += "?" + string.Join("&", parametros);
            }

            // Consultar los servicios médicos.
            var respuestaServicios =
                await client.GetAsync(urlServicios);

            if (respuestaServicios.StatusCode == HttpStatusCode.Unauthorized)
            {
                return RedirectToAction("Index", "Login");
            }

            if (!respuestaServicios.IsSuccessStatusCode)
            {
                return StatusCode(
                    (int)respuestaServicios.StatusCode,
                    "No fue posible consultar los servicios médicos.");
            }

            var servicios = await respuestaServicios.Content
                .ReadFromJsonAsync<List<ServicioMedicoModel>>()
                ?? new List<ServicioMedicoModel>();

            // Conservar los filtros seleccionados.
            ViewBag.Busqueda = busqueda;
            ViewBag.IdCategoria = idCategoria;

            return View(servicios);
        }
    }
}
