using FIDELANDIA.Data;
using FIDELANDIA.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FIDELANDIA.Services
{
    public class DashboardService
    {
        private readonly FidelandiaDbContext _dbContext;

        public DashboardService(FidelandiaDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public DashboardResumen ObtenerDashboard(DateTime desde, DateTime hasta)
        {
            int diasPeriodo = (hasta - desde).Days + 1;
            DateTime desdeAnterior = desde.AddDays(-diasPeriodo);
            DateTime hastaAnterior = hasta.AddDays(-diasPeriodo);

            // ===================== CONSULTAS =====================
            var lotes = _dbContext.LoteProduccion
                .Include(l => l.TipoPasta)
                .Where(l => l.FechaProduccion >= desdeAnterior && l.FechaProduccion <= hasta)
                .ToList();

            var ventas = _dbContext.DetalleVenta
                .Include(d => d.Venta)
                .Include(d => d.Lote)
                    .ThenInclude(l => l.TipoPasta)
                .Where(d => d.Venta.Fecha >= desdeAnterior && d.Venta.Fecha <= hasta)
                .ToList();

            // ===================== INDICADORES =====================
            // Cantidad Producida
            decimal cantidadProducidaActual = lotes
                .Where(l => l.FechaProduccion >= desde && l.FechaProduccion <= hasta)
                .Sum(l => l.CantidadProducida);

            decimal cantidadProducidaAnterior = lotes
                .Where(l => l.FechaProduccion >= desdeAnterior && l.FechaProduccion < desde)
                .Sum(l => l.CantidadProducida);

            // Ventas Totales
            decimal ventasTotalesActual = ventas
                .Where(v => v.Venta.Fecha >= desde && v.Venta.Fecha <= hasta)
                .Sum(v => v.Cantidad);

            decimal ventasTotalesAnterior = ventas
                .Where(v => v.Venta.Fecha >= desdeAnterior && v.Venta.Fecha < desde)
                .Sum(v => v.Cantidad);

            // Ticket Promedio
            var ventasActuales = ventas.Where(v => v.Venta.Fecha >= desde && v.Venta.Fecha <= hasta).ToList();
            var ventasAnteriores = ventas.Where(v => v.Venta.Fecha >= desdeAnterior && v.Venta.Fecha < desde).ToList();

            decimal ticketPromedioActual = ventasActuales.Any() ? ventasActuales.Average(v => v.CostoUnitario * v.Cantidad) : 0;
            decimal ticketPromedioAnterior = ventasAnteriores.Any() ? ventasAnteriores.Average(v => v.CostoUnitario * v.Cantidad) : 0;

            // ===================== STOCK =====================
            var stockActualPorTipo = lotes
                .Where(l => l.FechaProduccion <= hasta)
                .GroupBy(l => l.TipoPasta.Nombre)
                .ToDictionary(
                    g => g.Key,
                    g => g.Sum(l => l.CantidadProducida)
                         - ventas.Where(v => v.Lote.TipoPasta.Nombre == g.Key && v.Venta.Fecha <= hasta)
                                 .Sum(v => v.Cantidad)
                );

            var stockAnteriorPorTipo = lotes
                .Where(l => l.FechaProduccion <= hastaAnterior)
                .GroupBy(l => l.TipoPasta.Nombre)
                .ToDictionary(
                    g => g.Key,
                    g => g.Sum(l => l.CantidadProducida)
                         - ventas.Where(v => v.Lote.TipoPasta.Nombre == g.Key && v.Venta.Fecha <= hastaAnterior)
                                 .Sum(v => v.Cantidad)
                );

            decimal stockTotalActual = stockActualPorTipo.Values.Sum();
            decimal stockTotalAnterior = stockAnteriorPorTipo.Values.Sum();

            // ===================== FUNCION DE VARIACION % =====================
            decimal Variacion(decimal actual, decimal anterior) => anterior == 0 ? 0 : (actual - anterior) * 100 / anterior;

            // ===================== AGRUPACIONES =====================
            var produccionPorTipo = lotes
                .Where(l => l.FechaProduccion >= desde && l.FechaProduccion <= hasta)
                .GroupBy(l => l.TipoPasta.Nombre)
                .ToDictionary(g => g.Key, g => g.Sum(l => l.CantidadProducida));

            var ventasPorTipo = ventas
                .Where(v => v.Venta.Fecha >= desde && v.Venta.Fecha <= hasta)
                .GroupBy(v => v.Lote.TipoPasta.Nombre)
                .ToDictionary(g => g.Key, g => g.Sum(v => v.Cantidad));

            // ===================== SERIES DIARIAS =====================
            var rangoFechas = Enumerable.Range(0, diasPeriodo)
                                       .Select(offset => desde.AddDays(offset))
                                       .ToList();

            var produccionDiariaRaw = lotes
                .Where(l => l.FechaProduccion >= desde && l.FechaProduccion <= hasta)
                .GroupBy(l => l.FechaProduccion.Date)
                .ToDictionary(g => g.Key, g => g.Sum(l => l.CantidadProducida));

            var produccionDiaria = rangoFechas
                .Select(f => new { Fecha = f, Cantidad = produccionDiariaRaw.ContainsKey(f) ? produccionDiariaRaw[f] : 0 })
                .ToDictionary(x => x.Fecha.ToString("dd/MM"), x => x.Cantidad);

            var ventasDiariaRaw = ventas
                .Where(v => v.Venta.Fecha >= desde && v.Venta.Fecha <= hasta)
                .GroupBy(v => v.Venta.Fecha.Date)
                .ToDictionary(g => g.Key, g => g.Sum(v => v.Cantidad));

            var ventasDiaria = rangoFechas
                .Select(f => new { Fecha = f, Cantidad = ventasDiariaRaw.ContainsKey(f) ? ventasDiariaRaw[f] : 0 })
                .ToDictionary(x => x.Fecha.ToString("dd/MM"), x => x.Cantidad);

            var recaudacionDiariaRaw = ventas
                .Where(v => v.Venta.Fecha >= desde && v.Venta.Fecha <= hasta)
                .GroupBy(v => v.Venta.Fecha.Date)
                .ToDictionary(g => g.Key, g => g.Sum(v => v.Cantidad * v.CostoUnitario));

            var recaudacionDiaria = rangoFechas
                .Select(f => new { Fecha = f, Monto = recaudacionDiariaRaw.ContainsKey(f) ? recaudacionDiariaRaw[f] : 0 })
                .ToDictionary(x => x.Fecha.ToString("dd/MM"), x => x.Monto);

            // ===================== RESULTADO =====================
            return new DashboardResumen
            {
                CantidadProducida = cantidadProducidaActual,
                VentasTotales = ventasTotalesActual,
                StockPorTipo = stockActualPorTipo,
                TicketPromedio = ticketPromedioActual,

                VariacionCantidadProducida = Variacion(cantidadProducidaActual, cantidadProducidaAnterior),
                VariacionVentasTotales = Variacion(ventasTotalesActual, ventasTotalesAnterior),
                VariacionStockPromedio = Variacion(stockTotalActual, stockTotalAnterior),
                VariacionTicketPromedio = Variacion(ticketPromedioActual, ticketPromedioAnterior),

                ProduccionPorTipo = produccionPorTipo,
                VentasPorTipo = ventasPorTipo,

                ProduccionDiaria = produccionDiaria,
                VentasDiaria = ventasDiaria,
                RecaudacionDiaria = recaudacionDiaria
            };
        }
    }
}
