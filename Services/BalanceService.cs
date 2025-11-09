using FIDELANDIA.Data;
using FIDELANDIA.Models;
using FIDELANDIA.Views.Produccion;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FIDELANDIA.Services
{
    public class BalanceService
    {
        private readonly FidelandiaDbContext _dbContext;

        public BalanceService(FidelandiaDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        /// <summary>
        /// Obtiene todos los lotes con su detalle de producción, ventas y stock actual.
        /// </summary>
        public class BalanceDiarioResultado
        {
            public List<LoteDetalleViewModel1> LotesProduccionHoy { get; set; }
            public List<DetalleVentaModel> VentasHoy { get; set; }
            public List<LoteDetalleViewModel1> LotesAntiguosConVentasHoy { get; set; }
        }

        public BalanceDiarioResultado ObtenerLotesDetalle()
        {
            DateTime hoy = DateTime.Today;

            // 🔹 Traemos todos los lotes con su tipo de pasta y stock
            var lotes = _dbContext.LoteProduccion
                .Include(l => l.TipoPasta)
                .Include(l => l.StockActual)
                .ToList();

            // 🔹 Traemos todas las ventas de hoy con su lote y tipo de pasta
            var ventasHoy = _dbContext.DetalleVenta
                .Include(dv => dv.Venta)
                .Include(dv => dv.Lote)
                    .ThenInclude(l => l.TipoPasta)
                .Where(dv => dv.Venta.Fecha.Date == hoy)
                .ToList();

            // 🔹 Lotes producidos hoy (con o sin ventas)
            var lotesProduccionHoy = lotes
                .Where(l => l.FechaProduccion.Date == hoy)
                .Select(l => new LoteDetalleViewModel1
                {
                    IdLote = l.IdLote,
                    TipoPasta = l.TipoPasta.Nombre,
                    CantidadDisponible = l.CantidadDisponible,
                    CantidadProduccion = l.CantidadProducida,
                    FechaProduccion = l.FechaProduccion,
                    FechaVencimiento = l.FechaVencimiento,
                    Stock = l.StockActual?.CantidadDisponible ?? 0,
                    VentasTotales = ventasHoy.Where(v => v.IdLote == l.IdLote).Sum(v => v.Cantidad),
                    VentasAsociadas = ventasHoy.Where(v => v.IdLote == l.IdLote).ToList(),
                    Estado = l.Estado
                })
                .ToList();

            // 🔹 Lotes antiguos (anteriores a hoy) que tuvieron ventas hoy
            var lotesAntiguosConVentasHoy = lotes
                .Where(l => l.FechaProduccion.Date < hoy && ventasHoy.Any(v => v.IdLote == l.IdLote))
                .Select(l => new LoteDetalleViewModel1
                {
                    IdLote = l.IdLote,
                    TipoPasta = l.TipoPasta.Nombre,
                    CantidadDisponible = l.CantidadDisponible,
                    CantidadProduccion = l.CantidadProducida,
                    FechaProduccion = l.FechaProduccion,
                    FechaVencimiento = l.FechaVencimiento,
                    Stock = l.StockActual?.CantidadDisponible ?? 0,
                    VentasTotales = ventasHoy.Where(v => v.IdLote == l.IdLote).Sum(v => v.Cantidad),
                    VentasAsociadas = ventasHoy.Where(v => v.IdLote == l.IdLote).ToList(),
                    Estado = l.Estado
                })
                .ToList();

            return new BalanceDiarioResultado
            {
                LotesProduccionHoy = lotesProduccionHoy,
                VentasHoy = ventasHoy,
                LotesAntiguosConVentasHoy = lotesAntiguosConVentasHoy
            };
        }


        /// <summary>
        /// Obtiene un resumen diario por tipo de pasta (para gráfico y DataGrids)
        /// </summary>
        public List<BalanceDiarioModel> ObtenerBalanceDiario()
        {
            var lotesDetalle = ObtenerLotesDetalle();

            var lotesProduccionHoy = lotesDetalle.LotesProduccionHoy;

            // Agrupamos por tipo de pasta y calculamos totales
            var balancePorTipo = lotesProduccionHoy
                .GroupBy(l => l.TipoPasta)
                .Select(g => new BalanceDiarioModel
                {
                    TipoPasta = g.Key,
                    CantidadProducida = g.Sum(l => l.CantidadProduccion),
                    CantidadVendida = g.Sum(l => l.VentasTotales),
                    StockActual = g.First().Stock
                })
                .ToList();

            return balancePorTipo;
        }
    }
}
