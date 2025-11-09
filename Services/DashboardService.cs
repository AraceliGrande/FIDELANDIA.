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

        // ----------------------------
        // MODELO DE RESULTADO
        // ----------------------------

        // =====================================================
        // MÉTODO PRINCIPAL: Obtiene todos los datos del dashboard
        // =====================================================
        public DashboardResumen ObtenerDashboard(DateTime desde, DateTime hasta)
        {
            // Incluye relaciones para no hacer N+1 queries
            var lotes = _dbContext.LoteProduccion
                .Include(l => l.TipoPasta)
                .Where(l => l.FechaProduccion >= desde && l.FechaProduccion <= hasta)
                .ToList();

            var ventas = _dbContext.DetalleVenta
                .Include(d => d.Venta)
                .Include(d => d.Lote)
                    .ThenInclude(l => l.TipoPasta)
                .Where(d => d.Venta.Fecha >= desde && d.Venta.Fecha <= hasta)
                .ToList();

            // ===================== AGRUPACIONES =====================

            // Producción por tipo
            var produccionPorTipo = lotes
                .GroupBy(l => l.TipoPasta.Nombre)
                .ToDictionary(g => g.Key, g => g.Sum(l => l.CantidadProducida));

            // Ventas por tipo (basado en los lotes vendidos)
            var ventasPorTipo = ventas
                .GroupBy(v => v.Lote.TipoPasta.Nombre)
                .ToDictionary(g => g.Key, g => g.Sum(v => v.Cantidad));

            // Stock actual por tipo
            var stockPorTipo = _dbContext.LoteProduccion
                .Include(l => l.TipoPasta)
                .GroupBy(l => l.TipoPasta.Nombre)
                .ToDictionary(g => g.Key, g => g.Sum(l => l.CantidadDisponible));

            // ===================== INDICADORES =====================

            var cantidadProducida = produccionPorTipo.Values.Sum();
            var ventasTotales = ventasPorTipo.Values.Sum();
            var stockPromedio = stockPorTipo.Values.Average();
            var ticketPromedio = ventas.Any() ? ventas.Average(v => v.CostoUnitario * v.Cantidad) : 0;

            // ===================== SERIES DIARIAS =====================

            // Generar rango completo de fechas
            var rangoFechas = Enumerable.Range(0, (hasta - desde).Days + 1)
                                       .Select(offset => desde.AddDays(offset))
                                       .ToList();

            // Producción diaria
            var produccionDiariaRaw = lotes
                .GroupBy(l => l.FechaProduccion.Date)
                .ToDictionary(g => g.Key, g => g.Sum(l => l.CantidadProducida));

            var produccionDiaria = rangoFechas
                .Select(f => new
                {
                    Fecha = f,
                    Cantidad = produccionDiariaRaw.ContainsKey(f) ? produccionDiariaRaw[f] : 0
                })
                .OrderBy(x => x.Fecha)
                .ToDictionary(x => x.Fecha.ToString("dd/MM"), x => x.Cantidad);

            // Ventas diaria
            var ventasDiariaRaw = ventas
                .GroupBy(v => v.Venta.Fecha.Date)
                .ToDictionary(g => g.Key, g => g.Sum(v => v.Cantidad));

            var ventasDiaria = rangoFechas
                .Select(f => new
                {
                    Fecha = f,
                    Cantidad = ventasDiariaRaw.ContainsKey(f) ? ventasDiariaRaw[f] : 0
                })
                .OrderBy(x => x.Fecha)
                .ToDictionary(x => x.Fecha.ToString("dd/MM"), x => x.Cantidad);

            // Recaudación diaria
            var recaudacionDiariaRaw = ventas
                .GroupBy(v => v.Venta.Fecha.Date)
                .ToDictionary(g => g.Key, g => g.Sum(v => v.Cantidad * v.CostoUnitario));

            var recaudacionDiaria = rangoFechas
                .Select(f => new
                {
                    Fecha = f,
                    Monto = recaudacionDiariaRaw.ContainsKey(f) ? recaudacionDiariaRaw[f] : 0
                })
                .OrderBy(x => x.Fecha)
                .ToDictionary(x => x.Fecha.ToString("dd/MM"), x => x.Monto);

            // ===================== RESULTADO FINAL =====================
            return new DashboardResumen
            {
                CantidadProducida = cantidadProducida,
                VentasTotales = ventasTotales,
                StockPromedio = stockPromedio,
                TicketPromedio = ticketPromedio,

                ProduccionPorTipo = produccionPorTipo,
                VentasPorTipo = ventasPorTipo,
                StockPorTipo = stockPorTipo,

                ProduccionDiaria = produccionDiaria,
                VentasDiaria = ventasDiaria,
                RecaudacionDiaria = recaudacionDiaria
            };
        }

    }
}
