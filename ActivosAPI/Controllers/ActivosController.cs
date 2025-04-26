using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Dapper;
using ActivosAPI.Dependencias;
using ActivosAPI.Models;
using Microsoft.Extensions.Configuration;
using System;
using System.Data;
using System.Linq;

namespace ActivosAPI.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    // Controlador API para gestionar activos: listado, detalles, creación, edición, eliminación
    public class ActivosController : ControllerBase
    {
        // Inyección de configuración y utilitarios
        private readonly IConfiguration _configuration;
        private readonly IUtilitarios _utilitarios;

        // Constructor: inyectar IConfiguration y utilitarios
        public ActivosController(IConfiguration configuration, IUtilitarios utilitarios)
        {
            _configuration = configuration;
            _utilitarios = utilitarios;
        }

        // POST api/Activos/ListaActivos
        // Lista activos según departamento con validación de rol técnico
        [HttpPost]
        [Route("ListaActivos")]
        public IActionResult ListaActivos([FromBody] DepartamentoModel model)
        {
            // Obtener id de departamento del modelo
            var idDepartamento = model.idDepartamento;
            // Validar que el usuario tenga rol técnico según token
            if (!_utilitarios.ValidarTecnicoFromToken(User.Claims))
            {
                return Unauthorized(new { message = "Acceso no autorizado" });
            }

            // Conexión a la base de datos
            using var context = new SqlConnection(_configuration.GetConnectionString("BDConnection"));
            // Ejecutar SP con idDepartamento
            var result = context.Query<ActivosModel>(
                "SP_ListadoActivo",
                new { idDepartamento },
                commandType: CommandType.StoredProcedure
            ).ToList();

            // Devolver resultados o NotFound si vacíos
            return result.Any()
                ? Ok(result)
                : NotFound(new { Indicador = false, Mensaje = "No hay datos disponibles" });
        }

        // GET api/Activos/ListaActivosDrop?userId=1
        // Lista activos para dropdown según usuario, acceso anónimo
        [AllowAnonymous]
        [HttpGet]
        [Route("ListaActivosDrop")]
        public IActionResult ListaActivosDrop([FromQuery] int userId)
        {
            // Conexión a BD
            using var db = new SqlConnection(_configuration.GetConnectionString("BDConnection"));
            // Ejecutar SP para dropdown de activos
            var activos = db.Query<ActivosModel>(
                "SP_ListadoActivoDrop",
                new { idUsuario = userId },
                commandType: CommandType.StoredProcedure
            ).ToList();

            // Devolver OK o NotFound
            return activos.Any()
                ? Ok(activos)
                : NotFound(new { Indicador = false, Mensaje = "No hay activos para este usuario." });
        }

        // GET api/Activos/DetallesActivo?idActivo=1
        // Obtiene detalles de un activo con validación de rol técnico
        [HttpGet]
        [Route("DetallesActivo")]
        public async System.Threading.Tasks.Task<IActionResult> DetallesActivo([FromQuery] int idActivo)
        {
            // Validar rol técnico
            if (!_utilitarios.ValidarTecnicoFromToken(User.Claims))
            {
                return Unauthorized(new { message = "Acceso no autorizado" });
            }

            // Conexión y ejecución asincrónica del SP
            using var context = new SqlConnection(_configuration.GetConnectionString("BDConnection"));
            var activo = await context.QueryFirstOrDefaultAsync<ActivosModel>(
                "SP_DetallesActivo",
                new { idActivo },
                commandType: CommandType.StoredProcedure
            );

            // Retornar NotFound si no existe
            if (activo == null)
            {
                return NotFound(new { message = "Activo no encontrado" });
            }

            // Devolver resultado 200 OK con datos
            return Ok(new { Indicador = true, Mensaje = "Activo encontrado", Datos = activo });
        }

        // POST api/Activos/AgregarActivo
        // Crea un nuevo activo
        [HttpPost]
        [Route("AgregarActivo")]
        public IActionResult AgregarActivo([FromBody] ActivosModel model)
        {
            try
            {
                // Conexión a BD
                using var context = new SqlConnection(_configuration.GetConnectionString("BDConnection"));
                // Ejecutar SP de creación con parámetros del modelo
                context.Execute(
                    "SP_AgregarActivo",
                    new
                    {
                        model.nombreActivo,
                        model.placa,
                        model.serie,
                        model.descripcion,
                        model.idDepartamento,
                        model.idUsuario,
                        model.IdUsuarioSesion
                    },
                    commandType: CommandType.StoredProcedure
                );

                // Devolver 200 OK
                return Ok(new { Indicador = true, Mensaje = "Activo creado correctamente" });
            }
            catch (SqlException ex)
            {
                // Capturar error SQL
                return BadRequest(new { Indicador = false, Mensaje = ex.Message });
            }
        }

        // PUT api/Activos/EditarActivo
        // Edita un activo existente
        [HttpPut]
        [Route("EditarActivo")]
        public IActionResult EditarActivo([FromBody] ActivosModel model)
        {
            try
            {
                // Conexión a BD y ejecución del SP de edición
                using var context = new SqlConnection(_configuration.GetConnectionString("BDConnection"));
                context.Execute(
                    "SP_EditarActivo",
                    new
                    {
                        model.idActivo,
                        model.nombreActivo,
                        model.placa,
                        model.serie,
                        model.descripcion,
                        model.idDepartamento,
                        model.idUsuario,
                        model.IdUsuarioSesion
                    },
                    commandType: CommandType.StoredProcedure
                );

                // Devolver 200 OK
                return Ok(new { Indicador = true, Mensaje = "Activo editado correctamente" });
            }
            catch (SqlException ex)
            {
                // Capturar error SQL
                return BadRequest(new { Indicador = false, Mensaje = ex.Message });
            }
        }

        // PUT api/Activos/EliminarActivo
        // Elimina/desactiva un activo según modelo
        [HttpPut]
        [Route("EliminarActivo")]
        public IActionResult EliminarActivo([FromBody] ActivosModel model)
        {
            // Validar rol técnico
            if (!_utilitarios.ValidarTecnicoFromToken(User.Claims))
            {
                return Unauthorized(new { message = "Acceso no autorizado" });
            }

            var respuesta = new RespuestaModel();
            try
            {
                // Conexión y ejecución del SP de eliminación
                using var context = new SqlConnection(_configuration.GetConnectionString("BDConnection"));
                var result = context.Execute(
                    "SP_EliminarActivo",
                    new { model.idActivo, model.IdUsuarioSesion },
                    commandType: CommandType.StoredProcedure
                );

                // Construir respuesta según resultado
                respuesta.Indicador = result > 0;
                respuesta.Mensaje = result > 0
                    ? "El activo se ha desactivado correctamente"
                    : "El activo no se pudo desactivar";

                // Devolver 200 OK o 500
                return result > 0
                    ? Ok(respuesta)
                    : StatusCode(500, respuesta);
            }
            catch (SqlException ex)
            {
                // Capturar excepción SQL
                respuesta.Indicador = false;
                respuesta.Mensaje = ex.Message;
                return BadRequest(respuesta);
            }
        }
    }
}