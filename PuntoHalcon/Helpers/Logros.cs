using Dominio.Entidades;

namespace PuntoHalcon.Helpers;

// Reglas de insignias en un solo lugar para que Insignias, Perfil y Ranking den siempre el mismo resultado.
public static class Logros
{
    // Misiones que hay que completar para la insignia "especial" (la que no depende de una sola misión).
    public const int MisionesParaLogroEspecial = 3;

    // Insignias visibles para los estudiantes: ya aprobadas y, si dependen de una misión, que esa misión también esté publicada.
    public static IQueryable<Insignia> Publicadas(this IQueryable<Insignia> consulta) =>
        consulta.Where(i => !i.EsPropuesta && (i.MisionId == null || !i.Mision!.EsPropuesta));

    // completadas = ids de las misiones que el estudiante ya completó.
    public static bool Desbloqueada(Insignia insignia, ICollection<int> completadas) =>
        insignia.MisionId.HasValue
            ? completadas.Contains(insignia.MisionId.Value)
            : completadas.Count >= MisionesParaLogroEspecial;
}
