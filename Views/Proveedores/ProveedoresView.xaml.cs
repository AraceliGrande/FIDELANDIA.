using FIDELANDIA.Models;
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
    public partial class ProveedoresView : UserControl
    {
        public ProveedoresView()
        {
            InitializeComponent();

            // Vincular evento: cuando se selecciona un proveedor
            ListaProveedores.ProveedorSeleccionado += MostrarCuentaCorriente;
        }

        private void MostrarCuentaCorriente(ProveedorModel proveedor)
        {
            // Mostrar los datos del proveedor en el panel derecho
            CuentaCorriente.MostrarProveedor(proveedor, resetearEstado: true);
        }
    }
}
