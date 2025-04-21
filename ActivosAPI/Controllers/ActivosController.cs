using Dapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using ActivosAPI.Dependencias;
using ActivosAPI.Models;
using System.Data;

namespace ActivosAPI.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class ActivosController : ControllerBase
    {

        private readonly IConfiguration _configuration;
        private readonly IUtilitarios _utilitarios;
        public ActivosController(IConfiguration configuration, IUtilitarios utilitarios)
        {
            _configuration = configuration;
            _utilitarios = utilitarios;
        }

        [HttpPost]
        [Route("ListaActivos")]
        public IActionResult ListaActivos(int? idDepartamento)
        {

            if (!_utilitarios.ValidarTecnicoFromToken(User.Claims))
            {
                return Ok();
            }

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
        [Route("DetallesActivo")]
        public async Task<IActionResult> DetallesActivo([FromQuery] int idActivo)
        {
            using (var context = new SqlConnection(_configuration.GetSection("ConnectionStrings:BDConnection").Value))
            {
                string storedProcedure = "SP_DetallesActivo";
                var parameters = new { idActivo = idActivo };

                // Esperar correctamente la tarea asincrónica
                var activo = await context.QueryFirstOrDefaultAsync<ActivosModel>(
                    storedProcedure,
                    parameters,
                    commandType: CommandType.StoredProcedure);

                if (activo == null)
                {
                    return NotFound(new { message = "Activo no encontrado" });
                }

                return Ok(new { Indicador = true, Mensaje = "Activo encontrado", Datos = activo });
            }
        }

        [HttpPost]
        [Route("AgregarActivo")]
        public IActionResult AgregarActivo(ActivosModel model)
        {
            using (var context = new SqlConnection(_configuration.GetSection("ConnectionStrings:BDConnection").Value))
            {
                var result = context.Execute("SP_AgregarActivo",
                    new { model.nombreActivo, model.placa, model.serie, model.descripcion, model.idDepartamento, model.idResponsable });

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

        [HttpPut]
        [Route("EditarActivo")]
        public IActionResult EditarActivo(ActivosModel model)
        {
            using (var context = new SqlConnection(_configuration.GetSection("ConnectionStrings:BDConnection").Value))
            {
                var result = context.Execute("SP_EditarActivo",
                    new { model.idActivo, model.nombreActivo, model.placa, model.serie, model.descripcion, model.idDepartamento, model.idResponsable });

                var respuesta = new RespuestaModel();

                if (result > 0)
                {
                    respuesta.Indicador = true;
                    respuesta.Mensaje = "El activo se ha actualizado correctamente";
                    return Ok(respuesta);
                }
                else
                {
                    respuesta.Indicador = false;
                    respuesta.Mensaje = "El activo no ha actualizado";
                    return StatusCode(500, respuesta);

                }

            }
        }

        [HttpPut]
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
                    return Ok(respuesta);
                }
                else
                {
                    respuesta.Indicador = false;
                    respuesta.Mensaje = "El activo no ha desactivado";
                    return StatusCode(500, respuesta);
                }

            }
        }


    }
}
