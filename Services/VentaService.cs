using FIDELANDIA.Data;
using FIDELANDIA.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Windows;

namespace FIDELANDIA.Services
{
    public class VentaService
    {
        private readonly FidelandiaDbContext _dbContext;

        public VentaService(FidelandiaDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        /// <summary>
        /// Registra una venta asociada a un lote, descuenta del stock y actualiza el estado del lote.
        /// </summary>
        public bool RegistrarVenta(int idLote, decimal cantidad, decimal costoUnitario)
        {
            try
            {
                // 🔹 Buscar el lote de producción
                var lote = _dbContext.LoteProduccion
                    .Include(l => l.TipoPasta)
                    .FirstOrDefault(l => l.IdLote == idLote);

                if (lote == null)
                {
                    MessageBox.Show("No se encontró el lote especificado.");
                    return false;
                }

                if (lote.CantidadDisponible < cantidad)
                {
                    MessageBox.Show("No hay suficiente cantidad disponible en el lote.");
                    return false;
                }

                // 🔹 Crear la venta
                var venta = new VentaModel
                {
                    Fecha = DateTime.Now
                };

                _dbContext.Venta.Add(venta);
                _dbContext.SaveChanges(); // Guardamos para obtener el IdVenta generado

                // 🔹 Crear el detalle de la venta
                var detalle = new DetalleVentaModel
                {
                    IdVenta = venta.IdVenta,
                    IdLote = lote.IdLote,
                    Cantidad = cantidad,
                    CostoUnitario = costoUnitario
                };

                _dbContext.DetalleVenta.Add(detalle);

                // 🔹 Actualizar cantidad del lote
                lote.CantidadDisponible -= cantidad;

                if (lote.CantidadDisponible <= 0)
                {
                    lote.CantidadDisponible = 0;
                    lote.Estado = "Agotado";
                }

                // 🔹 Actualizar el stock actual
                var stock = _dbContext.StockActual
                    .FirstOrDefault(s => s.IdTipoPasta == lote.IdTipoPasta);

                if (stock != null)
                {
                    stock.CantidadDisponible -= cantidad;
                    stock.UltimaActualizacion = DateTime.Now;

                    if (stock.CantidadDisponible <= 0)
                    {
                        _dbContext.StockActual.Remove(stock);
                    }
                }

                _dbContext.SaveChanges();

                MessageBox.Show("✅ Venta registrada correctamente.");
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"❌ Error al registrar la venta: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Devuelve todas las ventas con sus detalles y tipo de pasta asociado (opcional).
        /// </summary>
        public List<VentaModel> ObtenerVentasConDetalles()
        {
            try
            {
                return _dbContext.Venta
                    .Include(v => v.DetalleVenta)
                        .ThenInclude(d => d.Lote)
                            .ThenInclude(l => l.TipoPasta)
                    .OrderByDescending(v => v.Fecha)
                    .ToList();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al obtener las ventas: {ex.Message}");
                return new List<VentaModel>();
            }
        }
    }
}

