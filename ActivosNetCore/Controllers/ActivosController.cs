using Microsoft.AspNetCore.Mvc;
using ActivosNetCore.Dependencias;
using Microsoft.Extensions.Configuration;
using System.Net.Http;
using System;
using System.Reflection;
using System.Text.Json;
using ActivosNetCore.Models;
using System.Net.Http.Headers;

namespace ActivosNetCore.Controllers
{
    [FiltroSeguridadSesion]
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
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

        [HttpGet]
        public IActionResult ListaActivos()
        {
            ViewBag.Token = HttpContext.Session.GetString("Token");//cargar token en cshtml
            return View();
        }



        [HttpGet]
        public IActionResult AgregarActivo()
        {
            return View();
        }

        [HttpPost]
        public IActionResult AgregarActivo(ActivosModel model)
        {
            var idUsuarioSesion = HttpContext.Session.GetInt32("UserId");

            if (idUsuarioSesion == null)
            {
                TempData["MensajeError"] = "Sesión expirada, por favor vuelva a iniciar sesión.";
                return RedirectToAction("IniciarSesion", "Login");
            }

            using (var api = _httpClient.CreateClient())
            {
                api.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", HttpContext.Session.GetString("Token"));

                var url = _configuration.GetSection("Variables:urlApi").Value + "Activos/AgregarActivo";

                var datos = new
                {
                    
                    model.nombreActivo,
                    model.placa,
                    model.serie,
                    model.descripcion,
                    model.idDepartamento,
                    model.idUsuario,
                    idUsuarioSesion = idUsuarioSesion.Value
                };

                var result = api.PostAsJsonAsync(url, datos).Result;

                if (result.IsSuccessStatusCode)
                {
                    TempData["MensajeOk"] = "Activo guardado correctamente";
                    return RedirectToAction("ListaActivos");
                }
                else
                {
                    var contenido = result.Content.ReadFromJsonAsync<RespuestaModel>().Result;
                    TempData["MensajeError"] = contenido?.Mensaje ?? "Error al guardar el activo.";
                    return RedirectToAction("AgregarActivo");
                }
            }
        }


        [HttpGet]
        public IActionResult DetallesActivo(int idActivo)
        {
            var response = _utilitarios.ObtenerInfoActivo(idActivo) ?? new ActivosModel();

            if (response == null)
            {
                TempData["MensajeError"] = "No se encontro el activo";
                return RedirectToAction("ListaActivos", "Activos");
            }

            return View(response);
        }

        [HttpGet]
        public IActionResult EditarActivo(int idActivo)
        {
            var response = _utilitarios.ObtenerInfoActivo(idActivo) ?? new ActivosModel();

            if (response == null)
            {
                TempData["MensajeError"] = "No se encontro el activo";
                return RedirectToAction("ListaActivos", "Activos");
            }

            return View(response);
        }

        //Editar activo
        [HttpPost]
        public IActionResult EditarActivo(ActivosModel model)
        {
            var idUsuarioSesion = HttpContext.Session.GetInt32("UserId");

            if (idUsuarioSesion == null)
            {
                TempData["MensajeError"] = "Sesión expirada, por favor vuelve a iniciar sesión.";
                return RedirectToAction("IniciarSesion", "Login");
            }

            using (var api = _httpClient.CreateClient())
            {
                api.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", HttpContext.Session.GetString("Token"));

                var url = _configuration.GetSection("Variables:urlApi").Value + "Activos/EditarActivo";

                var datos = new
                {
                    model.idActivo,
                    model.nombreActivo,
                    model.placa,
                    model.serie,
                    model.descripcion,
                    model.idDepartamento,
                    model.idUsuario,
                    idUsuarioSesion = idUsuarioSesion.Value
                };

                var result = api.PutAsJsonAsync(url, datos).Result;

                if (result.IsSuccessStatusCode)
                {
                    TempData["MensajeOk"] = "Activo editado correctamente.";
                    return RedirectToAction("ListaActivos");
                }
                else
                {
                    var contenido = result.Content.ReadFromJsonAsync<RespuestaModel>().Result;
                    TempData["MensajeError"] = contenido?.Mensaje ?? "Error al editar el activo.";
                    return RedirectToAction("ListaActivos");
                }
            }
        }



        [HttpPost]
        [HttpPost]
        public IActionResult EliminarActivo(ActivosModel model)
        {
            var idUsuarioSesion = HttpContext.Session.GetInt32("UserId");

            if (idUsuarioSesion == null)
            {
                TempData["MensajeError"] = "Sesión expirada, por favor vuelva a iniciar sesión.";
                return RedirectToAction("IniciarSesion", "Login");
            }

            using (var api = _httpClient.CreateClient())
            {
                api.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", HttpContext.Session.GetString("Token"));

                var url = _configuration.GetSection("Variables:urlApi").Value + "Activos/EliminarActivo";

                var datos = new
                {
                    model.idActivo,
                    idUsuarioSesion = idUsuarioSesion.Value
                };

                var response = api.PutAsJsonAsync(url, datos).Result;

                if (response.IsSuccessStatusCode)
                {
                    TempData["MensajeOk"] = "Activo eliminado correctamente.";
                    return RedirectToAction("ListaActivos", "Activos");
                }
                else
                {
                    var contenido = response.Content.ReadFromJsonAsync<RespuestaModel>().Result;
                    TempData["MensajeError"] = contenido?.Mensaje ?? "Error al eliminar el activo.";
                    return RedirectToAction("ListaActivos", "Activos");
                }
            }
        }



    }
}
