using Dominio.Entidades;

namespace Dominio.Interfaces
{
    public interface IMisionRepository
    {
        Task<List<Mision>> ObtenerTodosAsync();

        Task<Mision?> ObtenerPorIdAsync(int id);

        Task AgregarAsync(Mision mision);

        Task ActualizarAsync(Mision mision);

        Task EliminarAsync(int id);
    }
}
