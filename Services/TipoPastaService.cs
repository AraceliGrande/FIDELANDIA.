using FIDELANDIA.Data;
using FIDELANDIA.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace FIDELANDIA.Services
{
    public class TipoPastaService
    {
        private readonly FidelandiaDbContext _dbContext;

        public TipoPastaService(FidelandiaDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        // Traer todos los tipos de pasta ordenados por nombre
        public List<TipoPastaModel> ObtenerTodos()
        {
            try
            {
                return _dbContext.TiposPasta
                                 .OrderBy(tp => tp.Nombre)
                                 .ToList();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al obtener tipos de pasta: {ex.Message}");
                return new List<TipoPastaModel>();
            }
        }

        // Crear un nuevo tipo de pasta
        public bool CrearTipoPasta(string nombre, decimal contenidoEnvase, string descripcion, decimal costoActual)
        {
            try
            {
                var tipo = new TipoPastaModel
                {
                    Nombre = nombre,
                    ContenidoEnvase = contenidoEnvase,
                    Descripcion = descripcion,
                    CostoActual = costoActual
                };

                _dbContext.TiposPasta.Add(tipo);
                _dbContext.SaveChanges();
                return true;
            }
            catch (Exception ex)
            {
                var detalle = ex.InnerException?.Message ?? ex.Message;
                MessageBox.Show($"❌ Error al crear tipo de pasta:\n{detalle}");
                return false;
            }
        }

        // Obtener un tipo de pasta por Id
        public TipoPastaModel? ObtenerPorId(int idTipoPasta)
        {
            try
            {
                return _dbContext.TiposPasta
                                 .FirstOrDefault(tp => tp.IdTipoPasta == idTipoPasta);
            }
            catch (Exception ex)
            {
                var detalle = ex.InnerException?.Message ?? ex.Message;
                MessageBox.Show($"❌ Error al crear tipo de pasta:\n{detalle}");
                return null; 
            }
        }
    }
}
