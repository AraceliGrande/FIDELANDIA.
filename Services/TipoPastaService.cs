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
    /// Servicio para manejar operaciones sobre tipos de pasta,
    /// incluyendo creación, consulta, actualización y persistencia de cambios.
    /// </summary>
    public class TipoPastaService
    {
        private readonly FidelandiaDbContext _dbContext;

        public TipoPastaService(FidelandiaDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        // ================= Obtener todos los tipos de pasta =================
        /// <summary>
        /// Devuelve todos los tipos de pasta ordenados por nombre.
        /// </summary>
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
                MessageBox.Show(
                    $"Error al obtener tipos de pasta: {ex.Message}",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                );
                return new List<TipoPastaModel>();
            }
        }


        // ================= Crear un nuevo tipo de pasta =================
        /// <summary>
        /// Crea un nuevo tipo de pasta y lo guarda en la base de datos.
        /// </summary>
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
                MessageBox.Show(
                    $"Error al crear tipo de pasta:\n{detalle}",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                );
                return false;
            }
        }

        // ================= Obtener tipo de pasta por Id =================
        /// <summary>
        /// Obtiene un tipo de pasta específico por su Id.
        /// </summary>
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
                MessageBox.Show(
                    $"Error al crear tipo de pasta:\n{detalle}",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                );
                return null; 
            }
        }

        // ================= Actualizar costo y contenido =================
        /// <summary>
        /// Actualiza el costo y el contenido de un tipo de pasta existente.
        /// </summary>
        public bool ActualizarTipoPasta(int idTipoPasta, decimal nuevoCosto, decimal nuevoContenido)
        {
            try
            {
                var tipoPasta = _dbContext.TiposPasta.FirstOrDefault(tp => tp.IdTipoPasta == idTipoPasta);
                if (tipoPasta == null)
                    return false;

                if (nuevoCosto <= 0 || nuevoContenido <= 0)
                    return false;

                tipoPasta.CostoActual = nuevoCosto;
                tipoPasta.ContenidoEnvase = nuevoContenido;
                return true;
            }
            catch
            {
                return false;
            }
        }


        // ================= Guardar todos los cambios =================
        /// <summary>
        /// Guarda todos los cambios pendientes en la base de datos.
        /// </summary>
        public void GuardarCambios()
        {
            _dbContext.SaveChanges();
        }

    }
}
