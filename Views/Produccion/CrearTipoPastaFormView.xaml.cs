using FIDELANDIA.Data;
using FIDELANDIA.Services;
using System;
using System.Windows;
using System.Windows.Controls;

namespace FIDELANDIA.Views.Produccion
{
    public partial class CrearTipoPastaFormView : UserControl
    {
        private readonly FidelandiaDbContext _dbContext;
        private readonly TipoPastaService _tipoPastaService;

        public CrearTipoPastaFormView()
        {
            InitializeComponent();
            _dbContext = new FidelandiaDbContext();
            _tipoPastaService = new TipoPastaService(_dbContext);
        }

        private void Cancelar_Click(object sender, RoutedEventArgs e)
        {
            Window.GetWindow(this)?.Close();
        }

        private void Guardar_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(TxtNombre.Text))
                {
                    MessageBox.Show("Ingrese el nombre del tipo de pasta.");
                    return;
                }

                if (!decimal.TryParse(TxtContenido.Text, out decimal contenido) || contenido <= 0)
                {
                    MessageBox.Show("Ingrese un contenido válido mayor a 0.");
                    return;
                }

                if (!decimal.TryParse(TxtCosto.Text, out decimal costo) || costo <= 0)
                {
                    MessageBox.Show("Ingrese un costo válido mayor a 0.");
                    return;
                }

                string nombre = TxtNombre.Text.Trim();
                string descripcion = TxtDescripcion.Text.Trim();

                bool exito = _tipoPastaService.CrearTipoPasta(nombre, contenido, descripcion, costo);

                if (exito)
                {
                    MessageBox.Show("Tipo de pasta creado correctamente.");
                    Window.GetWindow(this)?.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al crear tipo de pasta: {ex.Message}");
            }
        }
    }
}
