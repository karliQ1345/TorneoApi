using TorneoApi.Models.Enums;
using System.ComponentModel.DataAnnotations;
namespace TorneoApi.Models
{
    public class Torneo
    {
        public int Id { get; set; }

        [Required]
        public string Nombre { get; set; } = string.Empty;
        public DateTime FechaInicio { get; set; }
        public DateTime FechaFin { get; set; }

        public TipoTorneo Tipo { get; set; }
        public EstadoTorneo Estado { get; set; } = EstadoTorneo.Pendiente;

        // Reglas: Mínimo 8, Máximo 32
        // Estas propiedades ayudarán a validar antes de iniciar
        public int MinEquipos { get; set; } = 8;
        public int MaxEquipos { get; set; } = 32;

        // Relaciones
        public List<Inscripcion> Inscripciones { get; set; } = new();
        public List<Partido> Partidos { get; set; } = new();
    }
}
