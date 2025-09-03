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

namespace ControlAsistencia
{
    /// <summary>
    /// Lógica de interacción para UsuariosWindow.xaml
    /// </summary>
    public partial class UsuariosWindow : Window
    {

        //Inicializa la aplicacion y carga los usuarios activos
        public UsuariosWindow()
        {
            InitializeComponent();
            CargarUsuariosActivos();
        }

        //Método para cargar los usuarios activos en el DataGrid
        private void CargarUsuariosActivos()
        {
            using (var db = new Data.AsistenciaDbContext())
            {
                var usuariosActivos = db.Usuarios
                    .Where(u => u.Activo) // Solo muestra a los usuarios activos
                    .ToList();
                dgUsuarios.ItemsSource = usuariosActivos;
            }

        }
        //Evento que abre la ventana para crear un nuevo usuario
        private void BtnCrear_Click(object sender, RoutedEventArgs e)
        {
            var win = new CrearUsuarioWindow();
            if (win.ShowDialog() == true)
            {
                CargarUsuariosActivos();
            }
        }

        //Evento para editar un usuario seleccionado
        private void BtnEditar_Click(object sender, RoutedEventArgs e)
        {
            // Lógica para editar usuario
        }

        //Evento para eliminar (desactivar) un usuario seleccionado
        private void BtnEliminar_Click(object sender, RoutedEventArgs e)
        {   // Obtener el usuario seleccionado  
            var usuarioSeleccionado = dgUsuarios.SelectedItem as Models.Usuario;

            // Validar que se haya seleccionado un usuario
            if (usuarioSeleccionado == null)
            {
                MessageBox.Show("Seleccione un usuario para desactivar.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // Confirmacion de desactivacion
            var result = MessageBox.Show($"¿Está seguro que desea desactivar al usuario {usuarioSeleccionado.Nombre}?"
                , "Confirmar desactivacion", MessageBoxButton.YesNo, MessageBoxImage.Warning);

            if (result != MessageBoxResult.Yes)
            {
                return;
            }


            // Desactivar el usuario en la base de datos
            try
            {
                using (var db = new Data.AsistenciaDbContext())
                {
                    //Buscar al usuario por id
                    var usuarioDb = db.Usuarios.FirstOrDefault(u => u.Id == usuarioSeleccionado.Id);
                    if (usuarioDb == null)
                    {
                        MessageBox.Show("El usuario ya no existe en la base de datos.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }
                    //Desactivar el usuario, actualizar la fecha y guardar cambios
                    usuarioDb.Activo = false;
                    usuarioDb.ActualizadoEn = DateTime.Now;
                    db.SaveChanges();
                }

                MessageBox.Show("Usuario desactivado exitosamente.", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
                //Recargar la lista de usuarios
                CargarUsuariosActivos();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al desactivar el usuario: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        //Evento para abrir la ventana de usuarios inactivos
        private void BtnInactivo_Click(object sender, RoutedEventArgs e)
        {
            new UInactivoWindow().Show();
        }
        //Evento para actualizar la lista de usuarios activos
        private void BtnActualziar_Click(object sender, RoutedEventArgs e)
        {
            CargarUsuariosActivos();
        }

        private void BtnAsignarHorario_Click(object sender, RoutedEventArgs e)
        {
            if (dgUsuarios.SelectedItem is not Models.Usuario u)
            {
                MessageBox.Show("Seleccione un usuario."); return;
            }
            var win = new AsignarHorarioWindow(u.Id) { Owner = this };
            if (win.ShowDialog() == true)
                CargarUsuariosActivos();
        }

        private async void BtnHistorialHorarios_Click(object sender, RoutedEventArgs e)
        {
            if (dgUsuarios.SelectedItem is not Models.Usuario u)
            {
                MessageBox.Show("Seleccione un usuario."); return;
            }

            using var db = new Data.AsistenciaDbContext();
            var svc = new Services.HorarioService(db);
            var hist = await svc.GetAsignacionesAsync(u.Id);

            var lineas = hist.Select(h =>
            {
                var desdeTxt = h.VigenteDesde.ToString("yyyy-MM-dd");
                var hastaTxt = h.VigenteHasta.HasValue
                    ? $"→ {h.VigenteHasta.Value:yyyy-MM-dd}"
                    : "→ ∞";
                var tramoTxt = $"{h.Horario.HoraInicio:hh\\:mm}-{h.Horario.HoraFin:hh\\:mm}"; // <- hh y mm
                return $"{desdeTxt} {hastaTxt}   {tramoTxt}";
            });

            var texto = string.Join(Environment.NewLine, lineas);
            MessageBox.Show(texto.Length == 0 ? "Sin asignaciones." : texto, "Historial de horarios");
        }

        }
    }



