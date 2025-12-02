namespace TorneoApi.Models
{
    public class Inscripcion
    {
        public int Id { get; set; }
        public DateTime FechaInscripcion { get; set; } = DateTime.UtcNow;

        // FKs
        public int TorneoId { get; set; }
        public Torneo? Torneo { get; set; }

        public int EquipoId { get; set; }
        public Equipo? Equipo { get; set; }

        // Aquí guardaremos los puntos acumulados EN ESTE TORNEO ESPECÍFICO
        public int Puntos { get; set; } = 0;
        public int GolesFavor { get; set; } = 0;
        public int GolesContra { get; set; } = 0;
        public int PartidosJugados { get; set; } = 0;
        // Para torneo Mixto:
        public string? Grupo { get; set; } // "A", "B", "C", "D"
    }
}
