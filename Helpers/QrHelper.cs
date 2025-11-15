using QRCoder;
using System.Drawing;
using System.IO;
using System.Windows.Media.Imaging;

namespace FIDELANDIA.Helpers
{
    public static class QrHelper
    {
        /// <summary>
        /// Genera un QR a partir de un texto y devuelve un BitmapImage listo para WPF
        /// </summary>
        /// <param name="texto">Texto o URL a codificar</param>
        /// <param name="pixelPerModule">Tamaño de los cuadros del QR (20 está bien)</param>
        public static BitmapImage GenerarQr(string texto, int pixelPerModule = 20)
        {
            using (QRCodeGenerator qrGenerator = new QRCodeGenerator())
            {
                QRCodeData qrCodeData = qrGenerator.CreateQrCode(texto, QRCodeGenerator.ECCLevel.Q);
                using (QRCode qrCode = new QRCode(qrCodeData))
                {
                    Bitmap qrBitmap = qrCode.GetGraphic(pixelPerModule);
                    return ConvertBitmapToImageSource(qrBitmap);
                }
            }
        }

        private static BitmapImage ConvertBitmapToImageSource(Bitmap bitmap)
        {
            using (MemoryStream memory = new MemoryStream())
            {
                bitmap.Save(memory, System.Drawing.Imaging.ImageFormat.Png);
                memory.Position = 0;

                BitmapImage bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.StreamSource = memory;
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.EndInit();
                return bitmapImage;
            }
        }
    }
}
