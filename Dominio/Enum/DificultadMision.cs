using System.Text.Json.Serialization;

namespace Dominio.Enum
{
    // Opciones de dificultad (radios) del formulario "Proponer una misión".
    public enum DificultadMision
    {
        [JsonStringEnumMemberName("Fácil")]
        Facil = 1,

        Media = 2,

        [JsonStringEnumMemberName("Difícil")]
        Dificil = 3
    }
}
