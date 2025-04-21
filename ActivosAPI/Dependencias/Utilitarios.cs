using System.Security.Claims;

namespace ActivosAPI.Dependencias
{
    public class Utilitarios : IUtilitarios
    {
        public long ObtenerUsuarioFromToken(IEnumerable<Claim> valores)
        {
            if (valores.Any())
            {
                var idUsuario = valores.FirstOrDefault(x => x.Type == "idUsuario")?.Value;
                return long.Parse(idUsuario!);
            }

            return 0;
        }

        public bool ValidarAdminFromToken(IEnumerable<Claim> valores)
        {
            if (valores.Any())
            {
                var idRol = valores.FirstOrDefault(x => x.Type == "idRol")?.Value;
                return idRol == "1";
            }

            return false;
        }
    }
}