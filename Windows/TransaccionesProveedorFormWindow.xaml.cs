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
    /// Lógica de interacción para TransaccionesProveedorFormWindow.xaml
    /// </summary>
    public partial class TransaccionesProveedorFormWindow : Window
    {
        public TransaccionesProveedorFormWindow()
        {
            InitializeComponent();
        }
        private void Cancelar_Click(object sender, RoutedEventArgs e)
        {
            this.Close(); // Cierra la ventana actual
        }
        private void Guardar_Click(object sender, RoutedEventArgs e)
        {
            this.Close(); // Cierra la ventana actual
        }
    }
}
