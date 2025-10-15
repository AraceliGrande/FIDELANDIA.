using FIDELANDIA.Data;
using FIDELANDIA.Models;
using FIDELANDIA.Services;
using FIDELANDIA.Windows;
using System;
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

            try
            {
                var dbContext = new FidelandiaDbContext();
                _service = new ProveedorService(dbContext);

                CargarProveedores();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al inicializar la vista de proveedores: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public event Action<ProveedorModel> ProveedorSeleccionado;

        private void CargarProveedores()
        {
            try
            {
                var proveedores = _service.ObtenerTodos();
                ProveedoresItemsControl.ItemsSource = proveedores;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar los proveedores: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnNuevoProveedor_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var ventana = new ProveedoresFormWindow();
                ventana.Owner = Window.GetWindow(this);
                ventana.ShowDialog();

                CargarProveedores();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al abrir la ventana de nuevo proveedor: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Evento de selección de proveedor
        private void ItemBorder_MouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            try
            {
                if (sender is Border border && border.DataContext is ProveedorModel proveedor)
                {
                    // Llamamos al evento externo
                    var proveedorCompleto = _service.ObtenerProveedorCompleto(proveedor.ProveedorID);
                    if (proveedorCompleto != null)
                        ProveedorSeleccionado?.Invoke(proveedorCompleto);
                    else
                        MessageBox.Show("No se pudo obtener el proveedor completo.", "Atención", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al seleccionar el proveedor: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
