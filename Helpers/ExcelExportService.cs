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
        /// <summary>
        /// Versión genérica existente (puede conservarse).
        /// </summary>
        public void ExportarAExcel<T>(IEnumerable<T> data, string nombreHoja = "Datos")
        {
            ExportarAExcel((IEnumerable)data, nombreHoja);
        }

        /// <summary>
        /// Sobrecarga que acepta IEnumerable y determina el tipo real en runtime.
        /// Funciona con tipos anónimos, DataRowView, IDictionary (ExpandoObject) y objetos normales.
        /// </summary>
        public void ExportarAExcel(IEnumerable data, string nombreHoja = "Datos")
        {
            try
            {
                if (data == null)
                {
                    MessageBox.Show("No hay datos para exportar.", "Advertencia", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var lista = data.Cast<object>().ToList();
                if (!lista.Any())
                {
                    MessageBox.Show("No hay datos para exportar.", "Advertencia", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Diálogo para guardar el archivo
                SaveFileDialog saveFileDialog = new SaveFileDialog
                {
                    Filter = "Archivos Excel (*.xlsx)|*.xlsx",
                    Title = "Guardar como Excel",
                    FileName = $"{nombreHoja}_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx"
                };

                if (saveFileDialog.ShowDialog() != true)
                    return;

                // Crear DataTable a partir de la lista (analizando el tipo runtime de la primera fila)
                DataTable dt = ConvertirEnumerableADataTable(lista);

                // Crear y guardar el archivo Excel
                using (var workbook = new XLWorkbook())
                {
                    var worksheet = workbook.Worksheets.Add(dt, nombreHoja);
                    worksheet.Columns().AdjustToContents();
                    workbook.SaveAs(saveFileDialog.FileName);
                }

                MessageBox.Show($"Datos exportados correctamente a Excel:\n{saveFileDialog.FileName}", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al exportar: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private DataTable ConvertirEnumerableADataTable(List<object> lista)
        {
            var dt = new DataTable();

            // Tomamos el primer elemento no nulo para inferir columnas
            var first = lista.FirstOrDefault(x => x != null);
            if (first == null)
                return dt;

            // Manejo especial para DataRowView (p. ej. DataGrid enlazado a DataTable)
            if (first is DataRowView drv)
            {
                dt = drv.DataView.Table.Clone();
                foreach (var item in lista)
                {
                    dt.ImportRow(((DataRowView)item).Row);
                }
                return dt;
            }

            // Manejo para IDictionary / ExpandoObject
            if (first is IDictionary<string, object> dictLike)
            {
                // Crear columnas a partir de las keys del primer diccionario
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

            // Manejo para objetos normales / anónimos: usamos reflection sobre el tipo runtime
            Type tipoRuntime = first.GetType();
            PropertyInfo[] props = tipoRuntime.GetProperties(BindingFlags.Public | BindingFlags.Instance);

            // Si no encontró propiedades, intentar con propiedades de componentes (INotifyPropertyChanged / TypeDescriptor)
            if (props == null || props.Length == 0)
            {
                // Intentamos obtener propiedades vía TypeDescriptor (útil para algunos bindings)
                var propiedades = TypeDescriptor.GetProperties(first);
                foreach (PropertyDescriptor pd in propiedades)
                {
                    dt.Columns.Add(pd.Name, pd.PropertyType ?? typeof(object));
                }

                foreach (var obj in lista)
                {
                    var row = dt.NewRow();
                    foreach (PropertyDescriptor pd in propiedades)
                        row[pd.Name] = pd.GetValue(obj) ?? DBNull.Value;
                    dt.Rows.Add(row);
                }

                return dt;
            }

            // Columnas con nombres de propiedades
            foreach (var p in props)
            {
                Type columnType = Nullable.GetUnderlyingType(p.PropertyType) ?? p.PropertyType;
                dt.Columns.Add(p.Name, columnType ?? typeof(object));
            }

            // Filas
            foreach (var obj in lista)
            {
                var valores = new object[props.Length];
                for (int i = 0; i < props.Length; i++)
                {
                    valores[i] = props[i].GetValue(obj, null) ?? DBNull.Value;
                }
                dt.Rows.Add(valores);
            }

            return dt;
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
    }
}
