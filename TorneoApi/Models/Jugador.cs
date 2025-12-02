namespace TorneoApi.Models
{
    public class Jugador
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public int NumeroCamiseta { get; set; }

        // FK
        public int EquipoId { get; set; }
        public Equipo? Equipo { get; set; }
    }
}
