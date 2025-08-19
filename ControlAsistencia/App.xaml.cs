using ControlAsistencia.Data;
using ControlAsistencia.Services;
using Microsoft.EntityFrameworkCore;
using System.Configuration;
using System.Data;
using System.Windows;

namespace ControlAsistencia
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override async void OnStartup(StartupEventArgs e)
        {
            // Crea/actualiza la BD y siembra admin
            using var db = new AsistenciaDbContext();

            // Si usas migraciones (mejor práctica):
            await db.Database.MigrateAsync();

            // Si aún no usas migraciones, usa EnsureCreated (solo desarrollo):
            // await db.Database.EnsureCreatedAsync();

            await new AuthService(db).EnsureAdminAsync();

            base.OnStartup(e);

            // Muestra la ventana principal
            new MainWindow().Show();
        }
    }

}
