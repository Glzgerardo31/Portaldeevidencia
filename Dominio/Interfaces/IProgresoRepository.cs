using Dominio.Entidades;

namespace Dominio.Interfaces
{
    public interface IProgresoRepository
    {
        Task<List<Progreso>> ObtenerTodosAsync();

        Task<Progreso?> ObtenerPorIdAsync(int id);

        Task AgregarAsync(Progreso progreso);

        Task ActualizarAsync(Progreso progreso);

        Task EliminarAsync(int id);
    }
}
