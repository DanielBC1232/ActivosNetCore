using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using ActivosNetCore.Dependencias;
using Microsoft.Extensions.Configuration;
using System.Net.Http;
using System;
using System.Reflection;
using System.Text.Json;
using ActivosNetCore.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Net.Sockets;

namespace ActivosNetCore.Controllers
{

    public class MantenimientoController : Controller
    {
        private readonly IHttpClientFactory _httpClient;
        private readonly IConfiguration _configuration;
        private readonly IUtilitarios _utilitarios;

        public MantenimientoController(IHttpClientFactory httpClient, IConfiguration configuration, IUtilitarios utilitarios)
        {
            _httpClient = httpClient;
            _configuration = configuration;
            _utilitarios = utilitarios;

        }

        [HttpGet]
        public async Task<IActionResult> ListaMantenimiento(MantenimientoModel? model)
        {
            using (var api = _httpClient.CreateClient())
            {
                var url = _configuration.GetSection("Variables:urlApi").Value + "Mantenimiento/ListaMantenimiento";
                var result = await api.GetAsync(url);

                if (result.IsSuccessStatusCode)
                {
                    var Mantenimientos = await result.Content.ReadFromJsonAsync<List<MantenimientoModel>>();
                    return View(Mantenimientos);
                }
            }
            var Mantenimiento = new List<MantenimientoModel>();
            return View(Mantenimiento);
        }

