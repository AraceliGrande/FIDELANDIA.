using DocumentFormat.OpenXml.Drawing;
using FIDELANDIA.Data;
using FIDELANDIA.Helpers;
using FIDELANDIA.Models;
using FIDELANDIA.Services;
using FIDELANDIA.Windows;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;

namespace FIDELANDIA.Views.Produccion
{
    public partial class CrearProduccionFormView : UserControl
    {
        private readonly FidelandiaDbContext _dbContext;
        private readonly TipoPastaService _tipoPastaService;
        private readonly LoteProduccionService _loteService;

        public CrearProduccionFormView()
        {
            InitializeComponent();

            _dbContext = new FidelandiaDbContext();
            _tipoPastaService = new TipoPastaService(_dbContext);
            _loteService = new LoteProduccionService(_dbContext);

            CargarTiposPasta();
            DpFechaProduccion.SelectedDate = DateTime.Now;
        }

        private void CargarTiposPasta()
        {
            var tipos = _tipoPastaService.ObtenerTodos();
            CardTipoPasta.ItemsSource = tipos;
        }
        private void CardTipoPasta_Checked(object sender, RoutedEventArgs e)
        {
            if (sender is ToggleButton selectedButton)
            {
                foreach (var item in CardTipoPasta.Items)
                {
                    var container = (FrameworkElement)CardTipoPasta.ItemContainerGenerator.ContainerFromItem(item);
                    if (container != null)
                    {
                        var toggleButton = FindVisualChild<ToggleButton>(container);
                        if (toggleButton != null && toggleButton != selectedButton)
                            toggleButton.IsChecked = false;
                    }
                }

                // Actualizar resumen
                if (selectedButton.DataContext != null)
                {
                    var tipoPasta = selectedButton.DataContext;
                    var nombreProp = tipoPasta.GetType().GetProperty("Nombre");
                    if (nombreProp != null)
                        LblTipoPastaSeleccionado.Text = nombreProp.GetValue(tipoPasta)?.ToString() ?? "-";
                }
            }
        }

        private void CardTipoPasta_Unchecked(object sender, RoutedEventArgs e)
        {
            // Si no hay cards seleccionadas
            if (CardTipoPasta.Items.Cast<object>().All(item =>
            {
                var container = (FrameworkElement)CardTipoPasta.ItemContainerGenerator.ContainerFromItem(item);
                var toggleButton = container != null ? FindVisualChild<ToggleButton>(container) : null;
                return toggleButton == null || toggleButton.IsChecked == false;
            }))
            {
                LblTipoPastaSeleccionado.Text = "-";
                PrevisualizarQR();
            }
        }

        // Actualizar cantidad en el resumen
        private void TxtCantidad_TextChanged(object sender, TextChangedEventArgs e)
        {
            LblCantidad.Text = TxtCantidad.Text;
            PrevisualizarQR();
        }

        // Actualizar fechas en el resumen
        private void DpFechaProduccion_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            LblFechaProduccion.Text = DpFechaProduccion.SelectedDate?.ToShortDateString() ?? "-";
            PrevisualizarQR();
        }

