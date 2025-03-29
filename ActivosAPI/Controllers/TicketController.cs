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
    public class TicketController : Controller
    {

        private readonly IConfiguration _configuration;

        public TicketController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [HttpGet]
        [Route("ListaTicket")]
        public IActionResult ListaTicket()
        {

            using (var context = new SqlConnection(_configuration.GetSection("ConnectionStrings:BDConnection").Value))
            {
                var result = context.Query<TicketModel>("sp_ConsultarTodosTickets",
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
        [Route("DetallesTicket")]
        public IActionResult DetallesTicket([FromQuery] int idTicket)
        {
            using (var context = new SqlConnection(_configuration.GetSection("ConnectionStrings:BDConnection").Value))
            {
                var parametros = new { idTicket };

                var ticket = context.QueryFirstOrDefault<TicketModel>(
                    "SP_DetallesTicket",
                    parametros,
                    commandType: CommandType.StoredProcedure
                );

                var respuesta = new RespuestaModel
                {
                    Indicador = ticket != null,
                    Mensaje = ticket != null ? "Ticket encontrado" : "Ticket no existe",
                    Datos = ticket  
                };

                return Ok(respuesta);
            }
        }



        [HttpPost]
        [Route("AgregarTicket")]
        public IActionResult AgregarTicket(TicketModel model)
        {
            using (var context = new SqlConnection(_configuration.GetSection("ConnectionStrings:BDConnection").Value))
            {
                var result = context.Execute("sp_CrearTicket",
                    new
                    {
                        model.Urgencia,
                        model.Detalle,
                        model.IdUsuario,
                        model.IdDepartamento
                    });

                var respuesta = new RespuestaModel();

                if (result > 0)
                {
                    respuesta.Indicador = true;
                    respuesta.Mensaje = "El ticket se ha registrado correctamente";
                }
                else
                {
                    respuesta.Indicador = false;
                    respuesta.Mensaje = "El ticket no ha registrado correctamente";
                }

                return Ok(respuesta);
            }
        }

        [HttpPut]
        [Route("EditarTicket")]
        public IActionResult EditarTicket(TicketModel model)
        {
            using (var context = new SqlConnection(_configuration.GetSection("ConnectionStrings:BDConnection").Value))
            {
                // Aquí se usan todos los parámetros enviados desde el modelo
                var result = context.Execute("sp_ActualizarTicket",
                    new
                    {
                        idTicket = model.IdTicket,
                        urgencia = model.Urgencia,
                        solucionado = model.Solucionado,
                        detalleTecnico = model.DetalleTecnico,
                        idResponsable = model.IdResponsable
                    });

                var respuesta = new RespuestaModel();

                if (result > 0)
                {
                    respuesta.Indicador = true;
                    respuesta.Mensaje = "El ticket se ha actualizado correctamente";
                    return Ok(respuesta);
                }
                else
                {
                    respuesta.Indicador = false;
                    respuesta.Mensaje = "El ticket no se ha actualizado";
                    return StatusCode(500, respuesta);
                }
            }
        }


        [HttpPut]
        [Route("EliminarTicket")]
        public IActionResult EliminarTicket(TicketModel model)
        {
            using (var context = new SqlConnection(_configuration.GetSection("ConnectionStrings:BDConnection").Value))
            {
                var result = context.Execute("sp_EliminarTicket",
                    new { model.IdTicket });

                var respuesta = new RespuestaModel();

                if (result > 0)
                {
                    respuesta.Indicador = true;
                    respuesta.Mensaje = "El ticket se ha desactivado correctamente";
                    return Ok(respuesta);
                }
                else
                {
                    respuesta.Indicador = false;
                    respuesta.Mensaje = "El ticket no ha desactivado";
                    return StatusCode(500, respuesta);
                }

            }
        }


        [HttpGet]
        [Route("ListaTicketIndividual")]
        public IActionResult ListaTicketIndividual([FromQuery] int idTicket)
        {
            using (var context = new SqlConnection(_configuration.GetSection("ConnectionStrings:BDConnection").Value))
            {
                var ticket = context.QueryFirstOrDefault<TicketModel>(
                    "sp_ConsultarTicket",
                    new { idTicket },
                    commandType: CommandType.StoredProcedure
                );

                var lista = new List<TicketModel>();

                if (ticket != null)
                {
                    lista.Add(ticket);
                }

                return Ok(lista);
            }
        }

        [HttpGet]
        [Route("ListaTicketFiltro")]
        public IActionResult ListaTicketFiltro([FromQuery] string estado = "Todos", [FromQuery] string urgencia = "Todos")
        {
            using (var context = new SqlConnection(_configuration.GetSection("ConnectionStrings:BDConnection").Value))
            {
                var result = context.Query<TicketModel>("sp_ConsultarTodosTicketsFiltro",
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
