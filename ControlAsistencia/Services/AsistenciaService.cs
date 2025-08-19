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
    public class AsistenciaService
    {
        private readonly AsistenciaDbContext _db;
        public AsistenciaService(AsistenciaDbContext db) => _db = db;


        public async Task MarcarAsync(int usuarioId, TipoMarca tipo, int? actorId = null)
        {
            var last = await _db.Asistencias.Where(a => a.UsuarioId == usuarioId)
            .OrderByDescending(a => a.MarcaEn).FirstOrDefaultAsync();
            if (last != null && last.Tipo == tipo)
                throw new InvalidOperationException($"No se puede marcar dos veces {tipo} seguidas.");


            _db.Asistencias.Add(new Asistencia
            {
                UsuarioId = usuarioId,
                Tipo = tipo,
                MarcaEn = DateTime.Now,
                Origen = OrigenMarca.UI,
                CreadoPorId = actorId
            });
            await _db.SaveChangesAsync();
        }
    }
}
