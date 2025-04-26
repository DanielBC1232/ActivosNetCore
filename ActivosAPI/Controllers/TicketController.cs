using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.Data.SqlClient;
using Dapper;
using ActivosAPI.Models;

namespace ActivosAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    // Controlador API para gestionar tickets: listar, detalles, crear, editar, eliminar, filtros
    public class TicketController : Controller
    {
        // Lectura de configuración para cadenas de conexión
        private readonly IConfiguration _configuration;

        // Constructor: inyecta IConfiguration
        public TicketController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        // GET api/Ticket/ListaTicket
        // Lista todos los tickets disponibles mediante un stored procedure
        [HttpGet]
        [Route("ListaTicket")]
        public IActionResult ListaTicket()
        {
            // Abrir conexión a la base de datos
            using var context = new SqlConnection(_configuration.GetSection("ConnectionStrings:BDConnection").Value);
            // Ejecutar SP para consultar tickets
            var result = context.Query<TicketModel>(
                "spp_ConsultarTodosTickets",
                commandType: CommandType.StoredProcedure
            );

            // Devolver 200 OK si hay datos o 404 NotFound si no
            if (result.Any())
                return Ok(result);

            return NotFound(new { Indicador = false, Mensaje = "No hay datos disponibles" });
        }

        // GET api/Ticket/DetallesTicket?idTicket=1
        // Obtiene detalle de un ticket específico
        [HttpGet]
        [Route("DetallesTicket")]
        public IActionResult DetallesTicket([FromQuery] int idTicket)
        {
            // Conexión a BD
            using var context = new SqlConnection(_configuration.GetSection("ConnectionStrings:BDConnection").Value);
            // Parámetros para SP
            var parametros = new { idTicket };
            // Ejecutar SP para obtener un ticket
            var ticket = context.QueryFirstOrDefault<TicketModel>(
                "SPP_DetallesTicket",
                parametros,
                commandType: CommandType.StoredProcedure
            );

            // Construir respuesta con indicador y datos
            var respuesta = new RespuestaModel
            {
                Indicador = ticket != null,
                Mensaje = ticket != null ? "Ticket encontrado" : "Ticket no existe",
                Datos = ticket
            };

            // Devolver resultado 200 OK con respuesta
            return Ok(respuesta);
        }

        // POST api/Ticket/AgregarTicket
        // Crea un nuevo ticket
        [HttpPost]
        [Route("AgregarTicket")]
        public IActionResult AgregarTicket([FromBody] TicketModel model)
        {
            // Conexión a BD
            using var context = new SqlConnection(_configuration.GetSection("ConnectionStrings:BDConnection").Value);
            // Ejecutar SP de inserción con parámetros del modelo
            var result = context.Execute(
                "spp_CrearTicket",
                new
                {
                    model.Urgencia,
                    model.Detalle,
                    model.IdUsuario,
                    model.IdDepartamento
                },
                commandType: CommandType.StoredProcedure
            );

            // Construir respuesta según cantidad de filas afectadas
            var respuesta = new RespuestaModel
            {
                Indicador = result > 0,
                Mensaje = result > 0 ? "El ticket se ha registrado correctamente" : "El ticket no ha registrado correctamente"
            };

            // Devolver 200 OK con respuesta
            return Ok(respuesta);
        }

        // PUT api/Ticket/EditarTicket
        // Actualiza un ticket existente
        [HttpPut]
        [Route("EditarTicket")]
        public IActionResult EditarTicket([FromBody] TicketModel model)
        {
            // Abrir conexión a BD
            using var context = new SqlConnection(_configuration.GetSection("ConnectionStrings:BDConnection").Value);
            // Ejecutar SP de actualización con todos los parámetros enviados
            var result = context.Execute(
                "spp_ActualizarTicket",
                new
                {
                    idTicket = model.IdTicket,
                    urgencia = model.Urgencia,
                    solucionado = model.Solucionado,
                    detalleTecnico = model.DetalleTecnico,
                    idResponsable = model.IdResponsable
                },
                commandType: CommandType.StoredProcedure
            );

            // Construir respuesta según éxito de la operación
            var respuesta = new RespuestaModel
            {
                Indicador = result > 0,
                Mensaje = result > 0 ? "El ticket se ha actualizado correctamente" : "El ticket no se ha actualizado"
            };

            // Devolver 200 OK o 500 Internal Server Error
            return result > 0 ? Ok(respuesta) : StatusCode(500, respuesta);
        }

        // DELETE api/Ticket/EliminarTicket/5
        // Elimina un ticket por ID mediante stored procedure
        [HttpDelete("EliminarTicket/{idTicket}")]
        public IActionResult EliminarTicket(int idTicket)
        {
            try
            {
                // Conexión y apertura de BD
                using var connection = new SqlConnection(_configuration.GetSection("ConnectionStrings:BDConnection").Value);
                connection.Open();

                // Configurar parámetros dinámicos
                var parametros = new DynamicParameters();
                parametros.Add("@idTicket", idTicket);

                // Ejecutar SP de eliminación
                var result = connection.Execute(
                    "sp_EliminarTicket",
                    parametros,
                    commandType: CommandType.StoredProcedure
                );

                // Construir respuesta según resultado
                var respuesta = new RespuestaModel
                {
                    Indicador = result > 0,
                    Mensaje = result > 0 ? "El Ticket se ha eliminado correctamente" : "No se pudo eliminar el Ticket"
                };

                // Devolver 200 OK o 500
                return result > 0 ? Ok(respuesta) : StatusCode(500, respuesta);
            }
            catch (Exception ex)
            {
                // Capturar excepción y devolver 500 con mensaje
                var respuesta = new RespuestaModel
                {
                    Indicador = false,
                    Mensaje = "Error al eliminar: " + ex.Message
                };
                return StatusCode(500, respuesta);
            }
        }

        // GET api/Ticket/ListaTicketIndividual?idTicket=1
        // Devuelve lista con un solo ticket si existe, o lista vacía
        [HttpGet]
        [Route("ListaTicketIndividual")]
        public IActionResult ListaTicketIndividual([FromQuery] int idTicket)
        {
            // Conexión a BD
            using var context = new SqlConnection(_configuration.GetSection("ConnectionStrings:BDConnection").Value);
            // Ejecutar SP para consulta individual
            var ticket = context.QueryFirstOrDefault<TicketModel>(
                "sp_ConsultarTicket",
                new { idTicket },
                commandType: CommandType.StoredProcedure
            );

            // Construir lista con ticket o vacía
            var lista = ticket != null ? new List<TicketModel> { ticket } : new List<TicketModel>();
            return Ok(lista);
        }

        // GET api/Ticket/ListaTicketFiltro?estado=Todos&urgencia=Todos
        // Lista tickets según filtros de estado y urgencia
        [HttpGet]
        [Route("ListaTicketFiltro")]
        public IActionResult ListaTicketFiltro([FromQuery] string estado = "Todos", [FromQuery] string urgencia = "Todos")
        {
            // Abrir conexión a BD
            using var context = new SqlConnection(_configuration.GetSection("ConnectionStrings:BDConnection").Value);
            // Ejecutar SP con parámetros de filtro
            var result = context.Query<TicketModel>(
                "sp_ConsultarTodosTicketsFiltro",
                new { estado, urgencia },
                commandType: CommandType.StoredProcedure
            );

            // Devolver OK con datos o NotFound si vacío
            return result.Any() ? Ok(result) : NotFound(new { Indicador = false, Mensaje = "No hay datos disponibles" });
        }

        // GET api/Ticket/ListaSoportes
        // Lista usuarios con rol de soporte para asignación
        [HttpGet]
        [Route("ListaSoportes")]
        public IActionResult ListaSoportes()
        {
            // Conexión y ejecución de SP
            using var context = new SqlConnection(_configuration.GetSection("ConnectionStrings:BDConnection").Value);
            var result = context.Query<UsuarioModel>(
                "sp_ListarSoportes",
                commandType: CommandType.StoredProcedure
            );

            // Devolver OK o NotFound
            return result.Any() ? Ok(result) : NotFound(new { Indicador = false, Mensaje = "No hay datos disponibles" });
        }

        // GET api/Ticket/HistorialTicket
        // Lista completo del historial de tickets
        [HttpGet]
        [Route("HistorialTicket")]
        public IActionResult HistorialTicket()
        {
            // Conexión y SP de historial
            using var context = new SqlConnection(_configuration.GetSection("ConnectionStrings:BDConnection").Value);
            var result = context.Query<TicketModel>(
                "SPP_ConsultarTodosTicketsHistorial",
                commandType: CommandType.StoredProcedure
            );

            // Devolver OK o NotFound si no hay datos
            return result.Any() ? Ok(result) : NotFound(new { Indicador = false, Mensaje = "No hay datos disponibles" });
        }
    }
}