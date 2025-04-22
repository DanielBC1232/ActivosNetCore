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
    public class LoginController : Controller
    {
        private readonly IHttpClientFactory _httpClient;
        private readonly IConfiguration _configuration;
        private readonly IUtilitarios _utilitarios;
        public LoginController(IHttpClientFactory httpClient, IConfiguration configuration, IUtilitarios utilitarios)
        {
            _httpClient = httpClient;
            _configuration = configuration;
            _utilitarios = utilitarios;
        }

        [HttpGet]
        public IActionResult ObtenerListaDepartamento(DepartamentoModel model)
        {
            var response = _utilitarios.ObtenerListaDepartamento();

            if (response.IsSuccessStatusCode)
            {
                var result = response.Content.ReadFromJsonAsync<List<DepartamentoModel>>().Result;

                if (result != null)
                { 
                    return Json(result);
                }
            }
            return Json(null);
        }

        #region Inicio sesion
        public IActionResult IniciarSesion()
        {
            return View();
        }

        [HttpPost]
        public IActionResult IniciarSesion(UsuarioModel model)
        {
            var datos = new
            {
                correo = model.correo,
                contrasenna = _utilitarios.Encrypt(model.contrasenna!)
            };
            using (var api = _httpClient.CreateClient())
            {
                var url = _configuration.GetSection("Variables:urlApi").Value + "Login/IniciarSesion";
                var response = api.PostAsJsonAsync(url, datos).Result;

                if (response.IsSuccessStatusCode)
                {
                    var result = response.Content.ReadFromJsonAsync<RespuestaModel>().Result;

                    if (result != null && result.Indicador)
                    {
                        var datosResult = JsonSerializer.Deserialize<UsuarioModel>((JsonElement)result.Datos!);
                        HttpContext.Session.SetString("Usuario", datosResult!.usuario!);
                        HttpContext.Session.SetString("Rol", datosResult!.tipo!);//Rol => /Administrador/Usuario/Soporte
                        HttpContext.Session.SetString("Token", datosResult!.Token!);

                        return RedirectToAction("ListaActivos", "Activos");
                    }
                    else
                    {
                        ViewBag.Msj = result!.Mensaje;
                    }
                }
                else
                {
                    ViewBag.Msj = "No se pudo completar su petición";
                }
            }

            return View();
        }

        #endregion

        #region Seguridad

        [FiltroSeguridadSesion]
        [HttpGet]
        public IActionResult Principal()
        {
            return View();
        }

        [FiltroSeguridadSesion]
        [HttpGet]
        public IActionResult CerrarSesion()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("IniciarSesion", "Login");
        }
        #endregion

    }
}
