using ControlAsistencia.Models.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ControlAsistencia.Models
{
    public class Asistencia
    {
        public int Id { get; set; }
        public TipoMarca Tipo { get; set; }
        public DateTime MarcaEn { get; set; }
        public OrigenMarca Origen { get; set; } = OrigenMarca.UI;
        public int UsuarioId { get; set; }
        public Usuario Usuario { get; set; } = null!;
        public int? CreadoPorId { get; set; }
        public Usuario? CreadoPor { get; set; }
    }
}
