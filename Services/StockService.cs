using FIDELANDIA.Data;
using FIDELANDIA.Models;
using FIDELANDIA.ViewModels;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;

namespace FIDELANDIA.Services
{
    public class StockService
    {
        private readonly FidelandiaDbContext _dbContext;

        public StockService(FidelandiaDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        // Obtener todo el stock actualizado
        public ProduccionDatos ObtenerStocksParaVista()
        {
            var stocks = _dbContext.StockActual
                                   .Include(s => s.TipoPasta)
                                   .Include(s => s.LotesDisponibles)
                                   .ToList();

            var hoy = DateTime.Today;

            // Total ventas del día (opcional si lo querés mostrar)
            var ventasDia = _dbContext.Venta
                                      .Include(v => v.DetalleVenta)
                                      .ThenInclude(dv => dv.Lote)
                                      .Where(v => v.Fecha.Date == hoy)
                                      .SelectMany(v => v.DetalleVenta)
                                      .Sum(dv => (int?)dv.Cantidad) ?? 0;

            var produccionVM = new ProduccionDatos();

            foreach (var stock in stocks.Where(s => s.CantidadDisponible > 0))
            {
                var seccion = new StockSeccionViewModel
                {
                    NombreTipoPasta = stock.TipoPasta.Nombre,
                    ContenidoEnvase = stock.TipoPasta.ContenidoEnvase,
                    CantidadDisponible = Math.Truncate(stock.CantidadDisponible),
                    UltimaActualizacion = stock.UltimaActualizacion,
                    Lotes = new ObservableCollection<LoteDetalleViewModel>(
                        stock.LotesDisponibles
                             .OrderBy(l => l.FechaProduccion)
                             .Select(l => new LoteDetalleViewModel
                             {
                                 IdLote = l.IdLote,
                                 FechaProduccion = l.FechaProduccion,
                                 FechaVencimiento = l.FechaVencimiento,
                                 CantidadDisponible = Math.Truncate(l.CantidadDisponible),
                                 CantidadProducida = Math.Truncate(l.CantidadProducida), // ✔ Nueva propiedad
                                 Estado = l.Estado
                             })
                    )
                };

                produccionVM.Secciones.Add(seccion);
            }

            // 🔹 Indicadores
            produccionVM.TotalTipos = produccionVM.Secciones.Count;
            produccionVM.StockTotal = (int)produccionVM.Secciones.Sum(s => s.CantidadDisponible);
            produccionVM.ProduccionTotal = (int)produccionVM.Secciones.Sum(s => s.Lotes.Sum(l => l.CantidadProducida));
            produccionVM.VentasDia = ventasDia;

            return produccionVM;
        }

        public List<LoteProduccionModel> ObtenerLotesDisponibles()
        {
            try
            {
                var lotes = _dbContext.LoteProduccion
                    .Include(l => l.TipoPasta)
                    .Where(l => l.CantidadDisponible > 0 && l.Estado != "Agotado")
                    .OrderBy(l => l.FechaProduccion)
                    .ToList();

                return lotes;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al obtener los lotes disponibles: {ex.Message}");
                return new List<LoteProduccionModel>();
            }
        }

        // Crear stock para un tipo de pasta si no existe
        public StockActualModel CrearOObtenerStock(TipoPastaModel tipoPasta)
        {
            var stock = _dbContext.StockActual
                                  .Include(s => s.LotesDisponibles)
                                  .FirstOrDefault(s => s.IdTipoPasta == tipoPasta.IdTipoPasta);

            if (stock == null)
            {
                stock = new StockActualModel
                {
                    TipoPasta = tipoPasta,
                    CantidadDisponible = 0
                };
                _dbContext.StockActual.Add(stock);
                _dbContext.SaveChanges();
            }

            return stock;
        }

        // Agregar lote al stock
        public bool AgregarLoteAlStock(LoteProduccionModel lote)
        {
            try
            {
                // Buscar stock
                var stock = _dbContext.StockActual
                                      .Include(s => s.LotesDisponibles)
                                      .FirstOrDefault(s => s.IdTipoPasta == lote.IdTipoPasta);

                if (stock == null)
                {
                    var tipoPasta = _dbContext.TiposPasta.Find(lote.IdTipoPasta);
                    if (tipoPasta == null) throw new Exception("No se encontró el tipo de pasta.");
                    stock = CrearOObtenerStock(tipoPasta);
                }

                // ⚡ Vincular lote al stock
                lote.IdStockActual = stock.IdStock;

                // Agregarlo a la colección de stock (solo para navegación en memoria)
                stock.LotesDisponibles.Add(lote);

                // Actualizar stock
                stock.CantidadDisponible += lote.CantidadDisponible;
                stock.UltimaActualizacion = DateTime.Now;

                _dbContext.SaveChanges();
                return true;
            }
            catch (DbUpdateException dbEx)
            {
                var detalle = dbEx.InnerException?.Message ?? dbEx.Message;
                MessageBox.Show($"❌ Error al actualizar stock (DB):\n{detalle}");
                return false;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"❌ Error al actualizar stock:\n{ex.Message}");
                return false;
            }
        }

        // Descontar stock al vender
        public bool DescontarStock(int idTipoPasta, decimal cantidad)
        {
            try
            {
                var stock = _dbContext.StockActual
                                      .Include(s => s.LotesDisponibles)
                                      .FirstOrDefault(s => s.IdTipoPasta == idTipoPasta);

                if (stock == null) throw new Exception("No existe stock para este tipo de pasta.");
                if (cantidad > stock.CantidadDisponible) throw new Exception("Stock insuficiente.");

                decimal restante = cantidad;

                foreach (var lote in stock.LotesDisponibles.OrderBy(l => l.FechaProduccion).ToList())
                {
                    if (lote.CantidadDisponible >= restante)
                    {
                        lote.CantidadDisponible -= restante;
                        restante = 0;
                        break;
                    }
                    else
                    {
                        restante -= lote.CantidadDisponible;
                        lote.CantidadDisponible = 0;
                    }
                }

                // Desvincular lotes agotados
                var lotesAgotados = stock.LotesDisponibles
                                         .Where(l => l.CantidadDisponible <= 0)
                                         .ToList();

                foreach (var lote in lotesAgotados)
                {
                    lote.IdStockActual = null; // ⚡ desvincula de la DB
                    stock.LotesDisponibles.Remove(lote); // opcional, navegación en memoria
                }

                stock.CantidadDisponible -= cantidad;
                stock.UltimaActualizacion = DateTime.Now;

                _dbContext.SaveChanges();
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"❌ Error al descontar stock:\n{ex.Message}");
                return false;
            }
        }
    }
}
