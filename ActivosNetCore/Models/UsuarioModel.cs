using System.ComponentModel.DataAnnotations;

namespace ActivosNetCore.Models
{
    public class UsuarioModel
    {
        public int? idUsuario { get; set; }

        [Required(ErrorMessage = "El nombre de usuario es obligatorio.")]
        public string? usuario { get; set; }

        [Required(ErrorMessage = "El nombre completo es obligatorio.")]
        public string? nombreCompleto { get; set; }

        [Required(ErrorMessage = "La cedula es obligatoria.")]
        public string? cedula { get; set; }

        [Required(ErrorMessage = "El correo es obligatorio.")]
        public string? correo { get; set; }

        [Required(ErrorMessage = "La contraseña es obligatorio.")]
        public string? contrasenna { get; set; }

        public bool? estado { get; set; }

        //FK
        public int? idDepartamento { get; set; }
        public string? nombreDepartamento { get; set; }
        public int? idRol { get; set; }
        public string? tipo { get; set; }
        
        //Token
        public string? Token { get; set; }


    }
}
