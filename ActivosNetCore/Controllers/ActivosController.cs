using Microsoft.AspNetCore.Mvc;
using ActivosNetCore.Dependencias;
using Microsoft.Extensions.Configuration;
using System.Net.Http;
using System;
using ActivosNetCore.Models;
using System.Net.Http.Headers;

namespace ActivosNetCore.Controllers
{
    [FiltroSeguridadSesion]
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    // Controlador para gestionar activos: listar, crear, detalles, editar, eliminar
    public class ActivosController : Controller
    {
        private readonly IHttpClientFactory _httpClient;
        private readonly IConfiguration _configuration;
        private readonly IUtilitarios _utilitarios;

        // Constructor: inyecta dependencias necesarias
        public ActivosController(IHttpClientFactory httpClient, IConfiguration configuration, IUtilitarios utilitarios)
        {
            _httpClient = httpClient;
            _configuration = configuration;
            _utilitarios = utilitarios;
        }

        // GET ListaActivos: muestra vista inicial y carga token
        [HttpGet]
        public IActionResult ListaActivos()
        {
            try
            {
                // Cargar token en ViewBag para usar en JavaScript
                ViewBag.Token = HttpContext.Session.GetString("Token");
                return View();
            }
            catch (Exception ex)
            {
                // Error inesperado
                TempData["MensajeError"] = "Error al cargar la lista de activos: " + ex.Message;
                return View();
            }
        }

        // GET AgregarActivo: muestra formulario para crear activo
        [HttpGet]
        public IActionResult AgregarActivo()
        {
            try
            {
                return View();
            }
            catch (Exception ex)
            {
                TempData["MensajeError"] = "Error al preparar formulario: " + ex.Message;
                return RedirectToAction("ListaActivos");
            }
        }

        // POST AgregarActivo: procesa creación de nuevo activo
        [HttpPost]
        public IActionResult AgregarActivo(ActivosModel model)
        {
            try
            {
                // Validar campos obligatorios
                if (!ModelState.IsValid
                    || string.IsNullOrWhiteSpace(model.nombreActivo)
                    || model.placa == 0
                    || string.IsNullOrWhiteSpace(model.serie)
                    || string.IsNullOrWhiteSpace(model.descripcion)
                    || model.idDepartamento == 0
                    || model.idUsuario == 0)
                {
                    TempData["MensajeError"] = "Por favor complete todos los campos obligatorios.";
                    return View(model);
                }

                // Verificar sesión de usuario
                var idUsuarioSesion = HttpContext.Session.GetInt32("UserId");
                if (idUsuarioSesion == null)
                {
                    TempData["MensajeError"] = "Sesión expirada, por favor vuelva a iniciar sesión.";
                    return RedirectToAction("IniciarSesion", "Login");
                }

                using var api = _httpClient.CreateClient();
                // Agregar encabezado Bearer
                api.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", HttpContext.Session.GetString("Token"));

                var url = _configuration["Variables:urlApi"] + "Activos/AgregarActivo";
                // Construir objeto con datos a enviar
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
                    TempData["MensajeOk"] = "Activo guardado correctamente.";
                    return RedirectToAction("ListaActivos");
                }

                // Leer mensaje de error de la API
                var contenido = result.Content.ReadFromJsonAsync<RespuestaModel>().Result;
                TempData["MensajeError"] = contenido?.Mensaje ?? "Error al guardar el activo.";
                return RedirectToAction("AgregarActivo");
            }
            catch (Exception ex)
            {
                // Excepción inesperada
                TempData["MensajeError"] = "Error al crear activo: " + ex.Message;
                return View(model);
            }
        }

        // GET DetallesActivo: muestra información de un activo por ID
        [HttpGet]
        public IActionResult DetallesActivo(int idActivo)
        {
            try
            {
                var response = _utilitarios.ObtenerInfoActivo(idActivo);
                if (response == null)
                {
                    TempData["MensajeError"] = "Activo no encontrado.";
                    return RedirectToAction("ListaActivos");
                }
                return View(response);
            }
            catch (Exception ex)
            {
                TempData["MensajeError"] = "Error al obtener detalles del activo: " + ex.Message;
                return RedirectToAction("ListaActivos");
            }
        }

