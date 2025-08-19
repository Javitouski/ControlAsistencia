using ControlAsistencia.Models.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ControlAsistencia.Models
{
    public class AjusteAsistencia
    {
        public int Id { get; set; }
        public int UsuarioId { get; set; }
        public Usuario Usuario { get; set; } = null!;
        public int? AsistenciaId { get; set; }
        public Asistencia? Asistencia { get; set; }
        public DateOnly FechaObjetivo { get; set; }
        public TipoMarca Tipo { get; set; }
        public TimeSpan HoraNueva { get; set; }
        public string Motivo { get; set; } = null!;
        public EstadoAjuste Estado { get; set; } = EstadoAjuste.PENDIENTE;
        public int CreadoPorId { get; set; }
        public Usuario CreadoPor { get; set; } = null!;
        public int? AprobadoPorId { get; set; }
        public Usuario? AprobadoPor { get; set; }
        public DateTime CreadoEn { get; set; }
    }
}
