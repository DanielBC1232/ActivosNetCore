using ActivosAPI.Models;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using ActivosAPI.Dependencias;
using Dapper;

namespace ActivosAPI.Controllers
{
    [ApiExplorerSettings(IgnoreApi = true)]
    [Route("api/[controller]")]
    [ApiController]
    public class ErrorController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly IUtilitarios _utilitarios;

        public ErrorController(IConfiguration configuration, IUtilitarios utilitarios)
        {
            _configuration = configuration;
            _utilitarios = utilitarios;
        }

        // Endpoint para capturar errores del proyecto
        [HttpPost]
        [Route("CapturarError")]
        public IActionResult CapturarError()
        {
            // Obtener detalles del error actual del contexto HTTP
            var ex = HttpContext.Features.Get<IExceptionHandlerFeature>();

            using (var context = new SqlConnection(_configuration.GetSection("ConnectionStrings:BDConnection").Value))
            {
                var IdUsuario = _utilitarios.ObtenerUsuarioFromToken(User.Claims);
                var Mensaje = ex!.Error.Message;
                var Origen = ex.Path;

                context.Execute("RegistrarError",
                new { IdUsuario, Mensaje, Origen });
            }

            var respuesta = new RespuestaModel();
            respuesta.Indicador = false;
            respuesta.Mensaje = "Se presentó un problema en el sistema";

            return Ok(respuesta);
        }
    }
}
