using ActivosNetCore.Models;
using System.Net.Http.Headers;
using System.Net.Http;
using System.Text.Json;
using System.Security.Cryptography;
using System.Text;

namespace ActivosNetCore.Dependencias
{
    public class Utilitarios : IUtilitarios
    {

        private readonly IHttpClientFactory _httpClient;
        private readonly IConfiguration _configuration;
        private readonly IHttpContextAccessor _accessor;

        public Utilitarios(IHttpClientFactory httpClient, IConfiguration configuration, IHttpContextAccessor accessor)
        {
            _httpClient = httpClient;
            _configuration = configuration;
            _accessor = accessor;

        }

        public HttpResponseMessage ObtenerListaDepartamento()
        {
            using (var api = _httpClient.CreateClient())
            {
                var url = _configuration.GetSection("Variables:urlApi").Value + "Login/ObtenerListaDepartamento";
                api.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _accessor.HttpContext!.Session.GetString("Token"));
                var response = api.GetAsync(url).Result;

                return response;
            }
        }

        public UsuarioModel? ObtenerInfoUsuario(int idUsuario)
        {
            using (var api = _httpClient.CreateClient())
            {
                api.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _accessor.HttpContext!.Session.GetString("Token"));
                var url = _configuration.GetSection("Variables:urlApi").Value + $"Usuarios/DetallesUsuario?idUsuario=" + idUsuario;
                var response = api.GetAsync(url).Result;

                if (response.IsSuccessStatusCode)
                {
                    var result = response.Content.ReadFromJsonAsync<RespuestaModel>().Result;

                    if (result != null && result.Indicador)
                    {

                        return JsonSerializer.Deserialize<UsuarioModel>((JsonElement)result.Datos!)!;
                    }
                }
            }
            return null;
        }

        public ActivosModel? ObtenerInfoActivo(int idActivo)
        {
            using (var api = _httpClient.CreateClient())
            {
                api.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _accessor.HttpContext!.Session.GetString("Token"));
                var url = _configuration.GetSection("Variables:urlApi").Value + $"Activos/DetallesActivo?idActivo=" + idActivo;
                var response = api.GetAsync(url).Result;

                if (response.IsSuccessStatusCode)
                {
                    var result = response.Content.ReadFromJsonAsync<RespuestaModel>().Result;

                    if (result != null && result.Indicador)
                    {

                        return JsonSerializer.Deserialize<ActivosModel>((JsonElement)result.Datos!)!;
                    }
                }
            }
            return null;
        }

        public TicketModel? ObtenerInfoTicket(int idTicket)
        {
            using (var api = _httpClient.CreateClient())
            {
                var url = _configuration.GetSection("Variables:urlApi").Value + $"Ticket/DetallesTicket?idTicket=" + idTicket;
                var response = api.GetAsync(url).Result;

                if (response.IsSuccessStatusCode)
                {
                    var result = response.Content.ReadFromJsonAsync<RespuestaModel>().Result;

                    if (result != null && result.Indicador)
                    {

                        return JsonSerializer.Deserialize<TicketModel>((JsonElement)result.Datos!)!;
                    }
                }
            }
            return null;
        }

        public MantenimientoModel? ObtenerInfoMantenimiento(int idMantenimiento)
        {
            using (var api = _httpClient.CreateClient())
            {
                var url = _configuration.GetSection("Variables:urlApi").Value + $"Mantenimiento/DetallesMantenimiento?idMantenimiento=" + idMantenimiento;
                var response = api.GetAsync(url).Result;

                if (response.IsSuccessStatusCode)
                {
                    var result = response.Content.ReadFromJsonAsync<RespuestaModel>().Result;

                    if (result != null && result.Indicador)
                    {

                        return JsonSerializer.Deserialize<MantenimientoModel>((JsonElement)result.Datos!)!;
                    }
                }
            }
            return null;
        }



        #region Usuarios

        public string Encrypt(string texto)
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

        public string Decrypt(string texto)
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

        #endregion

        //Listar usuarios responables del activo
        /*public HttpResponseMessage ObtenerResponsables()
        {
            using (var api = _httpClient.CreateClient())
            {
                var url = _configuration.GetSection("Variables:urlApi").Value + $"Activos/DetallesActivo";
                var response = api.GetAsync(url).Result;

                if (response.IsSuccessStatusCode)
                {
                    var result = response.Content.ReadFromJsonAsync<RespuestaModel>().Result;

                    if (result != null && result.Indicador)
                    {

                        return JsonSerializer.Deserialize<ActivosModel>((JsonElement)result.Datos!)!;
                    }
                }
            }
            return null;
        }*/

        /*
        public TicketModel? ObtenerInfoTicket(int idTicket)
        {
            using var api = _httpClient.CreateClient();
            var url = _configuration["Variables:urlApi"] + $"Ticket/DetallesTicket?idTicket={idTicket}";

            var response = api.GetAsync(url).Result;

            if (!response.IsSuccessStatusCode) return null;

            var respuesta = response.Content.ReadFromJsonAsync<RespuestaModel>().Result;

            if (respuesta?.Indicador == true && respuesta.Datos != null)
            {
                // Deserializar correctamente el JsonElement
                var datosJson = ((JsonElement)respuesta.Datos).GetRawText();
                return JsonSerializer.Deserialize<TicketModel>(datosJson);
            }

            return null;
        }*/


    }
}
