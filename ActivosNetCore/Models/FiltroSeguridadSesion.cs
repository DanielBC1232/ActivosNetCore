using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace ActivosNetCore.Models
{
    public class FiltroSeguridadSesion : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var token = context.HttpContext.Session.GetString("Token");

            if (string.IsNullOrEmpty(token))
            {
                context.Result = new RedirectToRouteResult(new { controller = "Login", action = "IniciarSesion" });
            }

            base.OnActionExecuting(context);
        }
    }
}
