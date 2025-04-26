using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Data;
using Dapper;
using ActivosAPI.Models;
using System;
using Microsoft.Extensions.Configuration;

namespace ActivosAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    // Controlador API para gestionar auditorías: lista con filtros
    public class AuditoriaController : Controller
    {
        // Inyección de configuración para obtener cadena de conexión
        private readonly IConfiguration _configuration;

        // Constructor: inyectar IConfiguration
        public AuditoriaController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        // POST api/Auditoria/ListaAuditoria
        // Obtiene registros de auditoría según filtros mediante SP
        [HttpPost("ListaAuditoria")]
        public IActionResult ListaAuditoria([FromBody] FiltroAuditoriaModel filtros)
        {
            // Abrir conexión a la base de datos
            using var context = new SqlConnection(_configuration.GetConnectionString("BDConnection"));

            // Ejecutar stored procedure con parámetros de filtro
            var result = context.Query<AuditoriaModel>(
                "sp_ConsultarAuditoriaGeneral",
                new
                {
                    tabla = filtros.Tabla,
                    fechaInicio = filtros.FechaInicio,
                    fechaFin = filtros.FechaFin,
                    idUsuarioSesion = filtros.IdUsuarioSesion,
                    accion = filtros.Accion
                },
                commandType: CommandType.StoredProcedure
            );

            // Devolver 200 OK con datos si existen
            if (result.Any())
                return Ok(result);

            // Devolver 404 NotFound si no hay registros
            return NotFound(new { mensaje = "No se encontraron registros de auditoría." });
        }
    }
}
