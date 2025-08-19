using ControlAsistencia.Models;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ControlAsistencia.Data
{
    public class AsistenciaDbContext : DbContext
    {
        public DbSet<Usuario> Usuarios => Set<Usuario>();
        public DbSet<Asistencia> Asistencias => Set<Asistencia>();
        public DbSet<Horario> Horarios => Set<Horario>();
        public DbSet<UsuarioHorario> UsuariosHorarios => Set<UsuarioHorario>();
        public DbSet<AjusteAsistencia> AjustesAsistencias => Set<AjusteAsistencia>();
        public DbSet<Auditoria> Auditorias => Set<Auditoria>();


        public string DbPath { get; }


        public AsistenciaDbContext()
        {
            var folder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Asistencia");
            Directory.CreateDirectory(folder);
            DbPath = Path.Combine(folder, "asistencia.db");
        }


        protected override void OnConfiguring(DbContextOptionsBuilder options)
        {
            var csb = new SqliteConnectionStringBuilder { DataSource = DbPath };
            options.UseSqlite(csb.ToString());
        }


        protected override void OnModelCreating(ModelBuilder b)
        {
            // Converters
            var timeSpanToString = new ValueConverter<TimeSpan, string>(v => v.ToString(@"hh\:mm\:ss"), v => TimeSpan.Parse(v));
            var dateOnlyToString = new ValueConverter<DateOnly, string>(v => v.ToString("yyyy-MM-dd"), v => DateOnly.Parse(v));
            var nullableDateOnlyToString = new ValueConverter<DateOnly?, string?>(v => v.HasValue ? v.Value.ToString("yyyy-MM-dd") : null, v => v != null ? DateOnly.Parse(v) : (DateOnly?)null);


            // Usuario
            b.Entity<Usuario>(e =>
            {
                e.HasIndex(x => x.Correo).IsUnique();
                e.Property(x => x.Rol).HasConversion<string>();
                e.Property(x => x.Activo).HasDefaultValue(true);
                e.Property(x => x.CreadoEn).HasDefaultValueSql("CURRENT_TIMESTAMP");
                e.Property(x => x.ActualizadoEn).HasDefaultValueSql("CURRENT_TIMESTAMP");
            });


            // Asistencia
            b.Entity<Asistencia>(e =>
            {
                e.Property(x => x.Tipo).HasConversion<string>();
                e.Property(x => x.Origen).HasConversion<string>();
                e.Property(x => x.MarcaEn).HasColumnType("TEXT");
                e.HasOne(x => x.Usuario).WithMany(u => u.Asistencias).HasForeignKey(x => x.UsuarioId).OnDelete(DeleteBehavior.Restrict);
                e.HasOne(x => x.CreadoPor).WithMany().HasForeignKey(x => x.CreadoPorId).OnDelete(DeleteBehavior.SetNull);
                e.HasIndex(x => new { x.UsuarioId, x.MarcaEn });
            });
            // Horario
            b.Entity<Horario>(e =>
            {
                e.Property(x => x.HoraInicio).HasConversion(timeSpanToString);
                e.Property(x => x.HoraFin).HasConversion(timeSpanToString);
                e.Property(x => x.CreadoEn).HasDefaultValueSql("CURRENT_TIMESTAMP");
            });


            // UsuarioHorario
            b.Entity<UsuarioHorario>(e =>
            {
                e.Property(x => x.VigenteDesde).HasConversion(dateOnlyToString);
                e.Property(x => x.VigenteHasta).HasConversion(nullableDateOnlyToString);
                e.HasIndex(x => new { x.UsuarioId, x.VigenteDesde });
                e.HasOne(x => x.Usuario).WithMany().HasForeignKey(x => x.UsuarioId).OnDelete(DeleteBehavior.Cascade);
                e.HasOne(x => x.Horario).WithMany().HasForeignKey(x => x.HorarioId).OnDelete(DeleteBehavior.Restrict);
            });


            // AjusteAsistencia
            b.Entity<AjusteAsistencia>(e =>
            {
                e.Property(x => x.Tipo).HasConversion<string>();
                e.Property(x => x.Estado).HasConversion<string>();
                e.Property(x => x.HoraNueva).HasConversion(timeSpanToString);
                e.Property(x => x.FechaObjetivo).HasConversion(dateOnlyToString);
                e.Property(x => x.CreadoEn).HasDefaultValueSql("CURRENT_TIMESTAMP");
                e.HasOne(x => x.Usuario).WithMany().HasForeignKey(x => x.UsuarioId).OnDelete(DeleteBehavior.Restrict);
                e.HasOne(x => x.Asistencia).WithMany().HasForeignKey(x => x.AsistenciaId).OnDelete(DeleteBehavior.SetNull);
                e.HasOne(x => x.CreadoPor).WithMany().HasForeignKey(x => x.CreadoPorId).OnDelete(DeleteBehavior.Restrict);
                e.HasOne(x => x.AprobadoPor).WithMany().HasForeignKey(x => x.AprobadoPorId).OnDelete(DeleteBehavior.SetNull);
                e.HasIndex(x => new { x.UsuarioId, x.FechaObjetivo });
            });


            // Auditoria
            b.Entity<Auditoria>(e =>
            {
                e.Property(x => x.Accion).HasConversion<string>();
                e.Property(x => x.CreadoEn).HasDefaultValueSql("CURRENT_TIMESTAMP");
                e.HasOne(x => x.Actor).WithMany().HasForeignKey(x => x.ActorId).OnDelete(DeleteBehavior.Restrict);
                e.HasIndex(x => new { x.ActorId, x.CreadoEn });
            });
        }


        public override int SaveChanges()
        {
            TouchTimestamps();
            return base.SaveChanges();
        }
        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            TouchTimestamps();
            return base.SaveChangesAsync(cancellationToken);
        }
        private void TouchTimestamps()
        {
            var now = DateTime.UtcNow;
            foreach (var e in ChangeTracker.Entries<Usuario>())
            {
                if (e.State == EntityState.Added) e.Entity.CreadoEn = now;
                if (e.State == EntityState.Modified) e.Entity.ActualizadoEn = now;
            }
        }


    }
}
