using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Headers;
using ActivosNetCore.Models;

namespace ActivosNetCore.Controllers
{
    public class AuditoriaController : Controller
    {
        private readonly IHttpClientFactory _httpClient;
        private readonly IConfiguration _configuration;

        public AuditoriaController(IHttpClientFactory httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _configuration = configuration;
        }

        [HttpGet]
        public IActionResult ListaAuditoria()
        {
            return View();
        }

        [HttpPost]
        public IActionResult ObtenerAuditoria(FiltroAuditoriaModel filtros)
        {
            using (var api = _httpClient.CreateClient())
            {
                api.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", HttpContext.Session.GetString("Token"));

                var url = _configuration.GetSection("Variables:urlApi").Value + "Auditoria/ListaAuditoria";
                var response = api.PostAsJsonAsync(url, filtros).Result;

                if (response.IsSuccessStatusCode)
                {
                    var auditorias = response.Content.ReadFromJsonAsync<IEnumerable<AuditoriaModel>>().Result;
                    return Json(auditorias);
                }
                else
                {
                    return Json(new List<AuditoriaModel>());
                }
            }
        }
    }

}
