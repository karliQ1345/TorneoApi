using Microsoft.EntityFrameworkCore;
using TorneoApi.Models;

namespace TorneoApi.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Torneo> Torneos { get; set; }
        public DbSet<Equipo> Equipos { get; set; }
        public DbSet<Jugador> Jugadores { get; set; }
        public DbSet<Inscripcion> Inscripciones { get; set; }

        public DbSet<Partido> Partidos { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configuración especial para PARTIDO (Dos FKs hacia Equipo)
            modelBuilder.Entity<Partido>()
                .HasOne(p => p.EquipoLocal)
                .WithMany()
                .HasForeignKey(p => p.EquipoLocalId)
                .OnDelete(DeleteBehavior.Restrict); // Evitar borrado en cascada

            modelBuilder.Entity<Partido>()
                .HasOne(p => p.EquipoVisitante)
                .WithMany()
                .HasForeignKey(p => p.EquipoVisitanteId)
                .OnDelete(DeleteBehavior.Restrict);

            // Evitar que un equipo se inscriba dos veces al mismo torneo
            modelBuilder.Entity<Inscripcion>()
                .HasIndex(i => new { i.TorneoId, i.EquipoId }).IsUnique();
        }
    }
}
