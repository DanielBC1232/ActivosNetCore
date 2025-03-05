using Microsoft.AspNetCore.Mvc;
using ActivosNetCore.Models;
using Microsoft.Extensions.Configuration;
using System.Net.Http;
using System;

namespace ActivosNetCore.Controllers
{
    public class ActivosController : Controller
    {
        private readonly IHttpClientFactory _httpClient;
        private readonly IConfiguration _configuration;

        public ActivosController(IHttpClientFactory httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _configuration = configuration;
        }

        [HttpGet]
        public IActionResult ListaActivos(ActivosModel? model)
        {

            using (var api = _httpClient.CreateClient())
            {
                var url = _configuration.GetSection("Variables:urlApi").Value + "Activos/ListaActivos";
                var result = api.PostAsJsonAsync(url, model).Result;

                if (result.IsSuccessStatusCode)
                {
                    return View(result);

                }
            }
            var activos = new List<ActivosModel>();
            return View(activos);
        }
    }
}
