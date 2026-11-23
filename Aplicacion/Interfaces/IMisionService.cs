using Dominio.Entidades;

namespace Aplicacion.Interfaces
{
    public interface IMisionService
    {
        Task<List<Mision>> ObtenerTodosAsync();

        Task<Mision?> ObtenerPorIdAsync(int id);

        Task AgregarAsync(Mision mision);

        Task ActualizarAsync(Mision mision);

        Task EliminarAsync(int id);
    }
}
