using FIDELANDIA.Data;
using FIDELANDIA.Models;
using FIDELANDIA.Services;
using FIDELANDIA.Windows;
using System.Windows;
using System.Windows.Controls;

namespace FIDELANDIA.Views
{
    public partial class ProveedoresListView : UserControl
    {
        private readonly ProveedorService _service;

        public ProveedoresListView()
        {
            InitializeComponent();

            var dbContext = new FidelandiaDbContext();
            _service = new ProveedorService(dbContext);

            CargarProveedores();
        }

        public event Action<ProveedorModel> ProveedorSeleccionado;

        private void CargarProveedores()
        {
            var proveedores = _service.ObtenerTodos();
            ProveedoresItemsControl.ItemsSource = proveedores;
        }

        private void BtnNuevoProveedor_Click(object sender, RoutedEventArgs e)
        {
            var ventana = new ProveedoresFormWindow();
            ventana.Owner = Window.GetWindow(this);
            ventana.ShowDialog();

            CargarProveedores();
        }

        // Evento de selección de proveedor
        private void ItemBorder_MouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (sender is Border border && border.DataContext is ProveedorModel proveedor)
            {
                // Llamamos al evento externo
                ProveedorSeleccionado?.Invoke(_service.ObtenerProveedorCompleto(proveedor.ProveedorID));
            }
        }
    }
}


