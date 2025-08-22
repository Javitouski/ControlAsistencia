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

        private async void BtnLog_Click(object sender, RoutedEventArgs e)
        {
            string correo = txtCorreo.Text.Trim();
            string password = txtPassword.Text; // si luego usas PasswordBox: .Password

            using var db = new AsistenciaDbContext();
            var auth = new AuthService(db);

            var user = await auth.LoginAsync(correo, password);
            if (user == null)
            {
                MessageBox.Show("Credenciales incorrectas o usuario inactivo.");
                return;
            }

            if (user.Rol == Rol.ADMIN)
            {
                new AdminWindow().Show();
            }
            else
            {
                new AttendanceWindow(user).Show();
            }
        }
    }
}
