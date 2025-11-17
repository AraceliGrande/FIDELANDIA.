using FIDELANDIA.Data;
using FIDELANDIA.Helpers;
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
using System.IO;


namespace FIDELANDIA.Views
{
    public partial class CuentaCorrienteView : UserControl
    {
        private readonly ProveedorService _service;
        private bool traerTodos = false;
        private int paginaActual = 1;
        private int tamanoPagina = 15;
        private int totalPaginas = 1; // se calcula según la cantidad total de transacciones
        private ProveedorModel _proveedorActual; // ← variable de instancia

        private Dictionary<int, decimal> ultimosSaldosPorPagina = new Dictionary<int, decimal>();

        private bool _modoFiltrado = false;

        public ICommand AbrirComprobanteCommand { get; }

        public CuentaCorrienteView()
        {
            InitializeComponent();

            var dbContext = new FidelandiaDbContext();
            _service = new ProveedorService(dbContext);

            AbrirComprobanteCommand = new RelayCommand<string>(AbrirComprobante);
            DataContext = this; // importante
        }

        private void BtnVerComprobante_Click(object sender, RoutedEventArgs e)
        {
            var btn = sender as Button;
            string ruta = btn?.CommandParameter as string;

            AbrirComprobante(ruta); 
        }

        private void AbrirComprobante(string rutaRelativa)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(rutaRelativa))
                {
                    MessageBox.Show(
                        "No hay comprobante disponible para esta transacción.",
                        "Aviso",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information
                    );
                    return;
                }

