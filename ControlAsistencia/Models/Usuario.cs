using ControlAsistencia.Models.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ControlAsistencia.Models
{
    public class Usuario
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = null!;
        public string Correo { get; set; } = null!;
        public string HashPassword { get; set; } = null!;
        public Rol Rol { get; set; } = Rol.USER;
        public bool Activo { get; set; } = true;
        public DateTime CreadoEn { get; set; }
        public DateTime ActualizadoEn { get; set; }
        public ICollection<Asistencia> Asistencias { get; set; } = new List<Asistencia>();
    }
}
