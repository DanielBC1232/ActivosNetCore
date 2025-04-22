using System.Security.Claims;

namespace ActivosAPI.Dependencias
{
    public interface IUtilitarios
    {
        int ObtenerUsuarioFromToken(IEnumerable<Claim> valores);

        bool ValidarAdminFromToken(IEnumerable<Claim> valores);

        bool ValidarTecnicoFromToken(IEnumerable<Claim> valores);

    }
}