                // MISMA carpeta base que usa tu ImagenHelper
                string carpetaBase = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                    "Fidelandia",
                    "Comprobantes"
                );

                // Interpretamos la ruta guardada en BD (ej: "Comprobantes\archivo.pdf")
                string nombreArchivo = Path.GetFileName(rutaRelativa);

                // Armamos la ruta completa real
                string rutaCompleta = Path.Combine(carpetaBase, nombreArchivo);

                if (!File.Exists(rutaCompleta))
                {
                    MessageBox.Show(
                        "El comprobante no existe en el sistema:\n" + rutaCompleta,
                        "Archivo no encontrado",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning
                    );
                    return;
                }

                // Determinar tipo por extensión
                string extension = Path.GetExtension(rutaCompleta).ToLower();

                if (extension == ".pdf" ||
                    extension == ".jpg" || extension == ".jpeg" ||
                    extension == ".png" || extension == ".bmp")
                {
                    // Abrimos el archivo con la app predeterminada
                    System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                    {
                        FileName = rutaCompleta,
                        UseShellExecute = true
                    });
                }
                else
                {
                    MessageBox.Show(
                        $"Tipo de archivo no soportado: {extension}",
                        "Aviso",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning
                    );
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Error al abrir el comprobante: " + ex.Message,
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                );
            }
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

        public void MostrarProveedor(ProveedorModel proveedor, int pagina = 1, int tamanoPagina = 15, bool resetearEstado = false, bool traerTodos = false)
        {
            if (proveedor == null) return;

            _proveedorActual = proveedor;

            if (resetearEstado)
            {
                paginaActual = 1;
                tamanoPagina = 15;
                ultimosSaldosPorPagina.Clear();
                CbTamanoPagina.SelectedIndex = 1;
            }

            try
            {
                // Detalles del proveedor
                DetalleNombre.Text = proveedor.Nombre ?? "(Sin nombre)";
                DetalleSaldo.Text = proveedor.SaldoActual.ToString("C2");

                NombreProveedor_Busqueda.Text = proveedor.Nombre ?? "(Sin nombre)";
                DetalleSaldoProveedor_Busqueda.Text = proveedor.SaldoActual.ToString("C2");

                DetalleNombre.Background = Brushes.Transparent;

                // Construyo el contenido del tooltip
                var tooltipPanel = new StackPanel { Margin = new Thickness(6) };
                tooltipPanel.Children.Add(new TextBlock
                {
                    Text = $"Datos del proveedor",
                    FontWeight = FontWeights.SemiBold,
                    Margin = new Thickness(0, 2, 0, 6)
                });
                tooltipPanel.Children.Add(new TextBlock
                {
                    Text = $"📋 CUIT: {proveedor.Cuit ?? "N/A"}  |  📞 Teléfono: {proveedor.Telefono ?? "N/A"}  |  📧 Email: {proveedor.Email ?? "N/A"}",
                    Margin = new Thickness(0, 2, 0, 2)
                });
                tooltipPanel.Children.Add(new TextBlock
                {
                    Text = $"🗂 Categoría: {proveedor.Categoria?.Nombre ?? "N/A"}  |  💰 Límite crédito: {proveedor.LimiteCredito.ToString("C2")}  |  ⚡ Activo: {(proveedor.IsActivo ? "Sí" : "No")}",
                    Margin = new Thickness(0, 2, 0, 2)
                });

                var tooltipBorder = new Border
                {
                    CornerRadius = new CornerRadius(8),
                    Padding = new Thickness(10),
                    Background = Brushes.White,
                    BorderBrush = Brushes.Transparent,
                    BorderThickness = new Thickness(0),
                    Child = tooltipPanel,
                    Effect = new System.Windows.Media.Effects.DropShadowEffect
                    {
                        Color = Colors.Black,
                        BlurRadius = 6,
                        ShadowDepth = 2,
                        Opacity = 0.25
                    }
                };

                var tt = new ToolTip
                {
                    Content = tooltipBorder,
                    Placement = System.Windows.Controls.Primitives.PlacementMode.Mouse,
                    Background = Brushes.Transparent,
                    BorderBrush = Brushes.Transparent,
                    BorderThickness = new Thickness(0),
                    HasDropShadow = false
                };

                DetalleNombre.ToolTip = tt;
                ToolTipService.SetInitialShowDelay(DetalleNombre, 50);
                ToolTipService.SetShowDuration(DetalleNombre, 20000);

                NombreProveedor_Busqueda.ToolTip = tt;
                ToolTipService.SetInitialShowDelay(NombreProveedor_Busqueda, 50);
                ToolTipService.SetShowDuration(NombreProveedor_Busqueda, 20000);

                // --- Aquí manejamos traerTodos ---
                if (traerTodos)
                {
                    // Traigo todas las transacciones sin paginación
                    var transacciones = _service.ObtenerTransaccionesPaginadas(proveedor.ProveedorID, 1, int.MaxValue, 0, true);

                    MovimientosGrid.ItemsSource = transacciones.Select(t => new
                    {
                        Fecha = t.Fecha.ToShortDateString(),
                        Tipo = t.TipoTransaccion,
                        Concepto = t.Detalle,
                        Debe = t.TipoTransaccion == "Debe" ? $"+ {t.Monto:C2}" : "",
                        Haber = t.TipoTransaccion == "Haber" ? $"- {t.Monto:C2}" : "",
                        Saldo = t.Saldo >= 0 ? $"+ {t.Saldo:C2}" : $"- {Math.Abs(t.Saldo):C2}",
                        ComprobanteRuta = t.ComprobanteRuta
                    }).ToList();

                    TxtPaginaActual.Text = $"Mostrando todos los registros ({transacciones.Count})";
                    BtnAnterior.Visibility = Visibility.Collapsed;
                    BtnSiguiente.Visibility = Visibility.Collapsed;
                }
                else
                {
                    // Lógica de paginación original
                    int totalTransacciones = _service.ContarTransacciones(proveedor.ProveedorID);
                    totalPaginas = (int)Math.Ceiling((double)totalTransacciones / tamanoPagina);

                    decimal saldoAnterior = 0;
                    if (pagina > 1 && ultimosSaldosPorPagina.ContainsKey(pagina - 1))
                        saldoAnterior = ultimosSaldosPorPagina[pagina - 1];

                    var transacciones = _service.ObtenerTransaccionesPaginadas(proveedor.ProveedorID, pagina, tamanoPagina, saldoAnterior, false);

                    if (transacciones.Any())
                    {
                        decimal saldoFinalPagina = transacciones.Last().Saldo;
                        ultimosSaldosPorPagina[pagina] = saldoFinalPagina;
                    }

                    MovimientosGrid.ItemsSource = transacciones.Select(t => new
                    {
                        Fecha = t.Fecha.ToShortDateString(),
                        Tipo = t.TipoTransaccion,
                        Concepto = t.Detalle,
                        Debe = t.TipoTransaccion == "Debe" ? $"+ {t.Monto:C2}" : "",
                        Haber = t.TipoTransaccion == "Haber" ? $"- {t.Monto:C2}" : "",
                        Saldo = t.Saldo >= 0 ? $"+ {t.Saldo:C2}" : $"- {Math.Abs(t.Saldo):C2}",
                        ComprobanteRuta = t.ComprobanteRuta
                    }).ToList();

                    TxtPaginaActual.Text = $"Página {paginaActual} / {totalPaginas}";
                    BtnAnterior.Visibility = Visibility.Visible;
                    BtnSiguiente.Visibility = Visibility.Visible;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Error al mostrar el proveedor: {ex.Message}",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                );
            }
        }

        private void BtnQuitarFiltro_Click(object sender, RoutedEventArgs e)
        {
            FechaDesdePicker.SelectedDate = null;
            FechaHastaPicker.SelectedDate = null;
            MostrarProveedor(_proveedorActual, pagina: 1, tamanoPagina: 0, traerTodos: true);
        }

        private void BtnCerrarFiltro_Click(object sender, RoutedEventArgs e)
        {
            PanelFiltros.Visibility = PanelFiltros.Visibility == Visibility.Visible
                ? Visibility.Collapsed
                : Visibility.Visible;
            EncabezadoCuentaCorriente.Visibility = PanelFiltros.Visibility == Visibility.Visible
              ? Visibility.Collapsed
              : Visibility.Visible;
            EncabezadoInfo.Visibility = PanelFiltros.Visibility != Visibility.Visible
             ? Visibility.Collapsed
             : Visibility.Visible;
            tamanoPagina = 15;
            traerTodos = false;
            ActualizarEstadoPaginacion(estaFiltrado: false);
            MostrarProveedor(_proveedorActual, 1, tamanoPagina, traerTodos: false);
        }

        private void BtnMostrarFiltros_Click(object sender, RoutedEventArgs e)
        {
            // Alternar visibilidad del panel de filtros
            PanelFiltros.Visibility = PanelFiltros.Visibility == Visibility.Visible
                ? Visibility.Collapsed
                : Visibility.Visible;
            EncabezadoCuentaCorriente.Visibility = PanelFiltros.Visibility == Visibility.Visible
                ? Visibility.Collapsed
                : Visibility.Visible;
            EncabezadoInfo.Visibility = PanelFiltros.Visibility != Visibility.Visible
             ? Visibility.Collapsed
             : Visibility.Visible;

            // Cuando se muestra el panel, ocultar paginación
            if (PanelFiltros.Visibility == Visibility.Visible)
            {
                _modoFiltrado = true;
                ActualizarEstadoPaginacion(estaFiltrado: true);
                MostrarProveedor(_proveedorActual, pagina: 1, tamanoPagina: 0, traerTodos: true);
            }
            else
            {
                _modoFiltrado = false;
                ActualizarEstadoPaginacion(estaFiltrado: false);
                MostrarProveedor(_proveedorActual, paginaActual, tamanoPagina, traerTodos: false);
            }
        }


        private void BtnBuscarPorFecha_Click(object sender, RoutedEventArgs e)
        {
            if (_proveedorActual == null)
            {
                MessageBox.Show(
                    "Debe seleccionar un proveedor primero.",
                    "Advertencia",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning
                );
                return;
            }

            DateTime? fechaDesde = FechaDesdePicker.SelectedDate;
            DateTime? fechaHasta = FechaHastaPicker.SelectedDate;

            var transacciones = _service.ObtenerTransaccionesPorFechas(_proveedorActual.ProveedorID, fechaDesde, fechaHasta);

            MovimientosGrid.ItemsSource = transacciones.Select(t => new
            {
                Fecha = t.Fecha.ToShortDateString(),
                Tipo = t.TipoTransaccion,
                Concepto = t.Detalle,
                Debe = t.TipoTransaccion == "Debe" ? $"+ {t.Monto:C2}" : "",
                Haber = t.TipoTransaccion == "Haber" ? $"- {t.Monto:C2}" : "",
                Saldo = t.Saldo >= 0 ? $"+ {t.Saldo:C2}" : $"- {Math.Abs(t.Saldo):C2}",
                ComprobanteRuta = t.ComprobanteRuta
            }).ToList();

            // 🔒 Deshabilitar paginación mientras haya filtro
            _modoFiltrado = true;
            ActualizarEstadoPaginacion(estaFiltrado: true);

            TxtPaginaActual.Text = $"Filtrado ({transacciones.Count}) resultados";
        }

        private void ActualizarEstadoPaginacion(bool estaFiltrado = false)
        {
            var visibilidad = estaFiltrado ? Visibility.Collapsed : Visibility.Visible;

            CbTamanoPagina.Visibility = visibilidad;
            BtnNuevoMovimiento.Visibility = visibilidad;
            BtnAnterior.Visibility = visibilidad;
            BtnSiguiente.Visibility = visibilidad;
            BtnMostrarFiltros.Visibility = visibilidad;
        }

        private void CbTamanoPagina_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (CbTamanoPagina.SelectedItem is ComboBoxItem item)
            {
                string contenido = item.Content.ToString();

                if (contenido == "Todos")
                {
                    ultimosSaldosPorPagina.Clear();

                    // Llamamos al método con traerTodos = true
                    MostrarProveedor(_proveedorActual, pagina: 1, tamanoPagina: 0, traerTodos: true);
                }
                else if (int.TryParse(contenido, out int nuevoTamano))
                {
                    tamanoPagina = nuevoTamano;
                    paginaActual = 1;
                    ultimosSaldosPorPagina.Clear();

                    MostrarProveedor(_proveedorActual, paginaActual, tamanoPagina, traerTodos: false);
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

            bool? resultado = ventana.ShowDialog();

            ultimosSaldosPorPagina.Clear();
            MostrarProveedor(_proveedorActual, paginaActual, tamanoPagina);

            if (resultado == true)
            {
                // Disparar evento global pasando el proveedor que acaba de modificarse
                AppEvents.OnTransaccionCreada(_proveedorActual.ProveedorID);
            }
        }

        private readonly ExcelExportService _excelExportService = new ExcelExportService();

        private void BtnExportarExcel_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (MovimientosGrid.ItemsSource == null)
                {
                    MessageBox.Show(
                        "No hay datos para exportar.",
                        "Advertencia",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning
                    );
                    return;
                }

                // Convertimos los elementos actuales del DataGrid en una lista de objetos
                var datos = MovimientosGrid.ItemsSource.Cast<object>().ToList();

                if (datos.Count == 0)
                {
                    MessageBox.Show(
                        "No hay registros para exportar.",
                        "Información",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information
                    );
                    return;
                }

                // Llamamos al servicio para exportar
                _excelExportService.ExportarAExcel(datos, $"CuentaCorriente_{_proveedorActual?.Nombre ?? "Proveedor"}");

            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Error al exportar a Excel: {ex.Message}",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                );
            }
        }

    }
}

