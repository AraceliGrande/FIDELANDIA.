using FIDELANDIA.Models;
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
    public partial class CuentaCorrienteView : UserControl
    {
        public CuentaCorrienteView()
        {
            InitializeComponent();
        }

        public void MostrarProveedor(ProveedorModel proveedor)
        {
            if (proveedor == null) return;

            // Actualizar nombre en el header
            DetalleNombre.Text = proveedor.Nombre;
            DetalleCuit.Text = proveedor.Cuit;
            DetalleTelefono.Text = proveedor.Telefono;
            DetalleEmail.Text = proveedor.Email;

            // Mostrar saldo actual
            DetalleSaldo.Text = proveedor.SaldoActual.ToString("C2"); // Formato $ 0,00

            // Llenar DataGrid con movimientos (transacciones)
            decimal saldoAcumulado = 0;
            MovimientosGrid.ItemsSource = proveedor.Transacciones.Select(t =>
            {
                if (t.TipoTransaccion == "Debe")
                    saldoAcumulado += t.Monto;
                else if (t.TipoTransaccion == "Haber")
                    saldoAcumulado -= t.Monto;

                return new
                {
                    Fecha = t.Fecha.ToShortDateString(),
                    Tipo = t.TipoTransaccion,
                    Concepto = t.Detalle,
                    Debe = t.TipoTransaccion == "Debe" ? t.Monto : 0,
                    Haber = t.TipoTransaccion == "Haber" ? t.Monto : 0,
                    Saldo = saldoAcumulado,
                };
            }).ToList();
        }

        private void BtnNuevaTransaccion_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            var ventana = new TransaccionesProveedorFormWindow();
            ventana.Owner = Window.GetWindow(this);
            ventana.ShowDialog();
        }
    }
}

