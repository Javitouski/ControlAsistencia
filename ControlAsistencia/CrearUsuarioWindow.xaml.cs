using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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
using ControlAsistencia.Models.Enums;

namespace ControlAsistencia
{
    /// <summary>
    /// Lógica de interacción para CrearUsuarioWindow.xaml
    /// </summary>
    public partial class CrearUsuarioWindow : Window
    {
        //Inicializa la aplicacion y carga los roles en el ComboBox
        public CrearUsuarioWindow()
        {
            InitializeComponent();
            cmbRol.ItemsSource = Enum.GetValues(typeof(Rol));
        }
        //Evento para guardar un nuevo usuario
        private void BtnGuardar_Click(object sender, RoutedEventArgs e)
        {
            // Validaciones de entrada
            bool isValid = true;
            // Ocultar todos los mensajes de error inicialmente
            lblRutError.Visibility = Visibility.Collapsed;
            lblNombreError.Visibility = Visibility.Collapsed;
            lblCorreoError.Visibility = Visibility.Collapsed;
            lblContraseñaError.Visibility = Visibility.Collapsed;
            lblRolError.Visibility = Visibility.Collapsed;

            //Validacion de RUT
            if (string.IsNullOrWhiteSpace(txtRut.Text) || txtRut.Text.Length < 7)
            {
                lblRutError.Text = "Ingrese un rut válido.";
                lblRutError.Visibility = Visibility.Visible;
                isValid = false;
            }
            //Validacion de Nombre
            if (string.IsNullOrWhiteSpace(txtNombre.Text))
            {
                lblNombreError.Text = "El nombre es obligatorio.";
                lblNombreError.Visibility = Visibility.Visible;
                isValid = false;
            }
            //Validacion de Correo
            string emailPattern = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";
            if (string.IsNullOrWhiteSpace(txtCorreo.Text) || !Regex.IsMatch(txtCorreo.Text, emailPattern))
            {
                lblCorreoError.Text = "Ingrese un correo válido";
                lblCorreoError.Visibility = Visibility.Visible;
                isValid = false;
            }

            //Validacion de Contrasñea
            if(string.IsNullOrWhiteSpace(txtContraseña.Password) || txtContraseña.Password.Length < 6)
            {
                lblContraseñaError.Text = "Ingrese una contraseña con almenos 6 caracteres.";
                lblContraseñaError.Visibility = Visibility.Visible;
                isValid = false;
            }
            //Validacion de Rol
            if(cmbRol.SelectedItem == null)
            {
                lblRolError.Text = "Seleccion el ROl del usuario";
                lblRolError.Visibility = Visibility.Visible;
                isValid = false;
            }
            // Si alguna validacion falla, se muestra un mensaje y se detiene el guardado
            if (!isValid)
            {
                MessageBox.Show("Por favor, corrija los errores antes de guardar.", "Validacion", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            try
            {
                
                using (var db = new AsistenciaDbContext())
                {//Hashear la contraseña usando BCrypt
                    string hashedPassword = BCrypt.Net.BCrypt.HashPassword(txtContraseña.Password);
                    //Crear el objeto usuario y guardarlo en la base de datos
                    var usuario = new Models.Usuario
                    {
                        Rut = txtRut.Text.Trim(),
                        Nombre = txtNombre.Text.Trim(),
                        Correo = txtCorreo.Text.Trim(),
                        HashPassword = hashedPassword,
                        Rol = (Rol)cmbRol.SelectedItem!,
                        Activo = true,
                        CreadoEn = DateTime.Now,
                        ActualizadoEn = DateTime.Now
                    };
                    db.Usuarios.Add(usuario);
                    db.SaveChanges();
                }
                MessageBox.Show("Usuario creado exitosamente", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
        $"Error al eliminar el usuario: {ex.Message}\n\nDetalle: {ex.InnerException?.Message}",
        "Error",
        MessageBoxButton.OK,
        MessageBoxImage.Error
    );
            }
        }
        //Evento para cancelar la creacion de un nuevo usuario
        private void BtnCancelar_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
