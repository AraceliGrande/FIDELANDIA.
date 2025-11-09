using FIDELANDIA.Helpers;
using FIDELANDIA.Models;
using FIDELANDIA.Services;
using System;
using System.Threading.Tasks;
using System.Windows;

namespace FIDELANDIA.Windows
{
    public partial class EliminarDetalleVentaWindow : Window
    {
        private readonly VentaService _ventaService;
        private readonly StockService _stockService; // si necesitas inyectar o usar
        private DetalleVentaModel _detalle;

        public EliminarDetalleVentaWindow(DetalleVentaModel detalle, VentaService ventaService, StockService stockService)
        {
            InitializeComponent();

            _detalle = detalle ?? throw new ArgumentNullException(nameof(detalle));
            _ventaService = ventaService ?? throw new ArgumentNullException(nameof(ventaService));
            _stockService = stockService; // opcional

            // inicializamos el UserControl con el detalle
            EliminarView.Detalle = _detalle;

            // suscribimos eventos
            EliminarView.EliminarRequested += EliminarView_EliminarRequested;
            EliminarView.CancelRequested += EliminarView_CancelRequested;
        }

        private void EliminarView_CancelRequested(object sender, EventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }

        private async void EliminarView_EliminarRequested(object sender, DetalleVentaModel detalle)
        {
            var confirm = MessageBox.Show(
                "¿Estás seguro? Esto sólo debe usarse para corregir errores. Se volverá a sumar la cantidad al stock.",
                "Confirmar eliminación",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning
            );

            if (confirm != MessageBoxResult.Yes) return;

            try
            {
                // Llama al servicio para eliminar el detalle (método que definiste previamente)
                bool ok = await _ventaService.EliminarDetalleVentaAsync(detalle.IdVenta, detalle.IdLote);

                if (ok)
                {
                    MessageBox.Show("Detalle eliminado correctamente. Stock actualizado.", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
                    this.DialogResult = true;

                    this.Close();
                }
                else
                {
                    MessageBox.Show("No se pudo eliminar el detalle de venta.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al eliminar detalle: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
