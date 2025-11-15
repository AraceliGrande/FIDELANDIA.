using FIDELANDIA.Data;
using FIDELANDIA.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace FIDELANDIA.Services
{
    /// <summary>
    /// Servicio encargado de manejar la lógica de lotes de producción.
    /// </summary>
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

        /// <summary>
        /// Crear un nuevo lote de producción y agregarlo al stock.
        /// </summary>
        public LoteProduccionModel CrearLote(int idTipoPasta, decimal cantidadDisponible, DateTime fechaProduccion,
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

                return lote;
            }
            catch (Exception ex)
            {
                // Mostrar detalle del error
                var detalle = ex.Message + Environment.NewLine + (ex.InnerException?.Message ?? "");
                MessageBox.Show(
                          $"Ocurrió un error al crear el tipo de pasta:{Environment.NewLine}{detalle}",
                          "Error al crear tipo de pasta",            // Título del MessageBox
                          MessageBoxButton.OK,                       // Botón OK
                          MessageBoxImage.Error                      // Icono de error
                      ); 
                return null;
            }
        }

        /// <summary>
        /// Obtener todos los lotes de producción, ordenados por fecha.
        /// </summary>
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
                MessageBox.Show(
                      $"Ocurrió un error al obtener lotes de producción:{Environment.NewLine}{ex.Message}",
                      "Error al obtener lotes",           // Título del MessageBox
                      MessageBoxButton.OK,                 // Botón OK
                      MessageBoxImage.Error                // Icono de error
                  );
                return new List<LoteProduccionModel>();
            }
        }

        /// <summary>
        /// Obtener un lote por su Id.
        /// </summary>
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
                MessageBox.Show(
                    $"Ocurrió un error al obtener el lote de producción:{Environment.NewLine}{ex.Message}",
                    "Error de Lote de Producción",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                ); return null;
            }
        }

        /// <summary>
        /// Eliminar un lote de producción junto con sus ventas asociadas y actualizar stock.
        /// </summary>
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

        /// <summary>
        /// Registrar defectos en un lote de producción, creando un nuevo lote "Defectuoso" y actualizando stock.
        /// </summary>
        public bool RegistrarDefectos(int idLote, decimal cantidadDefectuosa)
        {
            using var transaction = _dbContext.Database.BeginTransaction(); // Inicia transacción
            try
            {
                var loteOriginal = _dbContext.LoteProduccion
                    .Include(l => l.TipoPasta)
                    .Include(l => l.StockActual)
                    .FirstOrDefault(l => l.IdLote == idLote);

                if (loteOriginal == null)
                {
                    MessageBox.Show(
                        "No se encontró el lote especificado.",
                        "Error de Lote de Producción",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error
                    );
                    return false;
                }

                if (cantidadDefectuosa <= 0)
                {
                    MessageBox.Show(
                        "La cantidad defectuosa debe ser mayor a 0.",
                        "Advertencia",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning
                    );
                    return false;
                }

                // 1. Restar cantidad defectuosa del lote original
                loteOriginal.CantidadDisponible -= cantidadDefectuosa;
                if (loteOriginal.CantidadDisponible < 0)
                    loteOriginal.CantidadDisponible = 0;

                loteOriginal.CantidadProducida -= cantidadDefectuosa;
                if (loteOriginal.CantidadProducida < 0)
                    loteOriginal.CantidadProducida = 0;

                // 2. Restar cantidad del stock general
                var stock = _dbContext.StockActual
                    .Include(s => s.LotesDisponibles)
                    .FirstOrDefault(s => s.IdTipoPasta == loteOriginal.IdTipoPasta);

                if (stock != null)
                {
                    stock.CantidadDisponible -= cantidadDefectuosa;
                    if (stock.CantidadDisponible < 0)
                        stock.CantidadDisponible = 0;

                    stock.UltimaActualizacion = DateTime.Now;

                    // 3. Desvincular lotes agotados
                    var lotesAgotados = stock.LotesDisponibles
                                             .Where(l => l.CantidadDisponible <= 0)
                                             .ToList();

                    foreach (var lote in lotesAgotados)
                    {
                        stock.LotesDisponibles.Remove(lote);
                    }
                }

                // 4. Crear lote "Defectuoso"
                var loteDefectuoso = new LoteProduccionModel
                {
                    IdTipoPasta = loteOriginal.IdTipoPasta,
                    CantidadProducida = cantidadDefectuosa,
                    CantidadDisponible = 0,
                    FechaProduccion = loteOriginal.FechaProduccion,
                    FechaVencimiento = loteOriginal.FechaVencimiento,
                    Estado = loteOriginal.FechaVencimiento < DateTime.Today ? "Vencido" : "Defectuoso"
                };

                _dbContext.LoteProduccion.Add(loteDefectuoso);

                // 5. Guardar cambios
                _dbContext.SaveChanges();

                transaction.Commit(); // Confirma transacción

                return true;
            }
            catch (Exception ex)
            {
                transaction.Rollback(); // Revertir cambios
                MessageBox.Show(
                        $"Error al registrar defectos: {ex.Message}",  // mensaje con detalles del error
                        "Error",                                        // título de la ventana
                        MessageBoxButton.OK,                            // botón disponible
                        MessageBoxImage.Error                            // icono de error
                    );

                return false;
            }
        }

    }
}
