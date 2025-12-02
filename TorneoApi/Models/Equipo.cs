namespace TorneoApi.Models
{
    public class Equipo
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Entrenador { get; set; } = string.Empty;

        // Relaciones
        public List<Jugador> Jugadores { get; set; } = new();
        public List<Inscripcion> Inscripciones { get; set; } = new();
    }
}
