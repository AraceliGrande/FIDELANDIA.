using FIDELANDIA.Data;
using FIDELANDIA.Models;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FIDELANDIA.Services
{
    public class DatabaseSizeService
    {
        private readonly string _connectionString;
        private readonly FidelandiaDbContext _db;

        public DatabaseSizeService(FidelandiaDbContext db)
        {
            _db = db;
            // Usa tu cadena guardada en Settings
            _connectionString = FIDELANDIA.Properties.Settings.Default.ConnectionString;

        }

        // ---------------------------------------------------------
        // 1. Obtener tamaño de la base en MB
        // ---------------------------------------------------------
        // ---------------------------------------------------------
        // Obtener tamaño realmente ocupado de la base en MB
        // ---------------------------------------------------------
        public async Task<double> ObtenerTamanioMB()
        {
            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();

            string sql = @"
        SELECT SUM(a.total_pages) * 8.0 / 1024 AS TotalMB,
               SUM(a.used_pages) * 8.0 / 1024 AS UsadoMB,
               SUM(a.data_pages) * 8.0 / 1024 AS SoloDatosMB
        FROM sys.partitions p
        JOIN sys.allocation_units a ON p.partition_id = a.container_id
        WHERE p.object_id > 100;
    ";

            using var cmd = new SqlCommand(sql, conn);
            var result = await cmd.ExecuteReaderAsync();
            if (await result.ReadAsync())
            {
                return Convert.ToDouble(result["UsadoMB"]); // devuelve espacio usado real en MB
            }

            return 0;
        }




        // ---------------------------------------------------------
        // 2. Borrar todos los registros de TODAS las tablas
        // ---------------------------------------------------------
        public async Task BorrarTodosLosRegistros()
        {
            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();

            string sql = @"
                -- Desactivar restricciones
                EXEC sp_MSforeachtable 'ALTER TABLE ? NOCHECK CONSTRAINT ALL';

                -- Borrar contenido
                EXEC sp_MSforeachtable 'DELETE FROM ?';

                -- Reactivar restricciones
                EXEC sp_MSforeachtable 'ALTER TABLE ? WITH CHECK CHECK CONSTRAINT ALL';
            ";

            using var cmd = new SqlCommand(sql, conn);
            await cmd.ExecuteNonQueryAsync();
            await LimpiarYOptimizarBase();
        }

        public async Task BorrarTransaccionesAntiguas(int mesesAConservar)
        {
            try
            {
                var fechaLimite = DateTime.Now.AddMonths(-mesesAConservar);

                var transacciones = await _db.Transacciones
                    .Where(t => t.Fecha < fechaLimite)
                    .ToListAsync();

                if (transacciones.Count > 0)
                {
                    _db.Transacciones.RemoveRange(transacciones);
                    await _db.SaveChangesAsync();

                    // Limpiar y optimizar la base después de borrar
                    await LimpiarYOptimizarBase();
                }
            }
            catch (DbUpdateException dbEx)
            {
                var sqlEx = dbEx.InnerException as Microsoft.Data.SqlClient.SqlException;
                if (sqlEx != null)
                {
                    throw new Exception(
                        $"Error en la base de datos:\nMensaje: {sqlEx.Message}\nNúmero de error: {sqlEx.Number}\nOrigen: {sqlEx.Source}\nProcedimiento involucrado: {sqlEx.Errors[0].Procedure ?? "Desconocido"}"
                    );
                }
                else
                {
                    throw new Exception($"Error al actualizar la base: {dbEx.Message}");
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error general al borrar transacciones: {ex.Message}");
            }
        }


        public async Task BorrarProduccionVentasAntiguas(int mesesAConservar)
        {
            var fechaLimite = DateTime.Now.AddMonths(-mesesAConservar);

            using var transaction = await _db.Database.BeginTransactionAsync();
            try
            {
                // 1️⃣ Borrar detalle de ventas antiguas
                var detalles = await _db.DetalleVenta
                    .Where(d => d.Venta.Fecha < fechaLimite)
                    .ToListAsync();
                if (detalles.Any())
                    _db.DetalleVenta.RemoveRange(detalles);

                // 2️⃣ Borrar ventas antiguas
                var ventas = await _db.Venta
                    .Where(v => v.Fecha < fechaLimite)
                    .ToListAsync();
                if (ventas.Any())
                    _db.Venta.RemoveRange(ventas);

                // 3️⃣ Borrar lotes antiguos que:
                //    - No tengan ventas posteriores a la fecha límite
                //    - No estén asociados a un StockActual
                var lotes = await _db.LoteProduccion
                    .Where(l => l.FechaProduccion < fechaLimite)
                    .Where(l => !_db.DetalleVenta
                        .Any(d => d.IdLote == l.IdLote && d.Venta.Fecha >= fechaLimite))
                    .Where(l => l.IdStockActual == null) // ❌ protegemos lotes con stock
                    .ToListAsync();

                if (lotes.Any())
                    _db.LoteProduccion.RemoveRange(lotes);

                await _db.SaveChangesAsync();
                await transaction.CommitAsync();

                await LimpiarYOptimizarBase();
            }
            catch (DbUpdateException dbEx)
            {
                await transaction.RollbackAsync();
                var sqlEx = dbEx.InnerException as Microsoft.Data.SqlClient.SqlException;
                if (sqlEx != null)
                {
                    throw new Exception(
                        $"Error en la base de datos:\nMensaje: {sqlEx.Message}\nNúmero de error: {sqlEx.Number}\nOrigen: {sqlEx.Source}\nTabla/Constraint involucrada: {sqlEx.Errors[0].Procedure ?? "Desconocida"}"
                    );
                }
                else
                {
                    throw new Exception($"Error al actualizar la base: {dbEx.Message}");
                }
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                throw new Exception($"Error general: {ex.Message}");
            }
        }


        public async Task LimpiarYOptimizarBase()
        {
            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();

            using var cmd = conn.CreateCommand();

            // ⚠️ Nota: Cambiamos a SIMPLE para liberar log
            cmd.CommandText = @"
        -- Cambiar modo de recuperación a SIMPLE para truncar log
        ALTER DATABASE FidelandiaDB SET RECOVERY SIMPLE;

        -- Limpiar y truncar log
        DBCC SHRINKFILE (N'FidelandiaDB_log', 1);

        -- Compactar base de datos (reduce espacio libre al 10%)
        DBCC SHRINKDATABASE (FidelandiaDB, 10);

        -- Reorganizar índices (opcional, mejora fragmentación)
        EXEC sp_MSforeachtable 'ALTER INDEX ALL ON ? REORGANIZE';
    ";

            await cmd.ExecuteNonQueryAsync();

            // ⚠️ Opcional: volver a FULL si usabas FULL RECOVERY
            // cmd.CommandText = "ALTER DATABASE FidelandiaDB SET RECOVERY FULL;";
            // await cmd.ExecuteNonQueryAsync();
        }



    }
}