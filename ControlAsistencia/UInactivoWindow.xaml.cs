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
        public UInactivoWindow()
        {
            InitializeComponent();
            CargarUsuariosInactivos();
        }

        private void CargarUsuariosInactivos()
        {
            using (var db = new Data.AsistenciaDbContext())
            {
                listUsuarios.ItemsSource = db.Usuarios.Where(u => !u.Activo).ToList();
            }
        }

        private void BtnActivar_Click(object sender, RoutedEventArgs e)
        {
            var usuario = listUsuarios.SelectedItem as Usuario;
            if (usuario == null)
            {
                MessageBox.Show("Seleccione un usuarios", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            using (var db = new Data.AsistenciaDbContext())
            {
                var usuarioDb = db.Usuarios.FirstOrDefault(u => u.Id == usuario.Id);
                if(usuarioDb != null)
                {
                    usuarioDb.Activo = true;
                    usuarioDb.ActualizadoEn = DateTime.Now;
                    db.SaveChanges();
                }
            }

            MessageBox.Show("Usuario activado correctamente", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);

            CargarUsuariosInactivos();
        }
    }
}