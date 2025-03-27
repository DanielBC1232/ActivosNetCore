using ActivosNetCore.Models;
using System.Net.Http.Headers;
using System.Net.Http;
using System.Text.Json;

namespace ActivosNetCore.Dependencias
{
    public class Utilitarios : IUtilitarios
    {

        private readonly IHttpClientFactory _httpClient;
        private readonly IConfiguration _configuration;

        public Utilitarios(IHttpClientFactory httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _configuration = configuration;
        }

        public ActivosModel? ObtenerInfoActivo(int idActivo)
        {
            using (var api = _httpClient.CreateClient())
            {
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

    }
}
