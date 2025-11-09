using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FIDELANDIA.Models
{
    public class LoteProduccionModel
    {
        public int IdLote { get; set; }
        public int IdTipoPasta { get; set; }  // FK a TipoPasta
        public decimal CantidadDisponible { get; set; }
        public decimal CantidadProducida { get; set; }
        public DateTime FechaProduccion { get; set; }
        public DateTime FechaVencimiento { get; set; }
        public string Estado { get; set; }

        public virtual TipoPastaModel TipoPasta { get; set; }

        public int? IdStockActual { get; set; }
        public virtual StockActualModel StockActual { get; set; }

    }
}