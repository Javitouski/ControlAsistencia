using ControlAsistencia.Data;
using ControlAsistencia.Services;
using ControlAsistencia.Models.Enums;
using System.Linq;
using System.Windows;

namespace ControlAsistencia
{
    public partial class MainWindow : Window
    {
        public MainWindow() => InitializeComponent();


        // Función LogIn
        private async void BtnLog_Click(object sender, RoutedEventArgs e)
        {
            // Obtener los valores de los campos de texto
            string correo = txtCorreo.Text.Trim();
            string password = txtPassword.Text; // si luego usas PasswordBox: .Password

            
            using var db = new AsistenciaDbContext();
            var auth = new AuthService(db);

            // Valida que el usuario exista y las credenciales sean correctas
            var user = await auth.LoginAsync(correo, password);
            if (user == null)
            {
                MessageBox.Show("Credenciales incorrectas o usuario inactivo.");
                return;
            }
            //Si el usuario cuenta con el rol de admin, abrir la ventana de admin
            if (user.Rol == Rol.ADMIN)
            {
                new AdminWindow().Show();
            }
            //Si el usuario cuenta con el rol de user, abrir la ventana de asistencia
            else
            {
                new AttendanceWindow(user).Show();
            }
            //Cierra la ventana de login
            this.Close();
        }
    }
}
