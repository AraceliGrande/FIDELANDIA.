using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;

namespace FIDELANDIA.ViewModels
{
    public class ProduccionDatos : INotifyPropertyChanged
    {
        private int _totalTipos;
        private int _produccionTotal;
        private int _ventasDia;
        private int _stockTotal;

        public int TotalTipos
        {
            get => _totalTipos;
            set { _totalTipos = value; OnPropertyChanged(nameof(TotalTipos)); }
        }

        public int ProduccionTotal
        {
            get => _produccionTotal;
            set { _produccionTotal = value; OnPropertyChanged(nameof(ProduccionTotal)); }
        }

        public int VentasDia
        {
            get => _ventasDia;
            set { _ventasDia = value; OnPropertyChanged(nameof(VentasDia)); }
        }

        public int StockTotal
        {
            get => _stockTotal;
            set { _stockTotal = value; OnPropertyChanged(nameof(StockTotal)); }
        }

        // 🔹 Cambiado a ObservableCollection para que la UI detecte cambios en la lista
        public ObservableCollection<StockSeccionViewModel> Secciones { get; set; } = new ObservableCollection<StockSeccionViewModel>();

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string name)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
