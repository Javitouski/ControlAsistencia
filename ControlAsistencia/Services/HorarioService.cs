using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ControlAsistencia.Data;
using ControlAsistencia.Models;
using Microsoft.EntityFrameworkCore;

namespace ControlAsistencia.Services;

public class HorarioService
{
    private readonly AsistenciaDbContext _db;
    public HorarioService(AsistenciaDbContext db) => _db = db;

    // Listado de horarios disponibles
    public Task<List<Horario>> GetHorariosAsync()
        => _db.Horarios.Where(h => h.Activo).OrderBy(h => h.HoraInicio).ToListAsync();

    // Asignaciones del usuario (útil para mostrar historial)
    public Task<List<UsuarioHorario>> GetAsignacionesAsync(int usuarioId)
        => _db.UsuariosHorarios
             .Include(uh => uh.Horario)
             .Where(uh => uh.UsuarioId == usuarioId)
             .OrderByDescending(uh => uh.VigenteDesde)
             .ToListAsync();

    // Regla de solape: [desde, hasta] vs [VigenteDesde, VigenteHasta]
    private static bool Overlaps(DateOnly desde, DateOnly? hasta, UsuarioHorario e)
    {
        var eHasta = e.VigenteHasta ?? DateOnly.FromDateTime(DateTime.MaxValue);
        var nHasta = hasta ?? DateOnly.FromDateTime(DateTime.MaxValue);
        return e.VigenteDesde <= nHasta && desde <= eHasta;
    }

    // Valida que no haya solape con rangos existentes
    private async Task EnsureNoOverlapAsync(int usuarioId, DateOnly desde, DateOnly? hasta)
    {
        var existentes = await _db.UsuariosHorarios
            .Where(uh => uh.UsuarioId == usuarioId)
            .ToListAsync();

        if (existentes.Any(e => Overlaps(desde, hasta, e)))
            throw new InvalidOperationException("El rango propuesto se solapa con una asignación existente.");
    }

    // Asignar sin tocar rangos previos (falla si hay solape)
    public async Task<UsuarioHorario> AsignarAsync(int usuarioId, int horarioId,
        DateOnly desde, DateOnly? hasta)
    {
        if (hasta.HasValue && hasta.Value < desde)
            throw new ArgumentException("La fecha 'Hasta' no puede ser menor a 'Desde'.");

        await EnsureNoOverlapAsync(usuarioId, desde, hasta);

        var entity = new UsuarioHorario
        {
            UsuarioId = usuarioId,
            HorarioId = horarioId,
            VigenteDesde = desde,
            VigenteHasta = hasta
        };
        _db.UsuariosHorarios.Add(entity);
        await _db.SaveChangesAsync();
        return entity;
    }

    // Variante cómoda: si hay un rango abierto, lo cierra el día anterior
    public async Task<UsuarioHorario> AsignarCerrandoAnteriorAsync(int usuarioId, int horarioId,
        DateOnly desde, DateOnly? hasta)
    {
        if (hasta.HasValue && hasta.Value < desde)
            throw new ArgumentException("La fecha 'Hasta' no puede ser menor a 'Desde'.");

        // Cerrar el rango abierto (si existe) antes de 'desde'
        var abierto = await _db.UsuariosHorarios
            .Where(uh => uh.UsuarioId == usuarioId && uh.VigenteHasta == null)
            .OrderByDescending(uh => uh.VigenteDesde)
            .FirstOrDefaultAsync();

        if (abierto != null && abierto.VigenteDesde <= desde)
        {
            var nuevoHasta = desde.AddDays(-1);
            if (nuevoHasta < abierto.VigenteDesde)
                throw new InvalidOperationException("No se puede cerrar el rango abierto (fechas inválidas).");
            abierto.VigenteHasta = nuevoHasta;
            await _db.SaveChangesAsync();
        }

        // Ahora valida contra el resto (ya no debería solapar)
        await EnsureNoOverlapAsync(usuarioId, desde, hasta);

        var entity = new UsuarioHorario
        {
            UsuarioId = usuarioId,
            HorarioId = horarioId,
            VigenteDesde = desde,
            VigenteHasta = hasta
        };
        _db.UsuariosHorarios.Add(entity);
        await _db.SaveChangesAsync();
        return entity;
    }

