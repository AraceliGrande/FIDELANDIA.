using System;
using System.ComponentModel;

namespace FIDELANDIA.ViewModels
{
    public class LoteDetalleViewModel : INotifyPropertyChanged
    {
        private int _idLote;
        private DateTime _fechaProduccion;
        private DateTime _fechaVencimiento;
        private decimal _cantidadDisponible;
        private string _estado;

        public int IdLote
        {
            get => _idLote;
            set { _idLote = value; OnPropertyChanged(nameof(IdLote)); }
        }

        public DateTime FechaProduccion
        {
            get => _fechaProduccion;
            set { _fechaProduccion = value; OnPropertyChanged(nameof(FechaProduccion)); }
        }

        public DateTime FechaVencimiento
        {
            get => _fechaVencimiento;
            set { _fechaVencimiento = value; OnPropertyChanged(nameof(FechaVencimiento)); }
        }

        public decimal CantidadDisponible
        {
            get => _cantidadDisponible;
            set { _cantidadDisponible = value; OnPropertyChanged(nameof(CantidadDisponible)); }
        }

        public string Estado
        {
            get => _estado;
            set { _estado = value; OnPropertyChanged(nameof(Estado)); }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string nombrePropiedad)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nombrePropiedad));
        }
    }
}
