using ControlAsistencia.Models;
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
            // Usar el DataGrid correcto (dgUsuarios)
            var usuario = dgUsuarios.SelectedItem as Usuario;
            if (usuario == null)
            {
                MessageBox.Show("Seleccione un usuario para editar", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // Asegúrate que la ventana de edición se llame EditarUsuarioWindow y tenga un constructor que acepte un int (Id)
            var ventanaEdicion = new EditarUsuariosWindow(usuario.Id);
            if (ventanaEdicion.ShowDialog() == true)
            {
                CargarUsuariosActivos(); // refrescar lista si se guardaron cambios
            }
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
            var win = new UInactivoWindow();
            if (win.ShowDialog() == true)
            {
                CargarUsuariosActivos();
            }
        }
        //Evento para actualizar la lista de usuarios activos
        private void BtnActualziar_Click(object sender, RoutedEventArgs e)
        {
            CargarUsuariosActivos();
        }
    }
}



