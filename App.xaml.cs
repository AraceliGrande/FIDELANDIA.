using FIDELANDIA.Data;
using Microsoft.Data.SqlClient; // <- agregado para SqlConnection
using Microsoft.EntityFrameworkCore;
using System;
using System.Windows;
using FIDELANDIA.Properties;
using System.Windows.Threading;
using Microsoft.VisualBasic; // <- agregado para InputBox

namespace FIDELANDIA
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            // Captura errores generales
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
                // ⚡ Detectar la instancia de SQL Server y guardar la cadena solo la primera vez
                if (string.IsNullOrEmpty(FIDELANDIA.Properties.Settings.Default.ConnectionString))
                {
                    string connectionString = "";
                    bool connected = false;

                    // Instancias predeterminadas
                    string[] defaultServers = { @"localhost\SQLEXPRESS", "localhost" };

                    foreach (var server in defaultServers)
                    {
                        try
                        {
                            using (var testConn = new SqlConnection($"Server={server};Database=master;Trusted_Connection=True;Encrypt=False;"))
                            {
                                testConn.Open();
                                connectionString = $"Server={server};Database=FidelandiaDB;Trusted_Connection=True;TrustServerCertificate=True;";
                                connected = true;
                                break;
                            }
                        }
                        catch
                        {
                            // No hacer nada, probar siguiente
                        }
                    }

                    // Si ninguna instancia funciona, preguntar al usuario
                    if (!connected)
                    {
                        string userServer = Interaction.InputBox(
                            "No se detectó ninguna instancia de SQL Server local.\n" +
                            "Por favor ingrese el nombre del servidor SQL (ej: localhost\\SQLEXPRESS):",
                            "Configurar Servidor SQL",
                            @"localhost\SQLEXPRESS");

                        if (!string.IsNullOrEmpty(userServer))
                        {
                            try
                            {
                                using (var testConn = new SqlConnection($"Server={userServer};Database=master;Trusted_Connection=True;Encrypt=False;"))
                                {
                                    testConn.Open();
                                    connectionString = $"Server={userServer};Database=FidelandiaDB;Trusted_Connection=True;TrustServerCertificate=True;";
                                    connected = true;
                                }
                            }
                            catch (Exception ex)
                            {
                                MessageBox.Show($"No se pudo conectar al servidor proporcionado:\n{ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                                Current.Shutdown();
                                return;
                            }
                        }
                        else
                        {
                            MessageBox.Show("No se ingresó ningún servidor. La aplicación se cerrará.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                            Current.Shutdown();
                            return;
                        }
                    }

                    // Guardar la cadena si se conectó
                    if (connected)
                    {
                        FIDELANDIA.Properties.Settings.Default.ConnectionString = connectionString;
                        FIDELANDIA.Properties.Settings.Default.Save();
                    }
                }

                // Crear la base de datos y ejecutar seed
                using (var db = new FidelandiaDbContext())
                {
                    db.Database.EnsureCreated();
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
