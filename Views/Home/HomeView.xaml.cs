using FIDELANDIA.Data;
using FIDELANDIA.Services;
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
        private readonly DatabaseSizeService _dbService;

        public HomeView()
        {
            InitializeComponent();

            var dbContext = new FidelandiaDbContext();
            _dbService = new DatabaseSizeService(dbContext);

            // Ejecutar cuando se cargue la página
            Loaded += HomeView_Loaded;
        }

        private async void HomeView_Loaded(object sender, RoutedEventArgs e)
        {
            await CargarTamanioBaseAsync();
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
                vm.NavigateCommand.Execute("Dashboard_Produccion");
        }
        private const double LimiteBaseGB = 9.0; // Límite de tu base de datos en GB

        private async Task CargarTamanioBaseAsync()
        {
            try
            {
                // Obtener tamaño actual en MB
                double tamanioMB = await _dbService.ObtenerTamanioMB();

                // Convertir a GB
                double tamanioGB = tamanioMB / 1024.0;

                // Calcular espacio disponible
                double espacioDisponible = LimiteBaseGB - tamanioGB;
                if (espacioDisponible < 0) espacioDisponible = 0;

                // Mostrar mensaje
                DbSizeTextBlock.Text = $"Espacio usado: {tamanioGB:F2} GB | Espacio disponible: {espacioDisponible:F2} GB";
            }
            catch (Exception ex)
            {
                DbSizeTextBlock.Text = $"Error al obtener tamaño: {ex.Message}";
            }
        }

    }
}
