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

            // Directamente asignamos el ViewModel completo que devuelve el servicio
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
