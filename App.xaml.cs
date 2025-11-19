using FIDELANDIA.Data;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System;
using System.Windows;
using FIDELANDIA.Properties;
using System.Windows.Threading;
using Microsoft.VisualBasic;

namespace FIDELANDIA
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            // Captura errores generales no controlados
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
                string connectionString = Settings.Default.ConnectionString;

                // ⚡ Bucle hasta que tengamos una conexión válida o se cancele
                while (true)
                {
                    bool connected = false;

                    // Si ya había cadena guardada, probamos primero con ella
                    if (!string.IsNullOrEmpty(connectionString))
                    {
                        try
                        {
                            using (var testConn = new SqlConnection(connectionString))
                            {
                                testConn.Open();
                            }
                            connected = true;
                            break; // conexión válida, salimos del bucle
                        }
                        catch
                        {
                            // falla, pedimos la instancia al usuario
                        }
                    }

                    // Pedir al usuario la instancia de SQL Server
                    string userServer = Interaction.InputBox(
                        "Ingrese el nombre de la instancia SQL Server (ej: localhost\\SQLEXPRESS):",
                        "Configurar Servidor SQL",
                        @"localhost\SQLEXPRESS");

                    // ⚡ Detectamos si canceló o no escribió nada
                    if (string.IsNullOrEmpty(userServer))
                    {
                        MessageBox.Show("Se canceló la configuración. La aplicación se cerrará.", "Cancelado", MessageBoxButton.OK, MessageBoxImage.Information);
                        Current.Shutdown();
                        return;
                    }

                    connectionString = $"Server={userServer};Database=FidelandiaDB;Trusted_Connection=True;TrustServerCertificate=True;";

                    try
                    {
                        using (var testConn = new SqlConnection(connectionString))
                        {
                            testConn.Open();
                        }

                        // Guardamos la cadena si funciona
                        Settings.Default.ConnectionString = connectionString;
                        Settings.Default.Save();
                        break; // salimos del bucle
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(
                            $"No se pudo conectar al servidor proporcionado:\n{ex.Message}\n\nIntente nuevamente o presione Cancelar para salir.",
                            "Error de Conexión",
                            MessageBoxButton.OK,
                            MessageBoxImage.Error
                        );
                        connectionString = ""; // fuerza que vuelva a pedir
                    }
                }

                // ⚡ Crear la base y las tablas con EnsureCreated
                using (var db = new FidelandiaDbContext())
                {
                    db.Database.EnsureCreated(); // crea tablas si no existen
                    db.Seed();                   // llena datos iniciales
                }

                // ⚡ Lanzar la ventana principal
                MainWindow main = new MainWindow();
                main.Show();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"ERROR al inicializar la Base de Datos:\n\n{ex.Message}\n\nLa aplicación se cerrará.",
                    "Error de Base de Datos",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                );
                Current.Shutdown();
                return;
            }
        }
    }
}
