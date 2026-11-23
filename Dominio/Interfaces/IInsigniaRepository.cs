using Dominio.Entidades;

namespace Dominio.Interfaces
{
    public interface IInsigniaRepository
    {
        Task<List<Insignia>> ObtenerTodosAsync();

        Task<Insignia?> ObtenerPorIdAsync(int id);

        Task AgregarAsync(Insignia insignia);

        Task ActualizarAsync(Insignia insignia);

        Task EliminarAsync(int id);
    }
}
