using FIDELANDIA.Data;
using FIDELANDIA.Helpers;
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
using Microsoft.VisualBasic;

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
            get => _cantidadProducidaKg;
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

        public bool agruparPorMes;

        public SeriesCollection ProduccionVsVentasSeries { get; set; }


        public string BaseDatosGBTexto { get; set; }
        public SeriesCollection BaseDatosPieSeries { get; set; }




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
            FechaDesde = FechaHasta.AddMonths(-1);
            agruparPorMes= false;
            CboAgrupacion.SelectedIndex = 0; 

            CargarDashboardDesdeBD(agruparPorMes);
        }

        private void BtnExportarBackupProduccionVentas_Click(object sender, RoutedEventArgs e)
        {
            var db = new FidelandiaDbContext();
            var backupService = new BackupExcelService(db);
            backupService.ExportarBackupCompleto();
        }

        private void BtnExportarBackupProveedoresTransacciones_Click(object sender, RoutedEventArgs e)
        {
            var db = new FidelandiaDbContext();
            var backupService = new ProveedoresExcelService(db);
            backupService.ExportarProveedoresConTransacciones();
        }


        // =====================================================
        // FILTRAR POR RANGO DE FECHAS
        // =====================================================
        private void Filtrar_Click(object sender, RoutedEventArgs e)
        {
            var dias = (FechaHasta - FechaDesde).TotalDays;

            // =================== VALIDACIÓN DE RANGO ===================
            if (dias > 365)
            {
                MessageBox.Show(
                    "El rango máximo permitido es de 1 año.",
                    "Rango inválido",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning
                );
                return; // Salimos sin cargar el dashboard
            }

            // =================== AGRUPACIÓN ===================
            if (dias <= 31)
            {
                // Solo se puede agrupar por día
                agruparPorMes = false;
                CboAgrupacion.SelectedIndex = 0; // Día
            }
            else if (dias > 120)
            {
                // Solo se puede agrupar por mes
                agruparPorMes = true;
                CboAgrupacion.SelectedIndex = 1; // Mes
            }
            else
            {
                // Entre 31 y 90 días: permitir que el usuario elija
                if (CboAgrupacion.SelectedItem is ComboBoxItem item)
                {
                    agruparPorMes = item.Tag.ToString() == "Mes";
                }
                else
                {
                    agruparPorMes = false; // Por defecto día
                }
            }

            CargarDashboardDesdeBD(agruparPorMes);
        }
        private async void BtnTamanioBaseDatos(object sender, RoutedEventArgs e)
        {
            PanelBaseDatos.Visibility = Visibility.Visible;
            await CargarTamanioBaseDatos();
        }
        private void BtnCerrarPanelBaseDatos_Click(object sender, RoutedEventArgs e)
        {
            PanelBaseDatos.Visibility = Visibility.Collapsed;
        }

        public async Task CargarTamanioBaseDatos()
        {
            var dbContext = new FidelandiaDbContext();
            var service = new DatabaseSizeService(dbContext); 
            double sizeMB = await service.ObtenerTamanioMB();
            double sizeGB = sizeMB / 1024;

            BaseDatosGBTexto = $"{sizeGB:N2} GB";

            double limite = 9;
            double libre = Math.Max(0, limite - sizeGB);

            // GRAFICO
            BaseDatosPieSeries = new SeriesCollection
    {
        new PieSeries
        {
            Title = "Usado",
            Values = new ChartValues<double> { sizeGB },
            Fill = (SolidColorBrush)new BrushConverter().ConvertFrom("#E17055")
        },
        new PieSeries
        {
            Title = "Libre",
            Values = new ChartValues<double> { libre },
            Fill = (SolidColorBrush)new BrushConverter().ConvertFrom("#00CEC9")
        }
    };

            OnPropertyChanged(nameof(BaseDatosPieSeries));
            OnPropertyChanged(nameof(BaseDatosGBTexto));

            // TEXTOS Y RECOMENDACIONES
            if (sizeGB < 4)
            {
                TxtEstadoBD.Text = "Estado: Muy saludable";
                TxtRecomendacion.Text = "No se requieren acciones.";
            }
            else if (sizeGB < 7)
            {
                TxtEstadoBD.Text = "Estado: Moderado";
                TxtRecomendacion.Text = "Se recomienda exportar datos regularmente.";
            }
            else if (sizeGB < 9)
            {
                TxtEstadoBD.Text = "Estado: Crítico";
                TxtRecomendacion.Text = "Se recomienda exportar y vaciar datos cuanto antes.";
            }
            else
            {
                TxtEstadoBD.Text = "Estado: Límite alcanzado";
                TxtRecomendacion.Text = "Debe eliminar registros antiguos urgentemente.";
            }
        }


        private async void BtnBorrarBaseDatos(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("⚠ Esto borrará TODOS los datos de la base.\n¿Estás segura?",
                "Confirmar", MessageBoxButton.YesNo, MessageBoxImage.Warning) != MessageBoxResult.Yes)
                return;

            try
            {
                var dbContext = new FidelandiaDbContext();
                var service = new DatabaseSizeService(dbContext);
                await service.BorrarTodosLosRegistros();

                MessageBox.Show("Todos los registros fueron eliminados correctamente.",
                    "Base vaciada", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al borrar los datos: " + ex.Message);
            }
        }
        private async void BtnVaciarTransacciones_Click(object sender, RoutedEventArgs e)
        {
            int meses = PreguntarMesesAConservar("transacciones");
            if (meses < 0) return;

            try
            {
                var dbContext = new FidelandiaDbContext();
                var service = new DatabaseSizeService(dbContext);

                await service.BorrarTransaccionesAntiguas(meses);

                MessageBox.Show("Transacciones borradas correctamente.",
                    "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al borrar transacciones: " + ex.Message);
            }
        }


        private async void BtnVaciarProduccionVentas_Click(object sender, RoutedEventArgs e)
        {
            int meses = PreguntarMesesAConservar("producción/ventas");
            if (meses < 0) return;

            try
            {
                var dbContext = new FidelandiaDbContext();
                var service = new DatabaseSizeService(dbContext);

                await service.BorrarProduccionVentasAntiguas(meses);

                MessageBox.Show("Producción y ventas borradas correctamente.",
                    "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al borrar producción/ventas: " + ex.Message);
            }
        }


        /// <summary>
        /// Devuelve -1 si el usuario cancela.
        /// </summary>
        private int PreguntarMesesAConservar(string nombre)
        {
            string input = Interaction.InputBox(
                $"Ingrese la cantidad de meses que desea conservar de {nombre}:",
                "Conservar registros",
                "3"); // valor por defecto en el textbox

            if (string.IsNullOrWhiteSpace(input))
                return -1; // Canceló

            if (int.TryParse(input, out int meses))
                return meses;

            MessageBox.Show("Debe ingresar un número válido.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            return -1;
        }





        // =====================================================
        // CARGA PRINCIPAL DEL DASHBOARD
        // =====================================================
        private void CargarDashboardDesdeBD(bool agruparPorMes)
        {
            try
            {
                var resumen = _dashboardService.ObtenerDashboard(FechaDesde, FechaHasta, agruparPorMes);

                // ========== INDICADORES ==========
                // =================== VALORES PRINCIPALES ===================
                CantidadProducida = resumen.CantidadProducida ?? 0;
                VentasTotales = resumen.VentasTotales ?? 0;
                CantidadProducidaKg = resumen.ProduccionKg ?? 0;
                TicketPromedio = resumen.TicketPromedio ?? 0;

                // =================== VARIACIONES ===================
                // Solo asignar texto/color si existe valor, sino dejar null
                if (resumen.VariacionCantidadProducida.HasValue)
                {
                    VariacionProduccionText = $"{resumen.VariacionCantidadProducida:+0.0;-0.0;0.0}%";
                    VariacionProduccionColor = resumen.VariacionCantidadProducida >= 0 ? Brushes.ForestGreen : Brushes.Red;
                }
                else
                {
                    VariacionProduccionText = null;
                    VariacionProduccionColor = null;
                }

                if (resumen.VariacionVentasTotales.HasValue)
                {
                    VariacionVentasText = $"{resumen.VariacionVentasTotales:+0.0;-0.0;0.0}%";
                    VariacionVentasColor = resumen.VariacionVentasTotales >= 0 ? Brushes.ForestGreen : Brushes.Red;
                }
                else
                {
                    VariacionVentasText = null;
                    VariacionVentasColor = null;
                }

                if (resumen.VariacionProduccionKg.HasValue)
                {
                    VariacionCantidadProduccionKgText = $"{resumen.VariacionProduccionKg:+0.0;-0.0;0.0}%";
                    VariacionCantidadProduccionKgColor = resumen.VariacionProduccionKg >= 0 ? Brushes.ForestGreen : Brushes.Red;
                }
                else
                {
                    VariacionCantidadProduccionKgText = null;
                    VariacionCantidadProduccionKgColor = null;
                }

                if (resumen.VariacionTicketPromedio.HasValue)
                {
                    VariacionTicketText = $"{resumen.VariacionTicketPromedio:+0.0;-0.0;0.0}%";
                    VariacionTicketColor = resumen.VariacionTicketPromedio >= 0 ? Brushes.ForestGreen : Brushes.Red;
                }
                else
                {
                    VariacionTicketText = null;
                    VariacionTicketColor = null;
                }



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
                            Title = "Producción diaria (Envases)",
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
                            Title = "Ventas diarias (Envases)",
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
                // ===================== PRODUCCIÓN VS VENTAS =====================
                var diasPeriodo = (FechaHasta - FechaDesde).TotalDays;

                if (diasPeriodo > 30)
                {
                    // Usar LineSeries si el rango supera 30 días
                ProduccionVsVentasSeries = new SeriesCollection
                        {
                            new LineSeries
                            {
                                Title = "Producción",
                                Values = new ChartValues<double>(resumen.ProduccionDiariaEnvases.Values.Select(v => (double)v)),
                                Stroke = Brushes.SteelBlue,
                                Fill = new SolidColorBrush(Color.FromArgb(60, 70, 130, 180)),
                                PointGeometrySize = 6,
                                LineSmoothness = 0.4
                            },
                            new LineSeries
                            {
                                Title = "Ventas",
                                Values = new ChartValues<double>(resumen.VentasDiaria.Values.Select(v => (double)v)),
                                Stroke = Brushes.ForestGreen,
                                Fill = new SolidColorBrush(Color.FromArgb(60, 34, 139, 34)),
                                PointGeometrySize = 6,
                                LineSmoothness = 0.4
                            }
                        };
                                    }
                                    else
                                    {
                                        // Usar ColumnSeries si el rango es menor o igual a 30 días
                                        ProduccionVsVentasSeries = new SeriesCollection
                        {
                            new ColumnSeries
                            {
                                Title = "Producción",
                                Values = new ChartValues<double>(resumen.ProduccionDiariaEnvases.Values.Select(v => (double)v))
                            },
                            new ColumnSeries
                            {
                                Title = "Ventas",
                                Values = new ChartValues<double>(resumen.VentasDiaria.Values.Select(v => (double)v))
                            }
                        };
                                    }

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
