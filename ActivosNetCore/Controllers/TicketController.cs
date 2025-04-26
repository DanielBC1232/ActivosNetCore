using Microsoft.AspNetCore.Mvc;
using ActivosNetCore.Dependencias;
using Microsoft.Extensions.Configuration;
using System.Net.Http;
using System;
using ActivosNetCore.Models;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace ActivosNetCore.Controllers
{
    // Controlador para gestionar tickets: crear, listar, editar, eliminar
    public class TicketController : Controller
    {

        private readonly IHttpClientFactory _httpClient;
        private readonly IConfiguration _configuration;
        private readonly IUtilitarios _utilitarios;

        // Constructor: inyecta dependencias
        public TicketController(IHttpClientFactory httpClient, IConfiguration configuration, IUtilitarios utilitarios)
        {
            _httpClient = httpClient;
            _configuration = configuration;
            _utilitarios = utilitarios;
        }

        // GET AgregarTicket: muestra formulario de creación
        [HttpGet]
        public IActionResult AgregarTicket()
        {
            try
            {
                // Inicializar modelo con ID de usuario en sesión (default 1)
                var model = new TicketModel();
                int? userId = HttpContext.Session.GetInt32("UserId");
                model.IdUsuario = userId ?? 1;
                return View(model);
            }
            catch (Exception ex)
            {
                // Mostrar error y redirigir
                TempData["MensajeError"] = "Error al preparar formulario: " + ex.Message;
                return RedirectToAction("ListaTicket");
            }
        }

        // POST AgregarTicket: procesa creación de ticket
        [HttpPost]
        public IActionResult AgregarTicket(TicketModel model)
        {
            try
            {
                //Validar campos obligatorios segun el modelo
                if (!ModelState.IsValid)
                {
                    TempData["MensajeError"] = "Por favor, completa todos los campos obligatorios.";
                    return View(model);
                }

                using var api = _httpClient.CreateClient();
                var url = _configuration["Variables:urlApi"] + "Ticket/AgregarTicket";
                var result = api.PostAsJsonAsync(url, model).Result;

                if (result.IsSuccessStatusCode)
                {
                    TempData["MensajeOk"] = "Ticket creado correctamente.";
                    return RedirectToAction("AgregarTicket");
                }

                TempData["MensajeError"] = "No se pudo crear el ticket. Inténtalo de nuevo.";
                return View(model);
            }
            catch (Exception ex)
            {
                // Manejo de excepciones
                TempData["MensajeError"] = "Error al crear ticket: " + ex.Message;
                return View(model);
            }
        }

        // GET DetallesTicket: muestra detalles de un ticket
        [HttpGet]
        public IActionResult DetallesTicket(int idTicket)
        {
            try
            {
                var ticket = _utilitarios.ObtenerInfoTicket(idTicket);
                if (ticket == null)
                {
                    TempData["MensajeError"] = "Ticket no encontrado.";
                    return RedirectToAction("ListaTicket");
                }
                return View(ticket);
            }
            catch (Exception ex)
            {
                TempData["MensajeError"] = "Error al obtener detalles: " + ex.Message;
                return RedirectToAction("ListaTicket");
            }
        }

        // GET HistorialTicket: lista histórico de tickets
        [HttpGet]
        public async Task<IActionResult> HistorialTicket()
        {
            try
            {
                using var api = _httpClient.CreateClient();
                var url = _configuration["Variables:urlApi"] + "Ticket/HistorialTicket";
                var response = await api.GetAsync(url);

                if (response.IsSuccessStatusCode)
                {
                    var tickets = await response.Content.ReadFromJsonAsync<List<TicketModel>>();
                    return View(tickets);
                }

                TempData["MensajeError"] = "No se pudo cargar el historial de tickets.";
                return View(new List<TicketModel>());
            }
            catch (Exception ex)
            {
                TempData["MensajeError"] = "Error al cargar historial: " + ex.Message;
                return View(new List<TicketModel>());
            }
        }

        // GET DetallesTicketHistorial: muestra ticket del historial
        [HttpGet]
        public IActionResult DetallesTicketHistorial(int idTicket)
        {
            try
            {
                var ticket = _utilitarios.ObtenerInfoTicket(idTicket);
                if (ticket == null)
                {
                    TempData["MensajeError"] = "Ticket no encontrado en historial.";
                    return RedirectToAction("HistorialTicket");
                }
                return View(ticket);
            }
            catch (Exception ex)
            {
                TempData["MensajeError"] = "Error al obtener historial: " + ex.Message;
                return RedirectToAction("HistorialTicket");
            }
        }

        // GET EditarTicket: muestra formulario de edición
        [HttpGet]
        public async Task<IActionResult> EditarTicket(int idTicket)
        {
            try
            {
                var ticket = _utilitarios.ObtenerInfoTicket(idTicket);
                if (ticket == null)
                {
                    TempData["MensajeError"] = "Ticket no encontrado.";
                    return RedirectToAction("ListaTicket");
                }

                using var client = _httpClient.CreateClient();
                var resp = await client.GetAsync(_configuration["Variables:urlApi"] + "Ticket/ListaSoportes");
                var soportes = resp.IsSuccessStatusCode
                    ? await resp.Content.ReadFromJsonAsync<List<UsuarioModel>>()
                    : new List<UsuarioModel>();

                // Preparar lista de responsables para el dropdown
                ViewBag.Responsables = new SelectList(soportes, "idUsuario", "nombreCompleto", ticket.IdResponsable);
                return View(ticket);
            }
            catch (Exception ex)
            {
                TempData["MensajeError"] = "Error al preparar edición: " + ex.Message;
                return RedirectToAction("ListaTicket");
            }
        }

        // POST EditarTicket: procesa cambios en un ticket
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditarTicket(TicketModel model)
        {
            try
            {
                var soportes = await _httpClient.CreateClient()
                    .GetFromJsonAsync<List<UsuarioModel>>(_configuration["Variables:urlApi"] + "Ticket/ListaSoportes");
                ViewBag.Responsables = new SelectList(soportes, "idUsuario", "nombreCompleto", model.IdResponsable);

                if (!ModelState.IsValid)
                {
                    TempData["MensajeError"] = "Revisa los datos e intenta de nuevo.";
                    return View(model);
                }

                using var api = _httpClient.CreateClient();
                var url = _configuration["Variables:urlApi"] + "Ticket/EditarTicket";
                var result = await api.PutAsJsonAsync(url, model);

                if (result.IsSuccessStatusCode)
                {
                    TempData["MensajeOk"] = "Ticket actualizado correctamente.";
                    return RedirectToAction("ListaTicket");
                }

                TempData["MensajeError"] = "No se pudo actualizar el ticket.";
                return View(model);
            }
            catch (Exception ex)
            {
                TempData["MensajeError"] = "Error al actualizar ticket: " + ex.Message;
                return View(model);
            }
        }

        // POST EliminarTicket: elimina un ticket existente
        [HttpPost]
        public IActionResult EliminarTicket(TicketModel model)
        {
            try
            {
                using var api = _httpClient.CreateClient();
                var url = _configuration["Variables:urlApi"] + "Ticket/EliminarTicket/" + model.IdTicket;
                var result = api.DeleteAsync(url).Result;

                if (result.IsSuccessStatusCode)
                {
                    TempData["MensajeOk"] = "Ticket eliminado correctamente.";
                    return RedirectToAction("ListaTicket");
                }

                TempData["MensajeError"] = "No se pudo eliminar el ticket.";
                return RedirectToAction("DetallesTicket", new { idTicket = model.IdTicket });
            }
            catch (Exception ex)
            {
                TempData["MensajeError"] = "Error al eliminar ticket: " + ex.Message;
                return RedirectToAction("DetallesTicket", new { idTicket = model.IdTicket });
            }
        }

        // GET ListaTicketIndividual: devuelve JSON para AJAX
        [HttpGet]
        public async Task<IActionResult> ListaTicketIndividual(int idTicket)
        {
            try
            {
                using var client = _httpClient.CreateClient();
                var url = _configuration["Variables:urlApi"] + $"Ticket/ListaTicketIndividual?idTicket={idTicket}";
                var response = await client.GetAsync(url);

                if (response.IsSuccessStatusCode)
                {
                    var resultado = await response.Content.ReadFromJsonAsync<List<TicketModel>>();
                    return Json(resultado);
                }

                TempData["MensajeError"] = "No se pudo cargar datos individuales.";
                return Json(new List<TicketModel>());
            }
            catch (Exception ex)
            {
                TempData["MensajeError"] = "Error al obtener ticket: " + ex.Message;
                return Json(new List<TicketModel>());
            }
        }

        // GET ListaTicket: lista tickets con filtros
        [HttpGet]
        public async Task<IActionResult> ListaTicket(string estado = "Todos", string urgencia = "Todos")
        {
            try
            {
                using var api = _httpClient.CreateClient();
                var url = _configuration["Variables:urlApi"] + "Ticket/ListaTicketFiltro"
                          + $"?estado={estado}&urgencia={urgencia}";
                var response = await api.GetAsync(url);

                if (response.IsSuccessStatusCode)
                {
                    var tickets = await response.Content.ReadFromJsonAsync<List<TicketModel>>();
                    return View(tickets);
                }

                TempData["MensajeError"] = "No se pudo cargar los tickets.";
                return View(new List<TicketModel>());
            }
            catch (Exception ex)
            {
                TempData["MensajeError"] = "Error al cargar tickets: " + ex.Message;
                return View(new List<TicketModel>());
            }
        }
    }
}
