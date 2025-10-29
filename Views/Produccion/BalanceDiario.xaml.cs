using FIDELANDIA.Models;
using LiveCharts;
using LiveCharts.Wpf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace FIDELANDIA.Views.Produccion
{
    public partial class BalanceDiario : UserControl
    {
        public List<string> TipoPastaLabels { get; set; }
        public Func<double, string> FormatoCantidad { get; set; }
        public List<LoteDetalleViewModel1> LotesDetalle { get; set; }
        public List<LoteDetalleViewModel1> LotesDetallePorTipo { get; set; }

        public string SelectedTipoPasta { get; set; }
        public List<LoteDetalleViewModel1> LotesFiltrados { get; set; }

        private List<DetalleVentaModel> ventasGlobal;

        public BalanceDiario()
        {
            InitializeComponent();
            CargarDatosMockup();
            DataContext = this;
        }

        private void CargarDatosMockup()
        {
            var tiposPasta = new List<TipoPastaModel>
            {
                new TipoPastaModel { Nombre = "Fettuccine" },
                new TipoPastaModel { Nombre = "Spaghetti" },
                new TipoPastaModel { Nombre = "Penne" },
                new TipoPastaModel { Nombre = "Ravioli" }
            };

            var lotes = new List<LoteProduccionModel>
            {
                new LoteProduccionModel { IdLote=1, TipoPasta = tiposPasta[0], CantidadDisponible = 100, FechaProduccion = DateTime.Now.AddDays(-1), FechaVencimiento = DateTime.Now.AddMonths(1), Estado="OK" },
                new LoteProduccionModel { IdLote=2, TipoPasta = tiposPasta[0], CantidadDisponible = 50, FechaProduccion = DateTime.Now.AddDays(-2), FechaVencimiento = DateTime.Now.AddMonths(1), Estado="OK" },
                new LoteProduccionModel { IdLote=3, TipoPasta = tiposPasta[1], CantidadDisponible = 150, FechaProduccion = DateTime.Now.AddDays(-3), FechaVencimiento = DateTime.Now.AddMonths(1), Estado="OK" },
                new LoteProduccionModel { IdLote=4, TipoPasta = tiposPasta[2], CantidadDisponible = 120, FechaProduccion = DateTime.Now.AddDays(-1), FechaVencimiento = DateTime.Now.AddMonths(1), Estado="OK" },
                new LoteProduccionModel { IdLote=5, TipoPasta = tiposPasta[3], CantidadDisponible = 80, FechaProduccion = DateTime.Now.AddDays(-1), FechaVencimiento = DateTime.Now.AddMonths(1), Estado="OK" }
            };

            ventasGlobal = new List<DetalleVentaModel>
            {
                new DetalleVentaModel { Lote = lotes[0], Cantidad = 60 },
                new DetalleVentaModel { Lote = lotes[1], Cantidad = 20 },
                new DetalleVentaModel { Lote = lotes[2], Cantidad = 100 },
                new DetalleVentaModel { Lote = lotes[3], Cantidad = 30 },
                new DetalleVentaModel { Lote = lotes[0], Cantidad = 10 }
            };

            var stockActual = new List<StockActualModel>
            {
                new StockActualModel { TipoPasta = tiposPasta[0], CantidadDisponible = 70 },
                new StockActualModel { TipoPasta = tiposPasta[1], CantidadDisponible = 50 },
                new StockActualModel { TipoPasta = tiposPasta[2], CantidadDisponible = 70 },
                new StockActualModel { TipoPasta = tiposPasta[3], CantidadDisponible = 50 }
            };

            TipoPastaLabels = tiposPasta.Select(t => t.Nombre).ToList();

            // Gráfico
            var produccionPorTipo = tiposPasta.Select(t => (double)lotes.Where(l => l.TipoPasta.Nombre == t.Nombre).Sum(l => l.CantidadDisponible)).ToList();
            var ventasPorTipo = tiposPasta.Select(t => (double)ventasGlobal.Where(v => v.Lote.TipoPasta.Nombre == t.Nombre).Sum(v => v.Cantidad)).ToList();
            var stockPorTipo = tiposPasta.Select(t => (double)stockActual.Where(s => s.TipoPasta.Nombre == t.Nombre).Sum(s => s.CantidadDisponible)).ToList();

            MyChart.Series = new SeriesCollection
            {
                new ColumnSeries { Title="Producción", Values = new ChartValues<double>(produccionPorTipo)},
                new ColumnSeries { Title="Ventas", Values = new ChartValues<double>(ventasPorTipo)},
                new ColumnSeries { Title="Stock", Values = new ChartValues<double>(stockPorTipo)}
            };

            FormatoCantidad = value => value.ToString("N0");

            // Lotes detalle individual
            LotesDetalle = lotes.Select(l => new LoteDetalleViewModel1
            {
                IdLote = l.IdLote,
                TipoPasta = l.TipoPasta.Nombre,
                CantidadProduccion = l.CantidadDisponible,
                FechaProduccion = l.FechaProduccion,
                FechaVencimiento = l.FechaVencimiento,
                Stock = (decimal)stockActual.Where(s => s.TipoPasta.Nombre == l.TipoPasta.Nombre).Sum(s => s.CantidadDisponible),
                VentasTotales = (decimal)ventasGlobal.Where(v => v.Lote.IdLote == l.IdLote).Sum(v => v.Cantidad),
                VentasAsociadas = ventasGlobal.Where(v => v.Lote.IdLote == l.IdLote).ToList(),
                Estado = l.Estado
            }).ToList();

            // Lotes resumidos por tipo de pasta
            LotesDetallePorTipo = LotesDetalle
                .GroupBy(l => l.TipoPasta)
                .Select(g => new LoteDetalleViewModel1
                {
                    TipoPasta = g.Key,
                    CantidadProduccion = g.Sum(x => x.CantidadProduccion),
                    Stock = g.Sum(x => x.Stock),
                    VentasTotales = g.Sum(x => x.VentasTotales)
                }).ToList();

            LotesFiltrados = new List<LoteDetalleViewModel1>();
        }

        private void TipoPasta_Click(object sender, MouseButtonEventArgs e)
        {
            if (sender is Border border && border.DataContext is LoteDetalleViewModel1 lote)
            {
                SelectedTipoPasta = lote.TipoPasta;
                LotesFiltrados = LotesDetalle.Where(l => l.TipoPasta == SelectedTipoPasta).ToList();
                DataContext = null;
                DataContext = this;
            }
        }

        private void BuscarTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (sender is TextBox tb)
            {
                var filtro = tb.Text.ToLower();
                TarjetasLotes.ItemsSource = LotesDetallePorTipo
                    .Where(l => l.TipoPasta.ToLower().Contains(filtro))
                    .ToList();
            }
        }
    }

    public class LoteDetalleViewModel1
    {
        public int IdLote { get; set; }
        public string TipoPasta { get; set; }
        public decimal CantidadProduccion { get; set; }
        public DateTime FechaProduccion { get; set; }
        public DateTime FechaVencimiento { get; set; }
        public decimal Stock { get; set; }
        public decimal VentasTotales { get; set; }
        public List<DetalleVentaModel> VentasAsociadas { get; set; }
        public string Estado { get; set; }
    }
}
