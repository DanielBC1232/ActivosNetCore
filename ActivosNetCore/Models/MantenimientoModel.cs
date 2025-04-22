using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace ActivosNetCore.Models
{
    public class MantenimientoModel
    {
        public int? IdMantenimiento { get; set; }

        [Required(ErrorMessage = "El detalle es obligatorio.")]
        public string Detalle { get; set; }

        public DateTime Fecha { get; set; } = DateTime.Today;

        public bool Estado { get; set; }

        public int IdResponsable { get; set; } = 1;

        [JsonPropertyName("nombreResponsable")]
        public string NombreResponsable { get; set; } = "Sin nombre responsable";


        [Required(ErrorMessage = "El usuario es obligatorio.")]
        public int IdUsuario { get; set; }
        [JsonPropertyName("nombreUsuario")]
        public string NombreUsuario { get; set; } = "Sin nombre usuario";

        public int IdActivo { get; set; }
        [JsonPropertyName("nombreActivo")]
        public string NombreActivo { get; set; } = "Sin nombre de Activo";

    }
}
