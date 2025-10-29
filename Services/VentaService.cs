using FIDELANDIA.Data;
using FIDELANDIA.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FIDELANDIA.Services
{
    public class VentaService
    {
        private readonly FidelandiaDbContext _context;
        private readonly StockService _stockService;

        public VentaService(FidelandiaDbContext context, StockService stockService)
        {
            _context = context;
            _stockService = stockService;
        }

        /// Crea una venta, actualiza lotes y descuenta stock actual.
        /// Si falla el descuento de stock, no se guarda nada.
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

    }
}
