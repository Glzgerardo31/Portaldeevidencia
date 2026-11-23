using Dominio.Entidades;

namespace Aplicacion.Interfaces
{
    public interface IProgresoService
    {
        Task<List<Progreso>> ObtenerTodosAsync();

        Task<Progreso?> ObtenerPorIdAsync(int id);

        Task AgregarAsync(Progreso progreso);

        Task ActualizarAsync(Progreso progreso);

        Task EliminarAsync(int id);
    }
}
