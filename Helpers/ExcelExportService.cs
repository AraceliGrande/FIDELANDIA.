using ClosedXML.Excel;
using Microsoft.Win32;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.ComponentModel;

namespace FIDELANDIA.Helpers
{
    public class ExcelExportService
    {
        public void ExportarAExcel<T>(IEnumerable<T> data, string nombreHoja = "Datos")
        {
            ExportarAExcel((IEnumerable)data, nombreHoja);
        }

        public void ExportarAExcel(IEnumerable data, string nombreHoja = "Datos")
        {
            try
            {
                if (data == null || !data.Cast<object>().Any())
                {
                    MessageBox.Show("No hay datos para exportar.", "Advertencia", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                SaveFileDialog saveFileDialog = new SaveFileDialog
                {
                    Filter = "Archivos Excel (*.xlsx)|*.xlsx",
                    Title = "Guardar como Excel",
                    FileName = $"{nombreHoja}_{DateTime.Now:dd_MM_yyyy_HHmmss}.xlsx"
                };

                if (saveFileDialog.ShowDialog() != true)
                    return;

                DataTable dt = ConvertirEnumerableADataTable(data.Cast<object>().ToList());

                using (var workbook = new XLWorkbook())
                {
                    var ws = workbook.Worksheets.Add(dt, nombreHoja);
                    ws.Columns().AdjustToContents();

                    FormatearHoja(ws);

                    workbook.SaveAs(saveFileDialog.FileName);
                }

                MessageBox.Show($"Datos exportados correctamente a Excel:\n{saveFileDialog.FileName}", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al exportar: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public void ExportarMultiplesHojas(Dictionary<string, IEnumerable> datasets)
        {
            try
            {
                if (datasets == null || datasets.Count == 0)
                {
                    MessageBox.Show("No hay datos para exportar.", "Advertencia", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                SaveFileDialog saveFileDialog = new SaveFileDialog
                {
                    Filter = "Archivos Excel (*.xlsx)|*.xlsx",
                    Title = "Guardar como Excel",
                    FileName = $"Balance_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx"
                };

                if (saveFileDialog.ShowDialog() != true)
                    return;

                using (var workbook = new XLWorkbook())
                {
                    foreach (var kvp in datasets)
                    {
                        var dt = ConvertirEnumerableADataTable(kvp.Value.Cast<object>().ToList());
                        var ws = workbook.Worksheets.Add(dt, kvp.Key);
                        ws.Columns().AdjustToContents();

                        FormatearHoja(ws);
                    }

                    workbook.SaveAs(saveFileDialog.FileName);
                }

                MessageBox.Show($"Archivo exportado correctamente:\n{saveFileDialog.FileName}", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al exportar: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

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

        private DataTable ConvertirEnumerableADataTable(List<object> lista)
        {
            var dt = new DataTable();
            var first = lista.FirstOrDefault(x => x != null);
            if (first == null) return dt;

            if (first is DataRowView drv)
            {
                dt = drv.DataView.Table.Clone();
                foreach (var item in lista)
                    dt.ImportRow(((DataRowView)item).Row);
                return dt;
            }

            if (first is IDictionary<string, object> dictLike)
            {
                foreach (var key in dictLike.Keys)
                    dt.Columns.Add(key);
                foreach (IDictionary<string, object> item in lista)
                {
                    var row = dt.NewRow();
                    foreach (var kv in item)
                        row[kv.Key] = kv.Value ?? DBNull.Value;
                    dt.Rows.Add(row);
                }
                return dt;
            }

            Type tipoRuntime = first.GetType();
            PropertyInfo[] props = tipoRuntime.GetProperties(BindingFlags.Public | BindingFlags.Instance);

            if (props == null || props.Length == 0)
            {
                var propiedades = TypeDescriptor.GetProperties(first);
                foreach (PropertyDescriptor pd in propiedades)
                    dt.Columns.Add(pd.Name, pd.PropertyType ?? typeof(object));
                foreach (var obj in lista)
                {
                    var row = dt.NewRow();
                    foreach (PropertyDescriptor pd in propiedades)
                        row[pd.Name] = pd.GetValue(obj) ?? DBNull.Value;
                    dt.Rows.Add(row);
                }
                return dt;
            }

            foreach (var p in props)
            {
                Type columnType = Nullable.GetUnderlyingType(p.PropertyType) ?? p.PropertyType;
                dt.Columns.Add(p.Name, columnType ?? typeof(object));
            }

            foreach (var obj in lista)
            {
                var valores = new object[props.Length];
                for (int i = 0; i < props.Length; i++)
                    valores[i] = props[i].GetValue(obj, null) ?? DBNull.Value;
                dt.Rows.Add(valores);
            }

            return dt;
        }
    }
}
