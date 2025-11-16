using ClosedXML.Excel;
using FIDELANDIA.Data;
using FIDELANDIA.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Win32;
using System;
using System.Data;
using System.Linq;
using System.Windows;

namespace FIDELANDIA.Helpers
{
    public class ProveedoresExcelService
    {
        private readonly FidelandiaDbContext _db;

        public ProveedoresExcelService(FidelandiaDbContext db)
        {
            _db = db;
        }

        public void ExportarProveedoresConTransacciones()
        {
            try
            {
                var proveedores = _db.Proveedores
                    .Include(p => p.Transacciones)
                    .Include(p => p.Categoria)
                    .ToList();

                SaveFileDialog saveFileDialog = new SaveFileDialog
                {
                    Filter = "Archivos Excel (*.xlsx)|*.xlsx",
                    Title = "Exportar Proveedores y Transacciones",
                    FileName = $"Proveedores_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx"
                };

                if (saveFileDialog.ShowDialog() != true)
                    return;

                using (var workbook = new XLWorkbook())
                {
                    // ======================== Hoja resumen ========================
                    var dtResumen = new DataTable("ResumenProveedores");
                    dtResumen.Columns.Add("ID Proveedor");
                    dtResumen.Columns.Add("Nombre");
                    dtResumen.Columns.Add("CUIT");
                    dtResumen.Columns.Add("Dirección");
                    dtResumen.Columns.Add("Teléfono");
                    dtResumen.Columns.Add("Email");
                    dtResumen.Columns.Add("Saldo Actual");
                    dtResumen.Columns.Add("Fecha Alta");
                    dtResumen.Columns.Add("Límite de Crédito");
                    dtResumen.Columns.Add("Activo");
                    dtResumen.Columns.Add("Categoria");
                    dtResumen.Columns.Add("Transaccion ID");
                    dtResumen.Columns.Add("Tipo de Transaccion");
                    dtResumen.Columns.Add("Monto");
                    dtResumen.Columns.Add("Fecha Transaccion");
                    dtResumen.Columns.Add("Detalle");
                    dtResumen.Columns.Add("Saldo después de transaccion");
                    dtResumen.Columns.Add("Comprobante");

                    foreach (var p in proveedores)
                    {
                        if (p.Transacciones.Any())
                        {
                            foreach (var t in p.Transacciones.OrderBy(t => t.Fecha))
                            {
                                dtResumen.Rows.Add(
                                    p.ProveedorID,
                                    p.Nombre,
                                    p.Cuit,
                                    p.Direccion,
                                    p.Telefono,
                                    p.Email,
                                    p.SaldoActual,
                                    p.FechaAlta.ToString("dd/MM/yyyy"),
                                    p.LimiteCredito,
                                    p.IsActivo ? "Sí" : "No",
                                    p.Categoria?.Nombre ?? "",
                                    t.TransaccionID,
                                    t.TipoTransaccion,
                                    t.Monto,
                                    t.Fecha.ToString("dd/MM/yyyy"),
                                    t.Detalle,
                                    t.Saldo,
                                    t.ComprobanteRuta ?? ""
                                );
                            }
                        }
                        else
                        {
                            dtResumen.Rows.Add(
                                p.ProveedorID,
                                p.Nombre,
                                p.Cuit,
                                p.Direccion,
                                p.Telefono,
                                p.Email,
                                p.SaldoActual,
                                p.FechaAlta.ToString("dd/MM/yyyy"),
                                p.LimiteCredito,
                                p.IsActivo ? "Sí" : "No",
                                p.Categoria?.Nombre ?? "",
                                null, null, null, null, null, null, null
                            );
                        }
                    }

                    var wsResumen = workbook.Worksheets.Add(dtResumen, "ResumenProveedores");
                    FormatearHoja(wsResumen);
                    wsResumen.Columns().AdjustToContents();

                    // ======================== Hojas individuales por proveedor ========================
                    foreach (var p in proveedores)
                    {
                        var dtProv = new DataTable($"Proveedor_{p.Nombre}");
                        dtProv.Columns.Add("ID Proveedor");
                        dtProv.Columns.Add("Nombre");
                        dtProv.Columns.Add("CUIT");
                        dtProv.Columns.Add("Dirección");
                        dtProv.Columns.Add("Teléfono");
                        dtProv.Columns.Add("Email");
                        dtProv.Columns.Add("Saldo Actual");
                        dtProv.Columns.Add("Fecha Alta");
                        dtProv.Columns.Add("Límite de Crédito");
                        dtProv.Columns.Add("Activo");
                        dtProv.Columns.Add("Categoria");
                        dtProv.Columns.Add("Transaccion ID");
                        dtProv.Columns.Add("Tipo de Transaccion");
                        dtProv.Columns.Add("Monto");
                        dtProv.Columns.Add("Fecha Transaccion");
                        dtProv.Columns.Add("Detalle");
                        dtProv.Columns.Add("Saldo después de transaccion");
                        dtProv.Columns.Add("Comprobante");

                        if (p.Transacciones.Any())
                        {
                            foreach (var t in p.Transacciones.OrderBy(t => t.Fecha))
                            {
                                dtProv.Rows.Add(
                                    p.ProveedorID,
                                    p.Nombre,
                                    p.Cuit,
                                    p.Direccion,
                                    p.Telefono,
                                    p.Email,
                                    p.SaldoActual,
                                    p.FechaAlta.ToString("dd/MM/yyyy"),
                                    p.LimiteCredito,
                                    p.IsActivo ? "Sí" : "No",
                                    p.Categoria?.Nombre ?? "",
                                    t.TransaccionID,
                                    t.TipoTransaccion,
                                    t.Monto,
                                    t.Fecha.ToString("dd/MM/yyyy"),
                                    t.Detalle,
                                    t.Saldo,
                                    t.ComprobanteRuta ?? ""
                                );
                            }
                        }
                        else
                        {
                            dtProv.Rows.Add(
                                p.ProveedorID,
                                p.Nombre,
                                p.Cuit,
                                p.Direccion,
                                p.Telefono,
                                p.Email,
                                p.SaldoActual,
                                p.FechaAlta.ToString("dd/MM/yyyy"),
                                p.LimiteCredito,
                                p.IsActivo ? "Sí" : "No",
                                p.Categoria?.Nombre ?? "",
                                null, null, null, null, null, null, null
                            );
                        }

                        var wsProv = workbook.Worksheets.Add(dtProv, $"Prov_{p.ProveedorID}");
                        FormatearHoja(wsProv);
                        wsProv.Columns().AdjustToContents();
                    }

                    workbook.SaveAs(saveFileDialog.FileName);
                }

                MessageBox.Show($"Exportación de proveedores completa:\n{saveFileDialog.FileName}", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al exportar proveedores: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void FormatearHoja(IXLWorksheet ws)
        {
            if (ws.RangeUsed() == null) return;

            var rango = ws.RangeUsed();

            // Encabezados
            var encabezados = rango.FirstRow();
            encabezados.Style.Fill.BackgroundColor = XLColor.FromHtml("#FFD8A8");
            encabezados.Style.Font.Bold = true;

            // Desactivar filas alternadas / banded rows
            foreach (var tabla in ws.Tables)
            {
                tabla.ShowRowStripes = false;
                tabla.ShowColumnStripes = false;
            }
        }
    }
}
