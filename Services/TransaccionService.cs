using FIDELANDIA.Data;
using FIDELANDIA.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace FIDELANDIA.Services
{
    /// <summary>
    /// Servicio para manejar transacciones de proveedores,
    /// actualizando saldo y registrando movimientos.
    /// </summary>
    class TransaccionService
    {
        private readonly FidelandiaDbContext _dbContext;


        public TransaccionService(FidelandiaDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        /// <summary>
        /// Crea una transacción para un proveedor, actualiza el saldo y guarda en la base de datos.
        /// </summary>
        public void CrearTransaccion(TransaccionModel transaccion)
        {
            var proveedor = _dbContext.Proveedores
                .FirstOrDefault(p => p.ProveedorID == transaccion.ProveedorID);

            if (proveedor == null)
                throw new Exception("Proveedor no encontrado.");

            switch (transaccion.TipoTransaccion?.ToLower())
            {
                case "debe":
                    proveedor.SaldoActual += transaccion.Monto;
                    break;
                case "haber":
                    proveedor.SaldoActual -= transaccion.Monto;
                    break;
                default:
                    break;
            }

            _dbContext.Transacciones.Add(transaccion);
            _dbContext.SaveChanges();
        }
    }
}
