using FIDELANDIA.Data;
using FIDELANDIA.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
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
        public object ObtenerStocksParaVista()
        {
            var stocks = _dbContext.StockActual
                                   .Include(s => s.TipoPasta)
                                   .Include(s => s.LotesDisponibles)
                                   .ToList();

            var secciones = stocks.Select(stock => new
            {
                Nombre = stock.TipoPasta.Nombre,
                Filas = stock.LotesDisponibles
                            .OrderBy(l => l.FechaProduccion)
                            .Select(lote => new string[]
                            {
                            lote.IdLote.ToString(),
                            lote.FechaProduccion.ToString("dd/MM/yyyy"),
                            lote.FechaVencimiento.ToString("dd/MM/yyyy"),
                            lote.CantidadDisponible.ToString("0.##") + " paquetes",
                            }).ToArray()
            }).ToArray();

            return secciones;
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

        // Al agregar lote al stock
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

                // Agregarlo a la colección de stock (opcional, para mantener navegación)
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

                // Eliminar lotes agotados
                stock.LotesDisponibles = stock.LotesDisponibles
                                             .Where(l => l.CantidadDisponible > 0)
                                             .ToList();

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
