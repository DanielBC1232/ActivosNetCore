using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Data;
using ActivosAPI.Models;
using Dapper;

namespace ActivosAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuditoriaController : Controller
    {
        private readonly IConfiguration _configuration;

        public AuditoriaController(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        [HttpPost("ListaAuditoria")]
        public IActionResult ListaAuditoria([FromBody] FiltroAuditoriaModel filtros)
        {
            using var context = new SqlConnection(_configuration.GetConnectionString("BDConnection"));

            var result = context.Query<AuditoriaModel>("sp_ConsultarAuditoriaGeneral", new
            {
                tabla = filtros.Tabla,
                fechaInicio = filtros.FechaInicio,
                fechaFin = filtros.FechaFin,
                idUsuarioSesion = filtros.IdUsuarioSesion,
                accion = filtros.Accion
            }, commandType: CommandType.StoredProcedure);

            if (result.Any())
            {
                return Ok(result);
            }
            else
            {
                return NotFound(new { mensaje = "No se encontraron registros de auditoría." });
            }
        }

    }

}
