using FIDELANDIA.Data;
using FIDELANDIA.Models;
using FIDELANDIA.Services;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace FIDELANDIA.Views.Produccion
{
    public partial class CrearProduccionFormView : UserControl
    {
        private readonly FidelandiaDbContext _dbContext;
        private readonly TipoPastaService _tipoPastaService;
        private readonly LoteProduccionService _loteService;

        public CrearProduccionFormView()
        {
            InitializeComponent();

            _dbContext = new FidelandiaDbContext();
            _tipoPastaService = new TipoPastaService(_dbContext);
            _loteService = new LoteProduccionService(_dbContext);

            CargarTiposPasta();
            DpFechaProduccion.SelectedDate = DateTime.Now;
        }

        private void CargarTiposPasta()
        {
            var tipos = _tipoPastaService.ObtenerTodos();

            CmbTipoPasta.ItemsSource = tipos;
            CmbTipoPasta.DisplayMemberPath = "Nombre";        
            CmbTipoPasta.SelectedValuePath = "IdTipoPasta";  
        }

        private void Cancelar_Click(object sender, RoutedEventArgs e)
        {
            Window.GetWindow(this)?.Close();
        }

        private void Guardar_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (CmbTipoPasta.SelectedValue == null)
                {
                    MessageBox.Show("Debe seleccionar un tipo de pasta.");
                    return;
                }

                if (!decimal.TryParse(TxtCantidad.Text, out decimal cantidad) || cantidad <= 0)
                {
                    MessageBox.Show("Ingrese una cantidad válida mayor a 0.");
                    return;
                }

                if (DpFechaProduccion.SelectedDate == null)
                {
                    MessageBox.Show("Seleccione la fecha de producción.");
                    return;
                }

                if (DpFechaVencimiento.SelectedDate == null)
                {
                    MessageBox.Show("Seleccione la fecha de vencimiento.");
                    return;
                }

                int idTipoPasta = (int)CmbTipoPasta.SelectedValue;
                DateTime fechaProduccion = DpFechaProduccion.SelectedDate.Value;
                DateTime fechaVencimiento = DpFechaVencimiento.SelectedDate.Value;

                string estado = "Creado"; 

                bool exito = _loteService.CrearLote(idTipoPasta, cantidad, fechaProduccion, fechaVencimiento, estado);

                if (exito)
                {
                    MessageBox.Show("Lote de producción creado correctamente.");
                    Window.GetWindow(this)?.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ocurrió un error al crear el lote de producción: {ex.Message}");
            }
        }

    }
}
