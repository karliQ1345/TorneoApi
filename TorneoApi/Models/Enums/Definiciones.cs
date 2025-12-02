namespace TorneoApi.Models.Enums
{
    public enum TipoTorneo
    {
        Liga = 1, // Todos contra todos
        Copa = 2, // Eliminación directa
        Mixto = 3 // Grupos + Eliminación
    }

    public enum EstadoTorneo
    {
        Pendiente = 1, // Inscripciones abiertas
        EnJuego = 2,   // Calendario generado
        Finalizado = 3
    }
    
    public enum FasePartido 
    {
        FaseGrupos = 1,
        Octavos = 2,
        Cuartos = 3,
        Semifinal = 4,
        Final = 5
    }
}
