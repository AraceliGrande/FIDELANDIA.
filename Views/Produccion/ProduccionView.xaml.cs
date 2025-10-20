using FIDELANDIA.Data;
using FIDELANDIA.Services;
using FIDELANDIA.Windows;
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
            this.DataContext = _stockService.ObtenerStocksParaVista();
        }

        private void NuevoLote_Click(object sender, RoutedEventArgs e)
        {
            var ventana = new CrearLoteProducFormWindow();
            ventana.ShowDialog();
            RefrescarTablaStock(); // ✅ actualizar después de crear un lote
        }

        private void NuevaVenta_Click(object sender, RoutedEventArgs e)
        {
            var ventana = new CrearVentaProducFormWindow();
            ventana.ShowDialog();
            RefrescarTablaStock(); // ✅ actualizar después de una venta
        }

        private void BtnNuevoTipoPasta_Click(object sender, RoutedEventArgs e)
        {
            var ventana = new CrearTipoPastaWindow();
            ventana.Owner = Window.GetWindow(this);
            ventana.ShowDialog();
            RefrescarTablaStock(); // ✅ actualizar después de agregar un tipo de pasta
        }
    }

}
