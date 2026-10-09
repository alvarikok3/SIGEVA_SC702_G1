using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SIGEVA_Web.Models;
using System.Net.Http.Json;

namespace SIGEVA_Web.Controllers
{
    public class LoginController : Controller
    {
        private readonly IHttpClientFactory _httpClient;

        public LoginController(IHttpClientFactory httpClient)
        {
            _httpClient = httpClient;
        }

        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Index(LoginModel model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Mensaje = "Revise el correo y la contraseña.";
                return View(model);
            }

            var client = _httpClient.CreateClient();

            const string urlApi =
                "https://localhost:7273/api/Usuario/Login";

            try
            {
                using var response =
                    await client.PostAsJsonAsync(urlApi, model);

                if (!response.IsSuccessStatusCode)
                {
                    if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                    {
                        var mensaje = await response.Content.ReadAsStringAsync();

                        ViewBag.Mensaje = string.IsNullOrWhiteSpace(mensaje)
                            ? "Acceso rechazado. Verifique sus credenciales."
                            : mensaje;
                    }
                    else
                    {
                        ViewBag.Mensaje =
                            $"La API devolvió un error ({(int)response.StatusCode}).";
                    }

                    return View(model);
                }

                var datos = await response.Content
                    .ReadFromJsonAsync<LoginRespuestaModel>();

                if (datos == null || string.IsNullOrWhiteSpace(datos.Token))
                {
                    ViewBag.Mensaje =
                        "La API no devolvió un token válido.";

                    return View(model);
                }

                HttpContext.Session.SetString("Token", datos.Token);
                HttpContext.Session.SetString("Nombre", datos.Nombre ?? "");
                HttpContext.Session.SetString("Correo", datos.Correo ?? "");
                HttpContext.Session.SetInt32("IdUsuario", datos.IdUsuario);
                HttpContext.Session.SetInt32("IdRol", datos.IdRol);

                return RedirectToAction("Index", "Home");
            }
            catch (HttpRequestException)
            {
                ViewBag.Mensaje =
                    "No se pudo conectar con la API. Compruebe que esté ejecutándose en https://localhost:7273.";

                return View(model);
            }
            catch (TaskCanceledException)
            {
                ViewBag.Mensaje =
                    "La API tardó demasiado en responder. Intente nuevamente.";

                return View(model);
            }
        }

        [HttpGet]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Index", "Login");
        }
    }
}