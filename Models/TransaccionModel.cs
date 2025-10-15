using System;

namespace FIDELANDIA.Models
{
    public class TransaccionModel
    {
        public int TransaccionID { get; set; }          // EF Core lo reconoce como PK automáticamente
        public string TipoTransaccion { get; set; }
        public decimal Monto { get; set; }
        public DateTime Fecha { get; set; } = DateTime.Now;
        public string Detalle { get; set; }

        // Relación con Proveedor
        public int ProveedorID { get; set; }
        public ProveedorModel Proveedor { get; set; }
    }
}
