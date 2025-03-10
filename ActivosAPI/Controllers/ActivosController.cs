using Microsoft.AspNetCore.Mvc;
using ActivosNetCore.Models;
using Microsoft.Extensions.Configuration;
using System.Net.Http;
using Microsoft.Data.SqlClient;
using Dapper;
using ActivosAPI.Models;
using System.Data;

namespace ActivosNetCore.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ActivosController : Controller
    {

        private readonly IConfiguration _configuration;

        public ActivosController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [HttpGet]
        [Route("ListaActivos")]
        public IActionResult ListaActivos(int? idDepartamento)
        {

            using (var context = new SqlConnection(_configuration.GetSection("ConnectionStrings:BDConnection").Value))
            {
                var result = context.Query<ActivosModel>("SP_ListadoActivo",
                    new { idDepartamento },
                    commandType: CommandType.StoredProcedure).ToList();

                if (result.Any())
                {
                    return Ok(result);
                }
                else
                {
                    return NotFound(new { Indicador = false, Mensaje = "No hay datos disponibles" });
                }
            }

        }

        [HttpGet]
        [Route("DetallesActivos")]
        public IActionResult DetallesActivos(int? idActivo)
        {
            using (var context = new SqlConnection(_configuration.GetSection("ConnectionStrings:BDConnection").Value))
            {
                var result = context.Query<ActivosModel>("SP_DetallesActivo",
                    new { idActivo },
                    commandType: CommandType.StoredProcedure).ToList();

                if (result.Any())
                {
                    return Ok(result);
                }
                else
                {
                    return NotFound(new { Indicador = false, Mensaje = "No hay datos disponibles" });
                }
            }

        }

        [HttpPost]
        [Route("AgregarActivo")]
        public IActionResult AgregarActivo(ActivosModel model)
        {
            using (var context = new SqlConnection(_configuration.GetSection("ConnectionStrings:BDConnection").Value))
            {
                var result = context.Execute("SP_AgregarActivo",
                    new { model.nombreActivo, model.placa, model.serie, model.descripcion, model.idDepartamento, model.idResponsable});

                var respuesta = new RespuestaModel();

                if (result > 0)
                {
                    respuesta.Indicador = true;
                    respuesta.Mensaje = "El activo se ha registrado correctamente";
                }
                else
                {
                    respuesta.Indicador = false;
                    respuesta.Mensaje = "El activo no ha registrado correctamente";
                }

                return Ok(respuesta);
            }
        }

        [HttpPost]
        [Route("EditarActivo")]
        public IActionResult EditarActivo(ActivosModel model)
        {
            using (var context = new SqlConnection(_configuration.GetSection("ConnectionStrings:BDConnection").Value))
            {
                var result = context.Execute("SP_EditarActivo",
                    new { model.idActivo,model.nombreActivo, model.placa, model.serie, model.descripcion, model.idDepartamento, model.idResponsable });

                var respuesta = new RespuestaModel();

                if (result > 0)
                {
                    respuesta.Indicador = true;
                    respuesta.Mensaje = "El activo se ha actualizado correctamente";
                }
                else
                {
                    respuesta.Indicador = false;
                    respuesta.Mensaje = "El activo no ha actualizado correctamente";
                }

                return Ok(respuesta);
            }
        }

        [HttpPost]
        [Route("EliminarActivo")]
        public IActionResult EliminarActivo(ActivosModel model)
        {
            using (var context = new SqlConnection(_configuration.GetSection("ConnectionStrings:BDConnection").Value))
            {
                var result = context.Execute("SP_EliminarActivo",
                    new { model.idActivo });

                var respuesta = new RespuestaModel();

                if (result > 0)
                {
                    respuesta.Indicador = true;
                    respuesta.Mensaje = "El activo se ha desactivado correctamente";
                }
                else
                {
                    respuesta.Indicador = false;
                    respuesta.Mensaje = "El activo no ha desactivado correctamente";
                }

                return Ok(respuesta);
            }
        }

    }
}
