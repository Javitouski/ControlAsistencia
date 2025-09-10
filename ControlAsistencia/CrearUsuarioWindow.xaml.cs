using ControlAsistencia.Data;
using ControlAsistencia.Models.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
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
            if (!ValidarRut(txtRut.Text))
            {
                lblRutError.Text = "Ingrese un RUT válido.";
                lblRutError.Visibility = Visibility.Visible;
                isValid = false;
            }
            else
            {
                lblRutError.Visibility = Visibility.Collapsed;
            }

            //Validacion de Nombre
            if (string.IsNullOrWhiteSpace(txtNombre.Text))
            {
                lblNombreError.Text = "El nombre es obligatorio.";
                lblNombreError.Visibility = Visibility.Visible;
                isValid = false;
            }
            //Validacion de Correo
            if (!ValidarCorreo(txtCorreo.Text))
            {
                lblCorreoError.Text = "Ingrese un correo válido.";
                lblCorreoError.Visibility = Visibility.Visible;
                isValid = false;
            }
            else
            {
                lblCorreoError.Visibility = Visibility.Collapsed;
            }

            //Validacion de Contrasñea
            if (string.IsNullOrWhiteSpace(txtContraseña.Password) || txtContraseña.Password.Length < 6)
            {
                lblContraseñaError.Text = "Ingrese una contraseña con al menos 6 caracteres.";
                lblContraseñaError.Visibility = Visibility.Visible;
                isValid = false;
            }
            else
            {
                lblContraseñaError.Visibility = Visibility.Collapsed;
            }

            //Validacion de Rol
            if (cmbRol.SelectedItem == null)
            {
                lblRolError.Text = "Seleccion el rol del usuario";
                lblRolError.Visibility = Visibility.Visible;
                isValid = false;
            }
            else
            {
                lblRolError.Visibility = Visibility.Collapsed;
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
        $"Error al crear el usuario: {ex.Message}\n\nDetalle: {ex.InnerException?.Message}",
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

        public static bool ValidarRut(string rut)
        {
            if (string.IsNullOrWhiteSpace(rut))
                return false;

            // Eliminar puntos y guion
            rut = rut.Replace(".", "").Replace("-", "").Trim().ToUpper();

            if (rut.Length < 2)
                return false;

            string cuerpo = rut.Substring(0, rut.Length - 1);
            string dvIngresado = rut.Substring(rut.Length - 1, 1);

            // Validar que el cuerpo sea numérico
            if (!int.TryParse(cuerpo, out int rutNumerico))
                return false;

            // Calcular DV con algoritmo Módulo 11
            int suma = 0;
            int multiplicador = 2;

            for (int i = cuerpo.Length - 1; i >= 0; i--)
            {
                suma += int.Parse(cuerpo[i].ToString()) * multiplicador;
                multiplicador++;
                if (multiplicador > 7) multiplicador = 2;
            }

            int resto = 11 - (suma % 11);
            string dvCalculado;
            if (resto == 11) dvCalculado = "0";
            else if (resto == 10) dvCalculado = "K";
            else dvCalculado = resto.ToString();

            return dvIngresado == dvCalculado;
        }
    


public static bool ValidarCorreo(string correo)
        {
            if (string.IsNullOrWhiteSpace(correo))
                return false;

            try
            {
                var mail = new MailAddress(correo);
                return true;
            }
            catch
            {
                return false;
            }
        }
        public static string FormatearRut(string rut)
        {
            if (string.IsNullOrWhiteSpace(rut))
                return string.Empty;

            // Quitar puntos y guion
            rut = rut.Replace(".", "").Replace("-", "").Trim().ToUpper();
            if (rut.Length < 2)
                return rut;

            string cuerpo = rut.Substring(0, rut.Length - 1);
            string dv = rut.Substring(rut.Length - 1);

            // Insertar puntos de miles
            cuerpo = Convert.ToInt64(cuerpo).ToString("N0"); // Formato con separador de miles
            cuerpo = cuerpo.Replace(",", "."); // Asegurar que use puntos

            return $"{cuerpo}-{dv}";
        }

        private void txtRut_TextChanged(object sender, TextChangedEventArgs e)
        {
            int pos = txtRut.SelectionStart; // Guardar posición del cursor
            string rutFormateado = FormatearRut(txtRut.Text);
            txtRut.Text = rutFormateado;
            txtRut.SelectionStart = txtRut.Text.Length; // Llevar el cursor al final
        }
    }

}
