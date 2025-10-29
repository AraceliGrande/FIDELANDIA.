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
            try
            {
                using (var db = new FidelandiaDbContext())
                {
                    // Intentar acceder a la base de datos
                    db.Database.CanConnect();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"❌ Error al conectar con la base de datos:\n{ex.Message}",
                                "Error de conexión",
                                MessageBoxButton.OK,
                                MessageBoxImage.Error);
            }
        }

    }
}