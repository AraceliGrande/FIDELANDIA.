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

            var categorias = _service.ObtenerCategorias();
            CmbCategoria.ItemsSource = categorias;
        }

        private void Cancelar_Click(object sender, RoutedEventArgs e)
        {
            Window parentWindow = Window.GetWindow(this);
            if (parentWindow != null)
                parentWindow.Close();
        }

        private bool ValidarCampos(out decimal limiteCredito)
        {
            limiteCredito = 0;

            if (string.IsNullOrWhiteSpace(TxtNombre.Text))
            {
                MessageBox.Show("El nombre del proveedor es obligatorio.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (string.IsNullOrWhiteSpace(TxtCuit.Text))
            {
                MessageBox.Show("El CUIT es obligatorio.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (CmbCategoria.SelectedValue == null || (int)CmbCategoria.SelectedValue == 0)
            {
                MessageBox.Show("Debe seleccionar una categoría.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            // LimiteCredito opcional, pero si ingresaron algo debe ser válido
            if (!string.IsNullOrWhiteSpace(TxtLimiteCredito.Text))
            {
                // Usamos InvariantCulture para que tome punto como separador decimal
                if (!decimal.TryParse(TxtLimiteCredito.Text.Replace(',', '.'),
                                      System.Globalization.NumberStyles.Any,
                                      System.Globalization.CultureInfo.InvariantCulture,
                                      out limiteCredito))
                {
                    MessageBox.Show("El límite de crédito no es un número válido. Use punto (.) como separador decimal.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return false;
                }
            }

            return true;
        }

        private void Guardar_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidarCampos(out decimal limite))
                return;

            try
            {
                bool resultado = _service.CrearProveedor(
                    TxtNombre.Text,
                    TxtCuit.Text,
                    TxtDireccion.Text,
                    TxtTelefono.Text,
                    TxtEmail.Text,
                    (int)CmbCategoria.SelectedValue,
                    limite,
                    ChkActivo.IsChecked ?? true
                );

                if (resultado)
                {
                    MessageBox.Show("Proveedor creado correctamente.", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
                    Window.GetWindow(this)?.Close();
                }
                else
                {
                    MessageBox.Show("No se pudo crear el proveedor.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ocurrió un error al crear el proveedor:\n{ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

    }
}
