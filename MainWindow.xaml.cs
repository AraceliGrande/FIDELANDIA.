using FIDELANDIA.Data;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace FIDELANDIA
{
   
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            ProbarConexion();
        }
        private void ProbarConexion()
        {
            using (var db = new FidelandiaDbContext())
            {
                // Traer todos los proveedores de la base
                var proveedores = db.Proveedores.ToList();

                // Mostrar cantidad de proveedores
                MessageBox.Show($"Hay {proveedores.Count} proveedores en la base de datos");
            }
        }
    }
}