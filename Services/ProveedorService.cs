using FIDELANDIA.Data;
using FIDELANDIA.Models;
using FIDELANDIA.Views;
using System;
using System.Linq;
using System.Windows;
using Microsoft.EntityFrameworkCore;

namespace FIDELANDIA.Services
{
    /// <summary>
    /// Servicio para manejar operaciones relacionadas con proveedores,
    /// incluyendo creación, consultas, transacciones y categorías.
    /// </summary>
    public class ProveedorService
    {
        private readonly FidelandiaDbContext _dbContext;


        public ProveedorService(FidelandiaDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        // ================= Crear proveedor =================
        /// <summary>
        /// Crea un nuevo proveedor en la base de datos.
        /// </summary>
        public bool CrearProveedor(string nombre, string cuit, string direccion, string telefono, string email,
                            int categoriaProveedorID, decimal limiteCredito, bool isActivo)
        {
            try
            {
                var proveedor = new ProveedorModel
                {
                    Nombre = nombre,
                    Cuit = cuit,
                    Direccion = direccion,
                    Telefono = telefono,
                    Email = email,
                    SaldoActual = 0,
                    CategoriaProveedorID = categoriaProveedorID,
                    LimiteCredito = limiteCredito,
                    IsActivo = isActivo
                };

                _dbContext.Proveedores.Add(proveedor);
                _dbContext.SaveChanges();
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al crear proveedor {ex.Message}");
                return false;
            }
        }

        // ================= Obtener todos los proveedores =================
        /// <summary>
        /// Trae todos los proveedores con información básica.
        /// </summary>
        public List<ProveedorModel> ObtenerTodos()
        {
            try
            {
                return _dbContext.Proveedores
                                 .OrderBy(p => p.Nombre)
                                 .Select(p => new ProveedorModel
                                 {
                                     ProveedorID = p.ProveedorID,
                                     Nombre = p.Nombre,
                                     Cuit = p.Cuit,
                                     SaldoActual = p.SaldoActual,
                                 }).ToList();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al obtener los proveedores: {ex.Message}");
                return new List<ProveedorModel>();
            }
        }

        // ================= Obtener proveedor completo =================
        /// <summary>
        /// Obtiene un proveedor incluyendo su categoría y demás relaciones.
        /// </summary>
        public ProveedorModel? ObtenerProveedorCompleto(int proveedorId)
        {
            try
            {
                return _dbContext.Proveedores
                    .Include(p => p.Categoria) 
                    .FirstOrDefault(p => p.ProveedorID == proveedorId);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al obtener el proveedor: {ex.Message}");
                return null;
            }
        }

        // ================= Contar transacciones =================
        /// <summary>
        /// Cuenta todas las transacciones de un proveedor.
        /// </summary>
        public int ContarTransacciones(int proveedorId)
        {
            return _dbContext.Transacciones.Count(t => t.ProveedorID == proveedorId);
        }

        // ================= Obtener transacciones paginadas =================
        /// <summary>
        /// Obtiene un listado paginado de transacciones de un proveedor.
        /// </summary>
        public List<TransaccionModel> ObtenerTransaccionesPaginadas(int proveedorId, int pagina = 1, int tamanoPagina = 9, decimal saldoInicial = 0, bool traerTodos = false)
        {
            try
            {
                List<TransaccionModel> transaccionesPagina;

                if (traerTodos)
                {
                    // Traer todas las transacciones sin aplicar paginación
                    transaccionesPagina = _dbContext.Transacciones
                        .Where(t => t.ProveedorID == proveedorId)
                        .OrderBy(t => t.Fecha)
                        .ToList(); // importante hacer ToList()
                }
                else
                {
                    // Aplicar paginación normal
                    transaccionesPagina = _dbContext.Transacciones
                        .Where(t => t.ProveedorID == proveedorId)
                        .OrderBy(t => t.Fecha)
                        .Skip((pagina - 1) * tamanoPagina)
                        .Take(tamanoPagina)
                        .ToList();
                }


                decimal saldoAcumulado = saldoInicial;
                foreach (var t in transaccionesPagina)
                {
                    if (t.TipoTransaccion?.ToLower() == "debe")
                        saldoAcumulado += t.Monto;
                    else if (t.TipoTransaccion?.ToLower() == "haber")
                        saldoAcumulado -= t.Monto;

                    t.Saldo = saldoAcumulado; // llenamos solo en memoria
                }

                return transaccionesPagina;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al obtener transacciones: {ex.Message}");
                return new List<TransaccionModel>();
            }
        }

        // ================= Obtener transacciones por rango de fechas =================
        /// <summary>
        /// Obtiene transacciones de un proveedor dentro de un rango de fechas y calcula saldo acumulado.
        /// </summary>
        public List<TransaccionModel> ObtenerTransaccionesPorFechas(int proveedorId, DateTime? fechaDesde, DateTime? fechaHasta)
        {
            try
            {
                // Normalizamos las fechas
                DateTime? hastaInclusive = fechaHasta?.Date.AddDays(1).AddTicks(-1);

                // Base de la consulta
                var baseQuery = _dbContext.Transacciones
                    .Where(t => t.ProveedorID == proveedorId);

                if (fechaDesde.HasValue)
                    baseQuery = baseQuery.Where(t => t.Fecha >= fechaDesde.Value.Date);

                if (hastaInclusive.HasValue)
                    baseQuery = baseQuery.Where(t => t.Fecha <= hastaInclusive.Value);

                // ✅ Obtenemos las transacciones del rango ordenadas
                var transacciones = baseQuery
                    .OrderBy(t => t.Fecha)
                    .ThenBy(t => t.TransaccionID)
                    .AsNoTracking() // ⚡ evita trackeo innecesario
                    .ToList();

                // ✅ Calculamos el saldo inicial directamente en SQL (sin traer todos los anteriores)
                decimal saldoInicial = 0;
                if (fechaDesde.HasValue)
                {
                    var saldoDebe = _dbContext.Transacciones
                        .Where(t => t.ProveedorID == proveedorId
                                    && t.Fecha < fechaDesde.Value.Date
                                    && t.TipoTransaccion.ToLower() == "debe")
                        .Sum(t => (decimal?)t.Monto) ?? 0;

                    var saldoHaber = _dbContext.Transacciones
                        .Where(t => t.ProveedorID == proveedorId
                                    && t.Fecha < fechaDesde.Value.Date
                                    && t.TipoTransaccion.ToLower() == "haber")
                        .Sum(t => (decimal?)t.Monto) ?? 0;

                    saldoInicial = saldoDebe - saldoHaber;
                }

                // ✅ Calculamos el saldo acumulado
                decimal saldoAcumulado = saldoInicial;
                foreach (var t in transacciones)
                {
                    saldoAcumulado += string.Equals(t.TipoTransaccion, "debe", StringComparison.OrdinalIgnoreCase)
                        ? t.Monto
                        : -t.Monto;

                    t.Saldo = saldoAcumulado;
                }

                return transacciones;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al obtener transacciones por fecha: {ex.Message}");
                return new List<TransaccionModel>();
            }
        }

        // ================= Obtener categorías =================
        /// <summary>
        /// Obtiene todas las categorías de proveedores.
        /// </summary>

        public List<CategoriaProveedorModel> ObtenerCategorias()
        {
            try
            {
                return _dbContext.CategoriaProveedor
                                 .OrderBy(c => c.Nombre)
                                 .ToList();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Error al obtener categorías: {ex.Message}", // mensaje detallado
                    "Error",                                       // título de la ventana
                    MessageBoxButton.OK,                           // solo botón "Aceptar"
                    MessageBoxImage.Error                           // icono de error
                );
                return new List<CategoriaProveedorModel>();
            }
        }
    }
}
