using FIDELANDIA.Data;
using FIDELANDIA.Models;
using FIDELANDIA.Services;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;

namespace FIDELANDIA.Views.Produccion
{
    public partial class CrearVentaFormView : UserControl
    {
        private readonly StockService _stockService;
        private readonly VentaService _ventaService;

        private List<TipoPastaResumen> _tiposPasta;
        private Dictionary<int, int> _cantidadesSeleccionadas; // IdTipoPasta -> cantidad
        private ICollectionView _tiposPastaView;

        public CrearVentaFormView()
        {
            InitializeComponent();
            _stockService = new StockService(new FidelandiaDbContext());
            _ventaService = new VentaService(new FidelandiaDbContext());

            _cantidadesSeleccionadas = new Dictionary<int, int>();
            CargarTiposPastaDisponibles();
        }

        private void CargarTiposPastaDisponibles()
        {
            var lotes = _stockService.ObtenerLotesDisponibles();

            _tiposPasta = lotes
                .GroupBy(l => l.IdTipoPasta)
                .Select(g => new TipoPastaResumen
                {
                    IdTipoPasta = g.Key,
                    TipoPasta = g.First().TipoPasta,
                    CantidadDisponible = g.Sum(x => x.CantidadDisponible),
                    Lotes = g.OrderBy(x => x.FechaVencimiento).ToList(),
                    FechaVencimientoMasProxima = g.Min(x => x.FechaVencimiento)
                })
                .ToList();

            foreach (var tipo in _tiposPasta)
                _cantidadesSeleccionadas[tipo.IdTipoPasta] = 0;

            _tiposPastaView = CollectionViewSource.GetDefaultView(_tiposPasta);
            TiposPastaList.ItemsSource = _tiposPastaView;
        }

        private void BuscarTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (_tiposPastaView == null) return;

            _tiposPastaView.Filter = obj =>
            {
                if (obj is TipoPastaResumen t)
                {
                    var filtro = BuscarTextBox.Text?.ToLower() ?? "";
                    return string.IsNullOrEmpty(filtro) ||
                           (t.TipoPasta.Nombre != null && t.TipoPasta.Nombre.ToLower().Contains(filtro));
                }
                return false;
            };
        }

        private void CantidadComboBox_Loaded(object sender, RoutedEventArgs e)
        {
            var combo = sender as ComboBox;
            var item = combo?.DataContext as TipoPastaResumen;
            if (combo == null || item == null) return;

            combo.ItemsSource = Enumerable.Range(0, (int)item.CantidadDisponible + 1).ToList();
            combo.SelectedIndex = _cantidadesSeleccionadas[item.IdTipoPasta]; // Mantener selección previa
        }

        private void CantidadComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var combo = sender as ComboBox;
            var item = combo?.DataContext as TipoPastaResumen;
            var border = FindParent<Border>(combo);

            if (combo != null && item != null && combo.SelectedItem != null)
            {
                _cantidadesSeleccionadas[item.IdTipoPasta] = (int)combo.SelectedItem;

                // Cambiar color si cantidad > 0
                if (border != null)
                {
                    if (_cantidadesSeleccionadas[item.IdTipoPasta] > 0)
                    {
                        // 🔸 Resalta el borde en naranja cuando hay selección
                        border.BorderBrush = new SolidColorBrush(Color.FromRgb(255, 190, 70)); // Naranja suave (#FFA000)
                        border.BorderThickness = new Thickness(2);
                    }
                    else
                    {
                        // 🔹 Restaura el borde original
                        border.BorderBrush = new SolidColorBrush(Color.FromRgb(238, 238, 238)); // #EEE
                        border.BorderThickness = new Thickness(1);
                    }
                }

                ActualizarResumen();
            }
        }

        private void ActualizarResumen()
        {
            ResumenVentaList.Items.Clear();
            foreach (var t in _tiposPasta)
            {
                var cant = _cantidadesSeleccionadas[t.IdTipoPasta];
                if (cant > 0)
                {
                    ResumenVentaList.Items.Add(new
                    {
                        TipoPasta = t.TipoPasta.Nombre,
                        Cantidad = cant
                    });
                }
            }
        }

        private void Cancelar_Click(object sender, RoutedEventArgs e)
        {
            Window.GetWindow(this)?.Close();
        }

        private void RegistrarVenta_Click(object sender, RoutedEventArgs e)
        {
            var seleccion = _tiposPasta
                .Where(t => _cantidadesSeleccionadas[t.IdTipoPasta] > 0)
                .Select(t => (t.IdTipoPasta, (decimal)_cantidadesSeleccionadas[t.IdTipoPasta]))
                .ToList();

            if (!seleccion.Any())
            {
                MessageBox.Show("Seleccione al menos una cantidad para registrar la venta.");
                return;
            }

            // Simulación de guardado
            MessageBox.Show("Venta registrada correctamente (simulación).");
        }

        // Helper para encontrar el Border padre
        public static T FindParent<T>(DependencyObject child) where T : DependencyObject
        {
            DependencyObject parentObject = VisualTreeHelper.GetParent(child);
            if (parentObject == null) return null;

            T parent = parentObject as T;
            if (parent != null)
                return parent;
            else
                return FindParent<T>(parentObject);
        }

        private class TipoPastaResumen
        {
            public int IdTipoPasta { get; set; }
            public TipoPastaModel TipoPasta { get; set; }
            public decimal CantidadDisponible { get; set; }
            public DateTime FechaVencimientoMasProxima { get; set; }
            public List<LoteProduccionModel> Lotes { get; set; }
        }
    }
}
