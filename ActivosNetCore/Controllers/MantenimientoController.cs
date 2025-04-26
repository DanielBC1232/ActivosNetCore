using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using ActivosNetCore.Dependencias;
using Microsoft.Extensions.Configuration;
using System.Net.Http;
using System;
using ActivosNetCore.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Net.Http.Headers;

namespace ActivosNetCore.Controllers
{
    // Controlador para gestionar mantenimientos: listar, historial, crear, editar, eliminar
    public class MantenimientoController : Controller
    {
        private readonly IHttpClientFactory _httpClient;
        private readonly IConfiguration _configuration;
        private readonly IUtilitarios _utilitarios;

        // Constructor: inyectar dependencias
        public MantenimientoController(IHttpClientFactory httpClient, IConfiguration configuration, IUtilitarios utilitarios)
        {
            _httpClient = httpClient;
            _configuration = configuration;
            _utilitarios = utilitarios;
        }

        // GET ListaMantenimiento: muestra lista actual de mantenimientos
        [HttpGet]
        public async Task<IActionResult> ListaMantenimiento()
        {
            try
            {
                using var api = _httpClient.CreateClient();
                var url = _configuration["Variables:urlApi"] + "Mantenimiento/ListaMantenimiento";
                var result = await api.GetAsync(url);

                if (result.IsSuccessStatusCode)
                {
                    var mantenimientos = await result.Content.ReadFromJsonAsync<List<MantenimientoModel>>();
                    return View(mantenimientos);
                }

                // API devolvió error
                TempData["MensajeError"] = "No se pudo cargar la lista de mantenimientos.";
                return View(new List<MantenimientoModel>());
            }
            catch (Exception ex)
            {
                // Error inesperado
                TempData["MensajeError"] = "Error al cargar lista de mantenimientos: " + ex.Message;
                return View(new List<MantenimientoModel>());
            }
        }

        // GET HistorialMantenimiento: muestra historial completo
        [HttpGet]
        public async Task<IActionResult> HistorialMantenimiento()
        {
            try
            {
                using var api = _httpClient.CreateClient();
                var url = _configuration["Variables:urlApi"] + "Mantenimiento/HistorialMantenimiento";
                var result = await api.GetAsync(url);

                if (result.IsSuccessStatusCode)
                {
                    var historial = await result.Content.ReadFromJsonAsync<List<MantenimientoModel>>();
                    return View(historial);
                }

                TempData["MensajeError"] = "No se pudo cargar el historial de mantenimientos.";
                return View(new List<MantenimientoModel>());
            }
            catch (Exception ex)
            {
                TempData["MensajeError"] = "Error al cargar historial: " + ex.Message;
                return View(new List<MantenimientoModel>());
            }
        }

        // GET AgregarMantenimiento: muestra formulario de creación
        [HttpGet]
        public async Task<IActionResult> AgregarMantenimiento()
        {
            try
            {
                // Obtener token y usuario en sesión
                var token = HttpContext.Session.GetString("Token");
                int userId = HttpContext.Session.GetInt32("UserId") ?? 1;

                // Crear HttpClient con autorización
                var client = _httpClient.CreateClient();
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                // Obtener lista de activos para dropdown
                string baseApi = _configuration["Variables:urlApi"].TrimEnd('/');
                string url = $"{baseApi}/Activos/ListaActivosDrop?userId={userId}";
                var resp = await client.GetAsync(url);

                List<ActivosModel> activos = new();
                if (resp.IsSuccessStatusCode)
                {
                    activos = await resp.Content.ReadFromJsonAsync<List<ActivosModel>>() ?? new List<ActivosModel>();
                }
                else
                {
                    TempData["MensajeError"] = "No se pudo cargar sus activos.";
                }

                ViewBag.ListaActivos = new SelectList(activos, "idActivo", "nombreActivo");

                // Inicializar modelo por defecto
                var model = new MantenimientoModel
                {
                    Fecha = DateTime.Today,
                    Estado = true,
                    IdUsuario = userId,
                    IdResponsable = 1
                };
                return View(model);
            }
            catch (Exception ex)
            {
                TempData["MensajeError"] = "Error al preparar formulario: " + ex.Message;
                return RedirectToAction("ListaMantenimiento");
            }
        }

