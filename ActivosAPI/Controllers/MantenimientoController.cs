using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System.Net.Http;
using Microsoft.Data.SqlClient;
using Dapper;
using System.Data;
using ActivosAPI.Models;

namespace ActivosAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MantenimientoController : Controller
    {

        private readonly IConfiguration _configuration;

        public MantenimientoController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [HttpGet]
        [Route("ListaMantenimiento")]
        public IActionResult ListaMantenimiento()
        {

            using (var context = new SqlConnection(_configuration.GetSection("ConnectionStrings:BDConnection").Value))
            {
                var result = context.Query<MantenimientoModel>("SP_ConsultarTodosMantenimientos",
               commandType: CommandType.StoredProcedure);

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
        [Route("DetallesMantenimiento")]
        public IActionResult DetallesMantenimiento([FromQuery] int idMantenimiento)
        {
            using (var context = new SqlConnection(_configuration.GetSection("ConnectionStrings:BDConnection").Value))
            {
                var parametros = new { idMantenimiento };

                var Mantenimiento = context.QueryFirstOrDefault<MantenimientoModel>(
                    "SP_DetallesMantenimiento",
                    parametros,
                    commandType: CommandType.StoredProcedure
                );

                var respuesta = new RespuestaModel
                {
                    Indicador = Mantenimiento != null,
                    Mensaje = Mantenimiento != null ? "Mantenimiento encontrado" : "Mantenimiento no existe",
                    Datos = Mantenimiento  
                };

                return Ok(respuesta);
            }
        }



        [HttpPost]
        [Route("AgregarMantenimiento")]
        public IActionResult AgregarMantenimiento(MantenimientoModel model)
        {
            using (var context = new SqlConnection(_configuration.GetSection("ConnectionStrings:BDConnection").Value))
            {
                var result = context.Execute("sp_CrearMantenimiento",
                    new
                    {
                        model.Detalle,
                        model.IdUsuario,
                        model.IdActivo
                    });

                var respuesta = new RespuestaModel();

                if (result > 0)
                {
                    respuesta.Indicador = true;
                    respuesta.Mensaje = "El Mantenimiento se ha registrado correctamente";
                }
                else
                {
                    respuesta.Indicador = false;
                    respuesta.Mensaje = "El Mantenimiento no ha registrado correctamente";
                }

                return Ok(respuesta);
            }
        }

        [HttpPut]
        [Route("EditarMantenimiento")]
        public IActionResult EditarMantenimiento(MantenimientoModel model)
        {
            using (var context = new SqlConnection(_configuration.GetSection("ConnectionStrings:BDConnection").Value))
            {
                // Aquí se usan todos los parámetros enviados desde el modelo
                var result = context.Execute("sp_ActualizarMantenimiento",
                    new
                    {
                        idMantenimiento = model.IdMantenimiento,
                        detalle = model.Detalle,
                        estado = model.Estado,
                        fecha = model.Fecha,
                        idResponsable = model.IdResponsable,
                        idActivo = model.IdActivo,
                        idUsuario = model.IdUsuario
                    });

                var respuesta = new RespuestaModel();

                if (result > 0)
                {
                    respuesta.Indicador = true;
                    respuesta.Mensaje = "El Mantenimiento se ha actualizado correctamente";
                    return Ok(respuesta);
                }
                else
                {
                    respuesta.Indicador = false;
                    respuesta.Mensaje = "El Mantenimiento no se ha actualizado";
                    return StatusCode(500, respuesta);
                }
            }
        }


        [HttpPut]
        [Route("EliminarMantenimiento")]
        public IActionResult EliminarMantenimiento(MantenimientoModel model)
        {
            using (var context = new SqlConnection(_configuration.GetSection("ConnectionStrings:BDConnection").Value))
            {
                var result = context.Execute("sp_EliminarMantenimiento",
                    new { model.IdMantenimiento });

                var respuesta = new RespuestaModel();

                if (result > 0)
                {
                    respuesta.Indicador = true;
                    respuesta.Mensaje = "El Mantenimiento se ha desactivado correctamente";
                    return Ok(respuesta);
                }
                else
                {
                    respuesta.Indicador = false;
                    respuesta.Mensaje = "El Mantenimiento no ha desactivado";
                    return StatusCode(500, respuesta);
                }

            }
        }


        [HttpGet]
        [Route("ListaMantenimientoIndividual")]
        public IActionResult ListaMantenimientoIndividual([FromQuery] int idMantenimiento)
        {
            using (var context = new SqlConnection(_configuration.GetSection("ConnectionStrings:BDConnection").Value))
            {
                var Mantenimiento = context.QueryFirstOrDefault<MantenimientoModel>(
                    "sp_ConsultarMantenimiento",
                    new { idMantenimiento },
                    commandType: CommandType.StoredProcedure
                );
                var lista = new List<MantenimientoModel>();
                if (Mantenimiento != null)
                {
                    lista.Add(Mantenimiento);
                }

                return Ok(lista);
            }
        }

        [HttpGet]
        [Route("ListaMantenimientoFiltro")]
        public IActionResult ListaMantenimientoFiltro([FromQuery] string estado = "Todos", [FromQuery] string urgencia = "Todos")
        {
            using (var context = new SqlConnection(_configuration.GetSection("ConnectionStrings:BDConnection").Value))
            {
                var result = context.Query<MantenimientoModel>("sp_ConsultarTodosMantenimientosFiltro",
                    new { estado, urgencia },
                    commandType: CommandType.StoredProcedure);

                if (result.Any())
                    return Ok(result);
                else
                    return NotFound(new { Indicador = false, Mensaje = "No hay datos disponibles" });
            }
        }


    }
}
