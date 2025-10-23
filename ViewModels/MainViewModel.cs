using FIDELANDIA.Commands;
using FIDELANDIA.Helpers;
using FIDELANDIA.Views;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;

namespace FIDELANDIA.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private object _currentView;
        private GridLength _sidebarWidth = new GridLength(60);
        private bool _isExpanded = false;

        public object CurrentView
        {
            get => _currentView;
            set { _currentView = value; OnPropertyChanged(); }
        }

        public GridLength SidebarWidth
        {
            get => _sidebarWidth;
            set { _sidebarWidth = value; OnPropertyChanged(); }
        }

        public ICommand ToggleSidebarCommand { get; }
        public ICommand NavigateCommand { get; }

        public MainViewModel()
        {
            // Vista inicial
            CurrentView = new Views.Home.HomeView();

            ToggleSidebarCommand = new RelayCommand(_ => ToggleSidebar());

            // Inicializar NavigateCommand usando el método Navigate
            NavigateCommand = new RelayCommand(Navigate);

            AppEvents.LoteCreado += () =>
            {
                // Solo si la vista actual es ProduccionView
                if (CurrentView is Views.Produccion.ProduccionView)
                {
                    // Reemplazar la instancia para reiniciar todo
                    CurrentView = new Views.Produccion.ProduccionView();
                }
            };
        }

        private void ToggleSidebar()
        {
            SidebarWidth = _isExpanded ? new GridLength(60) : new GridLength(220);
            _isExpanded = !_isExpanded;
        }

        private void Navigate(object parameter)
        {
            string destination = parameter?.ToString();

            switch (destination)
            {
                case "Home":    
                    CurrentView = new Views.Home.HomeView();
                    break;
                case "Dashboard":
                    CurrentView = new DashboardView();
                    break;
                case "Proveedores":
                    CurrentView = new ProveedoresView();
                    break;
                case "Produccion":
                    CurrentView = new Views.Produccion.ProduccionView();
                    break;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string prop = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
    }
}
