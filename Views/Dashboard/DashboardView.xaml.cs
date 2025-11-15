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

        private decimal _cantidadProducidaKg;
        public decimal CantidadProducidaKg
        {
            get => _cantidadProducidaKg; // ✅ devuelve el campo privado
            set { _cantidadProducidaKg = value; OnPropertyChanged(nameof(CantidadProducidaKg)); }
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

        public string VariacionCantidadProduccionKgText { get; set; }
        public Brush VariacionCantidadProduccionKgColor { get; set; }

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
            // 🔹 Validar que el rango no supere 4 meses
            var mesesDiferencia = ((FechaHasta.Year - FechaDesde.Year) * 12) + (FechaHasta.Month - FechaDesde.Month);

            if (mesesDiferencia > 4)
            {
                MessageBox.Show(
                    "El rango de fechas no puede superar 4 meses. Ajuste las fechas seleccionadas.",
                    "Rango demasiado grande",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning
                );
                return; // 
            }

            // 🔹 Si es válido, cargar dashboard
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
                CantidadProducidaKg = resumen.ProduccionKg;
                TicketPromedio = resumen.TicketPromedio;

                // ========== VARIACIONES REALES ==========

                VariacionProduccionText = $"{resumen.VariacionCantidadProducida:+0.0;-0.0;0.0}%";
                VariacionProduccionColor = resumen.VariacionCantidadProducida >= 0 ? Brushes.ForestGreen : Brushes.Red;

                VariacionVentasText = $"{resumen.VariacionVentasTotales:+0.0;-0.0;0.0}%";
                VariacionVentasColor = resumen.VariacionVentasTotales >= 0 ? Brushes.ForestGreen : Brushes.Red;

                VariacionCantidadProduccionKgText = $"{resumen.VariacionProduccionKg:+0.0;-0.0;0.0}%";
                VariacionCantidadProduccionKgColor = resumen.VariacionProduccionKg >= 0 ? Brushes.ForestGreen : Brushes.Red;

                VariacionTicketText = $"{resumen.VariacionTicketPromedio:+0.0;-0.0;0.0}%";
                VariacionTicketColor = resumen.VariacionTicketPromedio >= 0 ? Brushes.ForestGreen : Brushes.Red;


                // ========== PRODUCCIÓN / VENTAS / STOCK ==========
                TipoPastaLabels = resumen.ProduccionPorTipo.Keys.ToList();

                ProduccionPorTipoSeries = new SeriesCollection
                {
                    new ColumnSeries { Title = "Producción", Values = new ChartValues<double>(resumen.ProduccionPorTipo.Values.Select(v => (double)v)) },
                    new ColumnSeries { Title = "Ventas", Values = new ChartValues<double>(resumen.VentasPorTipo.Values.Select(v => (double)v)) },
                    new ColumnSeries
                    {
                        Title = "Diferencia",
                        Values = new ChartValues<double>(
                            resumen.ProduccionPorTipo.Keys
                                    .Select(k => (double)(resumen.ProduccionPorTipo[k] -
                                                            (resumen.VentasPorTipo.ContainsKey(k) ? resumen.VentasPorTipo[k] : 0)))
                        )
                    }               
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
                // Labels por día
                MesesLabels = resumen.ProduccionDiariaEnvases.Keys.ToList();

                // Evolución de producción en envases
                ProduccionTotalSeries = new SeriesCollection
                    {
                        new LineSeries
                        {
                            Title = "Producción diaria (envases)",
                            Values = new ChartValues<double>(resumen.ProduccionDiariaEnvases.Values.Select(v => (double)v)),
                            Stroke = Brushes.SteelBlue,
                            Fill = new SolidColorBrush(Color.FromArgb(60, 70, 130, 180)),
                            PointGeometrySize = 6,
                            LineSmoothness = 0.4
                        }
                    };

                // Evolución de producción en kilogramos
                LoteComparativoSeries = new SeriesCollection
                    {
                        new LineSeries
                        {
                            Title = "Producción diaria (kg)",
                            Values = new ChartValues<double>(resumen.ProduccionDiariaKg.Values.Select(v => (double)v)),
                            Stroke = Brushes.DarkOrange,
                            Fill = new SolidColorBrush(Color.FromArgb(60, 255, 165, 0)),
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
                MessageBox.Show(
                    $"Error al cargar el dashboard: {ex.Message}",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                );
            }
        }


        // =====================================================
        // NOTIFICADOR DE CAMBIOS
        // =====================================================
        private void OnPropertyChanged(string propertyName)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
