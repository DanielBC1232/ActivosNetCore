using Microsoft.AspNetCore.Mvc;
using ActivosNetCore.Models;
using Microsoft.Extensions.Configuration;
using System.Net.Http;
using System;
using System.Reflection;

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

        //Listado de activos
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

        [HttpGet]
        public IActionResult AgregarActivo()
        {
            return View();
        }

        //Agregar nuevo activo
        [HttpPost]
        public IActionResult AgregarActivo(ActivosModel model)
        {
            using (var api = _httpClient.CreateClient())
            {
                var url = _configuration.GetSection("Variables:urlApi").Value + "Activos/AgregarActivo";
                var result = api.PostAsJsonAsync(url, model).Result;

                if (result.IsSuccessStatusCode)
                {
                    return RedirectToAction("ListaActivos", "Activos");
                }

                return View();
            }
        }

        [HttpGet]
        public async Task<IActionResult> DetallesActivo(int idActivo)
        {
            using (var api = _httpClient.CreateClient())
            {
                var url = _configuration.GetSection("Variables:urlApi").Value + "Activos/DetallesActivos/{idActivo}";
                var result = await api.GetAsync(url);

                if (result.IsSuccessStatusCode)
                {
                    var activo = await result.Content.ReadFromJsonAsync<ActivosModel>();
                    return View(activo);
                }
            };
            return View(new ActivosModel());
        }

        [HttpGet]
        public IActionResult EditarActivo()
        {
            return View();
        }

    }
}
