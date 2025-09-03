using ControlAsistencia.Data;
using ControlAsistencia.Models;
using ControlAsistencia.Services;
using Microsoft.EntityFrameworkCore;
using System.Windows;

namespace ControlAsistencia
{
    public partial class App : Application
    {
        protected override async void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            using var db = new AsistenciaDbContext();
            await db.Database.MigrateAsync();

            var auth = new AuthService(db);
            await auth.EnsureAdminAsync();      // admin@empresa.com / admin123
            await auth.EnsureDemoUserAsync();   // user@empresa.com / user123

            // ✅ Seed idempotente de horarios (agrega el que falte)
            await EnsureDefaultHorariosAsync(db);

            // 👇 Mostrar la UI DESPUÉS de terminar el seed
            new MainWindow().Show();
        }

        private static async Task EnsureDefaultHorariosAsync(AsistenciaDbContext db)
        {
            var h1Inicio = new TimeSpan(9, 30, 0);
            var h1Fin = new TimeSpan(17, 30, 0);

            var h2Inicio = new TimeSpan(8, 0, 0);
            var h2Fin = new TimeSpan(16, 0, 0);

            if (!await db.Horarios.AnyAsync(h => h.HoraInicio == h1Inicio && h.HoraFin == h1Fin))
                db.Horarios.Add(new Horario { HoraInicio = h1Inicio, HoraFin = h1Fin, Activo = true });

            if (!await db.Horarios.AnyAsync(h => h.HoraInicio == h2Inicio && h.HoraFin == h2Fin))
                db.Horarios.Add(new Horario { HoraInicio = h2Inicio, HoraFin = h2Fin, Activo = true });

            if (db.ChangeTracker.HasChanges())
                await db.SaveChangesAsync();
        }
    }
}
