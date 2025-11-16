using ClosedXML.Excel;
using FIDELANDIA.Data;
using FIDELANDIA.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows;

namespace FIDELANDIA.Helpers
{
    public class BackupExcelService
    {
        private readonly FidelandiaDbContext _db;

        public BackupExcelService(FidelandiaDbContext db)
        {
            _db = db;
        }

        public void ExportarBackupCompleto()
        {
            try
            {
                // ======================== LOTES DE PRODUCCIÓN ========================
                var lotes = _db.LoteProduccion
                    .Include(l => l.TipoPasta)
                    .Include(l => l.StockActual)
                    .OrderBy(l => l.FechaProduccion)
                    .ToList();

                var dtLotes = new DataTable("LotesProduccion");
                dtLotes.Columns.Add("ID del Lote");
                dtLotes.Columns.Add("ID del Tipo de Pasta");
                dtLotes.Columns.Add("Nombre del Tipo de Pasta");
                dtLotes.Columns.Add("Contenido del Envase (kg/l)");
                dtLotes.Columns.Add("Fecha de Producción");
                dtLotes.Columns.Add("Fecha de Vencimiento");
                dtLotes.Columns.Add("Cantidad Producida");
                dtLotes.Columns.Add("Cantidad Disponible");
                dtLotes.Columns.Add("Descripción del Tipo de Pasta");
                dtLotes.Columns.Add("Estado del Lote");

                foreach (var l in lotes)
                {
                    dtLotes.Rows.Add(
                        l.IdLote,
                        l.IdTipoPasta,
                        l.TipoPasta?.Nombre ?? "",
                        l.TipoPasta?.ContenidoEnvase ?? 0,
                        l.FechaProduccion.ToString("dd/MM/yyyy"),
                        l.FechaVencimiento.ToString("dd/MM/yyyy"),
                        l.CantidadProducida,
                        l.CantidadDisponible,
                        l.TipoPasta?.Descripcion ?? "",
                        l.Estado
                    );
                }

                // ======================== TIPOS DE PASTA ========================
                var tiposPasta = _db.TiposPasta.ToList();

                var dtTipos = new DataTable("TiposPasta");
                dtTipos.Columns.Add("ID del Tipo de Pasta");
                dtTipos.Columns.Add("Nombre del Tipo de Pasta");
                dtTipos.Columns.Add("Contenido del Envase (kg/l)");
                dtTipos.Columns.Add("Descripción");
                dtTipos.Columns.Add("Costo Actual ($)");

                foreach (var t in tiposPasta)
                {
                    dtTipos.Rows.Add(
                        t.IdTipoPasta,
                        t.Nombre,
                        t.ContenidoEnvase,
                        t.Descripcion,
                        t.CostoActual
                    );
                }

                // ======================== VENTAS ========================
                var ventas = _db.Venta
                    .Include(v => v.DetalleVenta)
                        .ThenInclude(d => d.Lote)
                            .ThenInclude(l => l.TipoPasta)
                    .ToList();

                var dtVentas = new DataTable("Ventas");
                dtVentas.Columns.Add("ID de la Venta");
                dtVentas.Columns.Add("Fecha de la Venta");
                dtVentas.Columns.Add("Costo Total de la Venta ($)");
                dtVentas.Columns.Add("ID del Detalle de Venta");
                dtVentas.Columns.Add("ID del Lote en el Detalle");
                dtVentas.Columns.Add("Cantidad Vendida");
                dtVentas.Columns.Add("Costo Unitario del Producto ($)");
                dtVentas.Columns.Add("Nombre del Tipo de Pasta Vendida");
                dtVentas.Columns.Add("Contenido del Envase (kg/l)");
                dtVentas.Columns.Add("Descripción del Tipo de Pasta");

                foreach (var v in ventas)
                {
                    decimal totalVenta = v.DetalleVenta.Sum(d => d.Cantidad * d.CostoUnitario);

                    if (v.DetalleVenta.Any())
                    {
                        foreach (var d in v.DetalleVenta)
                        {
                            dtVentas.Rows.Add(
                                v.IdVenta,
                                v.Fecha.ToString("dd/MM/yyyy"),
                                totalVenta,
                                d.IdDetalle,
                                d.IdLote,
                                d.Cantidad,
                                d.CostoUnitario,
                                d.Lote?.TipoPasta?.Nombre ?? "",
                                d.Lote?.TipoPasta?.ContenidoEnvase ?? 0,
                                d.Lote?.TipoPasta?.Descripcion ?? ""
                            );
                        }
                    }
                    else
                    {
                        dtVentas.Rows.Add(
                            v.IdVenta,
                            v.Fecha.ToString("dd/MM/yyyy"),
                            totalVenta,
                            null, null, null, null, null, null, null
                        );
                    }
                }

                // ======================== EXPORTAR ========================
                SaveFileDialog saveFileDialog = new SaveFileDialog
                {
                    Filter = "Archivos Excel (*.xlsx)|*.xlsx",
                    Title = "Exportar backup completo",
                    FileName = $"Backup_Fidelandia_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx"
                };

                if (saveFileDialog.ShowDialog() != true)
                    return;

                using (var workbook = new XLWorkbook())
                {
                    workbook.Worksheets.Add(dtLotes);
                    workbook.Worksheets.Add(dtTipos);
                    workbook.Worksheets.Add(dtVentas);

                    // Formateo y ajuste de columnas
                    foreach (var ws in workbook.Worksheets)
                    {
                        FormatearHoja(ws);
                        ws.Columns().AdjustToContents(); // ajusta automáticamente el ancho de las columnas
                    }

                    workbook.SaveAs(saveFileDialog.FileName);
                }

                MessageBox.Show($"Backup exportado correctamente a Excel:\n{saveFileDialog.FileName}", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al exportar backup: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // ======================== FORMATEO ========================
        private void FormatearHoja(IXLWorksheet ws)
        {
            if (ws.RangeUsed() == null) return;

            var rango = ws.RangeUsed();

            // Encabezados
            var encabezados = rango.FirstRow();
            encabezados.Style.Fill.BackgroundColor = XLColor.FromHtml("#FFD8A8"); // naranja clarito
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
