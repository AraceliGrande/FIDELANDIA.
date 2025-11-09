using System;

namespace FIDELANDIA.Models
{
    public class StockActualModel
    {
        public int IdStock { get; set; }  // EF Core lo reconoce como PK automáticamente

        public int IdTipoPasta { get; set; }

        public decimal CantidadDisponible { get; set; }

        public DateTime UltimaActualizacion { get; set; } = DateTime.Now;

        public virtual TipoPastaModel TipoPasta { get; set; }

        public virtual ICollection<LoteProduccionModel> LotesDisponibles { get; set; } = new List<LoteProduccionModel>();

    }
}
