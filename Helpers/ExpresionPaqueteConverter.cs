using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace FIDELANDIA.Helpers
{
    public class ExpresionPaqueteConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) return "0 paquetes";

            decimal cantidad;
            if (!decimal.TryParse(value.ToString(), out cantidad))
                return "0 paquetes";

            // Singular o plural
            string unidad = (cantidad == 1) ? "paquete" : "paquetes";

            return $"{cantidad} {unidad}";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
