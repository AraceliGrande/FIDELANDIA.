using FIDELANDIA.Windows;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace FIDELANDIA.Views.Produccion
{
    public partial class ProduccionView : UserControl
    {
        public ProduccionView()
        {
            InitializeComponent();

            // Array de secciones con filas (6 secciones de ejemplo)
            var secciones = new[]
            {
                new { Nombre = "Tallarines frescos", Filas = new[]
                    {
                        new[] { "1031", "2025-08-04", "300 kg", "30" },
                        new[] { "1036", "2025-08-06", "180 kg", "18" }
                    }
                },
                new { Nombre = "Ravioles caseros", Filas = new[]
                    {
                        new[] { "2041", "2025-08-10", "120 kg", "12" },
                        new[] { "2042", "2025-08-12", "150 kg", "15" }
                    }
                },
                new { Nombre = "Fideos integrales", Filas = new[]
                    {
                        new[] { "301", "2025-08-05", "250 kg", "25" },
                        new[] { "302", "2025-08-07", "200 kg", "20" }
                    }
                },
                new { Nombre = "Canelones", Filas = new[]
                    {
                        new[] { "401", "2025-08-03", "100 kg", "10" },
                        new[] { "402", "2025-08-08", "130 kg", "13" }
                    }
                },
                new { Nombre = "Tortellini", Filas = new[]
                    {
                        new[] { "501", "2025-08-01", "90 kg", "9" },
                        new[] { "502", "2025-08-02", "110 kg", "11" }
                    }
                },
                new { Nombre = "Tortellini", Filas = new[]
                    {
                        new[] { "501", "2025-08-01", "90 kg", "9" },
                        new[] { "502", "2025-08-02", "110 kg", "11" }
                    }
                },
                new { Nombre = "Tortellini", Filas = new[]
                    {
                        new[] { "501", "2025-08-01", "90 kg", "9" },
                        new[] { "502", "2025-08-02", "110 kg", "11" }
                    }
                },
                new { Nombre = "Ñoquis", Filas = new[]
                    {
                        new[] { "601", "2025-08-09", "140 kg", "14" },
                        new[] { "602", "2025-08-11", "160 kg", "16" }
                    }
                },

            };

            this.DataContext = secciones;
        }
        // Click de Nuevo Lote
        private void NuevoLote_Click(object sender, RoutedEventArgs e)
        {
            var ventana = new CrearLoteProducFormWindow(); // Reemplaza con tu ventana real
            ventana.ShowDialog(); // Modal
        }

        // Click de Nueva Venta
        private void NuevaVenta_Click(object sender, RoutedEventArgs e)
        {
            var ventana = new CrearVentaProducFormWindow(); // Podés usar otra ventana diferente si corresponde
            ventana.ShowDialog();
        }

        private void BtnNuevoTipoPasta_Click(object sender, RoutedEventArgs e)
        {
            var ventana = new CrearTipoPastaWindow();
            ventana.Owner = Window.GetWindow(this); // opcional, para que sea modal
            ventana.ShowDialog();
        }

    }
}