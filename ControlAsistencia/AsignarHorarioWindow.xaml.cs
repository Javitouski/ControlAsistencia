using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

using ControlAsistencia.Data;
using ControlAsistencia.Models;
using ControlAsistencia.Services;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Windows;

namespace ControlAsistencia
{
    public partial class AsignarHorarioWindow : Window
    {
        private readonly AsistenciaDbContext _db = new();
        private readonly HorarioService _svc;
        private readonly int? _usuarioPrefijadoId;

        public AsignarHorarioWindow(int? usuarioId = null)
        {
            InitializeComponent();
            _svc = new HorarioService(_db);
            _usuarioPrefijadoId = usuarioId;

            Loaded += async (_, __) =>
            {
                // Usuarios activos
                var usuarios = await _db.Usuarios
                    .Where(u => u.Activo)
                    .OrderBy(u => u.Nombre)
                    .ToListAsync();
                cmbUsuario.ItemsSource = usuarios;

                // Horarios activos (mostramos "HH:mm - HH:mm")
                var horarios = await _svc.GetHorariosAsync();
                cmbHorario.ItemsSource = horarios;


                dpDesde.SelectedDate = DateTime.Today;

                if (_usuarioPrefijadoId.HasValue)
                {
                    cmbUsuario.SelectedValue = _usuarioPrefijadoId.Value;
                    cmbUsuario.IsEnabled = false;
                }
            };
        }

        private void BtnCancelar_Click(object sender, RoutedEventArgs e) => Close();

        private async void BtnAsignar_Click(object sender, RoutedEventArgs e)
        {
            if (cmbUsuario.SelectedValue is not int usuarioId)
            {
                MessageBox.Show("Seleccione un usuario."); return;
            }
            if (cmbHorario.SelectedValue is not int horarioId)
            {
                MessageBox.Show("Seleccione un horario."); return;
            }
            if (dpDesde.SelectedDate is not DateTime desdeDt)
            {
                MessageBox.Show("Seleccione fecha 'Desde'."); return;
            }

            var desde = DateOnly.FromDateTime(desdeDt);
            DateOnly? hasta = null;
            if (dpHasta.SelectedDate is DateTime hastaDt)
                hasta = DateOnly.FromDateTime(hastaDt);

            try
            {
                if (chkReemplazar.IsChecked == true)
                    await _svc.AsignarReemplazandoSuperpuestosAsync(usuarioId, horarioId, desde, hasta);
                else
                    await _svc.AsignarAsync(usuarioId, horarioId, desde, hasta); // modo estricto: sin solapes

                MessageBox.Show("Asignación registrada.", "OK", MessageBoxButton.OK, MessageBoxImage.Information);
                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Atención", MessageBoxButton.OK, MessageBoxImage.Warning);
            }

        }
    }
}
