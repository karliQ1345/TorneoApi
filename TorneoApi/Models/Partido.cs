using TorneoApi.Models.Enums;
namespace TorneoApi.Models
{
    public class Partido
    {
        public int Id { get; set; }

        public int TorneoId { get; set; }
        public Torneo? Torneo { get; set; }

        // Equipos enfrentados
        public int EquipoLocalId { get; set; }
        public Equipo? EquipoLocal { get; set; }

        public int EquipoVisitanteId { get; set; }
        public Equipo? EquipoVisitante { get; set; }

        public DateTime FechaProgramada { get; set; }
        public FasePartido Fase { get; set; } // Grupos, Final, etc.
        public string? Grupo { get; set; } // "A" (si es fase de grupos)

        // Resultados
        public int GolesLocal { get; set; } = 0;
        public int GolesVisitante { get; set; } = 0;
        public bool Jugado { get; set; } = false; // ¿Ya se jugó?

    }
}
