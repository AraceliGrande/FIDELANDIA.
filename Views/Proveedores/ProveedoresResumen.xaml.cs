using FIDELANDIA.Models;
using FIDELANDIA.Services;
using LiveCharts;
using LiveCharts.Wpf;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Controls;

namespace FIDELANDIA.Views.Proveedores
{
    public partial class ProveedoresResumen : UserControl, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        // ================= KPIs =================
        public int TotalProveedores { get; set; }
        public decimal SaldoTotal { get; set; }
        public int TotalTransaccionesMes { get; set; }
        public int TotalProveedoresCriticos { get; set; }
        public decimal MayorSaldo { get; set; }

        // ================= Series gráficos =================
        public SeriesCollection SaldoPorProveedorSeries { get; set; }
        public SeriesCollection DeudaPorCategoriaSeries { get; set; }
        public SeriesCollection GastosPorCategoriaSeries { get; set; }
        public SeriesCollection MovimientoAcumuladoSeries { get; set; }
        public SeriesCollection DebeHaberSeries { get; set; }
        public List<string> ProveedoresLabels { get; set; }
        public List<string> DiasDelMes { get; set; }

        public SeriesCollection SaldoAcumuladoAnualSeries { get; set; }
        public SeriesCollection DebeHaberAnualSeries { get; set; }
        public SeriesCollection GastosPorCategoriaAnualSeries { get; set; }
        public List<string> MesesLabels { get; set; }

        // ================= Formato =================
        public Func<double, string> FormatoSaldo { get; set; }

        // ================= Mes y Año =================
        private int _mesSeleccionado = DateTime.Now.Month;
        public int MesSeleccionado
        {
            get => _mesSeleccionado;
            set
            {
                _mesSeleccionado = value;
                OnPropertyChanged(nameof(MesSeleccionado));
                OnPropertyChanged(nameof(MesSeleccionadoTexto)); // para actualizar el texto dinámico
            }
        }

        private int _anioSeleccionado = DateTime.Now.Year;
        public int AnioSeleccionado
        {
            get => _anioSeleccionado;
            set
            {
                _anioSeleccionado = value;
                OnPropertyChanged(nameof(AnioSeleccionado));
                OnPropertyChanged(nameof(MesSeleccionadoTexto)); // para actualizar el texto dinámico
            }
        }

        // ================= Propiedad calculada para mostrar mes y año en títulos =================
        public string MesSeleccionadoTexto
        {
            get
            {
                if (MesSeleccionado < 1 || MesSeleccionado > 12) return "";
                string nombreMes = new DateTime(1, MesSeleccionado, 1).ToString("MMMM");
                return $"{nombreMes} {AnioSeleccionado}";
            }
        }

        public ProveedoresResumen()
        {
            InitializeComponent();
            DataContext = this;
            CargarDatos();
        }

        public void CargarDatos()
        {
            var service = new ProveedorDashboardService(new FIDELANDIA.Data.FidelandiaDbContext());

            // ================= KPIs =================
            var kpis = service.ObtenerKPIs(MesSeleccionado, AnioSeleccionado);
            TotalProveedores = kpis.totalProveedores;
            SaldoTotal = kpis.saldoTotal;
            TotalProveedoresCriticos = kpis.proveedoresCriticos;
            TotalTransaccionesMes = kpis.transaccionesMes;
            MayorSaldo = kpis.mayorSaldo;

            // ================= Gráficos =================
            SaldoPorProveedorSeries = service.ObtenerSaldoPorProveedor(out var proveedoresLabels, MesSeleccionado, AnioSeleccionado);
            ProveedoresLabels = proveedoresLabels;

            DebeHaberSeries = service.ObtenerDebeHaberPorProveedor(out _, MesSeleccionado, AnioSeleccionado);

            MovimientoAcumuladoSeries = service.ObtenerMovimientoAcumulado(out var diasDelMes, MesSeleccionado, AnioSeleccionado);
            DiasDelMes = diasDelMes;

            DeudaPorCategoriaSeries = service.ObtenerDeudaPorCategoria(out _, MesSeleccionado, AnioSeleccionado);
            GastosPorCategoriaSeries = service.ObtenerGastosPorCategoria(out _, MesSeleccionado, AnioSeleccionado);

            // ==== Evolución anual ====
            SaldoAcumuladoAnualSeries = service.ObtenerSaldoAcumuladoAnual(out var mesesLabels);
            DebeHaberAnualSeries = service.ObtenerDebeHaberAnual(out _);
            MesesLabels = mesesLabels;


            FormatoSaldo = v => v.ToString("C0");

            OnPropertyChanged(null);
        }

        private void DebeHaberAnualChart_DataClick(object sender, ChartPoint chartPoint)
        {
            // chartPoint.X devuelve el índice de la barra clickeada (0..11)
            int mesSeleccionado = (int)chartPoint.X + 1; // +1 porque Enero=0 índice
            MesSeleccionado = mesSeleccionado;
            AnioSeleccionado = DateTime.Now.Year;

            // Actualizar datos del card de transacciones
            CargarDatos();

            // Hacer visible el card de transacciones
            CardTransaccionesMes.Visibility = System.Windows.Visibility.Visible;

            OnPropertyChanged(nameof(TotalTransaccionesMes));
            OnPropertyChanged(nameof(MesSeleccionadoTexto));

            // Optional: Scroll hasta el card
            CardTransaccionesMes.BringIntoView();
        }


        private void CbMes_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (CbMes.SelectedItem is ComboBoxItem item && int.TryParse(item.Tag.ToString(), out int mes))
            {
                MesSeleccionado = mes;
                CargarDatos();
            }
        }

        private void TbAnio_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            if (int.TryParse(TbAnio.Text, out int anio))
            {
                AnioSeleccionado = anio;
                CargarDatos();
            }
        }

        private void MovimientoAcumuladoChart_DataClick(object sender, ChartPoint chartPoint)
        {
            int mesSeleccionado = (int)chartPoint.X + 1; // +1 porque Enero=0 índice
            MesSeleccionado = mesSeleccionado;
            AnioSeleccionado = DateTime.Now.Year;

            CargarDatos();

            CardTransaccionesMes.Visibility = System.Windows.Visibility.Visible;

            // Actualizar los bindings
            OnPropertyChanged(nameof(TotalTransaccionesMes));
            OnPropertyChanged(nameof(MesSeleccionadoTexto));

            // Optional: Scroll hasta el card
            CardTransaccionesMes.BringIntoView();
        }


        private void OnPropertyChanged(string propertyName)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
