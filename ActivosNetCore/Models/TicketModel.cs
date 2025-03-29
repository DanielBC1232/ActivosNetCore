using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace ActivosNetCore.Models
{
    public class TicketModel
    {
        public int? IdTicket { get; set; }

        [Required(ErrorMessage = "La urgencia es obligatoria.")]
        public string Urgencia { get; set; }

        [Required(ErrorMessage = "El detalle es obligatorio.")]
        public string Detalle { get; set; }

        public DateTime Fecha { get; set; } = DateTime.Today;

        public bool Solucionado { get; set; } = false; 

        public string DetalleTecnico { get; set; } = "Sin información técnica";

        public bool Estado { get; set; }

        [Required(ErrorMessage = "El usuario es obligatorio.")]
        public int IdUsuario { get; set; }
        [JsonPropertyName("nombreUsuario")]
        public string NombreUsuario { get; set; } = "Sin nombre usuario";

        [Required(ErrorMessage = "El departamento es obligatorio.")]
        public int IdDepartamento { get; set; }
        [JsonPropertyName("nombreDepartamento")]
        public string NombreDepartamento { get; set; } = "Sin nombre departamento";

        public int IdResponsable { get; set; } = 1;

        [JsonPropertyName("nombreResponsable")]
        public string NombreResponsable { get; set; } = "Sin nombre responsable";

    }
}
