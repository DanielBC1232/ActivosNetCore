using System.ComponentModel.DataAnnotations;

namespace ActivosNetCore.Models
{
    public class ActivosModel
    {
        public int? idActivo { get; set; }

        public string? nombreActivo { get; set; }

        public int? placa { get; set; }

        public string? serie { get; set; }

        public string? descripcion { get; set; }
        public bool? estado { get; set; }

        // FK //
        //Departamento
        public int? idDepartamento { get; set; }
        public string? nombreDepartamento { get; set; }

        // FK //
        //Responsable
        public int? idUsuario { get; set; }

        public int? idResponsable { get; set; }
        public string? nombreResponsable { get; set; }

        public int? IdUsuarioSesion { get; set; }


    }
}
