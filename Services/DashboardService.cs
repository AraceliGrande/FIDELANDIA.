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
                .Where(l => l.FechaProduccion >= desdeAnterior && l.FechaProduccion <= hasta
                            && (l.Estado == "Creado" || l.Estado == "Confirmado"))
                .ToList();

            var ventas = _dbContext.DetalleVenta
                .Include(d => d.Venta)
                .Include(d => d.Lote)
                    .ThenInclude(l => l.TipoPasta)
                .Where(d => d.Venta.Fecha >= desdeAnterior && d.Venta.Fecha <= hasta
                            && (d.Lote.Estado == "Creado" || d.Lote.Estado == "Confirmado"))
                .ToList();

            // ===================== INDICADORES =====================
            // Cantidad producida en envases
            decimal cantidadProducidaActual = lotes
                .Where(l => l.FechaProduccion >= desde && l.FechaProduccion <= hasta)
                .Sum(l => l.CantidadProducida);

            decimal cantidadProducidaAnterior = lotes
                .Where(l => l.FechaProduccion >= desdeAnterior && l.FechaProduccion < desde)
                .Sum(l => l.CantidadProducida);

            // Producción en kilogramos
            decimal produccionKgActual = lotes
                .Where(l => l.FechaProduccion >= desde && l.FechaProduccion <= hasta)
                .Sum(l => l.CantidadProducida * l.TipoPasta.ContenidoEnvase);

            decimal produccionKgAnterior = lotes
                .Where(l => l.FechaProduccion >= desdeAnterior && l.FechaProduccion < desde)
                .Sum(l => l.CantidadProducida * l.TipoPasta.ContenidoEnvase);

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

            decimal ticketPromedioActual = ventasActuales.Any()
                ? ventasActuales.Sum(d => d.Cantidad * d.CostoUnitario) / ventasActuales.Count
                : 0;

            decimal ticketPromedioAnterior = ventasAnteriores.Any()
                ? ventasAnteriores.Sum(d => d.Cantidad * d.CostoUnitario) / ventasAnteriores.Count
                : 0;

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

            // Producción diaria en envases
            var produccionDiariaEnvasesRaw = lotes
                .Where(l => l.FechaProduccion >= desde && l.FechaProduccion <= hasta)
                .GroupBy(l => l.FechaProduccion.Date)
                .ToDictionary(g => g.Key, g => g.Sum(l => l.CantidadProducida));

            var produccionDiariaEnvases = rangoFechas
                .OrderBy(f => f)
                .ToDictionary(
                    f => f.ToString("dd/MM"),
                    f => produccionDiariaEnvasesRaw.ContainsKey(f.Date) ? produccionDiariaEnvasesRaw[f.Date] : 0
                );

            // Producción diaria en kg
            var produccionDiariaKgRaw = lotes
                .Where(l => l.FechaProduccion >= desde && l.FechaProduccion <= hasta)
                .GroupBy(l => l.FechaProduccion.Date)
                .ToDictionary(g => g.Key, g => g.Sum(l => l.CantidadProducida * l.TipoPasta.ContenidoEnvase));

            var produccionDiariaKg = rangoFechas
                .OrderBy(f => f)
                .ToDictionary(
                    f => f.ToString("dd/MM"),
                    f => produccionDiariaKgRaw.ContainsKey(f.Date) ? produccionDiariaKgRaw[f.Date] : 0
                );

            // Ventas diarias
            var ventasDiariaRaw = ventas
                .Where(v => v.Venta.Fecha >= desde && v.Venta.Fecha <= hasta)
                .GroupBy(v => v.Venta.Fecha.Date)
                .ToDictionary(g => g.Key, g => g.Sum(v => v.Cantidad));

            var ventasDiaria = rangoFechas
                .OrderBy(f => f)
                .ToDictionary(
                    f => f.ToString("dd/MM"),
                    f => ventasDiariaRaw.ContainsKey(f.Date) ? ventasDiariaRaw[f.Date] : 0
                );

            // Recaudación diaria
            var recaudacionDiariaRaw = ventas
                .Where(v => v.Venta.Fecha >= desde && v.Venta.Fecha <= hasta)
                .GroupBy(v => v.Venta.Fecha.Date)
                .ToDictionary(g => g.Key, g => g.Sum(v => v.Cantidad * v.CostoUnitario));

            var recaudacionDiaria = rangoFechas
                .OrderBy(f => f)
                .ToDictionary(
                    f => f.ToString("dd/MM"),
                    f => recaudacionDiariaRaw.ContainsKey(f.Date) ? recaudacionDiariaRaw[f.Date] : 0
                );

            // ===================== RESULTADO =====================
            return new DashboardResumen
            {
                CantidadProducida = cantidadProducidaActual,         // en envases
                VentasTotales = ventasTotalesActual,
                ProduccionKg = produccionKgActual,                  // en kg
                TicketPromedio = ticketPromedioActual,

                VariacionCantidadProducida = Variacion(cantidadProducidaActual, cantidadProducidaAnterior),
                VariacionVentasTotales = Variacion(ventasTotalesActual, ventasTotalesAnterior),
                VariacionProduccionKg = Variacion(produccionKgActual, produccionKgAnterior),
                VariacionTicketPromedio = Variacion(ticketPromedioActual, ticketPromedioAnterior),

                ProduccionPorTipo = produccionPorTipo,
                VentasPorTipo = ventasPorTipo,
                StockPorTipo = stockActualPorTipo,

                ProduccionDiariaEnvases = produccionDiariaEnvases,
                ProduccionDiariaKg = produccionDiariaKg,
                VentasDiaria = ventasDiaria,
                RecaudacionDiaria = recaudacionDiaria
            };
        }
  
    }
}
