using System;
using System.Collections.Generic;

namespace FIDELANDIA.Models
{
    public class ProveedorModel
    {
        public int ProveedorID { get; set; }  // EF Core lo reconoce como PK automáticamente
        public string Nombre { get; set; }
        public string Cuit { get; set; }
        public string Direccion { get; set; }
        public string Telefono { get; set; }
        public string Email { get; set; }
        public decimal SaldoActual { get; set; } = 0;
        public DateTime FechaAlta { get; set; } = DateTime.Now;
        public decimal LimiteCredito { get; set; } = 0;
        public bool IsActivo { get; set; } = true;

        // Relación con categoría
        public int CategoriaProveedorID { get; set; }
        public CategoriaProveedorModel Categoria { get; set; }

        // Relación 1 a muchos con Transacciones
        public virtual ICollection<TransaccionModel> Transacciones { get; set; } = new List<TransaccionModel>();
    }
}
