using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ControlAsistencia.Models
{
    public class ReporteAsistenciaDTO
    {
        public string Usuario { get; set; } = "";
        public string Rut { get; set; } = "";
        public DateTime MarcaEn { get; set; }
        public string Tipo { get; set; } = "";
        public string Origen { get; set; } = "";
    }
}
