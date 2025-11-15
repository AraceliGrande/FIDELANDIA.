using FIDELANDIA.Data;
using FIDELANDIA.Models;
using FIDELANDIA.Views.Produccion;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FIDELANDIA.Services
{
    /// <summary>
    /// Servicio encargado de calcular balances diarios de producción, ventas y stock de pastas.
    /// </summary>
    public class BalanceService
    {
        private readonly FidelandiaDbContext _dbContext;

        /// <summary>
        /// Constructor que recibe el DbContext de la aplicación.
        /// </summary>
        public BalanceService(FidelandiaDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        /// <summary>
        /// Resultado detallado del balance diario, incluyendo lotes producidos hoy, ventas de hoy y lotes antiguos con ventas.
        /// </summary>
        public class BalanceDiarioResultado
        {
            public List<LoteDetalleViewModel1> LotesProduccionHoy { get; set; }           // Lotes producidos en el día
            public List<DetalleVentaModel> VentasHoy { get; set; }                        // Ventas realizadas hoy
            public List<LoteDetalleViewModel1> LotesAntiguosConVentasHoy { get; set; }    // Lotes de días anteriores que tuvieron ventas hoy
        }

        /// <summary>
        /// Obtiene los lotes de producción y ventas del día actual, y los lotes antiguos que registraron ventas hoy.
        /// </summary>
        /// <returns>Objeto BalanceDiarioResultado con toda la información de lotes y ventas.</returns>
        public BalanceDiarioResultado ObtenerLotesDetalle()
        {
            try
            {
                DateTime hoy = DateTime.Today;

                // 🔹 Traemos todos los lotes con su tipo de pasta y stock
                var lotes = _dbContext.LoteProduccion
                    .Include(l => l.TipoPasta)      // Incluye relación con tipo de pasta
                    .Include(l => l.StockActual)    // Incluye relación con stock actual
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
            catch (Exception ex)
            {
                // Captura cualquier error durante la obtención de datos
                throw new Exception("Error al obtener los detalles de los lotes: " + ex.Message, ex);
            }
        }

        /// <summary>
        /// Obtiene un resumen diario por tipo de pasta (para gráfico y DataGrids)
        /// </summary>
        /// <returns>Lista de BalanceDiarioModel con resumen por tipo de pasta.</returns>
        public List<BalanceDiarioModel> ObtenerBalanceDiario()
        {
            try
            {
                var lotesDetalle = ObtenerLotesDetalle();   // Obtenemos todos los lotes y ventas del día
                var lotesProduccionHoy = lotesDetalle.LotesProduccionHoy;

                // 🔹 Agrupamos por tipo de pasta y calculamos totales de producción, ventas y stock
                var balancePorTipo = lotesProduccionHoy
                    .GroupBy(l => l.TipoPasta)
                    .Select(g => new BalanceDiarioModel
                    {
                        TipoPasta = g.Key,
                        CantidadProducida = g.Sum(l => l.CantidadProduccion),
                        CantidadVendida = g.Sum(l => l.VentasTotales),
                        StockActual = g.First().Stock   // Tomamos el stock del primer lote como referencia
                    })
                    .ToList();

                return balancePorTipo;
            }
            catch (Exception ex)
            {
                // Captura cualquier error durante el cálculo del balance
                throw new Exception("Error al calcular el balance diario: " + ex.Message, ex);
            }
        }
    }
}
