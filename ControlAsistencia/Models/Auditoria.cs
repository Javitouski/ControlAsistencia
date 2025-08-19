using ControlAsistencia.Models.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ControlAsistencia.Models
{
    public class Auditoria
    {
        public int Id { get; set; }
        public int ActorId { get; set; }
        public Usuario Actor { get; set; } = null!;
        public Accion Accion { get; set; }
        public string Entidad { get; set; } = null!;
        public int? EntidadId { get; set; }
        public string? DetalleJson { get; set; }
        public string? Ip { get; set; }
        public DateTime CreadoEn { get; set; }
    }
}
