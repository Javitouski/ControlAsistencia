using ControlAsistencia.Data;
using ControlAsistencia.Models;
using ControlAsistencia.Models.Enums;
using ControlAsistencia.Services;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Threading;

namespace ControlAsistencia
{
    public partial class AttendanceWindow : Window
    {
        private readonly Usuario _usuario;
        private readonly AsistenciaDbContext _db;
        private readonly AsistenciaService _asistenciaSvc;

        // Próxima acción calculada según la última marca del día
        private TipoMarca _siguiente = TipoMarca.ENTRADA;

        public AttendanceWindow(Usuario usuario)
        {
            InitializeComponent();
            _usuario = usuario;
            _db = new AsistenciaDbContext();
            _asistenciaSvc = new AsistenciaService(_db);

            TxtUsuario.Text = _usuario.Nombre;

            // Reloj simple
            var t = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
            t.Tick += (_, __) => TxtHora.Text = DateTime.Now.ToString("HH:mm:ss");
            t.Start();

            CargarEstado();
        }

        private void CargarEstado()
        {
            var hoy = DateTime.Today;
            var start = hoy;
            var end = hoy.AddDays(1);

            var last = _db.Asistencias
                .Where(a => a.UsuarioId == _usuario.Id && a.MarcaEn >= start && a.MarcaEn < end)
                .OrderBy(a => a.MarcaEn)
                .LastOrDefault();

            if (last == null)
            {
                TxtEstado.Text = "Sin marcas hoy";
                _siguiente = TipoMarca.ENTRADA;
                BtnMarcar.Content = "Registrar ENTRADA";
            }
            else
            {
                TxtEstado.Text = $"{last.Tipo} a las {last.MarcaEn:HH:mm}";
                _siguiente = last.Tipo == TipoMarca.ENTRADA ? TipoMarca.SALIDA : TipoMarca.ENTRADA;
                BtnMarcar.Content = _siguiente == TipoMarca.ENTRADA ? "Registrar ENTRADA" : "Registrar SALIDA";
            }
        }

        private async void BtnMarcar_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                await _asistenciaSvc.MarcarAsync(_usuario.Id, _siguiente, _usuario.Id);
                MessageBox.Show($"{_siguiente} registrada correctamente.", "OK",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                CargarEstado();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Atención", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
    }
}
