using FIDELANDIA.Data;
using FIDELANDIA.Models;
using FIDELANDIA.Services;
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
using System.Windows.Navigation;
using System.Windows.Shapes;
namespace FIDELANDIA.Views.Proveedores
{

    public partial class TransaccionesFormView : UserControl
    {
        private readonly TransaccionService _service;
        private readonly ProveedorModel _proveedorActual;

        public TransaccionesFormView(ProveedorModel proveedor)
        {
            InitializeComponent();
            _proveedorActual = proveedor;

            // Mostrar nombre del proveedor en el título
            TxtProveedorNombre.Text = proveedor.Nombre;

            // Instanciar el servicio
            var dbContext = new FidelandiaDbContext();
            _service = new TransaccionService(dbContext);

            // Cargar tipos de transacción
            CbTipo.ItemsSource = new[] { "Debe", "Haber" };
            CbTipo.SelectedIndex = 0;
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

                // Validaciones básicas
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

                // Crear nueva transacción
                var transaccion = new TransaccionModel
                {
                    ProveedorID = _proveedorActual.ProveedorID,
                    TipoTransaccion = CbTipo.SelectedItem?.ToString(),
                    Monto = monto,
                    Fecha = DpFecha.SelectedDate ?? DateTime.Now,
                    Detalle = TxtConcepto.Text
                };

                // Guardar en la base de datos
                _service.CrearTransaccion(transaccion);

                MessageBox.Show("Transacción registrada correctamente.", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);

                // Limpiar campos
                TxtConcepto.Clear();
                TxtDebe.Clear();
                TxtFactura.Clear();
                CbTipo.SelectedIndex = 0;
                DpFecha.SelectedDate = DateTime.Now;

                Window.GetWindow(this)?.Close();

            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al guardar la transacción: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Cancelar_Click(object sender, RoutedEventArgs e)
        {
            // Si esto está dentro de un diálogo, lo podés cerrar acá
            var window = Window.GetWindow(this);
            window?.Close();
        }
    }
}