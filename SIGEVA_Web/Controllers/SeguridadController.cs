using Microsoft.AspNetCore.Mvc;
using SIGEVA_Web.Models;
using System.Net;
using System.Net.Http.Headers;

namespace SIGEVA_Web.Controllers
{
    public class SeguridadController(
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

        #region Cambiar contraseña

        [HttpGet]
        public IActionResult CambiarContrasena()
        {
            if (string.IsNullOrEmpty(HttpContext.Session.GetString("Token")))
                return RedirectToAction("Index", "Login");

            return View(new CambiarContrasenaModel());
        }

        [HttpPost]
        public IActionResult CambiarContrasena(CambiarContrasenaModel model)
        {
            if (string.IsNullOrEmpty(HttpContext.Session.GetString("Token")))
                return RedirectToAction("Index", "Login");

            if (model.ContrasenaNueva != model.ConfirmarContrasena)
            {
                ViewBag.Mensaje = "La nueva contraseña y la confirmación no coinciden.";
                ViewBag.Tipo = "danger";
                return View(model);
            }

            model.IdUsuario = HttpContext.Session.GetInt32("IdUsuario") ?? 0;

            using var client = CrearCliente();
            var urlApi = _config["Valores:UrlApi"] + "Usuario/CambiarContrasena";
            var response = client.PutAsJsonAsync(urlApi, model).Result;

            if (response.StatusCode == HttpStatusCode.OK)
            {
                ViewBag.Mensaje = "Contraseña actualizada correctamente.";
                ViewBag.Tipo = "success";
                return View(new CambiarContrasenaModel());
            }

            if (response.StatusCode == HttpStatusCode.Unauthorized)
                return RedirectToAction("Index", "Login");

            ViewBag.Mensaje = response.Content.ReadAsStringAsync().Result
                .Trim('"');
            ViewBag.Tipo = "danger";
            return View(model);
        }

        #endregion

        #region Recuperar acceso

        [HttpGet]
        public IActionResult RecuperarAcceso()
        {
            return View();
        }

        #endregion
    }
}
