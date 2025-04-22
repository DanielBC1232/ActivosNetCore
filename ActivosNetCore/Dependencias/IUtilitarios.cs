using ActivosNetCore.Models;

namespace ActivosNetCore.Dependencias
{
    public interface IUtilitarios
    {
       //List<ActivosModel> ObtenerInfoActivo(long Id);
        ActivosModel? ObtenerInfoActivo(int idActivo);

        UsuarioModel? ObtenerInfoUsuario(int idUsuario);

        TicketModel? ObtenerInfoTicket(int idTicket);

        MantenimientoModel? ObtenerInfoMantenimiento(int idMantenimiento);

        string Encrypt(string texto);

        HttpResponseMessage ObtenerListaDepartamento();

    }
}