        private void DpFechaVencimiento_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            LblFechaVencimiento.Text = DpFechaVencimiento.SelectedDate?.ToShortDateString() ?? "-";
            PrevisualizarQR();
        }

        // Función auxiliar para buscar hijos en el visual tree
        private T FindVisualChild<T>(DependencyObject parent) where T : DependencyObject
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                if (child is T t) return t;
                var result = FindVisualChild<T>(child);
                if (result != null) return result;
            }
            return null;
        }

        private void Cancelar_Click(object sender, RoutedEventArgs e)
        {
            Window.GetWindow(this)?.Close();
        }

        private void Guardar_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Buscar la card seleccionada por el DataContext
                var selectedCard = CardTipoPasta.Items
                    .Cast<object>()
                    .FirstOrDefault(item =>
                    {
                        var container = CardTipoPasta.ItemContainerGenerator.ContainerFromItem(item) as ContentPresenter;
                        var toggleButton = FindVisualChild<ToggleButton>(container);
                        return toggleButton != null && toggleButton.IsChecked == true;
                    });

                if (selectedCard == null)
                {
                    MessageBox.Show(
                        "Debe seleccionar un tipo de pasta.",
                        "Atención",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning
                    );
                    return;
                }

                // Obtener IdTipoPasta desde el DataContext
                int idTipoPasta = (int)selectedCard.GetType().GetProperty("IdTipoPasta").GetValue(selectedCard);

                // Validar cantidad
                if (!decimal.TryParse(TxtCantidad.Text, out decimal cantidad) || cantidad <= 0)
                {
                    MessageBox.Show(
                        "Ingrese una cantidad válida mayor a 0.",
                        "Atención",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning
                    );
                    return;
                }

                if (DpFechaProduccion.SelectedDate == null)
                {
                    MessageBox.Show(
                        "Seleccione la fecha de producción.",
                        "Atención",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning
                    );
                    return;
                }

                if (DpFechaVencimiento.SelectedDate == null)
                {
                    MessageBox.Show(
                        "Seleccione la fecha de vencimiento.",
                        "Atención",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning
                    );
                    return;
                }

                DateTime fechaProduccion = DpFechaProduccion.SelectedDate.Value;
                DateTime fechaVencimiento = DpFechaVencimiento.SelectedDate.Value;
                string estado = "Creado";

                var loteCreado = _loteService.CrearLote(idTipoPasta, cantidad, fechaProduccion, fechaVencimiento, estado);
                if (loteCreado != null)
                {
                    // Creamos una lista con tantos elementos como cantidad producida
                    List<LoteProduccionModel> lotesAImprimir = new List<LoteProduccionModel>();
                    for (int i = 0; i < (int)loteCreado.CantidadProducida; i++)
                    {
                        lotesAImprimir.Add(loteCreado);
                    }

                    // Llamamos al helper que imprime todos los QR
                    ImpresionHelper.ImprimirQRs(lotesAImprimir);

                    MessageBox.Show(
                        "Lote de producción creado correctamente.",
                        "Éxito",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information
                    );
                    AppEvents.NotificarLoteCreado();
                    Window.GetWindow(this)?.Close();
                }


            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Ocurrió un error al crear el lote de producción: {ex.Message}",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                );
            }
        }
      
        private void PrevisualizarQR()
        {
            if (
                decimal.TryParse(TxtCantidad.Text, out decimal cantidad) &&
                DpFechaProduccion.SelectedDate.HasValue &&
                DpFechaVencimiento.SelectedDate.HasValue)

            {
                var selectedCard = CardTipoPasta.Items
                    .Cast<object>()
                    .FirstOrDefault(item =>
                    {
                        var container = CardTipoPasta.ItemContainerGenerator.ContainerFromItem(item) as ContentPresenter;
                        var toggleButton = FindVisualChild<ToggleButton>(container);
                        return toggleButton != null && toggleButton.IsChecked == true;
                    });

                if (selectedCard != null)
                {
                    // Obtenemos el objeto TipoPastaModel desde el DataContext del selectedCard
                    var tipoPasta = selectedCard;
                    string nombreTipoPasta = tipoPasta.GetType().GetProperty("Nombre")?.GetValue(tipoPasta)?.ToString() ?? "-";

                    string textoQr = $"Fidelandia - Pastas Frescas\n\n" +
                                     $"Tipo de pasta: {nombreTipoPasta}\n" +
                                     $"Cantidad producida: {cantidad}\n" +
                                     $"Fecha de produccion: {DpFechaProduccion.SelectedDate.Value:dd/MM/yyyy}\n" +
                                     $"Fecha de vencimiento: {DpFechaVencimiento.SelectedDate.Value:dd/MM/yyyy}\n";

                    QrImage.Source = QrHelper.GenerarQr(textoQr);
                }
            }
        }

    }
}
