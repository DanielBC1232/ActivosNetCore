namespace ActivosAPI.Models
{
    public class ActivosModel
    {
        public int? idActivo { get; set; }
        public string? nombreActivo { get; set; }
        public string? placa { get; set; }
        public string? serie { get; set; }
        public string? descripcion { get; set; }
        public bool? estado { get; set; }

        // FK //
        //Departamento
        public int? idDepartamento { get; set; }
        public string? nombreDepartamento { get; set; }

        //Responsable
        public int? idResponsable { get; set; }
        public string? nombreResponsable { get; set; }

    }
}
