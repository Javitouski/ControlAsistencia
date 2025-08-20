using ControlAsistencia.Data;
using ControlAsistencia.Models.Enums;
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
    /// Lógica de interacción para MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void BtnLog_Click(object sender, RoutedEventArgs e)
        {
            string correo = txtCorreo.Text;
            string password = txtPassword.Text;

            //Se conecta a la base de datos
            using var db = new AsistenciaDbContext();

            //Buscar usuario con el correo y contraseña ingresados más el rol de administrador
            var usuario = db.Usuarios
                .FirstOrDefault(u => u.Correo == correo && u.Rol == Rol.ADMIN);

            if (usuario != null && BCrypt.Net.BCrypt.Verify(password, usuario.HashPassword))
            {
                // Si el usuario existe y es admin, abre la ventana de administración
                var adminWin = new AdminWindow();
                adminWin.Show();
                this.Hide();
            }
            else
            {
                // Si no existe, muestra mensaje de error
                MessageBox.Show("Credenciales incorrectas o no eres administrador.");
            }
        }
    }
}
