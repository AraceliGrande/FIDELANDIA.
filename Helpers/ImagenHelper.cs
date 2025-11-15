using Microsoft.Win32;
using System;
using System.IO;
using System.Windows;

namespace FIDELANDIA.Helpers
{
    public static class ImagenHelper
    {
        // Carpeta interna dentro del sistema (junto al ejecutable)
        private static readonly string carpetaComprobantes = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data", "Comprobantes");

        public static string? GuardarImagenComprobante()
        {
            try
            {
                var dialog = new OpenFileDialog
                {
                    Title = "Seleccionar comprobante",
                    Filter = "Imágenes o PDF|*.jpg;*.jpeg;*.png;*.bmp;*.pdf",
                    Multiselect = false
                };

                if (dialog.ShowDialog() == true)
                {
                    // Crear carpeta interna si no existe
                    if (!Directory.Exists(carpetaComprobantes))
                        Directory.CreateDirectory(carpetaComprobantes);

                    // Generar nombre único
                    string nombreArchivo = $"{DateTime.Now:yyyyMMdd_HHmmss}_{Path.GetFileName(dialog.FileName)}";
                    string destino = Path.Combine(carpetaComprobantes, nombreArchivo);

                    // Copiar el archivo al sistema interno
                    File.Copy(dialog.FileName, destino, true);

                    // Guardar solo la ruta relativa (por ejemplo: Data\Comprobantes\archivo.jpg)
                    string rutaRelativa = Path.Combine("Data", "Comprobantes", nombreArchivo);

                    return rutaRelativa;
                }

                return null;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al guardar el comprobante: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return null;
            }
        }
    }
}
