using FIDELANDIA.Data;
using FIDELANDIA.Helpers;
using FIDELANDIA.Services;
using FIDELANDIA.ViewModels;
using FIDELANDIA.Windows;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace FIDELANDIA.Views.Produccion
{
    public partial class ProduccionView : UserControl
    {
        private readonly StockService _stockService;

        public ProduccionView()
        {
            InitializeComponent();
            _stockService = new StockService(new FidelandiaDbContext());
            RefrescarTablaStock();
        }

        private void RefrescarTablaStock()
        {
            var datos = _stockService.ObtenerStocksParaVista();
            if (datos == null)
            {
                this.DataContext = null;
                return;
            }

            this.DataContext = datos;

        }


        private void NuevoLote_Click(object sender, RoutedEventArgs e)
        {
            var ventana = new CrearLoteProducFormWindow();
            ventana.Owner = Window.GetWindow(this);
            ventana.ShowDialog();
        }

        private void NuevaVenta_Click(object sender, RoutedEventArgs e)
        {
            var ventana = new CrearVentaProducFormWindow();
            ventana.Owner = Window.GetWindow(this);
            ventana.ShowDialog();
        }

        private void BtnNuevoTipoPasta_Click(object sender, RoutedEventArgs e)
        {
            var ventana = new CrearTipoPastaWindow();
            ventana.Owner = Window.GetWindow(this);
            ventana.ShowDialog();
        }


        private void GenerarBalanceDiario_Click(object sender, RoutedEventArgs e)
        {
            var ventana = new BalanceDiarioWindow();
            ventana.Owner = Window.GetWindow(this);
            ventana.ShowDialog();
        }


        private void RegistrarDefectos_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (DataContext is ProduccionDatos datos)
                {
                    // 🔹 Buscar todos los lotes que tengan cantidad defectuosa
                    var lotesConDefectos = datos.Secciones
                        .SelectMany(s => s.Lotes)
                        .Where(l => l.CantidadDefectuosa > 0)
                        .ToList();

                    if (!lotesConDefectos.Any())
                    {
                        MessageBox.Show("No se registraron defectos en ningún lote.",
                            "Información", MessageBoxButton.OK, MessageBoxImage.Information);
                        return;
                    }

                    // 🔹 Validar cantidades antes de ejecutar cualquier registro
                    var loteConError = lotesConDefectos
                        .FirstOrDefault(l => l.CantidadDefectuosa > l.CantidadDisponible);

                    if (loteConError != null)
                    {
                        MessageBox.Show(
                            $"La cantidad defectuosa del lote {loteConError.IdLote} ({loteConError.CantidadDefectuosa}) " +
                            $"no puede ser mayor a la cantidad disponible ({loteConError.CantidadDisponible}). " +
                            "Corrija los valores y vuelva a intentarlo.",
                            "Error", MessageBoxButton.OK, MessageBoxImage.Error
                        );
                        return; // ❌ Cancelar toda la operación
                    }

                    var dbContext = new FidelandiaDbContext();
                    var loteService = new LoteProduccionService(dbContext);

                    // 🔹 Registrar defectos
                    foreach (var lote in lotesConDefectos)
                    {
                        loteService.RegistrarDefectos(
                            idLote: lote.IdLote,
                            cantidadDefectuosa: lote.CantidadDefectuosa
                        );
                    }

                    MessageBox.Show("Los defectos se registraron correctamente.",
                        "Confirmación", MessageBoxButton.OK, MessageBoxImage.Information);

                    // 🔹 Refrescar los datos
                    AppEvents.NotificarLoteCreado();
                }
                else
                {
                    MessageBox.Show("No se encontraron datos de producción.",
                        "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al registrar defectos: {ex.Message}",
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


        private void TipoPasta_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.DataContext is StockSeccionViewModel stock)
            {
                TablaDetalle.DataContext = null;
                TablaDetalle.DataContext = stock; 
            }
        }

    }
}
