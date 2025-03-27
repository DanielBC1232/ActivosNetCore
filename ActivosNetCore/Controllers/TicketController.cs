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

        [HttpGet]
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
        }

        [HttpGet]
        public IActionResult AgregarTicket()
        {
            return View();
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
            var response = _utilitarios.ObtenerInfoTicket(idTicket) ?? new TicketModel();

            if (response == null)
            {
                return NotFound("No se encontró el ticket.");
            }

            return View(response);
        }

        [HttpGet]
        public IActionResult EditarTicket(int idTicket)
        {
            var response = _utilitarios.ObtenerInfoTicket(idTicket) ?? new TicketModel();

            if (response == null)
            {
                return NotFound("No se encontró el ticket.");
            }

            return View(response);
        }

        [HttpPost]
        public IActionResult EditarTicket(TicketModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            using (var api = _httpClient.CreateClient())
            {
                var url = _configuration.GetSection("Variables:urlApi").Value + "Ticket/EditarTicket";
                var result = api.PutAsJsonAsync(url, model).Result;
                if (result.IsSuccessStatusCode)
                {
                    return RedirectToAction("ListaTicket", "Ticket");
                }

                return View();
            }
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


    }
}
