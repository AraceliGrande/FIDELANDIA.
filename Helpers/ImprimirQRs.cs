using FIDELANDIA.Models;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace FIDELANDIA.Helpers
{
    public static class ImpresionHelper
    {
        public static void ImprimirQRs(List<LoteProduccionModel> lotes)
        {
            // Tamaño de página A4 en pixeles (96 dpi)
            double pageWidth = 96 * 8.27;
            double pageHeight = 96 * 11.69;
            double margin = 20;

            // Definimos el tamaño del QR de forma que quepan varios por fila
            double qrSize = 150; // puedes ajustar si quieres más pequeños

            FixedDocument documento = new FixedDocument();

            FixedPage pagina = null;
            WrapPanel contenedor = null;

            int qrPorFila = (int)((pageWidth - 2 * margin) / qrSize);
            int qrPorColumna = (int)((pageHeight - 2 * margin) / qrSize);
            int maxQRsPorPagina = qrPorFila * qrPorColumna;
            int contador = 0;

            foreach (var lote in lotes)
            {
                if (contador % maxQRsPorPagina == 0)
                {
                    pagina = new FixedPage
                    {
                        Width = pageWidth,
                        Height = pageHeight
                    };

                    contenedor = new WrapPanel
                    {
                        Width = pageWidth - 2 * margin,
                        Height = pageHeight - 2 * margin,
                        Orientation = Orientation.Horizontal,
                        HorizontalAlignment = HorizontalAlignment.Center,
                        VerticalAlignment = VerticalAlignment.Top
                    };

                    FixedPage.SetLeft(contenedor, margin);
                    FixedPage.SetTop(contenedor, margin);
                    pagina.Children.Add(contenedor);

                    PageContent pageContent = new PageContent();
                    ((IAddChild)pageContent).AddChild(pagina);
                    documento.Pages.Add(pageContent);
                }

                // Generamos la imagen del QR
                BitmapImage qrImageSource = QrHelper.GenerarQr(
                    $"Fidelandia - Pastas Frescas\n\n" +
                    $"Identificador unico: {lote.IdLote}\n" +
                    $"Tipo de pasta: {lote.TipoPasta.Nombre}\n" +
                    $"Cantidad producida: {lote.CantidadProducida}\n" +
                    $"Fecha de produccion: {lote.FechaProduccion:dd/MM/yyyy}\n" +
                    $"Fecha de vencimiento: {lote.FechaVencimiento:dd/MM/yyyy}"
                );

                Image qr = new Image
                {
                    Source = qrImageSource,
                    Width = qrSize,
                    Height = qrSize,
                    Margin = new Thickness(5)
                };

                contenedor.Children.Add(qr);
                contador++;
            }

            // Mostrar diálogo de impresión y mandar a imprimir
            PrintDialog printDialog = new PrintDialog();
            if (printDialog.ShowDialog() == true)
            {
                printDialog.PrintDocument(documento.DocumentPaginator, "QR Impresos");
            }
        }
    }
}
