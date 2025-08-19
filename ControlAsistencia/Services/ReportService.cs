using ControlAsistencia.Data;
using ControlAsistencia.Models;
using ControlAsistencia.Models.Enums;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ControlAsistencia.Services
{
    public class ReportService
    {
        private readonly AsistenciaDbContext _db;
        public ReportService(AsistenciaDbContext db) => _db = db;


        private async Task<Horario> GetHorarioVigenteAsync(long usuarioId, DateOnly fecha)
        => await _db.UsuariosHorarios
        .Where(uh => uh.UsuarioId == usuarioId
        && uh.VigenteDesde <= fecha
        && (uh.VigenteHasta == null || uh.VigenteHasta >= fecha))
        .OrderByDescending(uh => uh.VigenteDesde)
        .Select(uh => uh.Horario)
        .FirstAsync();


        public async Task<List<(Usuario u, DateTime marca)>> AtrasosAsync(DateOnly desde, DateOnly hasta)
        {
            var desdeDt = desde.ToDateTime(TimeOnly.MinValue);
            var hastaDt = hasta.AddDays(1).ToDateTime(TimeOnly.MinValue);


            var query = from a in _db.Asistencias
                        where a.Tipo == TipoMarca.ENTRADA && a.MarcaEn >= desdeDt && a.MarcaEn < hastaDt
                        join uh in _db.UsuariosHorarios on a.UsuarioId equals uh.UsuarioId
                        where uh.VigenteDesde <= DateOnly.FromDateTime(a.MarcaEn.Date)
                        && (uh.VigenteHasta == null || uh.VigenteHasta >= DateOnly.FromDateTime(a.MarcaEn.Date))
                        join h in _db.Horarios on uh.HorarioId equals h.Id
                        where a.MarcaEn.TimeOfDay > h.HoraInicio
                        orderby a.Usuario.Nombre, a.MarcaEn
                        select new { a.Usuario, a.MarcaEn };


            var list = await query.ToListAsync();
            return list.Select(x => (x.Usuario, x.MarcaEn)).ToList();
        }


        public async Task<List<(Usuario u, DateTime marca)>> SalidasAnticipadasAsync(DateOnly desde, DateOnly hasta)
        {
            var desdeDt = desde.ToDateTime(TimeOnly.MinValue);
            var hastaDt = hasta.AddDays(1).ToDateTime(TimeOnly.MinValue);


            var query = from a in _db.Asistencias
                        where a.Tipo == TipoMarca.SALIDA && a.MarcaEn >= desdeDt && a.MarcaEn < hastaDt
                        join uh in _db.UsuariosHorarios on a.UsuarioId equals uh.UsuarioId
                        where uh.VigenteDesde <= DateOnly.FromDateTime(a.MarcaEn.Date)
                        && (uh.VigenteHasta == null || uh.VigenteHasta >= DateOnly.FromDateTime(a.MarcaEn.Date))
                        join h in _db.Horarios on uh.HorarioId equals h.Id
                        where a.MarcaEn.TimeOfDay < h.HoraFin
                        orderby a.Usuario.Nombre, a.MarcaEn
                        select new { a.Usuario, a.MarcaEn };


            var list = await query.ToListAsync();
            return list.Select(x => (x.Usuario, x.MarcaEn)).ToList();
        }
    }
}
