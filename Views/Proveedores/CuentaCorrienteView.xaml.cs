using FIDELANDIA.Data;
using FIDELANDIA.Models;
using FIDELANDIA.Services;
using FIDELANDIA.Windows;
using Microsoft.EntityFrameworkCore;
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
        private readonly ProveedorService _service;
        private int paginaActual = 1;
        private int tamanoPagina = 13;
        private int totalPaginas = 1; // se calcula según la cantidad total de transacciones
        private ProveedorModel _proveedorActual; // ← variable de instancia

        private Dictionary<int, decimal> ultimosSaldosPorPagina = new Dictionary<int, decimal>();


        public CuentaCorrienteView()
        {
            InitializeComponent();

            var dbContext = new FidelandiaDbContext();
            _service = new ProveedorService(dbContext);

        }

        private void BtnPaginaAnterior_Click(object sender, RoutedEventArgs e)
        {
            if (paginaActual > 1)
            {
                paginaActual--;
                MostrarProveedor(_proveedorActual, paginaActual, tamanoPagina, false);
            }
        }

        private void BtnPaginaSiguiente_Click(object sender, RoutedEventArgs e)
        {
            if (paginaActual < totalPaginas)
            {
                paginaActual++;
                MostrarProveedor(_proveedorActual, paginaActual, tamanoPagina, false);
            }
        }

        public void MostrarProveedor(ProveedorModel proveedor, int pagina = 1, int tamanoPagina = 13, bool resetearEstado = false)
        {
            if (proveedor == null) return;

            _proveedorActual = proveedor;

            if (resetearEstado)
            {
                paginaActual = 1;
                tamanoPagina = 13; 
                ultimosSaldosPorPagina.Clear();

                // if (CbTamanoPagina != null)
                CbTamanoPagina.SelectedIndex = 1;
            }

            try
            {
                // Detalles del proveedor
                DetalleNombre.Text = proveedor.Nombre;
                DetalleCuit.Text = proveedor.Cuit;
                DetalleTelefono.Text = proveedor.Telefono;
                DetalleEmail.Text = proveedor.Email;
                DetalleCategoria.Text = proveedor.Categoria?.Nombre ?? "N/A";
                DetalleLimiteCredito.Text = proveedor.LimiteCredito.ToString("C2");
                DetalleActivo.Text = proveedor.IsActivo ? "Sí" : "No";
                DetalleSaldo.Text = proveedor.SaldoActual.ToString("C2");

                // Total de páginas
                int totalTransacciones = _service.ContarTransacciones(proveedor.ProveedorID);
                totalPaginas = (int)Math.Ceiling((double)totalTransacciones / tamanoPagina);

                // Obtener saldo inicial desde el diccionario
                decimal saldoAnterior = 0;
                if (pagina > 1 && ultimosSaldosPorPagina.ContainsKey(pagina - 1))
                    saldoAnterior = ultimosSaldosPorPagina[pagina - 1];

                // Traer transacciones de la página
                var transacciones = _service.ObtenerTransaccionesPaginadas(proveedor.ProveedorID, pagina, tamanoPagina, saldoAnterior);

                // Guardar saldo final de la página
                if (transacciones.Any())
                {
                    decimal saldoFinalPagina = transacciones.Last().Saldo;
                    ultimosSaldosPorPagina[pagina] = saldoFinalPagina;
                }

                // Llenar DataGrid
                MovimientosGrid.ItemsSource = transacciones.Select(t => new
                {
                    Fecha = t.Fecha.ToShortDateString(),
                    Tipo = t.TipoTransaccion,
                    Concepto = t.Detalle,
                    Debe = t.TipoTransaccion == "Debe" ? $"+ {t.Monto:C2}" : "",
                    Haber = t.TipoTransaccion == "Haber" ? $"- {t.Monto:C2}" : "",
                    Saldo = t.Saldo >= 0 ? $"+ {t.Saldo:C2}" : $"- {Math.Abs(t.Saldo):C2}"
                }).ToList();

                // Actualizar paginación
                TxtPaginaActual.Text = $"Página {paginaActual} / {totalPaginas}";
                BtnAnterior.IsEnabled = paginaActual > 1;
                BtnSiguiente.IsEnabled = paginaActual < totalPaginas;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al mostrar el proveedor: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


        private void CbTamanoPagina_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (CbTamanoPagina.SelectedItem is ComboBoxItem item)
            {
                if (int.TryParse(item.Content.ToString(), out int nuevoTamano))
                {
                    tamanoPagina = nuevoTamano;
                    paginaActual = 1;

                    // ✅ Reiniciar array de saldos al cambiar tamaño de página
                    ultimosSaldosPorPagina.Clear();

                    MostrarProveedor(_proveedorActual, paginaActual, tamanoPagina);
                }
            }
        }


        private void BtnNuevaTransaccion_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (_proveedorActual == null)
            {
                MessageBox.Show("Debe seleccionar un proveedor antes de registrar una transacción.", "Advertencia", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var ventana = new TransaccionesProveedorFormWindow(_proveedorActual);
            ventana.Owner = Window.GetWindow(this);
            ventana.ShowDialog();

            // Refrescar la tabla después de guardar
            ultimosSaldosPorPagina.Clear();
            MostrarProveedor(_proveedorActual, paginaActual, tamanoPagina);
        }
    }
}

