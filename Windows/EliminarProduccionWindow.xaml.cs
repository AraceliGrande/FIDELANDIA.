using FIDELANDIA.Models;
using FIDELANDIA.Services;
using FIDELANDIA.Views.Produccion;
using System;
using System.Threading.Tasks;
using System.Windows;

namespace FIDELANDIA.Windows
{
    public partial class EliminarProduccionWindow : Window
    {
        private readonly LoteProduccionService _loteService;
        private readonly StockService _stockService;
        private readonly VentaService _ventaService;
        private readonly LoteDetalleViewModel1 _produccion;

        public EliminarProduccionWindow(LoteDetalleViewModel1 produccion,
                                        LoteProduccionService loteService,
                                        StockService stockService,
                                        VentaService ventaService)
        {
            InitializeComponent();

            _produccion = produccion ?? throw new ArgumentNullException(nameof(produccion));
            _loteService = loteService ?? throw new ArgumentNullException(nameof(loteService));
            _stockService = stockService;
            _ventaService = ventaService;

            EliminarProduccionView.Produccion = _produccion;

            EliminarProduccionView.EliminarRequested += EliminarView_EliminarRequested;
            EliminarProduccionView.CancelRequested += EliminarView_CancelRequested;
        }

        private void EliminarView_CancelRequested(object sender, EventArgs e)
        {
            this.DialogResult = false;
            Close();
        }

        private async void EliminarView_EliminarRequested(object sender, LoteDetalleViewModel1 produccion)
        {
            var confirm = MessageBox.Show(
                "¿Seguro que deseas eliminar esta producción?\nSe ajustará el stock y se eliminarán las ventas asociadas.",
                "Confirmar eliminación",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning
            );

            if (confirm != MessageBoxResult.Yes)
                return;

            try
            {
                // Llamamos al método correcto
                var (ok, msg) = await _loteService.EliminarLoteAsync(produccion.IdLote);

                MessageBox.Show(msg,
                    ok ? "Éxito" : "Error",
                    MessageBoxButton.OK,
                    ok ? MessageBoxImage.Information : MessageBoxImage.Error);

                if (ok)
                {
                    this.DialogResult = true;
                    this.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al eliminar producción: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