        // POST AgregarMantenimiento: procesa creación de mantenimiento
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AgregarMantenimiento(MantenimientoModel model)
        {
            try
            {
                // Validar modelo
                if (!ModelState.IsValid)
                {
                    // Recargar activos para formulario en caso de error
                    var client = _httpClient.CreateClient();
                    var url = _configuration["Variables:urlApi"] + "Activos/ListaActivosDrop";
                    var resp = await client.GetAsync(url);
                    var activos = resp.IsSuccessStatusCode
                        ? await resp.Content.ReadFromJsonAsync<List<ActivosModel>>()
                        : new List<ActivosModel>();
                    ViewBag.ListaActivos = new SelectList(activos, "idActivo", "nombreActivo");

                    TempData["MensajeError"] = "No se han insertado los datos correctamente.";
                    return View(model);
                }

                using var api = _httpClient.CreateClient();
                var urlApi = _configuration["Variables:urlApi"] + "Mantenimiento/AgregarMantenimiento";
                var response = await api.PostAsJsonAsync(urlApi, model);

                if (response.IsSuccessStatusCode)
                {
                    TempData["MensajeOk"] = "Mantenimiento solicitado correctamente.";
                    return RedirectToAction("ListaMantenimiento");
                }

                // Error al llamar API
                TempData["MensajeError"] = "Error al guardar el mantenimiento.";
                return View(model);
            }
            catch (Exception ex)
            {
                // Excepción inesperada
                TempData["MensajeError"] = "Error al crear mantenimiento: " + ex.Message;
                return View(model);
            }
        }

        // GET DetallesMantenimiento: muestra detalle de un mantenimiento
        [HttpGet]
        public IActionResult DetallesMantenimiento(int idMantenimiento)
        {
            try
            {
                var mantenimiento = _utilitarios.ObtenerInfoMantenimiento(idMantenimiento);
                if (mantenimiento == null)
                {
                    TempData["MensajeError"] = "Mantenimiento no encontrado.";
                    return RedirectToAction("ListaMantenimiento");
                }
                return View(mantenimiento);
            }
            catch (Exception ex)
            {
                TempData["MensajeError"] = "Error al obtener detalles: " + ex.Message;
                return RedirectToAction("ListaMantenimiento");
            }
        }

        // GET DetallesMantenimientoHistorial: muestra mantenimiento del historial
        [HttpGet]
        public IActionResult DetallesMantenimientoHistorial(int idMantenimiento)
        {
            try
            {
                var mantenimiento = _utilitarios.ObtenerInfoMantenimiento(idMantenimiento);
                if (mantenimiento == null)
                {
                    TempData["MensajeError"] = "Mantenimiento no encontrado en historial.";
                    return RedirectToAction("HistorialMantenimiento");
                }
                return View(mantenimiento);
            }
            catch (Exception ex)
            {
                TempData["MensajeError"] = "Error al obtener historial: " + ex.Message;
                return RedirectToAction("HistorialMantenimiento");
            }
        }

        // GET EditarMantenimiento: muestra formulario de edición
        [HttpGet]
        public async Task<IActionResult> EditarMantenimiento(int idMantenimiento)
        {
            try
            {
                var model = _utilitarios.ObtenerInfoMantenimiento(idMantenimiento);
                if (model == null)
                {
                    TempData["MensajeError"] = "Problemas al obtener detalles del mantenimiento.";
                    return RedirectToAction("ListaMantenimiento");
                }

                // Cargar responsables para dropdown
                var soportes = await _httpClient.CreateClient()
                    .GetFromJsonAsync<List<UsuarioModel>>(
                        _configuration["Variables:urlApi"] + "Ticket/ListaSoportes"
                    ) ?? new List<UsuarioModel>();
                ViewBag.Responsables = new SelectList(soportes, "idUsuario", "nombreCompleto", model.IdResponsable);

                return View(model);
            }
            catch (Exception ex)
            {
                TempData["MensajeError"] = "Error al preparar edición: " + ex.Message;
                return RedirectToAction("ListaMantenimiento");
            }
        }