    public async Task<UsuarioHorario> AsignarReemplazandoSuperpuestosAsync(
    int usuarioId, int horarioId, DateOnly desde, DateOnly? hasta)
    {
        if (hasta.HasValue && hasta.Value < desde)
            throw new ArgumentException("La fecha 'Hasta' no puede ser menor a 'Desde'.");

        var max = DateOnly.FromDateTime(DateTime.MaxValue);
        var newEnd = hasta ?? max;

        using var tx = await _db.Database.BeginTransactionAsync();

        // 1) Traer todos los rangos que se superponen con [desde, newEnd]
        var overlappers = await _db.UsuariosHorarios
            .Where(uh => uh.UsuarioId == usuarioId &&
                         uh.VigenteDesde <= newEnd &&
                         (uh.VigenteHasta ?? max) >= desde)
            .OrderBy(uh => uh.VigenteDesde)
            .ToListAsync();

        foreach (var ex in overlappers)
        {
            var exEnd = ex.VigenteHasta ?? max;
            var exStartsBefore = ex.VigenteDesde < desde;
            var exEndsAfter = exEnd > newEnd;

            if (exStartsBefore && exEndsAfter)
            {
                // El nuevo queda al medio del rango existente -> partir en dos
                // a) recorta la 1ª parte
                ex.VigenteHasta = desde.AddDays(-1);

                // b) crea la 2ª parte (cola) después del nuevo
                var tailDesde = newEnd.AddDays(1);
                DateOnly? tailHasta = (ex.VigenteHasta == null) ? null : (exEnd == max ? null : exEnd);
                // Ojo: usamos exEnd ORIGINAL (guardado arriba)
                if (tailDesde <= (exEnd == max ? max : exEnd))
                {
                    _db.UsuariosHorarios.Add(new UsuarioHorario
                    {
                        UsuarioId = ex.UsuarioId,
                        HorarioId = ex.HorarioId,
                        VigenteDesde = tailDesde,
                        VigenteHasta = (exEnd == max ? (DateOnly?)null : exEnd)
                    });
                }
            }
            else if (exStartsBefore && !exEndsAfter)
            {
                // El existente llega hasta dentro del periodo nuevo -> recortar por izquierda
                ex.VigenteHasta = desde.AddDays(-1);
            }
            else if (!exStartsBefore && exEndsAfter)
            {
                // El existente empieza dentro del nuevo y termina después -> recortar por derecha
                // borramos el existente y creamos solo su cola después del nuevo
                _db.UsuariosHorarios.Remove(ex);

                var tailDesde = newEnd.AddDays(1);
                var tailHasta = (exEnd == max ? (DateOnly?)null : exEnd);
                _db.UsuariosHorarios.Add(new UsuarioHorario
                {
                    UsuarioId = ex.UsuarioId,
                    HorarioId = ex.HorarioId,
                    VigenteDesde = tailDesde,
                    VigenteHasta = tailHasta
                });
            }
            else
            {
                // El existente queda totalmente cubierto por el nuevo -> eliminarlo
                _db.UsuariosHorarios.Remove(ex);
            }
        }

        await _db.SaveChangesAsync();

        // 2) Insertar el nuevo rango (ya no hay solapes)
        var nuevo = new UsuarioHorario
        {
            UsuarioId = usuarioId,
            HorarioId = horarioId,
            VigenteDesde = desde,
            VigenteHasta = hasta
        };
        _db.UsuariosHorarios.Add(nuevo);
        await _db.SaveChangesAsync();

        await tx.CommitAsync();
        return nuevo;
    }

}
