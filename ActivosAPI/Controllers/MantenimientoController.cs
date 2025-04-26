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
    // Controlador API para gestionar mantenimientos: listar, historial, detalles, crear, editar, eliminar
    public class MantenimientoController : Controller
    {
        // Inyección de configuración para leer cadena de conexión
        private readonly IConfiguration _configuration;

        // Constructor: inyectar IConfiguration
        public MantenimientoController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        // GET api/Mantenimiento/ListaMantenimiento
        // Lista todos los mantenimientos activos mediante SP
        [HttpGet]
        [Route("ListaMantenimiento")]
        public IActionResult ListaMantenimiento()
        {
            // Abrir conexión a la base de datos
            using var context = new SqlConnection(_configuration.GetSection("ConnectionStrings:BDConnection").Value);
            // Ejecutar stored procedure para obtener mantenimientos
            var result = context.Query<MantenimientoModel>(
                "SP_ConsultarTodosMantenimientos",
                commandType: CommandType.StoredProcedure
            );

            // Devolver 200 OK con datos si existen, o 404 NotFound si está vacío
            return result.Any()
                ? Ok(result)
                : NotFound(new { Indicador = false, Mensaje = "No hay datos disponibles" });
        }

        // GET api/Mantenimiento/HistorialMantenimiento
        // Lista historial completo de mantenimientos mediante SP
        [HttpGet]
        [Route("HistorialMantenimiento")]
        public IActionResult HistorialMantenimiento()
        {
            // Abrir conexión a BD
            using var context = new SqlConnection(_configuration.GetSection("ConnectionStrings:BDConnection").Value);
            // Ejecutar SP para historial de mantenimientos
            var result = context.Query<MantenimientoModel>(
                "SPP_ConsultarTodosMantenimientosHistorial",
                commandType: CommandType.StoredProcedure
            );

            // Devolver OK o NotFound según el resultado
            return result.Any()
                ? Ok(result)
                : NotFound(new { Indicador = false, Mensaje = "No hay datos disponibles" });
        }

        // GET api/Mantenimiento/DetallesMantenimiento?idMantenimiento=1
        // Obtiene detalle de un mantenimiento específico
        [HttpGet]
        [Route("DetallesMantenimiento")]
        public IActionResult DetallesMantenimiento([FromQuery] int idMantenimiento)
        {
            // Abrir conexión a BD
            using var context = new SqlConnection(_configuration.GetSection("ConnectionStrings:BDConnection").Value);
            // Parámetros para el SP
            var parametros = new { idMantenimiento };
            // Ejecutar SP para obtener un solo mantenimiento
            var mantenimiento = context.QueryFirstOrDefault<MantenimientoModel>(
                "SP_DetallesMantenimiento",
                parametros,
                commandType: CommandType.StoredProcedure
            );

            // Preparar respuesta con indicador y datos
            var respuesta = new RespuestaModel
            {
                Indicador = mantenimiento != null,
                Mensaje = mantenimiento != null ? "Mantenimiento encontrado" : "Mantenimiento no existe",
                Datos = mantenimiento
            };

            // Devolver resultado 200 OK
            return Ok(respuesta);
        }

        // POST api/Mantenimiento/AgregarMantenimiento
        // Crea un nuevo mantenimiento
        [HttpPost]
        [Route("AgregarMantenimiento")]
        public IActionResult AgregarMantenimiento([FromBody] MantenimientoModel model)
        {
            // Abrir conexión a BD
            using var context = new SqlConnection(_configuration.GetSection("ConnectionStrings:BDConnection").Value);
            // Ejecutar SP de inserción con datos del modelo
            var result = context.Execute(
                "sp_CrearMantenimiento",
                new
                {
                    model.Detalle,
                    model.IdUsuario,
                    model.IdActivo
                },
                commandType: CommandType.StoredProcedure
            );

            // Construir respuesta según filas afectadas
            var respuesta = new RespuestaModel
            {
                Indicador = result > 0,
                Mensaje = result > 0
                    ? "El Mantenimiento se ha registrado correctamente"
                    : "El Mantenimiento no ha registrado correctamente"
            };

            // Devolver 200 OK con respuesta
            return Ok(respuesta);
        }

        // PUT api/Mantenimiento/EditarMantenimiento
        // Actualiza un mantenimiento existente
        [HttpPut]
        [Route("EditarMantenimiento")]
        public IActionResult EditarMantenimiento([FromBody] MantenimientoModel model)
        {
            // Abrir conexión a BD
            using var context = new SqlConnection(_configuration.GetSection("ConnectionStrings:BDConnection").Value);
            // Ejecutar SP de actualización
            var result = context.Execute(
                "sp_ActualizarMantenimiento",
                new
                {
                    idMantenimiento = model.IdMantenimiento,
                    detalle = model.Detalle,
                    estado = model.Estado,
                    fecha = model.Fecha,
                    idResponsable = model.IdResponsable,
                    idActivo = model.IdActivo,
                    idUsuario = model.IdUsuario
                },
                commandType: CommandType.StoredProcedure
            );

            // Preparar respuesta
            var respuesta = new RespuestaModel
            {
                Indicador = result > 0,
                Mensaje = result > 0
                    ? "El Mantenimiento se ha actualizado correctamente"
                    : "El Mantenimiento no se ha actualizado"
            };

            // Devolver 200 OK o 500 Internal Server Error
            return result > 0
                ? Ok(respuesta)
                : StatusCode(500, respuesta);
        }

        // DELETE api/Mantenimiento/EliminarMantenimiento/5
        // Elimina un mantenimiento por ID usando SP
        [HttpDelete("EliminarMantenimiento/{idMantenimiento}")]
        public IActionResult EliminarMantenimiento(int idMantenimiento)
        {
            try
            {
                // Abrir y abrir conexión a BD
                using var connection = new SqlConnection(_configuration.GetSection("ConnectionStrings:BDConnection").Value);
                connection.Open();

                // Parámetros dinámicos para SP
                var parametros = new DynamicParameters();
                parametros.Add("@idMantenimiento", idMantenimiento);

                // Ejecutar SP de eliminación
                var result = connection.Execute(
                    "sp_EliminarMantenimiento",
                    parametros,
                    commandType: CommandType.StoredProcedure
                );

                // Preparar respuesta según resultado
                var respuesta = new RespuestaModel
                {
                    Indicador = result > 0,
                    Mensaje = result > 0
                        ? "El mantenimiento se ha eliminado correctamente"
                        : "No se pudo eliminar el mantenimiento"
                };

                // Devolver 200 OK o 500
                return result > 0 ? Ok(respuesta) : StatusCode(500, respuesta);
            }
            catch (Exception ex)
            {
                // Manejo de excepciones genericas
                var respuesta = new RespuestaModel
                {
                    Indicador = false,
                    Mensaje = "Error al eliminar: " + ex.Message
                };
                return StatusCode(500, respuesta);
            }
        }

        // GET api/Mantenimiento/ListaMantenimientoIndividual?idMantenimiento=1
        // Devuelve lista con un solo mantenimiento o vacía
        [HttpGet]
        [Route("ListaMantenimientoIndividual")]
        public IActionResult ListaMantenimientoIndividual([FromQuery] int idMantenimiento)
        {
            // Abrir conexión a BD
            using var context = new SqlConnection(_configuration.GetSection("ConnectionStrings:BDConnection").Value);
            // Ejecutar SP para obtener un mantenimiento
            var mantenimiento = context.QueryFirstOrDefault<MantenimientoModel>(
                "sp_ConsultarMantenimiento",
                new { idMantenimiento },
                commandType: CommandType.StoredProcedure
            );

            // Construir lista de resultados
            var lista = mantenimiento != null
                ? new List<MantenimientoModel> { mantenimiento }
                : new List<MantenimientoModel>();

            return Ok(lista);
        }

        // GET api/Mantenimiento/ListaMantenimientoFiltro?estado=Todos&urgencia=Todos
        // Lista mantenimientos según filtros de estado y urgencia
        [HttpGet]
        [Route("ListaMantenimientoFiltro")]
        public IActionResult ListaMantenimientoFiltro([FromQuery] string estado = "Todos", [FromQuery] string urgencia = "Todos")
        {
            // Abrir conexión a BD
            using var context = new SqlConnection(_configuration.GetSection("ConnectionStrings:BDConnection").Value);
            // Ejecutar SP con parámetros de filtro
            var result = context.Query<MantenimientoModel>(
                "sp_ConsultarTodosMantenimientosFiltro",
                new { estado, urgencia },
                commandType: CommandType.StoredProcedure
            );

            // Devolver OK o NotFound según existan datos
            return result.Any()
                ? Ok(result)
                : NotFound(new { Indicador = false, Mensaje = "No hay datos disponibles" });
        }
    }
}
