public class DashboardResumen
{
    public decimal? CantidadProducida { get; set; }
    public decimal? VentasTotales { get; set; }
    public decimal? ProduccionKg { get; set; }
    public decimal? TicketPromedio { get; set; }

    public decimal? VariacionCantidadProducida { get; set; }
    public decimal? VariacionVentasTotales { get; set; }
    public decimal? VariacionProduccionKg { get; set; }
    public decimal? VariacionTicketPromedio { get; set; }

    public Dictionary<string, decimal> ProduccionPorTipo { get; set; }
    public Dictionary<string, decimal> VentasPorTipo { get; set; }
    public Dictionary<string, decimal> StockPorTipo { get; set; } // opcional, podés mantenerlo

    public Dictionary<string, decimal> ProduccionDiariaEnvases { get; set; }
    public Dictionary<string, decimal> ProduccionDiariaKg { get; set; }
    public Dictionary<string, decimal> VentasDiaria { get; set; }
    public Dictionary<string, decimal> RecaudacionDiaria { get; set; }
}
