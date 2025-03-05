using Microsoft.AspNetCore.Mvc;

namespace ActivosNetCore.Controllers
{
    public class ErrorController : Controller
    {
        public IActionResult CapturarError()
        {
            return View("Error");
        }
    }
}
