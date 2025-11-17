using Microsoft.Win32;
using System;
using System.IO;
using System.Windows;

namespace FIDELANDIA.Helpers
{
    public static class ImagenHelper
    {
        // Carpeta segura dentro del perfil del usuario (recomendado)
        private static readonly string carpetaComprobantes =
            Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "Fidelandia",
                "Comprobantes"
            );

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
                    Directory.CreateDirectory(carpetaComprobantes);

                    // Generar nombre único
                    string nombreArchivo = $"{DateTime.Now:yyyyMMdd_HHmmss}_{Path.GetFileName(dialog.FileName)}";
                    string destino = Path.Combine(carpetaComprobantes, nombreArchivo);

                    // Copiar el archivo al sistema interno
                    File.Copy(dialog.FileName, destino, true);

                    // Guardar solo la ruta relativa (solo el nombre del archivo o subcarpetas internas)
                    string rutaRelativa = Path.Combine("Comprobantes", nombreArchivo);

                    return rutaRelativa;
                }

                return null;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al guardar el comprobante: {ex.Message}",
                                "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return null;
            }
        }
    }
}
