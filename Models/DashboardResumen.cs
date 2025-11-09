using System;
using System.Collections.Generic;

namespace FIDELANDIA.Models
{
    public class DashboardResumen
    {
        public decimal CantidadProducida { get; set; }
        public decimal VentasTotales { get; set; }
        public decimal StockPromedio { get; set; }
        public decimal TicketPromedio { get; set; }

        public Dictionary<string, decimal> ProduccionPorTipo { get; set; }
        public Dictionary<string, decimal> VentasPorTipo { get; set; }
        public Dictionary<string, decimal> StockPorTipo { get; set; }

        public Dictionary<string, decimal> ProduccionDiaria { get; set; }
        public Dictionary<string, decimal> VentasDiaria { get; set; }
        public Dictionary<string, decimal> RecaudacionDiaria { get; set; }
    }
}
