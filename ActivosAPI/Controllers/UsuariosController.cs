﻿using Microsoft.AspNetCore.Mvc;
using Dapper;
using ActivosAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Data.SqlClient;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Data;
using ActivosAPI.Dependencias;
using System.Reflection;
using System.Net.Mail;

namespace ActivosAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsuariosController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly IUtilitarios _utilitarios;
        public UsuariosController(IConfiguration configuration, IUtilitarios utilitarios)
        {
            _configuration = configuration;
            _utilitarios = utilitarios;
        }

        [HttpPost]
        [Route("RegistrarCuenta")]
        public IActionResult RegistrarCuenta(UsuarioModel model)
        {
            var respuesta = new RespuestaModel();
            try
            {
                using var context = new SqlConnection(_configuration.GetConnectionString("BDConnection"));

                var result = context.Execute("SP_RegistrarCuenta", new
                {
                    model.usuario,
                    model.nombre,
                    model.apellido,
                    model.cedula,
                    model.correo,
                    model.contrasenna,
                    model.idDepartamento,
                    model.idRol,
                    idUsuarioSesion = model.IdUsuarioSesion 
                }, commandType: CommandType.StoredProcedure);

                if (result > 0)
                {
                    respuesta.Indicador = true;
                    respuesta.Mensaje = "Su información se ha registrado correctamente";
                    return Ok(respuesta);
                }
                else
                {
                    respuesta.Indicador = false;
                    respuesta.Mensaje = "Su información no se ha registrado correctamente";
                    return StatusCode(500, respuesta);
                }
            }
            catch (SqlException ex)
            {
                respuesta.Indicador = false;
                respuesta.Mensaje = ex.Message;
                return BadRequest(respuesta);
            }
        }

        [HttpGet]
        [Route("DetallesUsuario")]
        public async Task<IActionResult> DetallesUsuario([FromQuery] int idUsuario)
        {
            using (var context = new SqlConnection(_configuration.GetSection("ConnectionStrings:BDConnection").Value))
            {
                string storedProcedure = "SP_DetallesUsuario";
                var parameters = new { idUsuario };
                // Esperar correctamente la tarea asincrónica
                var Usuario = await context.QueryFirstOrDefaultAsync<UsuarioModel>(
                    storedProcedure,
                    parameters,
                    commandType: CommandType.StoredProcedure);

                if (Usuario == null)
                {
                    return NotFound(new { message = "Usuario no encontrado" });
                }

                return Ok(new { Indicador = true, Mensaje = "Usuario encontrado", Datos = Usuario });
            }

        }

        [HttpPost("ObtenerListaUsuarios")]
        public IActionResult ObtenerListaUsuarios([FromBody] UsuarioModel? model)
        {
            //limpiar parametros
            var parametros = new
            {
                nombreCompleto = string.IsNullOrWhiteSpace(model?.nombreCompleto) ? null : model.nombreCompleto,
                cedula = string.IsNullOrWhiteSpace(model?.cedula) ? null : model.cedula,
                idDepartamento = model?.idDepartamento > 0 ? model.idDepartamento : (int?)null,
                idRol = model?.idRol > 0 ? model.idRol : (int?)null
            };

            var respuesta = new RespuestaModel();
            using (var context = new SqlConnection(_configuration.GetSection("ConnectionStrings:BDConnection").Value))
            {
                var result = context.Query<UsuarioModel>("SP_ObtenerListaUsuarios", parametros, commandType: CommandType.StoredProcedure); ;

                if (result.Any())
                {
                    respuesta.Indicador = true;
                    respuesta.Mensaje = "Información consultada";
                    respuesta.Datos = result;
                }
                else
                {
                    respuesta.Indicador = false;
                    respuesta.Mensaje = "No hay información registrada en este momento";
                }
                return Ok(result);
            }

        }

        [HttpPut]
        [Route("EditarUsuario")]
        public IActionResult EditarUsuario(UsuarioModel model)
        {
            var respuesta = new RespuestaModel();
            try
            {
                using var context = new SqlConnection(_configuration.GetConnectionString("BDConnection"));

                var result = context.Execute("SP_EditarUsuario", new
                {
                    model.idUsuario,
                    model.usuario,
                    model.nombre,
                    model.apellido,
                    model.cedula,
                    model.correo,
                    model.idDepartamento,
                    model.idRol,
                    idUsuarioSesion = model.IdUsuarioSesion
                }, commandType: CommandType.StoredProcedure);

                if (result > 0)
                {
                    respuesta.Indicador = true;
                    respuesta.Mensaje = "El usuario se ha actualizado correctamente";
                    return Ok(respuesta);
                }
                else
                {
                    respuesta.Indicador = false;
                    respuesta.Mensaje = "El usuario no se ha actualizado";
                    return StatusCode(500, respuesta);
                }
            }
            catch (SqlException ex)
            {
                respuesta.Indicador = false;
                respuesta.Mensaje = ex.Message;
                return BadRequest(respuesta);
            }
        }
        [HttpPut]
        [Route("ActualizarContrasenna")]
        public IActionResult ActualizarContrasenna(UsuarioModel model)
        {
            var idUsuario = _utilitarios.ObtenerUsuarioFromToken(User.Claims);

            using (var context = new SqlConnection(_configuration.GetSection("ConnectionStrings:BDConnection").Value))
            {
                var result = context.Execute("SP_ActualizarContrasenna",
                    new { idUsuario, model.contrasenna });

                var respuesta = new RespuestaModel();

                if (result > 0)
                {
                    // ✅ Contraseña actualizada, ahora enviar correo
                    var usuario = context.QueryFirstOrDefault<UsuarioModel>(
                        "SP_DetallesUsuario", new { idUsuario }, commandType: CommandType.StoredProcedure);

                    if (usuario != null && !string.IsNullOrEmpty(usuario.correo))
                    {
                        var contenido = $"Hola {usuario.nombre} {usuario.apellido},<br><br>Su contraseña fue cambiada exitosamente en el sistema Gestor de Activos.<br><br>Si usted no realizó este cambio, contacte al administrador.";
                        EnviarCorreo(usuario.correo, "Cambio de Contraseña - Gestor de Activos", contenido);
                    }

                    respuesta.Indicador = true;
                    respuesta.Mensaje = "Información actualizada";
                }
                else
                {
                    respuesta.Indicador = false;
                    respuesta.Mensaje = "No se actualizó la información correctamente";
                }

                return Ok(respuesta);
            }
        }


        [HttpPut]
        [Route("EliminarUsuario")]
        public IActionResult EliminarUsuario(UsuarioModel model)
        {
            var respuesta = new RespuestaModel();
            try
            {
                using var context = new SqlConnection(_configuration.GetConnectionString("BDConnection"));

                var result = context.Execute("SP_EliminarUsuario", new
                {
                    model.idUsuario,
                    idUsuarioSesion = model.IdUsuarioSesion
                }, commandType: CommandType.StoredProcedure);

                if (result > 0)
                {
                    respuesta.Indicador = true;
                    respuesta.Mensaje = "El usuario se ha desactivado correctamente";
                    return Ok(respuesta);
                }
                else
                {
                    respuesta.Indicador = false;
                    respuesta.Mensaje = "El usuario no se ha desactivado";
                    return StatusCode(500, respuesta);
                }
            }
            catch (SqlException ex)
            {
                respuesta.Indicador = false;
                respuesta.Mensaje = ex.Message;
                return BadRequest(respuesta);
            }
        }


        private void EnviarCorreo(string destino, string asunto, string contenido)
        {
            string cuenta = _configuration.GetSection("Variables:CorreoEmail").Value!;
            string contrasenna = _configuration.GetSection("Variables:ClaveEmail").Value!;

            MailMessage message = new MailMessage();
            message.From = new MailAddress(cuenta);
            message.To.Add(new MailAddress(destino));
            message.Subject = asunto;
            message.Body = contenido;
            message.Priority = MailPriority.Normal;
            message.IsBodyHtml = true;

            SmtpClient client = new SmtpClient("smtp.gmail.com", 587);
            client.Credentials = new System.Net.NetworkCredential(cuenta, contrasenna);
            client.EnableSsl = true;

            if (!string.IsNullOrEmpty(contrasenna))
            {
                client.Send(message);
            }
        }

    }
}
