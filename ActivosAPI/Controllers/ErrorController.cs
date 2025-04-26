using ActivosAPI.Models;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace ActivosAPI.Controllers
{
    [ApiExplorerSettings(IgnoreApi = true)]
    [Route("api/[controller]")]
    [ApiController]
    public class ErrorController : ControllerBase
    {

        // Endpoint para capturar errores del proyecto
        [HttpPost]
        [Route("CapturarError")]
        public IActionResult CapturarError()
        {
            // Obtener detalles del error actual del contexto HTTP
            var ex = HttpContext.Features.Get<IExceptionHandlerFeature>();

            var respuesta = new RespuestaModel();
            respuesta.Indicador = false;
            respuesta.Mensaje = "Se presentó un problema en el sistema";

            return Ok(respuesta);
        }
    }
}
