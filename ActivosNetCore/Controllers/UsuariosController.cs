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
                return View();
            }

            var idUsuarioSesion = HttpContext.Session.GetInt32("UserId");

            if (idUsuarioSesion == null)
            {
                TempData["MensajeError"] = "Sesión expirada, por favor vuelva a iniciar sesión.";
                return RedirectToAction("IniciarSesion", "Login");
            }

            var datos = new
            {
                usuario = model.usuario,
                nombre = model.nombre,
                apellido = model.apellido,
                cedula = model.cedula,
                correo = model.correo,
                contrasenna = _utilitarios.Encrypt(model.contrasenna!),
                idDepartamento = model.idDepartamento,
                idRol = model.idRol,
                idUsuarioSesion = idUsuarioSesion.Value 
            };

            using (var api = _httpClient.CreateClient())
            {
                var url = _configuration.GetSection("Variables:urlApi").Value + "Usuarios/RegistrarCuenta";
                var response = api.PostAsJsonAsync(url, datos).Result;

                if (response.IsSuccessStatusCode)
                {
                    return RedirectToAction("ListaUsuarios", "Usuarios");
                }
                else
                {
                    var contenido = response.Content.ReadFromJsonAsync<RespuestaModel>().Result;
                    ViewBag.Msj = contenido?.Mensaje ?? "No se pudo completar su petición.";
                }
            }

            return View();
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
        public IActionResult DetallesUsuario(int idUsuario)
        {
            //if (idUsuario == null)//si no recibe parametro, toma el id de sesion (mi perfil)
            //{
            //    var idUsuarioSesion = Context.Session.GetString("idUsuario");

            //    if (string.IsNullOrEmpty(idUsuarioSesion) || !int.TryParse(idUsuarioSesion, out int idUsuarioParseado))
            //        return NotFound("Usuario no válido.");

            //    idUsuario = idUsuarioParseado;
            //}
            var response = _utilitarios.ObtenerInfoUsuario(idUsuario) ?? new UsuarioModel();

            if (response == null)
            {
                return NotFound("No se encontró el usuario.");
            }

            return View(response);
        }

        [HttpGet]
        public IActionResult EditarUsuario(int idUsuario)
        {
            var response = _utilitarios.ObtenerInfoUsuario(idUsuario) ?? new UsuarioModel();

            if (response == null)
            {
                return NotFound("No se encontró el usuario.");
            }

            return View(response);
        }

        //Editar usuario
        [HttpPost]
        public IActionResult EditarUsuario(UsuarioModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var idUsuarioSesion = HttpContext.Session.GetInt32("UserId");

            if (idUsuarioSesion == null)
            {
                TempData["MensajeError"] = "Sesión expirada, por favor vuelve a iniciar sesión.";
                return RedirectToAction("IniciarSesion", "Login");
            }

            using (var api = _httpClient.CreateClient())
            {
                api.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", HttpContext.Session.GetString("Token"));

                var url = _configuration.GetSection("Variables:urlApi").Value + "Usuarios/EditarUsuario";

                var datos = new
                {
                    model.idUsuario,
                    model.usuario,
                    model.nombre,
                    model.apellido,
                    model.cedula,
                    model.correo,
                    model.idDepartamento,
                    model.idRol,
                    idUsuarioSesion = idUsuarioSesion.Value
                };

                var response = api.PutAsJsonAsync(url, datos).Result;

                if (response.IsSuccessStatusCode)
                {
                    return RedirectToAction("ListaUsuarios", "Usuarios");
                }
                else
                {
                    var contenido = response.Content.ReadFromJsonAsync<RespuestaModel>().Result;
                    ViewBag.Msj = contenido?.Mensaje ?? "Error al editar el usuario.";
                    return View(model);
                }
            }
        }

        [HttpPost]
        public IActionResult ActualizarContrasenna(UsuarioModel model)
        {
            if (model.contrasenna != model.contrasennaConfirmar)
            {
                ViewBag.Msj = "Las contraseñas deben ser iguales";
                return View();
            }

            using (var api = _httpClient.CreateClient())
            {
                var url = _configuration.GetSection("Variables:urlApi").Value + "Usuarios/ActualizarContrasenna";

                model.contrasenna = _utilitarios.Encrypt(model.contrasenna!);// encriptar

                var contrasenna = new { contrasenna = model.contrasenna }; //extraer contrasena y crear objeto
                // El id de usuario se envia en token para el where=
                Console.WriteLine(contrasenna);
                api.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", HttpContext.Session.GetString("Token"));
                var response = api.PutAsJsonAsync(url, contrasenna).Result;

                if (response.IsSuccessStatusCode)
                {
                    var result = response.Content.ReadFromJsonAsync<RespuestaModel>().Result;

                    if (result != null && result.Indicador)
                    {
                        return RedirectToAction("ListaUsuarios", "Usuarios");
                    }
                    else
                        ViewBag.Msj = result!.Mensaje;
                }
                else
                    ViewBag.Msj = "No se pudo completar su petición";
            }

            return View();
        }

        [HttpPost]
        public IActionResult EliminarUsuario(UsuarioModel model)
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

                var param = new
                {
                    model.idUsuario,
                    idUsuarioSesion = idUsuarioSesion.Value 
                };

                var url = _configuration.GetSection("Variables:urlApi").Value + "Usuarios/EliminarUsuario";
                var response = api.PutAsJsonAsync(url, param).Result;

                if (response.IsSuccessStatusCode)
                {
                    TempData["MensajeOk"] = "Usuario eliminado correctamente.";
                    return RedirectToAction("ListaUsuarios", "Usuarios");
                }
                else
                {
                    var contenido = response.Content.ReadFromJsonAsync<RespuestaModel>().Result;
                    TempData["MensajeError"] = contenido?.Mensaje ?? "Error al eliminar el usuario.";
                    return RedirectToAction("ListaUsuarios", "Usuarios");
                }
            }
        }


    }
}
