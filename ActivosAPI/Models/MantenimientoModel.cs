namespace ActivosAPI.Models
{
    public class MantenimientoModel
    {
        public int? IdMantenimiento { get; set; }
        public string Detalle { get; set; }
        public DateTime Fecha { get; set; } = DateTime.Today;
        public bool Estado { get; set; }

        public int IdUsuario { get; set; }
        public string nombreUsuario { get; set; } = "Sin nombre usuario";

        public int IdResponsable { get; set; } = 1;
        public string nombreResponsable { get; set; } = "Sin nombre responsable";

        public int IdActivo { get; set; }
        public string nombreActivo { get; set; } = "Sin nombre activo";

    }
}
