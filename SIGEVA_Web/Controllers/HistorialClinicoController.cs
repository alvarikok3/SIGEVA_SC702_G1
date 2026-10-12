
using Microsoft.AspNetCore.Mvc;
using SIGEVA_Web.Models;
using System.Net;
using System.Net.Http.Headers;

namespace SIGEVA_Web.Controllers
{
    public class HistorialClinicoController(
        IHttpClientFactory _http,
        IConfiguration _config) : Controller
    {
        private HttpClient CrearCliente()
        {
            var client = _http.CreateClient();
            var token = HttpContext.Session.GetString("Token");

            if (!string.IsNullOrEmpty(token))
            {
                client.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", token);
            }

            return client;
        }

        [HttpGet]
        public async Task<IActionResult> Consultar(int id)
        {
            if (string.IsNullOrEmpty(
                HttpContext.Session.GetString("Token")))
            {
                return RedirectToAction("Index", "Login");
            }

            if (id <= 0)
            {
                return BadRequest("Identificador de paciente inválido.");
            }

            using var client = CrearCliente();
            var urlApi = _config["Valores:UrlApi"]
                + $"HistorialClinico/Consultar/{id}";

            var response = await client.GetAsync(urlApi);

            if (response.StatusCode == HttpStatusCode.OK)
            {
                var historial = await response.Content
                    .ReadFromJsonAsync<HistorialClinicoModel>();

                if (historial == null)
                    return NotFound();

                return View(historial);
            }

            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                return RedirectToAction("Index", "Login");
            }

            if (response.StatusCode == HttpStatusCode.Forbidden)
            {
                return StatusCode(403,
                    "No tiene permisos para consultar este historial clínico.");
            }

            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                return NotFound("El paciente no existe.");
            }

            return StatusCode(502,
                "No fue posible consultar el historial clínico.");
        }
    }
}
