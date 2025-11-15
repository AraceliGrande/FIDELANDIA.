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
using System.Windows.Shapes;

namespace FIDELANDIA.Windows
{
    /// <summary>
    /// Lógica de interacción para ProveedoresFormWindow.xaml
    /// </summary>
    public partial class ProveedoresFormWindow : Window
    {
        public ProveedoresFormWindow()
        {
            InitializeComponent();

        }


        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            var ventanaPrincipal = Application.Current.Windows
                .OfType<MainWindow>()
                .FirstOrDefault();

            if (ventanaPrincipal != null)
            {
                // Igualar tamaño completo
                this.Width = ventanaPrincipal.ActualWidth;
                this.Height = ventanaPrincipal.ActualHeight;

                // Alinear posiciones
                this.Left = ventanaPrincipal.Left;
                this.Top = ventanaPrincipal.Top;
            }
        }

    }
}

