using Microsoft.AspNetCore.Mvc;
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

namespace ActivosAPI.Controllers
{
    [AllowAnonymous]
    [Route("api/[controller]")]
    [ApiController]
    public class LoginController : Controller
    {
        private readonly IConfiguration _configuration;
        private readonly IUtilitarios _utilitarios;
        public LoginController(IConfiguration configuration, IUtilitarios utilitarios)
        {
            _configuration = configuration;
            _utilitarios = utilitarios;
        }

        [HttpPost]
        [Route("IniciarSesion")]
        public IActionResult IniciarSesion(UsuarioModel model)
        {
            using (var context = new SqlConnection(_configuration.GetSection("ConnectionStrings:BDConnection").Value))
            {
                var result = context.QueryFirstOrDefault<UsuarioModel>("SP_IniciarSesion",
                    new { model.correo, model.contrasenna },
                    commandType: CommandType.StoredProcedure);

                var respuesta = new RespuestaModel();

                if (result != null)
                {
                    var Token = GenerarToken(result.idUsuario, result.idRol);
                    result.Token = Token;

                    Console.WriteLine($"LLAVE: {Token}");
                    respuesta.Indicador = true;
                    respuesta.Mensaje = "Su información se ha validado correctamente";
                    respuesta.Datos = result;
                }
                else
                {
                    respuesta.Indicador = false;
                    respuesta.Mensaje = "Su información no se ha validado correctamente";
                }

                return Ok(respuesta);
            }
        }

        [Authorize]
        [HttpPost]
        [Route("RegistrarCuenta")]
        public IActionResult RegistrarCuenta(UsuarioModel model)
        {
            if (!_utilitarios.ValidarAdminFromToken(User.Claims)) //solo admin
            {
                return Unauthorized(new { message = "Acceso no autorizado" });
            }

            using (var context = new SqlConnection(_configuration.GetSection("ConnectionStrings:BDConnection").Value))
            {
                var result = context.Execute("SP_RegistrarCuenta", new
                {
                    model.usuario,
                    model.nombreCompleto,
                    model.cedula,
                    model.correo,
                    model.contrasenna,
                    model.idDepartamento,
                    model.idRol
                }, commandType: CommandType.StoredProcedure);

                var respuesta = new RespuestaModel();

                if (result > 0)
                {
                    respuesta.Indicador = true;
                    respuesta.Mensaje = "Su información se ha registrado correctamente";
                }
                else
                {
                    respuesta.Indicador = false;
                    respuesta.Mensaje = "Su información no ha registrado correctamente";
                }

                return Ok(respuesta);
            }
        }

        [Authorize]
        [HttpPost("ObtenerListaUsuarios")]
        public IActionResult ObtenerListaUsuarios(UsuarioModel? model)
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

        [HttpGet("ObtenerListaDepartamento")]
        public IActionResult ObtenerListaDepartamento()
        {
            var respuesta = new RespuestaModel();

            using (var context = new SqlConnection(_configuration.GetSection("ConnectionStrings:BDConnection").Value))
            {
                var result = context.Query<DepartamentoModel>("SP_ObtenerListaDepartamento");

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

                return Ok(respuesta.Datos);
            }
        }

        #region Token

        private string Encrypt(string texto)
        {
            byte[] iv = new byte[16];
            byte[] array;

            using (Aes aes = Aes.Create())
            {
                aes.Key = Encoding.UTF8.GetBytes(_configuration.GetSection("Variables:llaveCifrado").Value!);
                aes.IV = iv;

                ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV);

                using (MemoryStream memoryStream = new MemoryStream())
                {
                    using (CryptoStream cryptoStream = new CryptoStream(memoryStream, encryptor, CryptoStreamMode.Write))
                    {
                        using (StreamWriter streamWriter = new StreamWriter(cryptoStream))
                        {
                            streamWriter.Write(texto);
                        }

                        array = memoryStream.ToArray();
                    }
                }
            }

            return Convert.ToBase64String(array);
        }
        private string Decrypt(string texto)
        {
            byte[] iv = new byte[16];
            byte[] buffer = Convert.FromBase64String(texto);

            using (Aes aes = Aes.Create())
            {
                aes.Key = Encoding.UTF8.GetBytes(_configuration.GetSection("Variables:llaveCifrado").Value!);
                aes.IV = iv;

                ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, aes.IV);

                using (MemoryStream memoryStream = new MemoryStream(buffer))
                {
                    using (CryptoStream cryptoStream = new CryptoStream(memoryStream, decryptor, CryptoStreamMode.Read))
                    {
                        using (StreamReader streamReader = new StreamReader(cryptoStream))
                        {
                            return streamReader.ReadToEnd();
                        }
                    }
                }
            }
        }
        private string GenerarToken(int idUsuario, int idRol)
        {
            string SecretKey = _configuration.GetSection("Variables:llaveToken").Value!;

            List<Claim> claims = new List<Claim>();
            claims.Add(new Claim("idUsuario", idUsuario.ToString()));
            claims.Add(new Claim("idRol", idRol.ToString()));

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(SecretKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(60),
                signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        #endregion

    }
}
