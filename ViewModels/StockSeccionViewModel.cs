using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;

namespace FIDELANDIA.ViewModels
{
    public class StockSeccionViewModel : INotifyPropertyChanged
    {
        private string _nombreTipoPasta;
        private decimal _cantidadDisponible;
        private decimal _contenidoEnvase;
        private DateTime _ultimaActualizacion;
        private ObservableCollection<LoteDetalleViewModel> _lotes;

        public string NombreTipoPasta
        {
            get => _nombreTipoPasta;
            set
            {
                if (_nombreTipoPasta != value)
                {
                    _nombreTipoPasta = value;
                    OnPropertyChanged(nameof(NombreTipoPasta));
                    OnPropertyChanged(nameof(Nombre));
                }
            }
        }

        public decimal CantidadDisponible
        {
            get => _cantidadDisponible;
            set
            {
                if (_cantidadDisponible != value)
                {
                    _cantidadDisponible = value;
                    OnPropertyChanged(nameof(CantidadDisponible));
                }
            }
        }

        public decimal ContenidoEnvase
        {
            get => _contenidoEnvase;
            set
            {
                if (_contenidoEnvase != value)
                {
                    _contenidoEnvase = value;
                    OnPropertyChanged(nameof(ContenidoEnvase));
                }
            }
        }

        public DateTime UltimaActualizacion
        {
            get => _ultimaActualizacion;
            set
            {
                if (_ultimaActualizacion != value)
                {
                    _ultimaActualizacion = value;
                    OnPropertyChanged(nameof(UltimaActualizacion));
                }
            }
        }

        // 🔹 ObservableCollection para que WPF detecte cambios en los lotes
        public ObservableCollection<LoteDetalleViewModel> Lotes
        {
            get => _lotes;
            set
            {
                if (_lotes != value)
                {
                    _lotes = value;
                    OnPropertyChanged(nameof(Lotes));
                    OnPropertyChanged(nameof(Filas));
                }
            }
        }

        // 🔹 Propiedad auxiliar para binding en XAML
        public string Nombre => NombreTipoPasta;

        public IEnumerable<string[]> Filas => Lotes?.Select(l => new string[]
        {
            l.IdLote.ToString(),
            l.FechaProduccion.ToString("dd/MM/yyyy"),
            l.FechaVencimiento.ToString("dd/MM/yyyy"),
            $"{l.CantidadDisponible} paquetes"
        }) ?? new List<string[]>();

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string nombrePropiedad)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nombrePropiedad));
        }
    }
}
