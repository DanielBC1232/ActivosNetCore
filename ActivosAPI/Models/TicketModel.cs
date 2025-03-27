namespace ActivosAPI.Models
{
    public class TicketModel
    {
        public int? IdTicket { get; set; }
        public string Urgencia { get; set; }
        public string Detalle { get; set; }
        public DateTime Fecha { get; set; } = DateTime.Today;
        public bool Solucionado { get; set; } = false;
        public string DetalleTecnico { get; set; } = "Sin información técnica";
        public bool Estado { get; set; }


        public int IdUsuario { get; set; }
        public string nombreUsuario { get; set; } = "Sin nombre usuario";

        public int IdResponsable { get; set; } = 1;
        public string nombreResponsable { get; set; } = "Sin nombre responsable";

        public int IdDepartamento { get; set; }
        public string nombreDepartamento { get; set; } = "Sin nombre departamento";

    }
}
