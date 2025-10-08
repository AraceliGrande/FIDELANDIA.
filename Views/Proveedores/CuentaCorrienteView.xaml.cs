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

namespace FIDELANDIA.Views
{
    /// <summary>
    /// Lógica de interacción para CuentaCorrienteView.xaml
    /// </summary>
    public partial class CuentaCorrienteView : UserControl
    {
        public CuentaCorrienteView()
        {
            InitializeComponent();
        }

        private void BtnNuevaTransaccion_Click(object sender, RoutedEventArgs e)
        {
            // Crear y abrir la ventana modal para el formulario
            var ventana = new TransaccionesProveedorFormWindow();
            ventana.Owner = Window.GetWindow(this); // Para que sea modal sobre la ventana principal
            ventana.ShowDialog();
        }
    }
}
