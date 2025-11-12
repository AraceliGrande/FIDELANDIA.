using FIDELANDIA.Data;
using FIDELANDIA.Models;
using FIDELANDIA.Services;
using System;
using System.Windows;
using System.Windows.Controls;
using FIDELANDIA.Helpers;

namespace FIDELANDIA.Views.Proveedores
{
    public partial class TransaccionesFormView : UserControl
    {
        private readonly TransaccionService _service;
        private readonly ProveedorModel _proveedorActual;
        private string? _rutaComprobanteActual;

        public TransaccionesFormView(ProveedorModel proveedor)
        {
            InitializeComponent();
            _proveedorActual = proveedor;

            TxtProveedorNombre.Text = proveedor.Nombre;

            var dbContext = new FidelandiaDbContext();
            _service = new TransaccionService(dbContext);

            CbTipo.ItemsSource = new[] { "Debe", "Haber" };
            CbTipo.SelectedIndex = 0;
        }

        private void BtnAdjuntarComprobante_Click(object sender, RoutedEventArgs e)
        {
            string? ruta = ImagenHelper.GuardarImagenComprobante();

            if (ruta != null)
            {
                _rutaComprobanteActual = ruta;
                TxtRutaComprobante.Text = System.IO.Path.GetFileName(ruta);
            }
        }

        private void Guardar_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_proveedorActual == null)
                {
                    MessageBox.Show("No se ha especificado un proveedor.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                if (string.IsNullOrWhiteSpace(TxtConcepto.Text))
                {
                    MessageBox.Show("Debe ingresar un concepto.", "Advertencia", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (!decimal.TryParse(TxtDebe.Text, out decimal monto) || monto <= 0)
                {
                    MessageBox.Show("Debe ingresar un monto válido mayor que cero.", "Advertencia", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var transaccion = new TransaccionModel
                {
                    ProveedorID = _proveedorActual.ProveedorID,
                    TipoTransaccion = CbTipo.SelectedItem?.ToString(),
                    Monto = monto,
                    Fecha = DpFecha.SelectedDate ?? DateTime.Now,
                    Detalle = TxtConcepto.Text,
                    ComprobanteRuta = _rutaComprobanteActual // ✅ ya es relativa
                };

                _service.CrearTransaccion(transaccion);

                MessageBox.Show("Transacción registrada correctamente.", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);

                AppEvents.OnTransaccionCreada(_proveedorActual.ProveedorID);
                Window.GetWindow(this)?.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al guardar la transacción: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Cancelar_Click(object sender, RoutedEventArgs e)
        {
            Window.GetWindow(this)?.Close();
        }
    }
}
