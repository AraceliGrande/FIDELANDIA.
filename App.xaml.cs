using System;
using System.Windows;
using System.Windows.Threading;

namespace FIDELANDIA
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            // Captura excepciones globales del AppDomain
            AppDomain.CurrentDomain.UnhandledException += (sender, args) =>
            {
                Exception ex = (Exception)args.ExceptionObject;
                MessageBox.Show($"Error no controlado: {ex.Message}\n{ex.StackTrace}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            };

            // Captura excepciones del Dispatcher (UI thread)
            DispatcherUnhandledException += (sender, args) =>
            {
                MessageBox.Show($"Excepción en UI: {args.Exception.Message}\n{args.Exception.StackTrace}", "Error UI", MessageBoxButton.OK, MessageBoxImage.Error);
                args.Handled = true; // evita que la app se cierre automáticamente
            };

            try
            {
                MainWindow mainWindow = new MainWindow();
                mainWindow.Show();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al iniciar MainWindow: {ex.Message}\n{ex.StackTrace}", "Error Inicio", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            base.OnStartup(e);
        }
    }
}
