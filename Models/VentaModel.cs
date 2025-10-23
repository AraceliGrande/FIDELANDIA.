using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FIDELANDIA.Models
{
    public class VentaModel
    {
        public int IdVenta { get; set; }  // PK
        public DateTime Fecha { get; set; } = DateTime.Now;

        // Relación 1 a muchos con DetalleVenta
        public virtual ICollection<DetalleVentaModel> DetalleVenta { get; set; } = new List<DetalleVentaModel>();
    }
}
