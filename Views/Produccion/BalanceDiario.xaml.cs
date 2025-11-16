using FIDELANDIA.Data;
using FIDELANDIA.Helpers;
using FIDELANDIA.Models;
using FIDELANDIA.Services;
using FIDELANDIA.Windows;
using LiveCharts;
using LiveCharts.Wpf;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using static FIDELANDIA.Services.BalanceService;

namespace FIDELANDIA.Views.Produccion
{
    public partial class BalanceDiario : UserControl, INotifyPropertyChanged
    {
        private readonly BalanceService _balanceService;

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string propertyName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        public List<string> TipoPastaLabels { get; set; }
        public Func<double, string> FormatoCantidad { get; set; }

        public BalanceDiarioResultado LotesDetalle { get; set; }
        public ObservableCollection<LoteDetalleViewModel1> LotesProduccion { get; set; }

        // 🔹 ObservableCollection para actualización automática
        public ObservableCollection<DetalleVentaModel> VentasDetalladas { get; set; }
        public List<LoteDetalleViewModel1> LotesResumenVentasHoy { get; set; }
        public ObservableCollection<LoteDetalleViewModel1> LotesFiltrados { get; set; }

        public List<BalanceDiarioModel> BalanceDiarioModel { get; set; }

        public BalanceDiario()
        {
            InitializeComponent();

            var dbContext = new FidelandiaDbContext();
            _balanceService = new BalanceService(dbContext);

            CargarDatos();
            DataContext = this;

            // 🔹 Escuchar cuando se crea o elimina un lote
            AppEvents.LoteCreado += OnDatosActualizados;
        }

        private void OnDatosActualizados()
        {
            Application.Current.Dispatcher.Invoke(CargarDatos);
        }

        ~BalanceDiario()
        {
            AppEvents.LoteCreado -= OnDatosActualizados;
        }

        private void CargarDatos()
        {
            try
            {
                // 🔹 Obtener todos los lotes con producción, ventas y stock
                LotesDetalle = _balanceService.ObtenerLotesDetalle();

                // Producción de hoy
                LotesProduccion = new ObservableCollection<LoteDetalleViewModel1>(LotesDetalle.LotesProduccionHoy);
                OnPropertyChanged(nameof(LotesProduccion));

                // Ventas del día
                VentasDetalladas = new ObservableCollection<DetalleVentaModel>(LotesDetalle.VentasHoy);
                OnPropertyChanged(nameof(VentasDetalladas));

                // Lotes antiguos con ventas hoy
                var LotesAntiguosConVentasHoy = LotesDetalle.LotesAntiguosConVentasHoy;

                // 🔹 Lista fusionada con cantidad vendida hoy y ajuste de TipoLote
                LotesResumenVentasHoy = LotesProduccion
                    .Select(l =>
                    {
                        // Si el lote está defectuoso, mostrar "Defectuoso" en la tabla
                        l.TipoLote = l.Estado == "Defectuoso" ? "Defectuoso" : "Hoy";
                        l.CantidadVendidaHoy = VentasDetalladas
                            .Where(v => v.IdLote == l.IdLote)
                            .Sum(v => v.Cantidad);
                        return l;
                    })
                    .Concat(LotesDetalle.LotesAntiguosConVentasHoy.Select(l =>
                    {
                        l.TipoLote = l.Estado == "Defectuoso" ? "Defectuoso" : "Stock";
                        l.CantidadVendidaHoy = VentasDetalladas
                            .Where(v => v.IdLote == l.IdLote)
                            .Sum(v => v.Cantidad);
                        return l;
                    }))
                    .ToList();


                // 🔹 Inicialmente mostrar todos los lotes
                LotesFiltrados = new ObservableCollection<LoteDetalleViewModel1>(
                    LotesResumenVentasHoy
                        .OrderByDescending(l => l.TipoLote)
                        .ThenBy(l => l.TipoPasta)
                        .ToList()
                );
                OnPropertyChanged(nameof(LotesFiltrados));

                // 🔹 Preparar datos para gráfico
                BalanceDiarioModel = LotesResumenVentasHoy
                    .GroupBy(l => l.TipoPasta)
                    .Select(g => new BalanceDiarioModel
                    {
                        TipoPasta = g.Key,
                        CantidadProducida = g.Where(x => x.TipoLote == "Hoy").Sum(x => x.CantidadProduccion),
                        CantidadVendida = g.Sum(x => x.VentasTotales),
                        StockActual = g.First()?.Stock ?? 0
                    })
                    .ToList();

                TipoPastaLabels = BalanceDiarioModel.Select(b => b.TipoPasta).ToList();
                FormatoCantidad = val => val.ToString("N0");

                // 🔹 Configurar gráfico
                MyChart.Series = new SeriesCollection
                {
                    new ColumnSeries { Title = "Producción", Values = new ChartValues<decimal>(BalanceDiarioModel.Select(b => b.CantidadProducida)) },
                    new ColumnSeries { Title = "Ventas", Values = new ChartValues<decimal>(BalanceDiarioModel.Select(b => b.CantidadVendida)) },
                    new ColumnSeries { Title = "Stock", Values = new ChartValues<decimal>(BalanceDiarioModel.Select(b => b.StockActual)) }
                };

                MyChart.AxisX[0].Labels = TipoPastaLabels;
                MyChart.AxisY[0].LabelFormatter = FormatoCantidad;
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show(
                    $"Error al cargar balance: {ex.Message}",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                );
            }
        }

        private void NuevoLote_Click(object sender, RoutedEventArgs e)
        {
            var ventana = new CrearLoteProducFormWindow();
            ventana.Owner = Window.GetWindow(this);
            ventana.ShowDialog();

            AppEvents.NotificarEliminado();

        }

