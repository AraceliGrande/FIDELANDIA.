using System;
using System.Collections.Generic;

namespace FIDELANDIA.Models
{
    public class TipoPastaModel
    {
        public int IdTipoPasta { get; set; }
        public string Nombre { get; set; }
        public decimal ContenidoEnvase { get; set; }
        public string Descripcion { get; set; }
        public decimal CostoActual { get; set; }

        // Relación 1 a muchos
        public virtual ICollection<LoteProduccionModel> Lotes { get; set; } = new List<LoteProduccionModel>();
    }
}

