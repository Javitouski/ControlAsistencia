using ClosedXML.Excel;
using ControlAsistencia.Data;
using ControlAsistencia.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Win32;
using System;
using System.Linq;
using System.Windows;

namespace ControlAsistencia
{
    public partial class ExportarReportesWindow : Window
    {
        public ExportarReportesWindow()
        {
            InitializeComponent();
        }

        private void BtnExportar_Click(object sender, RoutedEventArgs e)
        {
            var fechaInicio = dpFechaInicio.SelectedDate;
            var fechaFin = dpFechaFin.SelectedDate;
            var rutFiltro = txtRut.Text.Trim(); // Nuevo filtro RUT

            if (!fechaInicio.HasValue || !fechaFin.HasValue)
            {
                MessageBox.Show("Debe seleccionar ambas fechas.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (fechaInicio > fechaFin)
            {
                MessageBox.Show("La fecha de inicio no puede ser mayor que la fecha fin.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var saveDialog = new SaveFileDialog
            {
                Filter = "Archivo Excel (*.xlsx)|*.xlsx",
                FileName = $"ReporteAsistencia_{fechaInicio:yyyyMMdd}_{fechaFin:yyyyMMdd}.xlsx"
            };

            if (saveDialog.ShowDialog() == true)
            {
                try
                {
                    using var db = new AsistenciaDbContext();

                    var query = db.Asistencias
                        .Include(a => a.Usuario)
                        .Where(a => a.MarcaEn >= fechaInicio && a.MarcaEn <= fechaFin);

                    // 🔎 Filtrar por RUT solo si el usuario escribió algo
                    if (!string.IsNullOrEmpty(rutFiltro))
                    {
                        query = query.Where(a => a.Usuario.Rut.Contains(rutFiltro));
                    }

                    var datos = query
                        .Select(a => new ReporteAsistenciaDTO
                        {
                            Usuario = a.Usuario.Nombre,
                            Rut = a.Usuario.Rut,
                            MarcaEn = a.MarcaEn,
                            Tipo = a.Tipo.ToString(),
                            Origen = a.Origen.ToString()
                        })
                        .OrderBy(r => r.MarcaEn)
                        .ToList();

                    using var wb = new XLWorkbook();
                    var ws = wb.Worksheets.Add("Reporte");

                    // Encabezados
                    ws.Cell(1, 1).Value = "Usuario";
                    ws.Cell(1, 2).Value = "RUT";
                    ws.Cell(1, 3).Value = "Fecha/Hora";
                    ws.Cell(1, 4).Value = "Tipo";
                    ws.Cell(1, 5).Value = "Origen";

                    // Datos
                    int row = 2;
                    foreach (var d in datos)
                    {
                        ws.Cell(row, 1).Value = d.Usuario;
                        ws.Cell(row, 2).Value = d.Rut;
                        ws.Cell(row, 3).Value = d.MarcaEn.ToString("yyyy-MM-dd HH:mm:ss");
                        ws.Cell(row, 4).Value = d.Tipo;
                        ws.Cell(row, 5).Value = d.Origen;
                        row++;
                    }

                    ws.Columns().AdjustToContents();
                    wb.SaveAs(saveDialog.FileName);

                    MessageBox.Show("Reporte exportado correctamente.", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error al exportar: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
    }
}
