using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Net.Http.Headers;
using ActivosNetCore.Models;
using Microsoft.Extensions.Configuration;

namespace ActivosNetCore.Controllers
{
    // Controlador para gestionar auditorías: listado y filtrado
    public class AuditoriaController : Controller
    {
        private readonly IHttpClientFactory _httpClient;
        private readonly IConfiguration _configuration;

        // Constructor: inyecta dependencias necesarias
        public AuditoriaController(IHttpClientFactory httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _configuration = configuration;
        }

        // GET ListaAuditoria: muestra la vista principal de auditorías
        [HttpGet]
        public IActionResult ListaAuditoria()
        {
            try
            {
                // Renderizar la vista de auditorías
                return View();
            }
            catch (Exception ex)
            {
                // Manejar errores inesperados
                TempData["MensajeError"] = "Error al cargar auditorías: " + ex.Message;
                return View();
            }
        }

        // POST ObtenerAuditoria: obtiene auditorías filtradas según criterios
        [HttpPost]
        public IActionResult ObtenerAuditoria(FiltroAuditoriaModel filtros)
        {
            try
            {
                // Crear cliente HTTP para llamada a la API
                using var api = _httpClient.CreateClient();
                // Añadir token de sesión en el encabezado Authorization
                api.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", HttpContext.Session.GetString("Token"));

                // Construir la URL de la API para obtener auditorías
                var url = _configuration["Variables:urlApi"] + "Auditoria/ListaAuditoria";
                // Enviar petición POST con los filtros
                var response = api.PostAsJsonAsync(url, filtros).Result;

                if (response.IsSuccessStatusCode)
                {
                    // Leer la lista de auditorías y devolver como JSON
                    var auditorias = response.Content.ReadFromJsonAsync<IEnumerable<AuditoriaModel>>().Result;
                    return Json(auditorias);
                }

                // Si la respuesta no es exitosa, devolver lista vacía
                return Json(new List<AuditoriaModel>());
            }
            catch (Exception ex)
            {
                // Capturar cualquier excepción y devolver lista vacía
                TempData["MensajeError"] = "Error al obtener auditorías: " + ex.Message;
                return Json(new List<AuditoriaModel>());
            }
        }
    }
}
