using FIDELANDIA.Models;
using FIDELANDIA.Views.Proveedores;
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
        public TransaccionesProveedorFormWindow(ProveedorModel proveedorActual)
        {
            InitializeComponent();

            // Cargar el formulario dentro de la ventana
            Content = new TransaccionesFormView(proveedorActual);
        }

        private void Cancelar_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Guardar_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
