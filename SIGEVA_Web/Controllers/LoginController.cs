using Microsoft.AspNetCore.Mvc;
using SIGEVA_Web.Models;
using System.Net.Http.Json;

namespace SIGEVA_Web.Controllers
{
    public class LoginController(
        IHttpClientFactory _httpClient,
        IConfiguration _config) : Controller
    {
        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Index(LoginModel model)
        {
            var client = _httpClient.CreateClient();

            var urlApi = _config["Valores:UrlApi"] + "Usuario/Login";
            var response = await client.PostAsJsonAsync(urlApi, model);

            if (response.IsSuccessStatusCode)
            {
                var datos = await response.Content
                    .ReadFromJsonAsync<LoginRespuestaModel>();

                HttpContext.Session.SetString("Token", datos!.Token);
                HttpContext.Session.SetString("Nombre", datos.Nombre);
                HttpContext.Session.SetString("Correo", datos.Correo);
                HttpContext.Session.SetInt32("IdUsuario", datos.IdUsuario);
                HttpContext.Session.SetInt32("IdRol", datos.IdRol);

                return RedirectToAction("Index", "Home");
            }

            ViewBag.Mensaje = "Credenciales incorrectas.";
            return View();
        }

        [HttpGet]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();

            return RedirectToAction("Index", "Login");
        }
    }
}