using Dominio.Enum;
using System.ComponentModel.DataAnnotations;

namespace Dominio.Entidades
{
    // Tarjeta de la vista "Explorar misiones".
    // Las reglas de longitud son las mismas que valida el formulario del front end.
    public class Mision
    {
        public int MisionId { get; set; }

        // Cuenta estudiante que envió la propuesta (null para contenido inicial/admin).
        public int? UsuarioId { get; set; }
        public Usuario? Usuario { get; set; }

        [Required]
        [StringLength(40, MinimumLength = 3)]
        public string Nombre { get; set; } = string.Empty;

        [Required]
        [StringLength(120, MinimumLength = 10)]
        public string Descripcion { get; set; } = string.Empty;

        public CategoriaMision Categoria { get; set; }

        [Required]
        [StringLength(40, MinimumLength = 3)]
        public string Lugar { get; set; } = string.Empty;

        public DificultadMision Dificultad { get; set; }

        // Código que el estudiante escribe para completar la misión.
        // Solo lo ve y lo edita el administrador. Nunca se envía a los estudiantes.
        [StringLength(40)]
        public string? Codigo { get; set; }

        // true cuando el administrador rechazó la propuesta (sigue sin publicarse y el estudiante ve el motivo).
        public bool Rechazada { get; set; }

        [StringLength(200)]
        public string? NotaAdmin { get; set; }

        // true cuando la misión la creó un estudiante desde el formulario
        // (en pantalla aparece con la etiqueta "PROPUESTA").
        public bool EsPropuesta { get; set; }

        public DateTime FechaCreacion { get; set; }

        public ICollection<Progreso> Progresos { get; set; } = new List<Progreso>();

        public ICollection<Insignia> Insignias { get; set; } = new List<Insignia>();
    }
}
