using ControlAsistencia.Data;
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

            // Crear/actualizar BD y sembrar usuarios
            using var db = new AsistenciaDbContext();
            await db.Database.MigrateAsync();

            var auth = new AuthService(db);
            await auth.EnsureAdminAsync();     // admin@empresa.com / admin123
            await auth.EnsureDemoUserAsync();  // user@empresa.com / user123  (nuevo)

            // Mostrar login
            new MainWindow().Show();
        }
    }
}
