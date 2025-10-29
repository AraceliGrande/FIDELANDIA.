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

            // Crear ViewModel
            var produccionVM = new ProduccionDatos();
            produccionVM.Secciones.Clear(); // importante si se reutiliza

            foreach (var stock in datos.Secciones)
            {
                var seccionVM = new StockSeccionViewModel
                {
                    NombreTipoPasta = stock.Nombre,
                    CantidadDisponible = stock.CantidadDisponible,
                    Lotes = new ObservableCollection<LoteDetalleViewModel>(
                        stock.Filas.Select(f => new LoteDetalleViewModel
                        {
                            IdLote = int.Parse(f[0]),
                            FechaProduccion = DateTime.Parse(f[1]),
                            FechaVencimiento = DateTime.Parse(f[2]),
                            CantidadDisponible = decimal.Parse(f[3].Replace(" paquetes", "")),
                            Estado = "Disponible"
                        }))
                };

                produccionVM.Secciones.Add(seccionVM);
            }

            // Actualizar indicadores
            produccionVM.TotalTipos = produccionVM.Secciones.Count;
            produccionVM.StockTotal = (int)produccionVM.Secciones.Sum(s => s.CantidadDisponible);
            produccionVM.ProduccionTotal = (int)produccionVM.Secciones.Sum(s => s.Lotes.Sum(l => l.CantidadDisponible));
            produccionVM.VentasDia = 0;

            this.DataContext = produccionVM;
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
            var ventana = new CrearLoteProducFormWindow();
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
                TablaDetalle.DataContext = stock;
            }
        }
    }
}
