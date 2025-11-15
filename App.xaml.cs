using FIDELANDIA.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Windows;
using System.Windows.Threading;

namespace FIDELANDIA
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            // Captura de errores generales
            AppDomain.CurrentDomain.UnhandledException += (sender, args) =>
            {
                Exception ex = (Exception)args.ExceptionObject;
                MessageBox.Show($"ERROR NO CONTROLADO:\n{ex.Message}", "Error Fatal", MessageBoxButton.OK, MessageBoxImage.Error);
            };

            // Captura errores de UI
            DispatcherUnhandledException += (sender, args) =>
            {
                MessageBox.Show($"ERROR EN INTERFAZ:\n{args.Exception.Message}", "Error UI", MessageBoxButton.OK, MessageBoxImage.Error);
                args.Handled = true;
            };

            base.OnStartup(e);

            try
            {
                using (var db = new FidelandiaDbContext())
                {
                    // Crea la base de datos si no existe
                    db.Database.EnsureCreated();

                    // 👉 EJECUTA EL SEED SOLO SI ES NECESARIO
                    db.Seed();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"ERROR al inicializar la Base de Datos:\n\n" +
                    $"{ex.Message}\n\n" +
                    $"La aplicación se cerrará.",
                    "Error de Base de Datos",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                );

                Current.Shutdown();
                return;
            }

            // Lanzar la ventana principal
            MainWindow main = new MainWindow();
            main.Show();
        }
    }
}
