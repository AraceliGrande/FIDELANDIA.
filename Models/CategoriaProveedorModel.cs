using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FIDELANDIA.Models
{
    public class CategoriaProveedorModel
    {
        public int CategoriaProveedorID { get; set; }  // PK
        public string Nombre { get; set; }             // Nombre de la categoría

        // Relación 1 a muchos con Proveedores
        public virtual ICollection<ProveedorModel> Proveedores { get; set; } = new List<ProveedorModel>();
    }
}

