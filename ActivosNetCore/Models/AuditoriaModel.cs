
namespace ActivosNetCore.Models
{
    public class FiltroAuditoriaModel
    {
        public DateTime? FechaInicio { get; set; }
        public DateTime? FechaFin { get; set; }
        public int? IdUsuario { get; set; }
        public string? Accion { get; set; }
        public string? Tabla { get; set; }

    }

    public class AuditoriaModel
    {
        public int IdAuditoria { get; set; }
        public DateTime FechaAccion { get; set; }
        public string Tabla { get; set; }
        public string Accion { get; set; }
        public int IdRegistro { get; set; }
        public string NombreUsuario { get; set; }
    }

}
