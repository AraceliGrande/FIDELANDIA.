public class DashboardResumen
{
    public decimal CantidadProducida { get; set; } // en envases
    public decimal VentasTotales { get; set; }
    public decimal ProduccionKg { get; set; } // nuevo indicador en kg
    public decimal TicketPromedio { get; set; }

    public decimal VariacionCantidadProducida { get; set; } // en envases
    public decimal VariacionVentasTotales { get; set; }
    public decimal VariacionProduccionKg { get; set; } // nueva variación en kg
    public decimal VariacionTicketPromedio { get; set; }

    public Dictionary<string, decimal> ProduccionPorTipo { get; set; }
    public Dictionary<string, decimal> VentasPorTipo { get; set; }
    public Dictionary<string, decimal> StockPorTipo { get; set; } // opcional, podés mantenerlo

    public Dictionary<string, decimal> ProduccionDiariaEnvases { get; set; }
    public Dictionary<string, decimal> ProduccionDiariaKg { get; set; }
    public Dictionary<string, decimal> VentasDiaria { get; set; }
    public Dictionary<string, decimal> RecaudacionDiaria { get; set; }
}
