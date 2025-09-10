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
    /// Lógica de interacción para UInactivoWindow.xaml
    /// </summary>
    public partial class UInactivoWindow : Window
    {

        //Inicializa la aplicacion y carga los usuarios inactivos
        public UInactivoWindow()
        {
            InitializeComponent();
            CargarUsuariosInactivos();
        }

        //Función para cargar los usuarios inactivos en el DataGrid
        private void CargarUsuariosInactivos()
        {
            using (var db = new Data.AsistenciaDbContext())
            {
                listUsuarios.ItemsSource = db.Usuarios.Where(u => !u.Activo).ToList();
            }
        }

        //Evento para activar un usuario seleccionado
        private void BtnActivar_Click(object sender, RoutedEventArgs e)
        {
            // Obtener el usuario seleccionado
            var usuario = listUsuarios.SelectedItem as Usuario;
            if (usuario == null)
            {
                MessageBox.Show("Seleccione un usuario", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            // Se llama a la base de datos
            using (var db = new Data.AsistenciaDbContext())
            {
                // Se busca el usuario por su Id
                var usuarioDb = db.Usuarios.FirstOrDefault(u => u.Id == usuario.Id);
                if(usuarioDb != null) // Si el usuario existe, se activa
                {
                    // Se activa el usuario y se actualiza la fecha de actualización
                    usuarioDb.Activo = true;
                    usuarioDb.ActualizadoEn = DateTime.Now;
                    db.SaveChanges();
                }
            }

            MessageBox.Show("Usuario activado correctamente", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
            // Se recarga la lista de usuarios inactivos
            CargarUsuariosInactivos();
        }
    }
}