using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace SIGEVA_Web.Filters
{
    public class AccesoFilter : IActionFilter
    {
        public void OnActionExecuting(ActionExecutingContext context)
        {
            var controller = context.RouteData.Values["controller"]
                ?.ToString() ?? "";

            var action = context.RouteData.Values["action"]
                ?.ToString() ?? "";

            // Este filtro protege Inicio y administración de usuarios.
            var esHome = controller.Equals(
                "Home", StringComparison.OrdinalIgnoreCase);

            var esUsuario = controller.Equals(
                "Usuario", StringComparison.OrdinalIgnoreCase);

            if (!esHome && !esUsuario)
                return;

            // Permitir mostrar la página de error.
            if (esHome && action.Equals(
                "Error", StringComparison.OrdinalIgnoreCase))
                return;

            var session = context.HttpContext.Session;

            if (string.IsNullOrWhiteSpace(session.GetString("Token")))
            {
                context.Result =
                    new RedirectToActionResult("Index", "Login", null);
                return;
            }

            if (esUsuario && session.GetInt32("IdRol") != 1)
            {
                context.Result = new ContentResult
                {
                    StatusCode = StatusCodes.Status403Forbidden,
                    ContentType = "text/plain; charset=utf-8",
                    Content = "No tiene permiso para administrar usuarios."
                };
            }
        }

        public void OnActionExecuted(ActionExecutedContext context)
        {
        }
    }
}