        // GET EditarActivo: muestra formulario de edición de un activo
        [HttpGet]
        public IActionResult EditarActivo(int idActivo)
        {
            try
            {
                var response = _utilitarios.ObtenerInfoActivo(idActivo);
                if (response == null)
                {
                    TempData["MensajeError"] = "Activo no encontrado.";
                    return RedirectToAction("ListaActivos");
                }
                return View(response);
            }
            catch (Exception ex)
            {
                TempData["MensajeError"] = "Error al preparar edición: " + ex.Message;
                return RedirectToAction("ListaActivos");
            }
        }

        // POST EditarActivo: procesa actualización de activo existente
        [HttpPost]
        public IActionResult EditarActivo(ActivosModel model)
        {
            try
            {
                // Validar campos obligatorios
                if (!ModelState.IsValid
                    || string.IsNullOrWhiteSpace(model.nombreActivo)
                    || model.placa == 0
                    || string.IsNullOrWhiteSpace(model.serie)
                    || string.IsNullOrWhiteSpace(model.descripcion)
                    || model.idDepartamento == 0
                    || model.idUsuario == 0)
                {
                    TempData["MensajeError"] = "Por favor complete todos los campos obligatorios.";
                    return View(model);
                }

                // Verificar sesión de usuario
                var idUsuarioSesion = HttpContext.Session.GetInt32("UserId");
                if (idUsuarioSesion == null)
                {
                    TempData["MensajeError"] = "Sesión expirada, por favor vuelva a iniciar sesión.";
                    return RedirectToAction("IniciarSesion", "Login");
                }

                using var api = _httpClient.CreateClient();
                api.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", HttpContext.Session.GetString("Token"));

                var url = _configuration["Variables:urlApi"] + "Activos/EditarActivo";
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

                var contenido = result.Content.ReadFromJsonAsync<RespuestaModel>().Result;
                TempData["MensajeError"] = contenido?.Mensaje ?? "Error al editar el activo.";
                return RedirectToAction("ListaActivos");
            }
            catch (Exception ex)
            {
                TempData["MensajeError"] = "Error al editar activo: " + ex.Message;
                return View(model);
            }
        }

        // POST EliminarActivo: elimina un activo
        [HttpPost]
        public IActionResult EliminarActivo(ActivosModel model)
        {
            try
            {
                // Verificar sesión de usuario
                var idUsuarioSesion = HttpContext.Session.GetInt32("UserId");
                if (idUsuarioSesion == null)
                {
                    TempData["MensajeError"] = "Sesión expirada, por favor vuelva a iniciar sesión.";
                    return RedirectToAction("IniciarSesion", "Login");
                }

                using var api = _httpClient.CreateClient();
                api.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", HttpContext.Session.GetString("Token"));

                var url = _configuration["Variables:urlApi"] + "Activos/EliminarActivo";
                var datos = new
                {
                    model.idActivo,
                    idUsuarioSesion = idUsuarioSesion.Value
                };

                var response = api.PutAsJsonAsync(url, datos).Result;
                if (response.IsSuccessStatusCode)
                {
                    TempData["MensajeOk"] = "Activo eliminado correctamente.";
                    return RedirectToAction("ListaActivos");
                }

                var contenido = response.Content.ReadFromJsonAsync<RespuestaModel>().Result;
                TempData["MensajeError"] = contenido?.Mensaje ?? "Error al eliminar el activo.";
                return RedirectToAction("ListaActivos");
            }
            catch (Exception ex)
            {
                TempData["MensajeError"] = "Error al eliminar activo: " + ex.Message;
                return RedirectToAction("ListaActivos");
            }
        }
    }
}
