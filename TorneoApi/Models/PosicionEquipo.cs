namespace TorneoApi.Models
{
    public class PosicionEquipo
    {
        public int EquipoId { get; set; }
        public string Grupo { get; set; } = string.Empty;

        public int Puntos { get; set; }
        public int GolesFavor { get; set; }
        public int GolesContra { get; set; }
        public int DiferenciaGoles => GolesFavor - GolesContra;
    }
}
