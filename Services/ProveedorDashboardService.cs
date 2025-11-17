using FIDELANDIA.Data;
using FIDELANDIA.Models;
using LiveCharts;
using LiveCharts.Wpf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media;
using Microsoft.EntityFrameworkCore;

namespace FIDELANDIA.Services
{

    /// <summary>
    /// Servicio encargado de generar indicadores y gráficos para el dashboard de proveedores.
    /// </summary>
    public class ProveedorDashboardService
    {
        private readonly FidelandiaDbContext _dbContext;

        public ProveedorDashboardService(FidelandiaDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        // ================= KPIs =================
        /// <summary>
        /// Obtiene los indicadores clave de desempeño de los proveedores.
        /// </summary>
        public (int totalProveedores, decimal saldoTotal, int proveedoresCriticos, int transaccionesMes, decimal mayorSaldo) ObtenerKPIs(int mes, int anio)
        {
            // Si no se pasa mes o año, usamos el actual
            if (mes <= 0) mes = DateTime.Now.Month;
            if (anio <= 0) anio = DateTime.Now.Year;

            int totalProveedores = _dbContext.Proveedores.Count();

            decimal saldoTotal = _dbContext.Transacciones
                .Where(t => t.TipoTransaccion == "debe")
                .Sum(t => (decimal?)t.Monto) ?? 0;

            decimal mayorSaldo = _dbContext.Proveedores
                .Max(p => (decimal?)p.SaldoActual) ?? 0;

            int proveedoresCriticos = _dbContext.Proveedores
                .Count(p => p.SaldoActual > p.LimiteCredito);

            int transaccionesMes = _dbContext.Transacciones
                .Count(t => t.Fecha.Month == mes && t.Fecha.Year == anio);

            return (totalProveedores, saldoTotal, proveedoresCriticos, transaccionesMes, mayorSaldo);
        }


        // ================= Gráficos =================


        /// <summary>
        /// Obtiene el saldo por proveedor para graficar.
        /// </summary>
        public SeriesCollection ObtenerSaldoPorProveedor(out List<string> labels, int mes, int anio)
        {
            var proveedores = _dbContext.Proveedores
                .OrderBy(p => p.Nombre)
                .AsNoTracking()
                .ToList();

            labels = new List<string>();

            // Serie principal
            var columnSeries = new ColumnSeries
            {
                Title = "Saldo",
                DataLabels = true,
                Values = new ChartValues<double>(),
                LabelPoint = point => point.Y.ToString("C0"),
                Fill = Brushes.Transparent // se define por mapper
            };

            var brushNegativo = new SolidColorBrush(Color.FromRgb(144, 238, 144)); // verde pastel
            var brushPositivo = new SolidColorBrush(Color.FromRgb(255, 160, 122)); // rojo pastel
            var brushCritico = new SolidColorBrush(Color.FromRgb(178, 34, 34));   // rojo oscuro (límite excedido)

            var valores = new List<double>();
            var colores = new List<Brush>();

            foreach (var p in proveedores)
            {
                if (p.SaldoActual != 0)
                {
                    labels.Add(p.Nombre);
                    valores.Add((double)p.SaldoActual);

                    if (p.SaldoActual < 0)
                        colores.Add(brushNegativo);
                    else if (p.SaldoActual > p.LimiteCredito)
                        colores.Add(brushCritico);
                    else
                        colores.Add(brushPositivo);
                }
            }

            columnSeries.Configuration = LiveCharts.Configurations.Mappers.Xy<double>()
                .X((value, index) => index)
                .Y(value => value)
                .Fill((value, index) => colores[index]);

            columnSeries.Values = new ChartValues<double>(valores);

            return new SeriesCollection { columnSeries };
        }




        /// <summary>
        /// Obtiene el debe/haber por proveedor en un mes determinado.
        /// </summary>
        public SeriesCollection ObtenerDebeHaberPorProveedor(out List<string> labels, int mes, int anio)
        {
            var proveedores = _dbContext.Proveedores
                .OrderBy(p => p.Nombre)
                .Include(p => p.Transacciones)
                .AsNoTracking()
                .ToList();

            labels = new List<string>();
            var debeValores = new ChartValues<decimal>();
            var haberValores = new ChartValues<decimal>();

            foreach (var p in proveedores)
            {
                decimal debe = p.Transacciones
                    .Where(t => t.TipoTransaccion.Equals("debe", StringComparison.OrdinalIgnoreCase)
                                && t.Fecha.Month == mes && t.Fecha.Year == anio)
                    .Sum(t => t.Monto);

                decimal haber = p.Transacciones
                    .Where(t => t.TipoTransaccion.Equals("haber", StringComparison.OrdinalIgnoreCase)
                                && t.Fecha.Month == mes && t.Fecha.Year == anio)
                    .Sum(t => t.Monto);

                if (debe != 0 || haber != 0)
                {
                    labels.Add(p.Nombre);
                    debeValores.Add(debe);
                    haberValores.Add(haber);
                }
            }

            var series = new SeriesCollection
            {
                new ColumnSeries { Title = "Debe", Values = debeValores, Fill = Brushes.CornflowerBlue },
                new ColumnSeries { Title = "Haber", Values = haberValores, Fill = Brushes.Orange }
            };

            return series;
        }

        /// <summary>
        /// Obtiene el movimiento acumulado del mes.
        /// </summary>
        public SeriesCollection ObtenerMovimientoAcumulado(out List<string> diasDelMes, int mes, int anio)
        {
            int diasMes = DateTime.DaysInMonth(anio, mes);
            diasDelMes = Enumerable.Range(1, diasMes).Select(d => d.ToString()).ToList();

            var transacciones = _dbContext.Transacciones
                .Where(t => t.Fecha.Month == mes && t.Fecha.Year == anio)
                .AsNoTracking()
                .ToList();

            var acumulado = new List<decimal>();
            decimal totalAcum = 0;

            for (int dia = 1; dia <= diasMes; dia++)
            {
                totalAcum += transacciones.Where(t => t.Fecha.Day == dia).Sum(t => t.Monto);
                acumulado.Add(totalAcum);
            }

            var series = new SeriesCollection
            {
                new LineSeries
                {
                    Title = "Acumulado",
                    Values = new ChartValues<decimal>(acumulado),
                    Stroke = Brushes.CornflowerBlue,
                    Fill = Brushes.Transparent,
                    PointGeometrySize = 6
                }
            };

            return series;
        }

        /// <summary>
        /// Obtiene la deuda por categoría de proveedor.
        /// </summary>
        public SeriesCollection ObtenerDeudaPorCategoria(out List<string> categorias, int mes, int anio)
        {
            var categoriasData = _dbContext.CategoriaProveedor
                .Include(c => c.Proveedores)
                .ThenInclude(p => p.Transacciones)
                .AsNoTracking()
                .ToList();

            categorias = new List<string>();
            var series = new SeriesCollection();

            foreach (var c in categoriasData)
            {
                decimal totalDebe = c.Proveedores
                    .SelectMany(p => p.Transacciones)
                    .Where(t => t.TipoTransaccion.Equals("debe", StringComparison.OrdinalIgnoreCase)
                                && t.Fecha.Month == mes
                                && t.Fecha.Year == anio)
                    .Sum(t => t.Monto);

                if (totalDebe != 0)
                {
                    categorias.Add(c.Nombre);
                    series.Add(new PieSeries { Title = c.Nombre, Values = new ChartValues<decimal> { totalDebe } });
                }
            }

            return series;
        }

        /// <summary>
        /// Obtiene el saldo acumulado por mes en el año.
        /// </summary>
        public SeriesCollection ObtenerSaldoAcumuladoAnual(int anio, out List<string> labels)
        {
            labels = new List<string>
    { "Ene","Feb","Mar","Abr","May","Jun","Jul","Ago","Sep","Oct","Nov","Dic" };

            var saldos = _dbContext.Transacciones
                .Where(t => t.Fecha.Year == anio && t.TipoTransaccion == "debe")
                .GroupBy(t => t.Fecha.Month)
                .Select(g => new { Mes = g.Key, Monto = g.Sum(x => x.Monto) })
                .ToList();

            decimal acumulado = 0;
            var valores = new double[12];

            foreach (var m in saldos)
            {
                acumulado += m.Monto;
                valores[m.Mes - 1] = (double)acumulado;
            }

            return new SeriesCollection
    {
        new LineSeries
        {
            Title = $"Saldo acumulado {anio}",
            Values = new ChartValues<double>(valores),
            Stroke = Brushes.OrangeRed,
            Fill = Brushes.Transparent
        }
    };
        }

        /// <summary>
        /// Obtiene Debe/Haber anual por mes.
        /// </summary>
        public SeriesCollection ObtenerDebeHaberAnual(int anio, out List<string> labels)
        {
            labels = new List<string>
    { "Ene","Feb","Mar","Abr","May","Jun","Jul","Ago","Sep","Oct","Nov","Dic" };

            var debe = new double[12];
            var haber = new double[12];

            var movimientos = _dbContext.Transacciones
                .Where(t => t.Fecha.Year == anio)
                .GroupBy(t => new { t.Fecha.Month, t.TipoTransaccion })
                .Select(g => new { g.Key.Month, g.Key.TipoTransaccion, Monto = g.Sum(x => x.Monto) })
                .ToList();

            foreach (var m in movimientos)
            {
                if (m.TipoTransaccion == "debe")
                    debe[m.Month - 1] = (double)m.Monto;
                else
                    haber[m.Month - 1] = (double)m.Monto;
            }

            return new SeriesCollection
    {
        new ColumnSeries
        {
            Title = $"Debe {anio}",
            Values = new ChartValues<double>(debe),
            Fill = Brushes.IndianRed
        },
        new ColumnSeries
        {
            Title = $"Haber {anio}",
            Values = new ChartValues<double>(haber),
            Fill = Brushes.SteelBlue
        }
    };
        }


        /// <summary>
        /// Obtiene gastos por categoría anual.
        /// </summary>
        public SeriesCollection ObtenerGastosPorCategoriaAnual(out List<string> categorias)
        {
            var categoriasData = _dbContext.CategoriaProveedor
                .Include(c => c.Proveedores)
                .ThenInclude(p => p.Transacciones)
                .AsNoTracking()
                .ToList();

            categorias = new List<string>();
            var series = new SeriesCollection();

            foreach (var c in categoriasData)
            {
                decimal total = c.Proveedores
                    .SelectMany(p => p.Transacciones)
                    .Where(t => t.TipoTransaccion == "debe")
                    .Sum(t => t.Monto);

                if (total != 0)
                {
                    categorias.Add(c.Nombre);
                    series.Add(new PieSeries { Title = c.Nombre, Values = new ChartValues<decimal> { total } });
                }
            }

            return series;
        }


        /// <summary>
        /// Obtiene gastos por categoría para un mes específico.
        /// </summary>
        public SeriesCollection ObtenerGastosPorCategoria(out List<string> categorias, int mes, int anio)
        {
            var categoriasData = _dbContext.CategoriaProveedor
                .Include(c => c.Proveedores)
                .AsNoTracking()
                .ToList();

            categorias = new List<string>();
            var series = new SeriesCollection();

            foreach (var c in categoriasData)
            {
                decimal totalDebe = c.Proveedores
                    .Where(p => p.SaldoActual > 0) // asumimos que "Debe" genera saldo positivo
                    .Sum(p => p.SaldoActual);

                if (totalDebe != 0)
                {
                    categorias.Add(c.Nombre);
                    series.Add(new PieSeries
                    {
                        Title = c.Nombre,
                        Values = new ChartValues<decimal> { totalDebe }
                    });
                }
            }

            return series;
        }
    }
}
