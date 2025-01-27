using Microsoft.AspNetCore.Mvc;

namespace ActivosNetCore.Controllers
{
    public class LoginController : Controller
    {
        public IActionResult IniciarSesion()
        {
            return View();
        }
    }
}
