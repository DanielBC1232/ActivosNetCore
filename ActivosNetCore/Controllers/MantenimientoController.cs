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
using System.Net.Http.Headers;

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
        public async Task<IActionResult> HistorialMantenimiento(MantenimientoModel? model)
        {
            using (var api = _httpClient.CreateClient())
            {
                var url = _configuration.GetSection("Variables:urlApi").Value + "Mantenimiento/HistorialMantenimiento";
                var result = await api.GetAsync(url);

                if (result.IsSuccessStatusCode)
                {
                    var Mantenimientos = await result.Content.ReadFromJsonAsync<List<MantenimientoModel>>();
                    return View(Mantenimientos);
                } else
                {
                    TempData["MensajeError"] = "No se pudo cargar el Historial de Mantenimientos";
                }
            }
            var Mantenimiento = new List<MantenimientoModel>();
            return View(Mantenimiento);
        }

        [HttpGet]
        public async Task<IActionResult> AgregarMantenimiento()
        {
            //1)Recuperar token y userId
            var token = HttpContext.Session.GetString("Token");
            int userId = HttpContext.Session.GetInt32("UserId") ?? 1;

            //2)Crear cliente
            var client = _httpClient.CreateClient();
            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);

            //3Construir URL sin duplicar "api"
            string baseApi = _configuration["Variables:urlApi"].TrimEnd('/');
            string url = $"{baseApi}/Activos/ListaActivosDrop?userId={userId}";

            var resp = await client.GetAsync(url);

            //4) Procesar respuesta
            List<ActivosModel> activos = new();
            if (resp.IsSuccessStatusCode)
            {
                activos = await resp.Content.ReadFromJsonAsync<List<ActivosModel>>()
                          ?? new List<ActivosModel>();
            }
            else
            {
                TempData["MensajeError"] = "No se pudo cargar sus activos";
            }

            ViewBag.ListaActivos = new SelectList(activos, "idActivo", "nombreActivo");

            var model = new MantenimientoModel
            {
                Fecha = DateTime.Today,
                Estado = true,
                IdUsuario = userId,
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
                var url = _configuration["Variables:urlApi"] + "Activos/ListaActivosDrop";
                var resp = await client.GetAsync(url);
                List<ActivosModel> activos = resp.IsSuccessStatusCode
                    ? await resp.Content.ReadFromJsonAsync<List<ActivosModel>>()
                    : new List<ActivosModel>();
                ViewBag.ListaActivos = new SelectList(activos, "idActivo", "nombreActivo");
                TempData["MensajeError"] = "No se han insertado los datos correctamente.";
                return View(model);
            }

            using (var api = _httpClient.CreateClient())
            {
                var urlApi = _configuration["Variables:urlApi"] + "Mantenimiento/AgregarMantenimiento";
                var response = await api.PostAsJsonAsync(urlApi, model);

                if (response.IsSuccessStatusCode)
                {
                    TempData["MensajeOk"] = "Mantenimiento guardado correctamente";
                    return RedirectToAction("ListaMantenimiento");
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Error al guardar el mantenimiento.");
                    // Recargar los activos nuevamente
                    var client = _httpClient.CreateClient();
                    var url = _configuration["Variables:urlApi"] + "Activos/ListaActivosDrop";
                    var resp = await client.GetAsync(url);
                    List<ActivosModel> activos = resp.IsSuccessStatusCode
                        ? await resp.Content.ReadFromJsonAsync<List<ActivosModel>>()
                        : new List<ActivosModel>();
                    ViewBag.ListaActivos = new SelectList(activos, "idActivo", "nombreActivo");
                    TempData["MensajeError"] = "Error al guardar el mantenimiento.";
                    return View(model);
                }
            }
        }

        [HttpGet]
        public IActionResult DetallesMantenimiento(int idMantenimiento)
        {
            // 1) Leer mensajes previos
            ViewBag.MensajeOk = TempData["MensajeOk"] as string;
            ViewBag.MensajeError = TempData["MensajeError"] as string;

            // 2) Obtener datos
            var mantenimiento = _utilitarios.ObtenerInfoMantenimiento(idMantenimiento);
            if (mantenimiento == null)
                return NotFound("Mantenimiento no encontrado");

            return View(mantenimiento);
        }

        [HttpGet]
        public IActionResult DetallesMantenimientoHistorial(int idMantenimiento)
        {
            var Mantenimiento = _utilitarios.ObtenerInfoMantenimiento(idMantenimiento);
            return Mantenimiento != null
                ? View(Mantenimiento)
                : NotFound("Mantenimiento no encontrado");
        }

        [HttpGet]
        public async Task<IActionResult> EditarMantenimiento(int idMantenimiento)
        {
            // 1) Leer mensajes previos de TempData
            ViewBag.MensajeOk = TempData["MensajeOk"] as string;
            ViewBag.MensajeError = TempData["MensajeError"] as string;

            // 2) Obtener detalle del mantenimiento
            var model = _utilitarios.ObtenerInfoMantenimiento(idMantenimiento);
            if (model == null)
                return NotFound("No se encontró el mantenimiento.");

            // 3) Cargar dropdown de responsables
            var soportes = await _httpClient.CreateClient()
                .GetFromJsonAsync<List<UsuarioModel>>(
                    _configuration["Variables:urlApi"] + "Ticket/ListaSoportes")
                ?? new List<UsuarioModel>();

            ViewBag.Responsables = new SelectList(
                soportes,
                "idUsuario",
                "nombreCompleto",
                model.IdResponsable
            );

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditarMantenimiento(MantenimientoModel model)
        {
            // 1) Volver a cargar responsables para el dropdown
            var soportes = await _httpClient.CreateClient()
                .GetFromJsonAsync<List<UsuarioModel>>(
                    _configuration["Variables:urlApi"] + "Ticket/ListaSoportes")
                ?? new List<UsuarioModel>();

            ViewBag.Responsables = new SelectList(
                soportes,
                "idUsuario",
                "nombreCompleto",
                model.IdResponsable
            );

            // 2) Validación de modelo
            if (!ModelState.IsValid)
                return View(model);

            // 3) Llamada a la API para guardar cambios
            var api = _httpClient.CreateClient();
            var url = _configuration["Variables:urlApi"] + "Mantenimiento/EditarMantenimiento";
            var resp = await api.PutAsJsonAsync(url, model);

            if (resp.IsSuccessStatusCode)
            {
                // Mensaje de éxito sin iconos
                TempData["MensajeOk"] = "El mantenimiento se editó correctamente";
                return RedirectToAction("ListaMantenimiento");
            }
            else
            {
                // Mensaje de error sin iconos
                TempData["MensajeError"] = "No se pudo editar el mantenimiento";
                return View(model);
            }
        }

        [HttpPost]
        public IActionResult EliminarMantenimiento(MantenimientoModel model)
        {
            try
            {
                using (var api = _httpClient.CreateClient())
                {
                    // Solo necesitamos el ID para eliminar
                    var mantenimientoParaEliminar = new MantenimientoModel
                    {
                        IdMantenimiento = model.IdMantenimiento
                    };

                    var url = _configuration.GetSection("Variables:urlApi").Value + "Mantenimiento/EliminarMantenimiento";
                    var result = api.DeleteAsync(url + "/" + model.IdMantenimiento).Result;

                    if (result.IsSuccessStatusCode)
                    {
                        TempData["Mensaje"] = "Mantenimiento eliminado correctamente";
                        return RedirectToAction("ListaMantenimiento");
                    }
                    else
                    {
                        TempData["Error"] = "No se pudo eliminar el mantenimiento";
                        return RedirectToAction("DetalleMantenimiento", new { idMantenimiento = model.IdMantenimiento });
                    }
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error al eliminar: " + ex.Message;
                return RedirectToAction("DetalleMantenimiento", new { idMantenimiento = model.IdMantenimiento });
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
