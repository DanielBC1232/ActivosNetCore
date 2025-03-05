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
        private object respuesta;
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
    }
}
