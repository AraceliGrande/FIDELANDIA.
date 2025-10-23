using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FIDELANDIA.Models
{
    public class DetalleVentaModel
    {
        public int IdDetalle { get; set; }  // PK
        public int IdVenta { get; set; }
        public int IdLote { get; set; }
        public decimal Cantidad { get; set; }
        public decimal CostoUnitario { get; set; }

        public virtual VentaModel Venta { get; set; }
        public virtual LoteProduccionModel Lote { get; set; }


    }
}
