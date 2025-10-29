using FIDELANDIA.ViewModels;
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

namespace FIDELANDIA.Views.Home
{
    /// <summary>
    /// Lógica de interacción para HomeView.xaml
    /// </summary>
    public partial class HomeView : UserControl
    {
        public HomeView()
        {
            InitializeComponent();
        }
        private void Border_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (DataContext is MainViewModel vm)
            {
                vm.NavigateCommand.Execute("Proveedores");
            }
        }

        private void ProduccionBorder_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var ventana = new CrearLoteProducFormWindow();
            ventana.Owner = Window.GetWindow(this);
            ventana.ShowDialog();
            if (DataContext is MainViewModel vm)
                vm.NavigateCommand.Execute("Produccion");
        }

        private void VentaBorder_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var ventana = new CrearVentaProducFormWindow();
            ventana.Owner = Window.GetWindow(this);
            ventana.ShowDialog();
            if (DataContext is MainViewModel vm)
                vm.NavigateCommand.Execute("Produccion");
        }

        private void DashboardBorder_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (DataContext is MainViewModel vm)
                vm.NavigateCommand.Execute("Dashboard");
        }
    }
}
