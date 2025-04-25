using System.ComponentModel.DataAnnotations;

namespace ActivosAPI.Models
{
    public class UsuarioModel
    {
        public int idUsuario { get; set; }

        public string? usuario { get; set; }

        public string? nombre { get; set; }

        public string? apellido { get; set; }

        public string? nombreCompleto { get; set; }

        public string? cedula { get; set; }

        public string? correo { get; set; }

        public string? contrasenna { get; set; }
        public bool? estado { get; set; }

        //FK
        public int? idDepartamento { get; set; }
        public string? nombreDepartamento { get; set; }
        public int idRol { get; set; }
        public string? tipo { get; set; }

        //Token
        public string? Token { get; set; }

    }
}