        // POST EditarMantenimiento: procesa edición de mantenimiento
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditarMantenimiento(MantenimientoModel model)
        {
            try
            {
                // Recargar responsables
                var soportes = await _httpClient.CreateClient()
                    .GetFromJsonAsync<List<UsuarioModel>>(
                        _configuration["Variables:urlApi"] + "Ticket/ListaSoportes"
                    ) ?? new List<UsuarioModel>();
                ViewBag.Responsables = new SelectList(soportes, "idUsuario", "nombreCompleto", model.IdResponsable);

                // Validar modelo
                if (!ModelState.IsValid)
                {
                    TempData["MensajeError"] = "Revisa los datos e intenta de nuevo.";
                    return View(model);
                }

                var api = _httpClient.CreateClient();
                var url = _configuration["Variables:urlApi"] + "Mantenimiento/EditarMantenimiento";
                var resp = await api.PutAsJsonAsync(url, model);

                if (resp.IsSuccessStatusCode)
                {
                    TempData["MensajeOk"] = "Mantenimiento editado correctamente.";
                    return RedirectToAction("ListaMantenimiento");
                }

                TempData["MensajeError"] = "No se pudo editar el mantenimiento.";
                return View(model);
            }
            catch (Exception ex)
            {
                TempData["MensajeError"] = "Error al editar mantenimiento: " + ex.Message;
                return View(model);
            }
        }

        // POST EliminarMantenimiento: elimina un mantenimiento
        [HttpPost]
        public IActionResult EliminarMantenimiento(MantenimientoModel model)
        {
            try
            {
                using var api = _httpClient.CreateClient();
                var url = _configuration["Variables:urlApi"] + "Mantenimiento/EliminarMantenimiento/" + model.IdMantenimiento;
                var result = api.DeleteAsync(url).Result;

                if (result.IsSuccessStatusCode)
                {
                    TempData["MensajeOk"] = "Mantenimiento eliminado correctamente.";
                    return RedirectToAction("ListaMantenimiento");
                }

                TempData["MensajeError"] = "No se pudo eliminar el mantenimiento.";
                return RedirectToAction("DetallesMantenimiento", new { idMantenimiento = model.IdMantenimiento });
            }
            catch (Exception ex)
            {
                TempData["MensajeError"] = "Error al eliminar mantenimiento: " + ex.Message;
                return RedirectToAction("DetallesMantenimiento", new { idMantenimiento = model.IdMantenimiento });
            }
        }

        // GET ListaMantenimientoIndividual: devuelve JSON para AJAX
        [HttpGet]
        public async Task<IActionResult> ListaMantenimientoIndividual(int idMantenimiento)
        {
            try
            {
                using var client = _httpClient.CreateClient();
                var url = _configuration["Variables:urlApi"] + $"Mantenimiento/ListaMantenimientoIndividual?idMantenimiento={idMantenimiento}";
                var response = await client.GetAsync(url);

                if (response.IsSuccessStatusCode)
                {
                    var resultado = await response.Content.ReadFromJsonAsync<List<MantenimientoModel>>();
                    return Json(resultado);
                }

                TempData["MensajeError"] = "No se pudo cargar datos individuales.";
                return Json(new List<MantenimientoModel>());
            }
            catch (Exception ex)
            {
                TempData["MensajeError"] = "Error al obtener mantenimiento: " + ex.Message;
                return Json(new List<MantenimientoModel>());
            }
        }
    }
}
