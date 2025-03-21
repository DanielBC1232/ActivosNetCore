using Microsoft.AspNetCore.Mvc;
using ActivosNetCore.Dependencias;
using Microsoft.Extensions.Configuration;
using System.Net.Http;
using System;
using System.Reflection;
using System.Text.Json;
using ActivosNetCore.Models;

namespace ActivosNetCore.Controllers
{
    public class ActivosController : Controller
    {
        private readonly IHttpClientFactory _httpClient;
        private readonly IConfiguration _configuration;
        private readonly IUtilitarios _utilitarios;

        public ActivosController(IHttpClientFactory httpClient, IConfiguration configuration, IUtilitarios utilitarios)
        {
            _httpClient = httpClient;
            _configuration = configuration;
            _utilitarios = utilitarios;

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
            if (!ModelState.IsValid)//Evitar enviar campos vacios
            {
                return View();
            }
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
        public IActionResult DetallesActivo(int idActivo)
        {
            var response = _utilitarios.ObtenerInfoActivo(idActivo) ?? new ActivosModel();

            if (response == null)
            {
                return NotFound("No se encontró el activo.");
            }

            return View(response);
        }

        [HttpGet]
        public IActionResult EditarActivo(int idActivo)
        {
            var response = _utilitarios.ObtenerInfoActivo(idActivo) ?? new ActivosModel();

            if (response == null)
            {
                return NotFound("No se encontró el activo.");
            }

            return View(response);
        }

        //Editar activo
        [HttpPost]
        public IActionResult EditarActivo(ActivosModel model)
        {
            if (!ModelState.IsValid)//Evitar enviar campos vacios
            {
                return View(model);
            }
            using (var api = _httpClient.CreateClient())
            {
                var url = _configuration.GetSection("Variables:urlApi").Value + "Activos/EditarActivo";
                var result = api.PutAsJsonAsync(url, model).Result;
                if (result.IsSuccessStatusCode)
                {
                    return RedirectToAction("ListaActivos", "Activos");
                }

                return View();
            }
        }

        [HttpPost]
        public IActionResult EliminarActivo(ActivosModel model)
        {
            using (var api = _httpClient.CreateClient())
            {
                var url = _configuration.GetSection("Variables:urlApi").Value + "Activos/EliminarActivo";
                var result = api.PutAsJsonAsync(url, model).Result;
                if (result.IsSuccessStatusCode)
                {
                    return RedirectToAction("ListaActivos", "Activos");
                }
                else
                {
                    return RedirectToAction("ListaActivos", "Activos");
                }
            }
        }

    }
}
