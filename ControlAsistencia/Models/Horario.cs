using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ControlAsistencia.Models
{
    public class Horario
    {
        public int Id { get; set; }
        public TimeSpan HoraInicio { get; set; } 
        public TimeSpan HoraFin { get; set; } 
        public bool Activo { get; set; } = true;
        public DateTime CreadoEn { get; set; }
       

        [NotMapped]
        public string Descripcion => $"{HoraInicio:hh\\:mm} - {HoraFin:hh\\:mm}";

    }
}