        private void NuevaVenta_Click(object sender, RoutedEventArgs e)
        {
            var ventana = new CrearVentaProducFormWindow();
            ventana.Owner = Window.GetWindow(this);
            ventana.ShowDialog();

            AppEvents.NotificarEliminado();
        }

        // 🔹 Filtrado al hacer click en el gráfico
        private void MyChart_DataClick(object sender, LiveCharts.ChartPoint chartPoint)
        {
            int index = (int)chartPoint.Key;
            if (index < 0 || index >= TipoPastaLabels.Count) return;

            string tipoSeleccionado = TipoPastaLabels[index];

            // 🔹 Filtrar la lista fusionada
            LotesFiltrados = new ObservableCollection<LoteDetalleViewModel1>(
                LotesResumenVentasHoy
                    .Where(l => l.TipoPasta == tipoSeleccionado)
                    .OrderByDescending(l => l.TipoLote)
                    .ToList()
            );
            OnPropertyChanged(nameof(LotesFiltrados));
        }

        private void Cancelar_Click(object sender, RoutedEventArgs e)
        {
            Window.GetWindow(this)?.Close();
        }

        private void BtnVerTodos_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            // 🔹 Volver a mostrar todos los lotes
            LotesFiltrados = new ObservableCollection<LoteDetalleViewModel1>(
                LotesResumenVentasHoy
                    .OrderByDescending(l => l.TipoLote)
                    .ThenBy(l => l.TipoPasta)
                    .ToList()
            );
            OnPropertyChanged(nameof(LotesFiltrados));
        }

        private void EliminarDetalleVenta_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.DataContext is DetalleVentaModel detalle)
            {
                var dbContext = new FidelandiaDbContext();
                var stockService = new StockService(dbContext);
                var ventaService = new VentaService(dbContext, stockService);

                var ventana = new EliminarDetalleVentaWindow(detalle, ventaService, stockService);
                ventana.Owner = Window.GetWindow(this);
                ventana.ShowDialog();

                AppEvents.NotificarEliminado();
            }
        }

        private void BtnExportarExcel_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var excelService = new ExcelExportService();

                // Proyectamos los datos a objetos planos antes de exportar
                var produccionPlano = LotesProduccion.Select(l => new
                {
                    l.IdLote,
                    l.TipoPasta,
                    l.CantidadProduccion,
                    l.CantidadDisponible,
                    FechaProduccion = l.FechaProduccion.ToString("dd/MM/yyyy"),
                    FechaVencimiento = l.FechaVencimiento.ToString("dd/MM/yyyy"),
                    l.Stock,
                    l.VentasTotales,
                    Estado = l.Estado,
                    CantidadVendidaHoy = l.CantidadVendidaHoy
                }).ToList();

                var ventasPlano = VentasDetalladas.Select(v => new
                {
                    v.IdVenta,
                    v.IdLote,
                    TipoPasta = v.Lote?.TipoPasta?.Nombre ?? "",
                    v.Cantidad,
                    FechaVenta = v.Venta.Fecha.ToString("dd/MM/yyyy")
                }).ToList();

                var resumenPlano = LotesFiltrados.Select(l => new
                {
                    l.IdLote,
                    l.TipoPasta,
                    l.CantidadProduccion,
                    l.CantidadDisponible,
                    FechaProduccion = l.FechaProduccion.ToString("dd/MM/yyyy"),
                    FechaVencimiento = l.FechaVencimiento.ToString("dd/MM/yyyy"),
                    l.Stock,
                    l.VentasTotales,
                    Estado = l.Estado,
                    CantidadVendidaHoy = l.CantidadVendidaHoy
                }).ToList();

                var datasets = new Dictionary<string, IEnumerable>
        {
            { "Producción", produccionPlano },
            { "Ventas", ventasPlano },
            { "Resumen", resumenPlano }
        };

                excelService.ExportarMultiplesHojas(datasets);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al exportar: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }



        private void EliminarProduccion_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.DataContext is LoteDetalleViewModel1 loteSeleccionado)
            {
                loteSeleccionado.VentasAsociadas = LotesDetalle.VentasHoy
                    .Where(v => v.IdLote == loteSeleccionado.IdLote)
                    .ToList();

                var dbContext = new FidelandiaDbContext();
                var loteService = new LoteProduccionService(dbContext);
                var stockService = new StockService(dbContext);
                var ventaService = new VentaService(dbContext, stockService);

                var ventana = new EliminarProduccionWindow(loteSeleccionado, loteService, stockService, ventaService);
                ventana.Owner = Window.GetWindow(this);
                ventana.ShowDialog();

                AppEvents.NotificarEliminado();
            }
        }

        private void BtnConfirmarBalance_Click(object sender, RoutedEventArgs e)
        {
            var dbContext = new FidelandiaDbContext();
            var stockService = new StockService(dbContext);

            var confirmado = stockService.ConfirmarBalanceDiario();
            Window.GetWindow(this)?.Close();
        }
    }

    // Modelo de vista
    public class LoteDetalleViewModel1
    {
        public int IdLote { get; set; }
        public string TipoPasta { get; set; }
        public decimal CantidadProduccion { get; set; }
        public decimal CantidadDisponible { get; set; }
        public DateTime FechaProduccion { get; set; }
        public DateTime FechaVencimiento { get; set; }
        public decimal Stock { get; set; }
        public decimal VentasTotales { get; set; }
        public List<DetalleVentaModel> VentasAsociadas { get; set; }
        public string Estado { get; set; }

        public string TipoLote { get; set; }
        public decimal CantidadVendidaHoy { get; set; }
    }
}
