using FIDELANDIA.Data;
using FIDELANDIA.Models;
using FIDELANDIA.Services;
using LiveCharts;
using LiveCharts.Wpf;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace FIDELANDIA.Views
{
    public partial class DashboardView : UserControl, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private readonly DashboardService _dashboardService;

        // ===================== INDICADORES =====================
        private decimal _cantidadProducida;
        public decimal CantidadProducida
        {
            get => _cantidadProducida;
            set { _cantidadProducida = value; OnPropertyChanged(nameof(CantidadProducida)); }
        }

        private decimal _ventasTotales;
        public decimal VentasTotales
        {
            get => _ventasTotales;
            set { _ventasTotales = value; OnPropertyChanged(nameof(VentasTotales)); }
        }

        private decimal _stockPromedio;
        public decimal StockPromedio
        {
            get => _stockPromedio;
            set { _stockPromedio = value; OnPropertyChanged(nameof(StockPromedio)); }
        }

        private decimal _ticketPromedio;
        public decimal TicketPromedio
        {
            get => _ticketPromedio;
            set { _ticketPromedio = value; OnPropertyChanged(nameof(TicketPromedio)); }
        }

        public string VariacionProduccionText { get; set; }
        public Brush VariacionProduccionColor { get; set; }

        public string VariacionVentasText { get; set; }
        public Brush VariacionVentasColor { get; set; }

        public string VariacionStockText { get; set; }
        public Brush VariacionStockColor { get; set; }

        public string VariacionTicketText { get; set; }
        public Brush VariacionTicketColor { get; set; }

        // ===================== SERIES =====================
        public SeriesCollection ProduccionPorTipoSeries { get; set; }
        public SeriesCollection ParticipacionVentasSeries { get; set; }

        public SeriesCollection ProduccionTotalSeries { get; set; }
        public SeriesCollection RendimientoSeries { get; set; }
        public SeriesCollection LoteComparativoSeries { get; set; }
        public SeriesCollection TipoSeries { get; set; }

        public List<string> TipoPastaLabels { get; set; }
        public List<string> MesesLabels { get; set; }

        public DateTime FechaDesde { get; set; }
        public DateTime FechaHasta { get; set; }

        // =====================================================
        // CONSTRUCTOR
        // =====================================================
        public DashboardView()
        {
            InitializeComponent();
            DataContext = this;

            var dbContext = new FidelandiaDbContext();
            _dashboardService = new DashboardService(dbContext);

            FechaHasta = DateTime.Today;
            FechaDesde = FechaHasta.AddMonths(-1); // 6 meses atrás para mejor análisis

            CargarDashboardDesdeBD();
        }

        // =====================================================
        // FILTRAR POR RANGO DE FECHAS
        // =====================================================
        private void Filtrar_Click(object sender, RoutedEventArgs e)
        {
            CargarDashboardDesdeBD();
        }

        // =====================================================
        // CARGA PRINCIPAL DEL DASHBOARD
        // =====================================================
        private void CargarDashboardDesdeBD()
        {
            try
            {
                var resumen = _dashboardService.ObtenerDashboard(FechaDesde, FechaHasta);

                // ========== INDICADORES ==========
                CantidadProducida = resumen.CantidadProducida;
                VentasTotales = resumen.VentasTotales;
                StockPromedio = resumen.StockPromedio;
                TicketPromedio = resumen.TicketPromedio;

                // ========== VARIACIONES REALES ==========

                VariacionProduccionText = $"{resumen.VariacionCantidadProducida:+0.0;-0.0;0.0}%";
                VariacionProduccionColor = resumen.VariacionCantidadProducida >= 0 ? Brushes.ForestGreen : Brushes.Red;

                VariacionVentasText = $"{resumen.VariacionVentasTotales:+0.0;-0.0;0.0}%";
                VariacionVentasColor = resumen.VariacionVentasTotales >= 0 ? Brushes.ForestGreen : Brushes.Red;

                VariacionStockText = $"{resumen.VariacionStockPromedio:+0.0;-0.0;0.0}%";
                VariacionStockColor = resumen.VariacionStockPromedio >= 0 ? Brushes.ForestGreen : Brushes.Red;

                VariacionTicketText = $"{resumen.VariacionTicketPromedio:+0.0;-0.0;0.0}%";
                VariacionTicketColor = resumen.VariacionTicketPromedio >= 0 ? Brushes.ForestGreen : Brushes.Red;


                // ========== PRODUCCIÓN / VENTAS / STOCK ==========
                TipoPastaLabels = resumen.ProduccionPorTipo.Keys.ToList();

                ProduccionPorTipoSeries = new SeriesCollection
                {
                    new ColumnSeries { Title = "Producción", Values = new ChartValues<double>(resumen.ProduccionPorTipo.Values.Select(v => (double)v)) },
                    new ColumnSeries { Title = "Ventas", Values = new ChartValues<double>(resumen.VentasPorTipo.Values.Select(v => (double)v)) },
                    new ColumnSeries { Title = "Diferencia", Values = new ChartValues<double>(resumen.StockPorTipo.Values.Select(v => (double)v)) }
                };

                // ========== PARTICIPACIÓN EN VENTAS ==========
                ParticipacionVentasSeries = new SeriesCollection();
                foreach (var kv in resumen.VentasPorTipo)
                {
                    ParticipacionVentasSeries.Add(new PieSeries
                    {
                        Title = kv.Key,
                        Values = new ChartValues<double> { (double)kv.Value },
                        DataLabels = true,
                        PushOut = 4
                    });
                }

                // ========== SERIES DIARIAS ==========
                MesesLabels = resumen.ProduccionDiaria.Keys.ToList();

                // Evolución de producción (por día)
                ProduccionTotalSeries = new SeriesCollection
{
                        new LineSeries
                        {
                            Title = "Producción diaria",
                            Values = new ChartValues<double>(resumen.ProduccionDiaria.Values.Select(v => (double)v)),
                            Stroke = Brushes.SteelBlue,
                            Fill = new SolidColorBrush(Color.FromArgb(60, 70, 130, 180)),
                            PointGeometrySize = 6,
                            LineSmoothness = 0.4
                        }
                    };

                // Evolución de ventas (por día)
                RendimientoSeries = new SeriesCollection
{
                        new LineSeries
                        {
                            Title = "Ventas diarias",
                            Values = new ChartValues<double>(resumen.VentasDiaria.Values.Select(v => (double)v)),
                            Stroke = Brushes.ForestGreen,
                            Fill = new SolidColorBrush(Color.FromArgb(60, 34, 139, 34)),
                            PointGeometrySize = 6,
                            LineSmoothness = 0.4
                        }
                    };
                LoteComparativoSeries = new SeriesCollection
                {
                    new LineSeries
                    {
                        Title = "Kg producidos",
                        Values = new ChartValues<double>(resumen.ProduccionDiaria.Values.Select(v => (double)v)),
                        Stroke = Brushes.DarkOrange,
                        Fill = new SolidColorBrush(Color.FromArgb(60, 255, 165, 0))
                    }
                };

                TipoSeries = new SeriesCollection
                {
                    new LineSeries
                    {
                        Title = "Recaudación",
                        Values = new ChartValues<double>(resumen.RecaudacionDiaria.Values.Select(v => (double)v)),
                        Stroke = Brushes.OrangeRed,
                        Fill = new SolidColorBrush(Color.FromArgb(60, 255, 69, 0))
                    }
                };

                OnPropertyChanged(null);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar el dashboard: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // =====================================================
        // FUNCIÓN AUXILIAR DE VARIACIÓN
        // =====================================================
        private string Variacion(decimal actual, decimal anterior, out Brush color)
        {
            if (anterior == 0)
            {
                color = Brushes.Gray;
                return "0%";
            }

            var diff = (actual - anterior) / anterior * 100;
            color = diff >= 0 ? Brushes.ForestGreen : Brushes.Red;
            return $"{diff:+0.0;-0.0;0.0}%";
        }

        // =====================================================
        // NOTIFICADOR DE CAMBIOS
        // =====================================================
        private void OnPropertyChanged(string propertyName)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
