using FIDELANDIA.Data;
using FIDELANDIA.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FIDELANDIA.Services
{
    /// <summary>
    /// Servicio encargado de manejar operaciones de venta,
    /// incluyendo creación de ventas, actualización de stock y eliminación de detalles.
    /// </summary>
    public class VentaService
    {
        private readonly FidelandiaDbContext _context;
        private readonly StockService _stockService;

        public VentaService(FidelandiaDbContext context, StockService stockService)
        {
            _context = context;
            _stockService = stockService;
        }

        /// <summary>
        /// Crea una venta y descuenta stock de los lotes correspondientes.
        /// Si falla alguna operación, se revierte todo con una transacción.
        /// </summary>
        public async Task<VentaModel> CrearVentaAsync(List<DetalleVentaModel> detalleVentas)
        {
            if (detalleVentas == null || !detalleVentas.Any())
                throw new ArgumentException("Debe proveer al menos un detalle de venta.");

            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var venta = new VentaModel
                {
                    Fecha = DateTime.Now,
                    DetalleVenta = new List<DetalleVentaModel>()
                };

                foreach (var detalle in detalleVentas)
                {
                    // Traer lote
                    var lote = await _context.LoteProduccion
                                             .FirstOrDefaultAsync(l => l.IdLote == detalle.IdLote);

                    if (lote == null)
                        throw new Exception($"No se encontró el lote con Id {detalle.IdLote}");

                    if (detalle.Cantidad > lote.CantidadDisponible)
                        throw new Exception($"Cantidad a vender ({detalle.Cantidad}) excede stock disponible ({lote.CantidadDisponible}) del lote {detalle.IdLote}");

                    // Descontar stock usando el service
                    bool stockActualizado = _stockService.DescontarStock(lote.IdTipoPasta, detalle.Cantidad);

                    if (!stockActualizado)
                        throw new Exception($"No se pudo descontar el stock para el tipo de pasta Id {lote.IdTipoPasta}");

                    // Agregar detalle a la venta (solo guardamos la cantidad y el lote)
                    venta.DetalleVenta.Add(new DetalleVentaModel
                    {
                        IdLote = detalle.IdLote,
                        Cantidad = detalle.Cantidad,
                        CostoUnitario = detalle.CostoUnitario
                    });
                }

                // Guardar venta + detalles
                _context.Venta.Add(venta);
                await _context.SaveChangesAsync();

                await transaction.CommitAsync();
                return venta;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        // ================= Eliminar detalle de venta =================
        /// <summary>
        /// Elimina un detalle de venta y revierte stock y lote correspondiente.
        /// Si no quedan detalles, elimina la venta completa.
        /// </summary>

        public async Task<bool> EliminarDetalleVentaAsync(int idVenta, int idLote)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                // Buscar detalle de venta
                var ventaDetalle = await _context.DetalleVenta
                    .Include(v => v.Lote)
                        .ThenInclude(l => l.TipoPasta)
                    .FirstOrDefaultAsync(v => v.IdVenta == idVenta && v.IdLote == idLote);

                if (ventaDetalle == null)
                    throw new Exception("No se encontró el detalle de venta.");

                // Buscar lote
                var lote = await _context.LoteProduccion
                    .Include(l => l.StockActual)
                    .FirstOrDefaultAsync(l => l.IdLote == ventaDetalle.IdLote);

                if (lote == null)
                    throw new Exception("No se encontró el lote asociado a esta venta.");

                // Buscar stock correspondiente
                var stock = await _context.StockActual
                    .Include(s => s.LotesDisponibles)
                    .FirstOrDefaultAsync(s => s.IdTipoPasta == lote.IdTipoPasta);

                // Crear stock si no existe
                if (stock == null)
                {
                    stock = new StockActualModel
                    {
                        IdTipoPasta = lote.IdTipoPasta,
                        CantidadDisponible = 0,
                        UltimaActualizacion = DateTime.Now
                    };
                    _context.StockActual.Add(stock);
                    await _context.SaveChangesAsync();
                }

                // Revertir stock y lote
                lote.CantidadDisponible += ventaDetalle.Cantidad;
                stock.CantidadDisponible += ventaDetalle.Cantidad;
                stock.UltimaActualizacion = DateTime.Now;

                // Vincular lote si no está vinculado
                if (!stock.LotesDisponibles.Any(l => l.IdLote == lote.IdLote))
                {
                    stock.LotesDisponibles.Add(lote);
                    lote.IdStockActual = stock.IdStock;
                    lote.StockActual = stock;
                }

                // Eliminar el detalle
                _context.DetalleVenta.Remove(ventaDetalle);

                // Verificar si quedan otros detalles de la venta
                bool hayOtrosDetalles = await _context.DetalleVenta.AnyAsync(d => d.IdVenta == idVenta);

                // Si no quedan otros detalles, eliminar la venta completa
                if (!hayOtrosDetalles)
                {
                    var venta = await _context.Venta.FindAsync(idVenta);
                    if (venta != null)
                    {
                        _context.Venta.Remove(venta);
                    }
                }

                // Guardar todos los cambios
                await _context.SaveChangesAsync();

                // Si todo sale bien, confirmar transacción
                await transaction.CommitAsync();

                return true;
            }
            catch (Exception ex)
            {
                // Si hay cualquier error, volver todo atrás
                await transaction.RollbackAsync();
                throw new Exception($"No se pudo eliminar el detalle de venta. Se canceló la operación.\n\nDetalles: {ex.Message}");
            }
        }

    }
}
