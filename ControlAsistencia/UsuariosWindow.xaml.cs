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
        public UsuariosWindow()
        {
            InitializeComponent();
            CargarUsuarios();
        }
        private void CargarUsuarios()
        {
            using var db = new Data.AsistenciaDbContext();
            dgUsuarios.ItemsSource = db.Usuarios.ToList();
        }


        private void dgUsuarios_RowEditEnding(object sender, DataGridRowEditEndingEventArgs e)
        {
            // Lógica para guardar/actualizar usuarios aquí
        }

        private void BtnCrear_Click(object sender, RoutedEventArgs e)
        {
            var win = new CrearUsuarioWindow();
            if (win.ShowDialog() == true)
            {
                CargarUsuarios();
            }
        }

        private void BtnEditar_Click(object sender, RoutedEventArgs e)
        {
            // Lógica para editar usuario
        }

        private void BtnEliminar_Click(object sender, RoutedEventArgs e)
        {
            var usuarioSeleccionado = dgUsuarios.SelectedItem as Models.Usuario;

            if (usuarioSeleccionado == null)
            {
                MessageBox.Show("Seleccione un usuario para eliminar.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            //Confirmacion
            var result = MessageBox.Show($"¿Está seguro que desea eliminar al usuario {usuarioSeleccionado.Nombre}?", "Confirmar eliminación", MessageBoxButton.YesNo, MessageBoxImage.Warning);

            if (result != MessageBoxResult.Yes)
            {
                return;
            }

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
                    db.Usuarios.Remove(usuarioDb);
                    db.SaveChanges();
                }

                MessageBox.Show("Usuario eliminado exitosamente.", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
                //Recargar la lista de usuarios
                CargarUsuarios();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al eliminar el usuario: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}



