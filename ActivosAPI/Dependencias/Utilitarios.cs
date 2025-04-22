using System.Security.Claims;

namespace ActivosAPI.Dependencias
{
    public class Utilitarios : IUtilitarios
    {
        private const string RolAdmin = "1";
        private const string RolUsuario = "2";
        private const string RolTecnico = "3";
        private const string ClaimUsuario = "idUsuario";
        private const string ClaimRol = "idRol";

        private string? ObtenerClaim(IEnumerable<Claim> claims, string tipo) =>
            claims.FirstOrDefault(c => c.Type == tipo)?.Value;

        public int ObtenerUsuarioFromToken(IEnumerable<Claim> claims)
        {
            var valor = ObtenerClaim(claims, ClaimUsuario);
            return int.TryParse(valor, out var id) ? id : 0;
        }

        private bool ValidarRol(IEnumerable<Claim> claims, params string[] rolesPermitidos)
        {
            var valor = ObtenerClaim(claims, ClaimRol);
            return valor != null && rolesPermitidos.Contains(valor);
        }

        public bool ValidarAdminFromToken(IEnumerable<Claim> claims) =>
            ValidarRol(claims, RolAdmin);

        public bool ValidarTecnicoFromToken(IEnumerable<Claim> claims) =>
            ValidarRol(claims, RolAdmin, RolTecnico);

    }
}
