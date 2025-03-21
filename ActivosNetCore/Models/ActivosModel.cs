using System.ComponentModel.DataAnnotations;

namespace ActivosNetCore.Models
{
    public class ActivosModel
    {
        public int? idActivo { get; set; }

        [Required(ErrorMessage = "El nombre del activo es obligatorio.")]
        public string? nombreActivo { get; set; }

        [Required(ErrorMessage = "La placa es obligatoria.")]
        public string? placa { get; set; }

        [Required(ErrorMessage = "La serie es obligatoria.")]
        public string? serie { get; set; }

        [Required(ErrorMessage = "La descripcion es obligatoria.")]
        public string? descripcion { get; set; }
        public bool? estado { get; set; }

        // FK //
        //Departamento
        [Required(ErrorMessage = "El departamento es obligatorio.")]
        [Range(1, int.MaxValue, ErrorMessage = "Seleccione un departamento valido.")]
        public int? idDepartamento { get; set; }
        public string? nombreDepartamento { get; set; }

        // FK //
        //Responsable
        [Required(ErrorMessage = "El responsable es obligatorio.")]
        [Range(1, int.MaxValue, ErrorMessage = "Seleccione un responsable valido.")]
        public int? idResponsable { get; set; }
        public string? nombreResponsable { get; set; }

    }
}
