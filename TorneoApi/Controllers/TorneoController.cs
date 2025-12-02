using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TorneoApi.Data;
using TorneoApi.Models;
using TorneoApi.Models.Enums;

namespace TorneoApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TorneoController : ControllerBase
    {
            private readonly AppDbContext _context;

            public TorneoController(AppDbContext context)
            {
                _context = context;
            }

            [HttpGet]
            public async Task<ActionResult<IEnumerable<Torneo>>> GetTorneos()
            {
                return await _context.Torneos.Include(t => t.Inscripciones).ToListAsync();
            }

            [HttpGet("{id}")]
            public async Task<ActionResult<Torneo>> GetTorneo(int id)
            {
                var torneo = await _context.Torneos
                    .Include(t => t.Inscripciones).ThenInclude(i => i.Equipo)
                    .Include(t => t.Partidos)
                    .FirstOrDefaultAsync(t => t.Id == id);

                if (torneo == null) return NotFound();
                return torneo;
            }

            [HttpPut("{id}")]
            public async Task<IActionResult> PutTorneo(int id, Torneo torneo)
            {
                if (id != torneo.Id) return BadRequest();
                _context.Entry(torneo).State = EntityState.Modified;

                try
                {
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TorneoExists(id)) return NotFound();
                    else throw;
                }
                return NoContent();
            }

            [HttpPost]
            public async Task<ActionResult<Torneo>> PostTorneo(Torneo torneo)
            {
                if (torneo.Estado == 0) torneo.Estado = EstadoTorneo.Pendiente;
                _context.Torneos.Add(torneo);
                await _context.SaveChangesAsync();
                return CreatedAtAction("GetTorneo", new { id = torneo.Id }, torneo);
            }

            [HttpDelete("{id}")]
            public async Task<IActionResult> DeleteTorneo(int id)
            {
                var torneo = await _context.Torneos.FindAsync(id);
                if (torneo == null) return NotFound();
                _context.Torneos.Remove(torneo);
                await _context.SaveChangesAsync();
                return NoContent();
            }

            private bool TorneoExists(int id)
            {
                return _context.Torneos.Any(e => e.Id == id);
            }


            [HttpPost("{id}/iniciar")]
            public async Task<IActionResult> IniciarTorneo(int id)
            {
                var torneo = await _context.Torneos
                    .Include(t => t.Inscripciones)
                    .Include(t => t.Partidos)
                    .FirstOrDefaultAsync(t => t.Id == id);

                if (torneo == null) return NotFound("Torneo no encontrado");

                if (torneo.Estado != EstadoTorneo.Pendiente)
                    return BadRequest($"El torneo no se puede iniciar porque su estado es {torneo.Estado}");

                if (torneo.Inscripciones.Count < torneo.MinEquipos)
                    return BadRequest($"Faltan equipos. Mínimo: {torneo.MinEquipos}. Actuales: {torneo.Inscripciones.Count}");

                if (torneo.Partidos.Any()) _context.Partidos.RemoveRange(torneo.Partidos);

                GenerarFaseGrupos(torneo);

                torneo.Estado = EstadoTorneo.EnJuego;
                await _context.SaveChangesAsync();

                return Ok(new { message = "Torneo iniciado", partidos = _context.ChangeTracker.Entries<Partido>().Count() });
            }

            [HttpPost("{id}/avanzar")]
            public async Task<IActionResult> AvanzarFase(int id)
            {
                var torneo = await _context.Torneos
                    .Include(t => t.Partidos)
                    .Include(t => t.Inscripciones)
                    .FirstOrDefaultAsync(t => t.Id == id);

                if (torneo == null) return NotFound("Torneo no encontrado");

             
                if (torneo.Partidos.Any(p => !p.Jugado))
                    return BadRequest("No se puede avanzar. Aún hay partidos sin jugar.");

               
                var ultimaFase = torneo.Partidos.Any() ? torneo.Partidos.Max(p => p.Fase) : FasePartido.FaseGrupos;

          
                switch (ultimaFase)
                {
                    case FasePartido.FaseGrupos:
                        GenerarCuartosDeFinal(torneo);
                        break;

                    case FasePartido.Cuartos:
                        GenerarSiguienteRondaEliminatoria(torneo, FasePartido.Cuartos, FasePartido.Semifinal);
                        break;

                    case FasePartido.Semifinal:
                        GenerarSiguienteRondaEliminatoria(torneo, FasePartido.Semifinal, FasePartido.Final);
                        break;

                    case FasePartido.Final:
                        torneo.Estado = EstadoTorneo.Finalizado;
                        await _context.SaveChangesAsync();
                        return Ok(new { message = "¡Torneo Finalizado! Tenemos Campeón." });

                    default:
                        return BadRequest("Fase desconocida o torneo ya finalizado.");
                }

                await _context.SaveChangesAsync();
                return Ok(new { message = $"Fase {ultimaFase} finalizada. Siguiente ronda generada." });
            }

            private void GenerarFaseGrupos(Torneo torneo)
            {
                var rng = new Random();
                var equipos = torneo.Inscripciones.OrderBy(x => rng.Next()).ToList();
                string[] grupos = { "A", "B", "C", "D" };
                int tamanoGrupo = equipos.Count / 4;

                for (int i = 0; i < 4; i++)
                {
                    var grupoActual = grupos[i];
                    var equiposDelGrupo = equipos.Skip(i * tamanoGrupo).Take(tamanoGrupo).ToList();

                    foreach (var ins in equiposDelGrupo) ins.Grupo = grupoActual;

                    for (int j = 0; j < equiposDelGrupo.Count; j++)
                    {
                        for (int k = j + 1; k < equiposDelGrupo.Count; k++)
                        {
                            _context.Partidos.Add(new Partido
                            {
                                TorneoId = torneo.Id,
                                EquipoLocalId = equiposDelGrupo[j].EquipoId,
                                EquipoVisitanteId = equiposDelGrupo[k].EquipoId,
                                FechaProgramada = DateTime.UtcNow.AddDays(1),
                                Fase = FasePartido.FaseGrupos,
                                Grupo = grupoActual,
                                Jugado = false
                            });
                        }
                    }
                }
            }

        private void GenerarCuartosDeFinal(Torneo torneo)
        {
            var partidosGrupos = torneo.Partidos.Where(p => p.Fase == FasePartido.FaseGrupos).ToList();
            var posiciones = new List<PosicionEquipo>();

            foreach (var ins in torneo.Inscripciones)
            {
                posiciones.Add(new PosicionEquipo
                {
                    EquipoId = ins.EquipoId,
                    Grupo = ins.Grupo ?? string.Empty
                });
            }

            foreach (var p in partidosGrupos)
            {
                var local = posiciones.FirstOrDefault(x => x.EquipoId == p.EquipoLocalId);
                var visita = posiciones.FirstOrDefault(x => x.EquipoId == p.EquipoVisitanteId);

                if (local == null || visita == null) continue;

                local.GolesFavor += p.GolesLocal;
                local.GolesContra += p.GolesVisitante;
                visita.GolesFavor += p.GolesVisitante;
                visita.GolesContra += p.GolesLocal;

                if (p.GolesLocal > p.GolesVisitante) local.Puntos += 3;
                else if (p.GolesVisitante > p.GolesLocal) visita.Puntos += 3;
                else { local.Puntos += 1; visita.Puntos += 1; }
            }

            var clasificados = posiciones
                .OrderByDescending(x => x.Puntos)
                .ThenByDescending(x => x.DiferenciaGoles)
                .ThenByDescending(x => x.GolesFavor)
                .GroupBy(x => x.Grupo)
                .Select(g => new { Grupo = g.Key, Primero = g.ElementAtOrDefault(0)?.EquipoId, Segundo = g.ElementAtOrDefault(1)?.EquipoId })
                .ToList();

            var gA = clasificados.FirstOrDefault(x => x.Grupo == "A");
            var gB = clasificados.FirstOrDefault(x => x.Grupo == "B");
            var gC = clasificados.FirstOrDefault(x => x.Grupo == "C");
            var gD = clasificados.FirstOrDefault(x => x.Grupo == "D");

            if (gA != null && gB != null && gA.Primero.HasValue && gB.Segundo.HasValue && gB.Primero.HasValue && gA.Segundo.HasValue)
            {
                CrearPartidoEliminatorio(torneo, gA.Primero.Value, gB.Segundo.Value, FasePartido.Cuartos);
                CrearPartidoEliminatorio(torneo, gB.Primero.Value, gA.Segundo.Value, FasePartido.Cuartos);
            }

            if (gC != null && gD != null && gC.Primero.HasValue && gD.Segundo.HasValue && gD.Primero.HasValue && gC.Segundo.HasValue)
            {
                CrearPartidoEliminatorio(torneo, gC.Primero.Value, gD.Segundo.Value, FasePartido.Cuartos);
                CrearPartidoEliminatorio(torneo, gD.Primero.Value, gC.Segundo.Value, FasePartido.Cuartos);
            }
        }

        private void GenerarSiguienteRondaEliminatoria(Torneo torneo, FasePartido faseActual, FasePartido faseSiguiente)
            {
                var partidosFase = torneo.Partidos.Where(p => p.Fase == faseActual).OrderBy(p => p.Id).ToList();
                var ganadores = new List<int>();

                foreach (var p in partidosFase)
                {
                    if (p.GolesLocal > p.GolesVisitante) ganadores.Add(p.EquipoLocalId);
                    else ganadores.Add(p.EquipoVisitanteId);
                }

                for (int i = 0; i < ganadores.Count; i += 2)
                {
                    if (i + 1 < ganadores.Count)
                    {
                        CrearPartidoEliminatorio(torneo, ganadores[i], ganadores[i + 1], faseSiguiente);
                    }
                }
            }

            private void CrearPartidoEliminatorio(Torneo torneo, int localId, int visitaId, FasePartido fase)
            {
                _context.Partidos.Add(new Partido
                {
                    TorneoId = torneo.Id,
                    EquipoLocalId = localId,
                    EquipoVisitanteId = visitaId,
                    FechaProgramada = DateTime.UtcNow.AddDays(2),
                    Fase = fase,
                    Jugado = false
                });
            }
        }
  }
