using ActivosNetCore.Models;

namespace ActivosNetCore.Dependencias
{
    public interface IUtilitarios
    {
       //List<ActivosModel> ObtenerInfoActivo(long Id);
        ActivosModel? ObtenerInfoActivo(int idActivo);


    }
}
