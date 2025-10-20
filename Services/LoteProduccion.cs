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

        public LoteProduccionService(FidelandiaDbContext dbContext)
        {
            _dbContext = dbContext;
            _tipoPastaService = new TipoPastaService(dbContext); // para validar el tipo de pasta
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
                    FechaProduccion = fechaProduccion,
                    FechaVencimiento = fechaVencimiento,
                    Estado = estado
                };

                _dbContext.LoteProduccion.Add(lote);
                _dbContext.SaveChanges();
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
    }
}
