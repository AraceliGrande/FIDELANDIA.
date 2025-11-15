using FIDELANDIA.Models;
using System;
using System.Windows;
using System.Windows.Controls;

namespace FIDELANDIA.Views.Produccion
{
    public partial class EliminarDetalleDeVentaView : UserControl
    {
        public EliminarDetalleDeVentaView()
        {
            InitializeComponent();
        }

        // Propiedad para enlazar el detalle desde la ventana
        public DetalleVentaModel Detalle
        {
            get => (DetalleVentaModel)DataContext;
            set => DataContext = value;
        }

        // Evento que avisa a la ventana que se pidió eliminar (la ventana maneja la acción)
        public event EventHandler<DetalleVentaModel> EliminarRequested;

        // Evento que avisa a la ventana que se canceló
        public event EventHandler CancelRequested;

        private void BtnEliminar_Click(object sender, RoutedEventArgs e)
        {
            // Disparar evento para que la ventana realice la eliminación
            EliminarRequested?.Invoke(this, Detalle);
        }

        private void BtnCancelar_Click(object sender, RoutedEventArgs e)
        {
            CancelRequested?.Invoke(this, EventArgs.Empty);
        }
    }
}
