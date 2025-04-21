using System.Security.Claims;

namespace ActivosAPI.Dependencias
{
    public interface IUtilitarios
    {
        long ObtenerUsuarioFromToken(IEnumerable<Claim> valores);

        bool ValidarAdminFromToken(IEnumerable<Claim> valores);

    }
}
