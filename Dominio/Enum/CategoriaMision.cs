using System.Text.Json.Serialization;

namespace Dominio.Enum
{
    // Categorías que ofrece el <select> del formulario "Proponer una misión".
    // El texto de JsonStringEnumMemberName es el que envía y espera el front end.
    public enum CategoriaMision
    {
        Biblioteca = 1,

        Cultura = 2,

        [JsonStringEnumMemberName("Tecnología")]
        Tecnologia = 3,

        Deportes = 4,

        Naturaleza = 5,

        Otra = 6
    }
}