        [HttpGet]
        public async Task<IActionResult> AgregarMantenimiento()
        {
            // 1. Obtener lista de activos desde tu API
            var client = _httpClient.CreateClient();
            var url = _configuration["Variables:urlApi"] + "Activos/ListaActivos";
            var resp = await client.GetAsync(url);

            List<ActivosModel> activos = new();
            if (resp.IsSuccessStatusCode)
            {
                activos = await resp.Content.ReadFromJsonAsync<List<ActivosModel>>();
            }

            // 2. Meterlos en ViewBag para el dropdown
            ViewBag.ListaActivos = new SelectList(activos, "idActivo", "nombreActivo");

            // 2. Inicializar modelo con fecha de hoy y usuario de la sesión
            int? userId = HttpContext.Session.GetInt32("UserId");
            var model = new MantenimientoModel
            {
                Fecha = DateTime.Today,
                Estado = true,
                IdUsuario = userId ?? 1,
                IdResponsable = 1
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AgregarMantenimiento(MantenimientoModel model)
        {
            if (!ModelState.IsValid)
            {
                // Recargar los activos en caso de error
                var client = _httpClient.CreateClient();
                var url = _configuration["Variables:urlApi"] + "Activos/ListaActivos";
                var resp = await client.GetAsync(url);
                List<ActivosModel> activos = resp.IsSuccessStatusCode
                    ? await resp.Content.ReadFromJsonAsync<List<ActivosModel>>()
                    : new List<ActivosModel>();
                ViewBag.ListaActivos = new SelectList(activos, "idActivo", "nombreActivo");
                return View(model);
            }

            using (var api = _httpClient.CreateClient())
            {
                var urlApi = _configuration["Variables:urlApi"] + "Mantenimiento/AgregarMantenimiento";
                var response = await api.PostAsJsonAsync(urlApi, model);

                if (response.IsSuccessStatusCode)
                {
                    return RedirectToAction("ListaMantenimiento");
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Error al guardar el mantenimiento.");
                    // Recargar los activos nuevamente
                    var client = _httpClient.CreateClient();
                    var url = _configuration["Variables:urlApi"] + "Activos/ListaActivos";
                    var resp = await client.GetAsync(url);
                    List<ActivosModel> activos = resp.IsSuccessStatusCode
                        ? await resp.Content.ReadFromJsonAsync<List<ActivosModel>>()
                        : new List<ActivosModel>();
                    ViewBag.ListaActivos = new SelectList(activos, "idActivo", "nombreActivo");
                    return View(model);
                }
            }
        }

        [HttpGet]
        public IActionResult DetallesMantenimiento(int idMantenimiento)
        {
            var Mantenimiento = _utilitarios.ObtenerInfoMantenimiento(idMantenimiento);
            return Mantenimiento != null
                ? View(Mantenimiento)
                : NotFound("Mantenimiento no encontrado");
        }


        [HttpGet]
        public async Task<IActionResult> EditarMantenimiento(int idMantenimiento)
        {
            // 1) Obtener detalle del mantenimiento
            var model = _utilitarios.ObtenerInfoMantenimiento(idMantenimiento);
            if (model == null)
                return NotFound("No se encontró el mantenimiento.");

            // 1) Obtengo la lista de soportes
            var client = _httpClient.CreateClient();
            var resp = await client.GetAsync(_configuration["Variables:urlApi"] + "Ticket/ListaSoportes");
            var soportes = resp.IsSuccessStatusCode
                ? await resp.Content.ReadFromJsonAsync<List<UsuarioModel>>()
                : new List<UsuarioModel>();

            // 2) La pongo en ViewBag usando exactamente "idUsuario" y "nombreCompleto"
            ViewBag.Responsables = new SelectList(
                soportes,
                "idUsuario",       // Valor de cada option
                "nombreCompleto",  // Texto de cada option
                model.IdResponsable
            );

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditarMantenimiento(MantenimientoModel model)
        {
    
            var soportes = await _httpClient.CreateClient()
                .GetFromJsonAsync<List<UsuarioModel>>(_configuration["Variables:urlApi"] + "Ticket/ListaSoportes");
            ViewBag.Responsables = new SelectList(soportes, "idUsuario", "nombreCompleto", model.IdResponsable);

            if (!ModelState.IsValid)
                return View(model);

            var api = _httpClient.CreateClient();
            var url = _configuration["Variables:urlApi"] + "Mantenimiento/EditarMantenimiento";
            var result = await api.PutAsJsonAsync(url, model);

            // 4) Si falla, devolver la misma vista con ViewBag
            if (!result.IsSuccessStatusCode)
                return View(model);

            // 5) Redirigir al listado
            return RedirectToAction("ListaMantenimiento");
        }

        [HttpPost]
        public IActionResult EliminarActivo(MantenimientoModel model)
        {
            using (var api = _httpClient.CreateClient())
            {
                var url = _configuration.GetSection("Variables:urlApi").Value + "Mantenimiento/EliminarMantenimiento";
                var result = api.PutAsJsonAsync(url, model).Result;
                if (result.IsSuccessStatusCode)
                {
                    return RedirectToAction("ListaMantenimiento", "Mantenimiento");
                }
                else
                {
                    return RedirectToAction("ListaMantenimiento", "Mantenimiento");
                }
            }
        }


        [HttpGet]
        public async Task<IActionResult> ListaMantenimientoIndividual(int idMantenimiento)
        {
            using (var client = _httpClient.CreateClient())
            {
                var url = _configuration.GetSection("Variables:urlApi").Value + $"Mantenimiento/ListaMantenimientoIndividual?idMantenimiento={idMantenimiento}";
                var response = await client.GetAsync(url);

                if (response.IsSuccessStatusCode)
                {
                    var resultado = await response.Content.ReadFromJsonAsync<List<MantenimientoModel>>();
                    return Json(resultado);
                }
                else
                {
                    return Json(new List<MantenimientoModel>());
                }
            }
        }

        /*  
        [HttpGet]
        public async Task<IActionResult> ListaMantenimiento(string estado = "Todos", string urgencia = "Todos")
        {
            using (var api = _httpClient.CreateClient())
            {

                var url = _configuration.GetSection("Variables:urlApi").Value + "Mantenimiento/ListaMantenimientoFiltro"
                          + $"?estado={estado}&urgencia={urgencia}";
                var result = await api.GetAsync(url);

                if (result.IsSuccessStatusCode)
                {
                    var Mantenimientos = await result.Content.ReadFromJsonAsync<List<MantenimientoModel>>();
                    return View(Mantenimientos);
                }
            }
            return View(new List<MantenimientoModel>());
        }*/



    }
}
