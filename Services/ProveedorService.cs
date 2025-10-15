using FIDELANDIA.Data;
using FIDELANDIA.Models;
using FIDELANDIA.Views;
using System;
using System.Linq;
using System.Windows;

namespace FIDELANDIA.Services
{
    public class ProveedorService
    {
        private readonly FidelandiaDbContext _dbContext;

        public ProveedorService(FidelandiaDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public bool CrearProveedor(string nombre, string cuit, string direccion, string telefono, string email)
        {
            try
            {
                var proveedor = new ProveedorModel
                {
                    Nombre = nombre,
                    Cuit = cuit,
                    Direccion = direccion,
                    Telefono = telefono,
                    SaldoActual = 0,
                    Email = email
                };

                _dbContext.Proveedores.Add(proveedor);
                _dbContext.SaveChanges();
                return true;
            }
            catch (Exception ex)
            {
                // Abrir ventana de error
                MessageBox.Show($"Error al crear proveedor {ex.Message}");
                return false;
            }
        }

        // ✅ Traer todos los proveedores
        public List<ProveedorModel> ObtenerTodos()
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

        public ProveedorModel ObtenerProveedorCompleto(int proveedorId)
        {
            return _dbContext.Proveedores
                             .Where(p => p.ProveedorID == proveedorId)
                             .FirstOrDefault();
        }
    }
}
