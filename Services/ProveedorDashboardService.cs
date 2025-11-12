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
    public class ProveedorDashboardService
    {
        private readonly FidelandiaDbContext _dbContext;

        public ProveedorDashboardService(FidelandiaDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        // ================= KPIs =================
        public (int totalProveedores, decimal saldoTotal, int proveedoresCriticos, int transaccionesMes, decimal mayorSaldo) ObtenerKPIs(int mes, int anio)
        {
            var proveedores = _dbContext.Proveedores.AsNoTracking().ToList();
            int totalProveedores = proveedores.Count;
            decimal saldoTotal = proveedores.Sum(p => p.SaldoActual);
            decimal mayorSaldo = proveedores.Max(p => p.SaldoActual);
            int proveedoresCriticos = proveedores.Count(p => p.SaldoActual > 0 || p.SaldoActual > p.LimiteCredito);

            int transaccionesMes = _dbContext.Transacciones
                .Count(t => t.Fecha.Month == mes && t.Fecha.Year == anio);

            return (totalProveedores, saldoTotal, proveedoresCriticos, transaccionesMes, mayorSaldo);
        }

        // ================= Gráficos =================

        // Saldo por proveedor
        public SeriesCollection ObtenerSaldoPorProveedor(out List<string> labels, int mes, int anio)
        {
            var proveedores = _dbContext.Proveedores
                .OrderBy(p => p.Nombre)
                .AsNoTracking()
                .ToList();

            labels = new List<string>();
            var valores = new ChartValues<decimal>();

            foreach (var p in proveedores)
            {
                if (p.SaldoActual != 0)
                {
                    labels.Add(p.Nombre);
                    valores.Add(p.SaldoActual);
                }
            }

            var series = new SeriesCollection
            {
                new ColumnSeries
                {
                    Title = "Saldo",
                    Values = valores,
                    Fill = Brushes.SteelBlue
                }
            };

            return series;
        }

        // Debe/Haber por proveedor
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

        // Movimiento acumulado del mes
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
                    Stroke = Brushes.Orange,
                    Fill = Brushes.Transparent,
                    PointGeometrySize = 6
                }
            };

            return series;
        }

        // Deuda por categoría
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

        // Saldo acumulado por mes
        public SeriesCollection ObtenerSaldoAcumuladoAnual(out List<string> meses)
        {
            meses = Enumerable.Range(1, 12)
                              .Select(m => new DateTime(1, m, 1).ToString("MMM"))
                              .ToList();

            var valores = new ChartValues<decimal>();

            for (int mes = 1; mes <= 12; mes++)
            {
                var saldo = _dbContext.Proveedores.Sum(p =>
                    p.Transacciones
                     .Where(t => t.Fecha.Month <= mes)
                     .Sum(t => t.TipoTransaccion == "debe" ? t.Monto : -t.Monto));
                valores.Add(saldo);
            }

            return new SeriesCollection
    {
        new LineSeries
        {
            Title = "Saldo acumulado",
            Values = valores,
            Stroke = Brushes.Orange,
            Fill = Brushes.Transparent,
            PointGeometrySize = 6
        }
    };
        }

        // Debe/Haber por mes
        public SeriesCollection ObtenerDebeHaberAnual(out List<string> meses)
        {
            meses = Enumerable.Range(1, 12)
                              .Select(m => new DateTime(1, m, 1).ToString("MMM"))
                              .ToList();

            var debe = new ChartValues<decimal>();
            var haber = new ChartValues<decimal>();

            for (int mes = 1; mes <= 12; mes++)
            {
                decimal totalDebe = _dbContext.Transacciones
                    .Where(t => t.Fecha.Month == mes && t.TipoTransaccion == "debe")
                    .Sum(t => t.Monto);
                decimal totalHaber = _dbContext.Transacciones
                    .Where(t => t.Fecha.Month == mes && t.TipoTransaccion == "haber")
                    .Sum(t => t.Monto);

                debe.Add(totalDebe);
                haber.Add(totalHaber);
            }

            return new SeriesCollection
    {
        new ColumnSeries { Title = "Debe", Values = debe, Fill = Brushes.CornflowerBlue },
        new ColumnSeries { Title = "Haber", Values = haber, Fill = Brushes.Orange }
    };
        }

        // Gastos por categoría anual
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


        // Gastos por categoría
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
