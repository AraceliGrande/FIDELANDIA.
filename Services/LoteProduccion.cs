using FIDELANDIA.Data;
using FIDELANDIA.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace FIDELANDIA.Services
{
    public class LoteProduccionService
    {
        private readonly FidelandiaDbContext _dbContext;
        private readonly TipoPastaService _tipoPastaService;
        private readonly StockService _stockService;

        public LoteProduccionService(FidelandiaDbContext dbContext)
        {
            _dbContext = dbContext;
            _tipoPastaService = new TipoPastaService(dbContext);
            _stockService = new StockService(dbContext);
        }

        // Crear un nuevo lote de producción
        public bool CrearLote(int idTipoPasta, decimal cantidadDisponible, DateTime fechaProduccion,
                              DateTime fechaVencimiento, string estado)
        {
            try
            {
               
                var lote = new LoteProduccionModel
                {
                    IdTipoPasta = idTipoPasta,
                    CantidadDisponible = cantidadDisponible,
                    CantidadProducida = cantidadDisponible,
                    FechaProduccion = fechaProduccion,
                    FechaVencimiento = fechaVencimiento,
                    Estado = estado
                };

                _dbContext.LoteProduccion.Add(lote);
                _dbContext.SaveChanges();
                
                _stockService.AgregarLoteAlStock(lote);

                return true;
            }
            catch (Exception ex)
            {
                // fallback general
                var detalle = ex.Message + Environment.NewLine + (ex.InnerException?.Message ?? "");
                MessageBox.Show($"Error al crear tipo de pasta:{Environment.NewLine}{detalle}");
                return false;
            }
        }

        // Traer todos los lotes
        public List<LoteProduccionModel> ObtenerTodos()
        {
            try
            {
                return _dbContext.LoteProduccion
                                 .Include(l => l.TipoPasta)
                                 .OrderByDescending(l => l.FechaProduccion)
                                 .ToList();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al obtener lotes de producción: {ex.Message}");
                return new List<LoteProduccionModel>();
            }
        }

        // Obtener un lote por Id
        public LoteProduccionModel? ObtenerPorId(int idLote)
        {
            try
            {
                return _dbContext.LoteProduccion
                                 .Include(l => l.TipoPasta)
                                 .FirstOrDefault(l => l.IdLote == idLote);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al obtener lote de producción: {ex.Message}");
                return null;
            }
        }

        public async Task<(bool ok, string mensaje)> EliminarLoteAsync(int idLote)
        {
            using var transaction = await _dbContext.Database.BeginTransactionAsync();
            try
            {
                var lote = await _dbContext.LoteProduccion
                    .Include(l => l.TipoPasta)
                    .Include(l => l.StockActual)
                    .FirstOrDefaultAsync(l => l.IdLote == idLote);

                if (lote == null)
                    return (false, "No se encontró el lote especificado.");

                // 🔹 Obtener los detalles de venta vinculados
                var detalles = await _dbContext.DetalleVenta
                    .Include(d => d.Venta)
                    .Where(d => d.IdLote == idLote)
                    .ToListAsync();

                // 🔹 Eliminar los detalles y ventas vacías
                foreach (var detalle in detalles)
                {
                    _dbContext.DetalleVenta.Remove(detalle);
                    await _dbContext.SaveChangesAsync();

                    var venta = await _dbContext.Venta
                        .Include(v => v.DetalleVenta)
                        .FirstOrDefaultAsync(v => v.IdVenta == detalle.IdVenta);

                    if (venta != null && !venta.DetalleVenta.Any())
                        _dbContext.Venta.Remove(venta);
                }

                // 🔹 Actualizar stock (restar lo producido)
                var stock = await _dbContext.StockActual
                    .FirstOrDefaultAsync(s => s.IdTipoPasta == lote.IdTipoPasta);

                if (stock != null)
                {
                    stock.CantidadDisponible -= lote.CantidadProducida;
                    if (stock.CantidadDisponible < 0)
                        stock.CantidadDisponible = 0;
                    stock.UltimaActualizacion = DateTime.Now;
                    _dbContext.StockActual.Update(stock);
                }

                // 🔹 Eliminar el lote
                _dbContext.LoteProduccion.Remove(lote);
                await _dbContext.SaveChangesAsync();

                await transaction.CommitAsync();
                return (true, "Producción eliminada correctamente junto con sus dependencias.");
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return (false, $"Error al eliminar producción: {ex.Message}");
            }
        }
    }
}
