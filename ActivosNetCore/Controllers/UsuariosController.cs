using Microsoft.AspNetCore.Mvc;
using ActivosNetCore.Dependencias;
using ActivosNetCore.Models;
using System.Text.Json;
using static System.Runtime.InteropServices.JavaScript.JSType;
using System.Net.Http.Headers;
using System.Reflection;

namespace ActivosNetCore.Controllers
{
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public class UsuariosController : Controller
    {
        private readonly IHttpClientFactory _httpClient;
        private readonly IConfiguration _configuration;
        private readonly IUtilitarios _utilitarios;
        public UsuariosController(IHttpClientFactory httpClient, IConfiguration configuration, IUtilitarios utilitarios)
        {
            _httpClient = httpClient;
            _configuration = configuration;
            _utilitarios = utilitarios;
        }

        [HttpGet]
        public IActionResult DetallesUsuario(int idUsuario)
        {
            var response = _utilitarios.ObtenerInfoUsuario(idUsuario) ?? new UsuarioModel();

            if (response == null)
            {
                return NotFound("No se encontró el usuario.");
            }

            return View(response);
        }

        [HttpGet]
        public IActionResult ListaUsuarios()
        {
            var datos = new UsuarioModel
            {
                idDepartamento = 0,
                idRol = 0
            };
            return View(datos);
        }

        [HttpPost]
        public IActionResult ObtenerUsuarios(UsuarioModel model)
        {
            var datos = new
            {
                nombreCompleto = model.nombreCompleto,
                cedula = model.cedula,
                idDepartamento = model.idDepartamento,
                idRol = model.idRol
            };

            using (var api = _httpClient.CreateClient())
            {
                var url = _configuration.GetSection("Variables:urlApi").Value + "Usuarios/ObtenerListaUsuarios";
                var response = api.PostAsJsonAsync(url, datos).Result;

                if (response.IsSuccessStatusCode)
                {
                    var result = response.Content.ReadFromJsonAsync<List<UsuarioModel>>().Result;

                    if (result != null && result.Any())
                    {
                        return Json(result);
                    }
                }
            }
            return Json(new List<object>());
        }

        [HttpGet]
        public IActionResult RegistrarCuenta()
        {
            return View();
        }

        [HttpPost]
        public IActionResult RegistrarCuenta(UsuarioModel model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Msj = "Datos incompletos";
                Console.WriteLine(model.nombreCompleto);
                return View();
            }

            var datos = new
            {
                usuario = model.usuario,
                nombreCompleto = model.nombreCompleto,
                cedula = model.cedula,
                correo = model.correo,
                contrasenna = _utilitarios.Encrypt(model.contrasenna!),
                idDepartamento = model.idDepartamento,
                idRol = model.idRol
            };

            using (var api = _httpClient.CreateClient())
            {
                var url = _configuration.GetSection("Variables:urlApi").Value + "Usuarios/RegistrarCuenta";
                var response = api.PostAsJsonAsync(url, datos).Result;

                if (response.IsSuccessStatusCode)
                {
                    //retornar a listado usuarios
                    return RedirectToAction("ListaUsuarios", "Usuarios");
                }
                else
                    ViewBag.Msj = "No se pudo completar su petición";
            }

            return View();
        }

    }
}
