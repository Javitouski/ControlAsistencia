using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ControlAsistencia.Models
{
    public class UsuarioHorario
    {
        public int Id { get; set; }
        public int UsuarioId { get; set; }
        public Usuario Usuario { get; set; } = null!;
        public int HorarioId { get; set; }
        public Horario Horario { get; set; } = null!;
        public DateOnly VigenteDesde { get; set; }
        public DateOnly? VigenteHasta { get; set; }
    }
}
