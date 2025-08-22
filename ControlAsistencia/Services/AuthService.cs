using ControlAsistencia.Data;
using ControlAsistencia.Models;
using ControlAsistencia.Models.Enums;
using Microsoft.EntityFrameworkCore;

namespace ControlAsistencia.Services
{
    internal class AuthService
    {
        private readonly AsistenciaDbContext _db;
        public AuthService(AsistenciaDbContext db) => _db = db;

        public async Task<Usuario?> LoginAsync(string correo, string password)
        {
            var user = await _db.Usuarios
                .SingleOrDefaultAsync(u => u.Correo == correo && u.Activo);
            if (user is null) return null;

            return BCrypt.Net.BCrypt.Verify(password, user.HashPassword) ? user : null;
        }

        public async Task EnsureAdminAsync()
        {
            if (!await _db.Usuarios.AnyAsync(u => u.Correo == "admin@empresa.com"))
            {
                var hash = BCrypt.Net.BCrypt.HashPassword("admin123");
                _db.Usuarios.Add(new Usuario
                {
                    Nombre = "Administrador",
                    Correo = "admin@empresa.com",
                    HashPassword = hash,
                    Rol = Rol.ADMIN,
                    Rut = "11111111-1",   // <-- RUT válido
                    Activo = true
                });
                await _db.SaveChangesAsync();
            }
        }

        public async Task EnsureDemoUserAsync()
        {
            const string email = "user@empresa.com";
            if (!await _db.Usuarios.AnyAsync(u => u.Correo == email))
            {
                _db.Usuarios.Add(new Usuario
                {
                    Nombre = "Usuario Demo",
                    Correo = email,
                    HashPassword = BCrypt.Net.BCrypt.HashPassword("user123"),
                    Rol = Rol.USER,       // usuario normal
                    Rut = "12345678-5",   // <-- RUT válido
                    Activo = true
                });
                await _db.SaveChangesAsync();
            }
        }
    }
}
