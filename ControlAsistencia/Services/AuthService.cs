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
    internal class AuthService
    {
        private readonly AsistenciaDbContext _db;
        public AuthService(AsistenciaDbContext db) => _db = db;


        public async Task<Usuario?> LoginAsync(string correo, string password)
        {
            var user = await _db.Usuarios.SingleOrDefaultAsync(u => u.Correo == correo && u.Activo == true);
            if (user is null) return null;
            return BCrypt.Net.BCrypt.Verify(password, user.HashPassword) ? user : null;
        }


        public async Task EnsureAdminAsync()
        {
            if (!await _db.Usuarios.AnyAsync())
            {
                var hash = BCrypt.Net.BCrypt.HashPassword("admin123");
                _db.Usuarios.Add(new Usuario { Nombre = "Administrador", Correo = "admin@empresa.com", HashPassword = hash, Rol = Rol.ADMIN });
                await _db.SaveChangesAsync();
            }
        }

        public async Task EnsureDemoUserAsync()
        {
            var email = "user@empresa.com";
            if (!await _db.Usuarios.AnyAsync(u => u.Correo == email))
            {
                var user = new Usuario
                {
                    Nombre = "Usuario Demo",
                    Correo = email,
                    HashPassword = BCrypt.Net.BCrypt.HashPassword("user123"),
                    Rol = Rol.USER,         // <- importante: NO admin
                    Activo = true,
                    CreadoEn = DateTime.Now,
                    ActualizadoEn = DateTime.Now
                };
                _db.Usuarios.Add(user);
                await _db.SaveChangesAsync();
            }
        }

    }
}
