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
                return View();
            }
            using (var api = _httpClient.CreateClient())
            {
                var url = _configuration.GetSection("Variables:urlApi").Value + "Ticket/AgregarTicket";
                Console.WriteLine(JsonSerializer.Serialize(model));
                var result = api.PostAsJsonAsync(url, model).Result;

                if (result.IsSuccessStatusCode)
                {
                    return RedirectToAction("ListaTicket", "Ticket");
                }

                return View();
            }
        }

        [HttpGet]
        public IActionResult DetallesTicket(int idTicket)
        {
            var ticket = _utilitarios.ObtenerInfoTicket(idTicket);
            return ticket != null
                ? View(ticket)
                : NotFound("Ticket no encontrado");

        }


        [HttpGet]
        public async Task<IActionResult> EditarTicket(int idTicket)
        {
            var ticket = _utilitarios.ObtenerInfoTicket(idTicket);
            if (ticket?.IdTicket == null)
                return NotFound();

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
                return View(model);

            // aquí model.IdResponsable ya viene del dropdown
            var api = _httpClient.CreateClient();
            var url = _configuration["Variables:urlApi"] + "Ticket/EditarTicket";
            var result = await api.PutAsJsonAsync(url, model);
            if (!result.IsSuccessStatusCode)
                return View(model);

            return RedirectToAction("ListaTicket");
        }


        [HttpPost]
        public IActionResult EliminarActivo(TicketModel model)
        {
            using (var api = _httpClient.CreateClient())
            {
                var url = _configuration.GetSection("Variables:urlApi").Value + "Ticket/EliminarTicket";
                var result = api.PutAsJsonAsync(url, model).Result;
                if (result.IsSuccessStatusCode)
                {
                    return RedirectToAction("ListaTicket", "Ticket");
                }
                else
                {
                    return RedirectToAction("ListaTicket", "Ticket");
                }
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
            return View(new List<TicketModel>());
        }



    }
}
