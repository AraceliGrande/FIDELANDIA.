using System;
using System.Windows;
using System.Windows.Controls;
using FIDELANDIA.Services;
using FIDELANDIA.Data;

namespace FIDELANDIA.Views
{
    public partial class ProveedoresFormView : UserControl
    {
        private readonly ProveedorService _service;

        public ProveedoresFormView()
        {
            InitializeComponent();

            // Inicializamos el service con el DbContext
            var dbContext = new FidelandiaDbContext();
            _service = new ProveedorService(dbContext);
        }

        private void Cancelar_Click(object sender, RoutedEventArgs e)
        {
            Window parentWindow = Window.GetWindow(this);
            if (parentWindow != null)
                parentWindow.Close();
        }

        private void Guardar_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Crear proveedor
                bool resultado = _service.CrearProveedor(
                    TxtNombre.Text,
                    TxtCuit.Text,
                    TxtDireccion.Text,
                    TxtTelefono.Text,
                    TxtEmail.Text
                );

                if (resultado)
                {
                    // Mensaje de éxito
                    MessageBox.Show("Proveedor creado correctamente.", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);

                    Window parentWindow = Window.GetWindow(this);
                    if (parentWindow != null)
                        parentWindow.Close();
                }
                else
                {
                    // Mensaje de error genérico si el service retorna false
                    MessageBox.Show("No se pudo crear el proveedor.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                // Mensaje de error con excepción
                MessageBox.Show($"Ocurrió un error al crear el proveedor:\n{ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
