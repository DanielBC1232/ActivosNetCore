using Microsoft.AspNetCore.Mvc;
using ActivosNetCore.Dependencias;
using Microsoft.Extensions.Configuration;
using System.Net.Http;
using System;
using System.Reflection;
using System.Text.Json;
using ActivosNetCore.Models;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace ActivosNetCore.Controllers
{
   
    public class TicketController : Controller
    {
        private readonly IHttpClientFactory _httpClient;
        private readonly IConfiguration _configuration;
        private readonly IUtilitarios _utilitarios;

        public TicketController(IHttpClientFactory httpClient, IConfiguration configuration, IUtilitarios utilitarios)
        {
            _httpClient = httpClient;
            _configuration = configuration;
            _utilitarios = utilitarios;

        }

        /*[HttpGet]
        public async Task<IActionResult>  ListaTicket(TicketModel? model)
        {
            using (var api = _httpClient.CreateClient())
            {
                var url = _configuration.GetSection("Variables:urlApi").Value + "Ticket/ListaTicket";
                var result = await api.GetAsync(url);

                if (result.IsSuccessStatusCode)
                {
                    var tickets = await result.Content.ReadFromJsonAsync<List<TicketModel>>();
                    return View(tickets);
                }
            }
            var ticket = new List<TicketModel>();
            return View(ticket);
        }*/

        [HttpGet]
        public IActionResult AgregarTicket()
        {
            // Obtener ID de usuario desde sesión
            var model = new TicketModel();
            int? userId = HttpContext.Session.GetInt32("UserId");
            model.IdUsuario = userId ?? 1;
            return View(model);
        }

        [HttpPost]
        public IActionResult AgregarTicket(TicketModel model)
        {
            if (!ModelState.IsValid)
            {
                TempData["MensajeError"] = "Por favor, completa todos los campos obligatorios.";
                return View();
            }
            using (var api = _httpClient.CreateClient())
            {
                var url = _configuration.GetSection("Variables:urlApi").Value + "Ticket/AgregarTicket";
                
                var result = api.PostAsJsonAsync(url, model).Result;

                if (result.IsSuccessStatusCode)
                {
                    TempData["MensajeOk"] = "Ticket creado correctamente.";
                    return RedirectToAction("AgregarTicket", "Ticket");
                }
                TempData["MensajeError"] = "No se pudo crear el ticket. Inténtalo de nuevo.";
                return View();
            }
        }

        [HttpGet]
        public IActionResult DetallesTicket(int idTicket)
        {
            var ticket = _utilitarios.ObtenerInfoTicket(idTicket);

            if (ticket == null)
            {
                TempData["MensajeError"] = "Ticket no encontrado";
                return RedirectToAction("ListaTicket");
            }

            return View(ticket);
        }



        [HttpGet]
        public async Task<IActionResult> EditarTicket(int idTicket)
        {
            var ticket = _utilitarios.ObtenerInfoTicket(idTicket);
            if (ticket?.IdTicket == null)

                if (ticket == null)
                {
                    TempData["MensajeError"] = "Ticket no encontrado";
                    return RedirectToAction("ListaTicket");
                }

            // 1) Obtengo la lista de soportes
            var client = _httpClient.CreateClient();
            var resp = await client.GetAsync(_configuration["Variables:urlApi"] + "Ticket/ListaSoportes");
            var soportes = resp.IsSuccessStatusCode
                ? await resp.Content.ReadFromJsonAsync<List<UsuarioModel>>()
                : new List<UsuarioModel>();


            ViewBag.Responsables = new SelectList(
                soportes,
                "idUsuario", 
                "nombreCompleto",  
                ticket.IdResponsable
            );

            return View(ticket);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditarTicket(TicketModel model)
        {
            var soportes = await _httpClient.CreateClient()
                .GetFromJsonAsync<List<UsuarioModel>>(_configuration["Variables:urlApi"] + "Ticket/ListaSoportes");
            ViewBag.Responsables = new SelectList(soportes, "idUsuario", "nombreCompleto", model.IdResponsable);
            
            if (!ModelState.IsValid)
            {
                TempData["MensajeError"] = "Revisa los datos e intenta de nuevo.";
                return View(model);
            }

            var api = _httpClient.CreateClient();
            var url = _configuration["Variables:urlApi"] + "Ticket/EditarTicket";
            var result = await api.PutAsJsonAsync(url, model);
            if (result.IsSuccessStatusCode)
            {
                TempData["MensajeOk"] = "Ticket actualizado correctamente.";
                return RedirectToAction("ListaTicket");
            }
            else
            {
                TempData["MensajeError"] = "No se pudo actualizar el ticket.";
                return View(model);
            }
        }

        [HttpPost]
        public IActionResult EliminarTicket(TicketModel model)
        {
            try
            {
                using (var api = _httpClient.CreateClient())
                {
                    // Solo necesitamos el ID para eliminar
                    var TicketParaEliminar = new TicketModel
                    {
                        IdTicket = model.IdTicket
                    };

                    var url = _configuration.GetSection("Variables:urlApi").Value + "Ticket/EliminarTicket";
                    var result = api.DeleteAsync(url + "/" + model.IdTicket).Result;

                    if (result.IsSuccessStatusCode)
                    {
                        TempData["MensajeOk"] = "Ticket eliminado correctamente";
                        return RedirectToAction("ListaTicket");
                    }
                    else
                    {
                        TempData["MensajeError"] = "No se pudo eliminar el Ticket";
                        return RedirectToAction("DetallesTicket", new { idTicket = model.IdTicket });
                    }
                }
            }
            catch (Exception ex)
            {
                TempData["MensajeError"] = "Error al eliminar: " + ex.Message;
                return RedirectToAction("DetallesTicket", new { idTicket = model.IdTicket });
            }
        }


        [HttpGet]
        public async Task<IActionResult> ListaTicketIndividual(int idTicket)
        {
            using (var client = _httpClient.CreateClient())
            {
                var url = _configuration.GetSection("Variables:urlApi").Value + $"Ticket/ListaTicketIndividual?idTicket={idTicket}";
                var response = await client.GetAsync(url);

                if (response.IsSuccessStatusCode)
                {
                    var resultado = await response.Content.ReadFromJsonAsync<List<TicketModel>>();
                    return Json(resultado);
                }
                else
                {
                    return Json(new List<TicketModel>());
                }
            }
        }


        [HttpGet]
        public async Task<IActionResult> ListaTicket(string estado = "Todos", string urgencia = "Todos")
        {
            using (var api = _httpClient.CreateClient())
            {

                var url = _configuration.GetSection("Variables:urlApi").Value + "Ticket/ListaTicketFiltro"
                          + $"?estado={estado}&urgencia={urgencia}";
                var result = await api.GetAsync(url);

                if (result.IsSuccessStatusCode)
                {
                    var tickets = await result.Content.ReadFromJsonAsync<List<TicketModel>>();
                    return View(tickets);
                }
            }
            TempData["MensajeError"] = "No se pudo cargar los tickets.";
            return View(new List<TicketModel>());
        }



    }
}
