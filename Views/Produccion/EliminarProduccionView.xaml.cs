using FIDELANDIA.Services;
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

namespace FIDELANDIA.Views.Produccion
{
    public partial class EliminarProduccionView : UserControl
    {
        public event EventHandler<LoteDetalleViewModel1> EliminarRequested;
        public event EventHandler CancelRequested;

        public LoteDetalleViewModel1 Produccion
        {
            get => (LoteDetalleViewModel1)DataContext;
            set => DataContext = value;
        }

        public EliminarProduccionView()
        {
            InitializeComponent();
        }

        private void BtnCancelar_Click(object sender, RoutedEventArgs e)
        {
            CancelRequested?.Invoke(this, EventArgs.Empty);
        }

        private void BtnEliminar_Click(object sender, RoutedEventArgs e)
        {
            if (Produccion != null)
                EliminarRequested?.Invoke(this, Produccion);
        }
    }
}