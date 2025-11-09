using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FIDELANDIA.Models
{
    public class BalanceDiarioModel
    {
        public DateTime Fecha { get; set; }
        public string TipoPasta { get; set; }
        public decimal CantidadProducida { get; set; }
        public decimal CantidadVendida { get; set; }
        public decimal StockActual { get; set; }
    }
}
