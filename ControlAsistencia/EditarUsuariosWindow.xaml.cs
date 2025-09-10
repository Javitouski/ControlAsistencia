using ControlAsistencia.Models.Enums;
using DocumentFormat.OpenXml.Office2010.Excel;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net.Mail;
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
    /// Lógica de interacción para EditarUsuariosWindow.xaml
    /// </summary>
    public partial class EditarUsuariosWindow : Window
    {
        private int usuarioId;
        public EditarUsuariosWindow(int id)
        {
            InitializeComponent();
            usuarioId = id;
            CargarUsuario();
        }

        private void CargarUsuario()
        {
            using (var db = new Data.AsistenciaDbContext())
            {
                var usuario = db.Usuarios.FirstOrDefault(u => u.Id == usuarioId);
                if (usuario != null)
                {
                    txtRut.Text = usuario.Rut;
                    txtNombre.Text = usuario.Nombre;
                    txtCorreo.Text = usuario.Correo;
                    cmbRol.ItemsSource = Enum.GetValues(typeof(Rol));
                    cmbRol.SelectedItem = usuario.Rol;
                    chkActivo.IsChecked = usuario.Activo;
                }
            }
        }

        private void BtnGuardar_Click(object sender, RoutedEventArgs e)
        {
            bool isValid = true;

            // Ocultar mensajes de error
            lblNombreError.Visibility = Visibility.Collapsed;
            lblCorreoError.Visibility = Visibility.Collapsed;
            lblContraseñaError.Visibility = Visibility.Collapsed;
            lblRolError.Visibility = Visibility.Collapsed;

            // Validar Nombre
            if (string.IsNullOrWhiteSpace(txtNombre.Text))
            {
                lblNombreError.Text = "El nombre es obligatorio.";
                lblNombreError.Visibility = Visibility.Visible;
                isValid = false;
            }

            // Validar Correo
            if (!Validador.ValidarCorreo(txtCorreo.Text))
            {
                lblCorreoError.Text = "Ingrese un correo válido.";
                lblCorreoError.Visibility = Visibility.Visible;
                isValid = false;
            }

            // Validar Rol
            if (cmbRol.SelectedItem == null)
            {
                lblRolError.Text = "Seleccione un rol.";
                lblRolError.Visibility = Visibility.Visible;
                isValid = false;
            }

            // Validar contraseña (opcional)
            if (!string.IsNullOrWhiteSpace(txtContraseña.Password) && txtContraseña.Password.Length < 6)
            {
                lblContraseñaError.Text = "La contraseña debe tener al menos 6 caracteres.";
                lblContraseñaError.Visibility = Visibility.Visible;
                isValid = false;
            }

            if (!isValid)
            {
                MessageBox.Show("Por favor, corrija los errores antes de guardar.", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Guardar cambios
            using (var db = new Data.AsistenciaDbContext())
            {
                var usuario = db.Usuarios.FirstOrDefault(u => u.Id == usuarioId);
                if (usuario != null)
                {
                    usuario.Nombre = txtNombre.Text.Trim();
                    usuario.Correo = txtCorreo.Text.Trim();
                    usuario.Rol = (Rol)cmbRol.SelectedItem;
                    usuario.Activo = chkActivo.IsChecked ?? true;
                    usuario.ActualizadoEn = DateTime.Now;

                    // Actualizar contraseña solo si el usuario escribió algo
                    if (!string.IsNullOrWhiteSpace(txtContraseña.Password))
                    {
                        usuario.HashPassword = BCrypt.Net.BCrypt.HashPassword(txtContraseña.Password);
                    }

                    db.SaveChanges();
                }
            }

            MessageBox.Show("Usuario actualizado correctamente", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
            DialogResult = true;
            Close();
        }

        private void BtnCancelar_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        public static class Validador
        {
            // Valida que el correo tenga un formato correcto
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

            // Valida RUT chileno (opcional, si en edición quieres validar)
            public static bool ValidarRut(string rut)
            {
                if (string.IsNullOrWhiteSpace(rut))
                    return false;

                rut = rut.Replace(".", "").Replace("-", "").Trim().ToUpper();
                if (rut.Length < 2) return false;

                string cuerpo = rut[..^1];
                string dvIngresado = rut[^1].ToString();

                if (!int.TryParse(cuerpo, out _)) return false;

                int suma = 0, multiplicador = 2;
                for (int i = cuerpo.Length - 1; i >= 0; i--)
                {
                    suma += (cuerpo[i] - '0') * multiplicador;
                    multiplicador = (multiplicador == 7) ? 2 : multiplicador + 1;
                }

                int resto = 11 - (suma % 11);
                string dvCalculado = resto switch
                {
                    11 => "0",
                    10 => "K",
                    _ => resto.ToString()
                };

                return dvIngresado == dvCalculado;
            }

            // Se pueden agregar más validaciones aquí: nombre, contraseña, etc.
        }
    }
}

