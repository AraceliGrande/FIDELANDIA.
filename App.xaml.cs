using System.Configuration;
using System.Data;
using System.Windows;

namespace FIDELANDIA
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            MainWindow mainWindow = new MainWindow();
            mainWindow.Show();
            base.OnStartup(e);
        }
    }

}
