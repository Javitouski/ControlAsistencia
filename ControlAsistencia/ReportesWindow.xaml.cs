using ControlAsistencia.Data;
using ControlAsistencia.Models;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Windows;

namespace ControlAsistencia
{
    public partial class ReportesWindow : Window
    {
        private readonly AsistenciaDbContext _context;

        public ReportesWindow()
        {
            InitializeComponent();
            _context = new AsistenciaDbContext();
            CargarReportes();
        }

        private void CargarReportes()
        {
            var reportes = _context.Asistencias
                .Include(a => a.Usuario)
                .Select(a => new ReporteAsistenciaDTO
                {
                    Usuario = a.Usuario.Nombre,
                    Rut = a.Usuario.Rut,
                    MarcaEn = a.MarcaEn,
                    Tipo = a.Tipo.ToString(),
                    Origen = a.Origen.ToString()
                })
                .OrderByDescending(r => r.MarcaEn)
                .ToList();

            dgAsistencias.ItemsSource = reportes;
        }

        private void DataGrid_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (dgAsistencias.SelectedItem is ReporteAsistenciaDTO reporte)
            {
                MessageBox.Show(
                    $"Usuario: {reporte.Usuario}\n" +
                    $"RUT: {reporte.Rut}\n" +
                    $"Fecha: {reporte.MarcaEn}\n" +
                    $"Tipo: {reporte.Tipo}\n" +
                    $"Origen: {reporte.Origen}");
            }
        }
    }
}
