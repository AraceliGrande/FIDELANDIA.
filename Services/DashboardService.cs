using FIDELANDIA.Data;
using FIDELANDIA.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FIDELANDIA.Services
{
    public class DashboardService
    {
        private readonly FidelandiaDbContext _dbContext;

        public DashboardService(FidelandiaDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public DashboardResumen ObtenerDashboard(DateTime desde, DateTime hasta, bool agruparPorMes)
        {
            try
            {
                // ===================== CALCULO DE PERIODO =====================
                int diasPeriodo = (hasta - desde).Days + 1;
                int mesesPeriodo = (hasta.Year - desde.Year) * 12 + hasta.Month - desde.Month + 1;
                DateTime desdeAnterior = desde.AddDays(-diasPeriodo);
                DateTime hastaAnterior = hasta.AddDays(-diasPeriodo);

                // ===================== PRODUCCIÓN =====================
                var produccionPeriodo = _dbContext.LoteProduccion
                    .Where(l => (l.Estado == "Creado" || l.Estado == "Confirmado")
                             && l.FechaProduccion >= desdeAnterior
                             && l.FechaProduccion <= hasta)
                    .Select(l => new
                    {
                        l.FechaProduccion,
                        l.CantidadProducida,
                        Tipo = l.TipoPasta.Nombre,
                        Contenido = l.TipoPasta.ContenidoEnvase
                    })
                    .ToList();

                // ===================== VENTAS =====================
                var ventasPeriodo = _dbContext.DetalleVenta
                    .Include(v => v.Venta)
                    .Include(v => v.Lote).ThenInclude(l => l.TipoPasta)
                    .Where(v => v.Venta.Fecha >= desdeAnterior && v.Venta.Fecha <= hasta)
                    .Where(v => v.Lote.Estado == "Creado" || v.Lote.Estado == "Confirmado")
                    .Select(v => new
                    {
                        v.Cantidad,
                        v.CostoUnitario,
                        Fecha = v.Venta.Fecha,
                        Tipo = v.Lote.TipoPasta.Nombre
                    })
                    .ToList();

                // ===================== VALIDACIÓN PARA PERIODOS LARGOS =====================
                bool periodoLargo = mesesPeriodo > 7;

                // ===================== INDICADORES =====================
                decimal? cantidadProducidaActual = null;
                decimal? produccionKgActual = null;
                decimal? ventasTotalesActual = null;
                decimal? ticketPromedioActual = null;

                decimal? variacionCantidadProducida = null;
                decimal? variacionVentasTotales = null;
                decimal? variacionProduccionKg = null;
                decimal? variacionTicketPromedio = null;

                if (!periodoLargo)
                {
                    // Solo calculamos indicadores y variaciones si el período es <= 7 meses
                    cantidadProducidaActual = produccionPeriodo
                        .Where(x => x.FechaProduccion >= desde && x.FechaProduccion <= hasta)
                        .Sum(x => x.CantidadProducida);

                    decimal cantidadProducidaAnterior = produccionPeriodo
                        .Where(x => x.FechaProduccion >= desdeAnterior && x.FechaProduccion < desde)
                        .Sum(x => x.CantidadProducida);

                    produccionKgActual = produccionPeriodo
                        .Where(x => x.FechaProduccion >= desde && x.FechaProduccion <= hasta)
                        .Sum(x => x.CantidadProducida * x.Contenido);

                    decimal produccionKgAnterior = produccionPeriodo
                        .Where(x => x.FechaProduccion >= desdeAnterior && x.FechaProduccion < desde)
                        .Sum(x => x.CantidadProducida * x.Contenido);

                    ventasTotalesActual = ventasPeriodo
                        .Where(x => x.Fecha >= desde && x.Fecha <= hasta)
                        .Sum(x => x.Cantidad);

                    decimal ventasTotalesAnterior = ventasPeriodo
                        .Where(x => x.Fecha >= desdeAnterior && x.Fecha < desde)
                        .Sum(x => x.Cantidad);

                    var ventasActualList = ventasPeriodo
                        .Where(x => x.Fecha >= desde && x.Fecha <= hasta)
                        .ToList();

                    var ventasAnteriorList = ventasPeriodo
                        .Where(x => x.Fecha >= desdeAnterior && x.Fecha < desde)
                        .ToList();

                    ticketPromedioActual = ventasActualList.Any()
                        ? ventasActualList.Sum(x => x.Cantidad * x.CostoUnitario) / ventasActualList.Count
                        : 0;

                    decimal ticketPromedioAnterior = ventasAnteriorList.Any()
                        ? ventasAnteriorList.Sum(x => x.Cantidad * x.CostoUnitario) / ventasAnteriorList.Count
                        : 0;

                    // ===================== VARIACIONES =====================
                    variacionCantidadProducida = cantidadProducidaAnterior == 0 ? 0 : (cantidadProducidaActual.Value - cantidadProducidaAnterior) * 100 / cantidadProducidaAnterior;
                    variacionVentasTotales = ventasTotalesAnterior == 0 ? 0 : (ventasTotalesActual.Value - ventasTotalesAnterior) * 100 / ventasTotalesAnterior;
                    variacionProduccionKg = produccionKgAnterior == 0 ? 0 : (produccionKgActual.Value - produccionKgAnterior) * 100 / produccionKgAnterior;
                    variacionTicketPromedio = ticketPromedioAnterior == 0 ? 0 : (ticketPromedioActual.Value - ticketPromedioAnterior) * 100 / ticketPromedioAnterior;
                }

                // ===================== STOCK =====================
                var produccionAgrupada = produccionPeriodo
                    .GroupBy(x => x.Tipo)
                    .ToDictionary(g => g.Key, g => g.Sum(x => x.CantidadProducida));

                var ventasAgrupadas = ventasPeriodo
                    .GroupBy(x => x.Tipo)
                    .ToDictionary(g => g.Key, g => g.Sum(x => x.Cantidad));

                var stockActualPorTipo = produccionAgrupada
                    .ToDictionary(g => g.Key, g => g.Value - (ventasAgrupadas.ContainsKey(g.Key) ? ventasAgrupadas[g.Key] : 0));

                // ===================== AGRUPACIONES POR TIPO =====================
                var produccionPorTipo = produccionPeriodo
                    .Where(x => x.FechaProduccion >= desde && x.FechaProduccion <= hasta)
                    .GroupBy(x => x.Tipo)
                    .ToDictionary(g => g.Key, g => g.Sum(x => x.CantidadProducida));

                var ventasPorTipo = ventasPeriodo
                    .Where(x => x.Fecha >= desde && x.Fecha <= hasta)
                    .GroupBy(x => x.Tipo)
                    .ToDictionary(g => g.Key, g => g.Sum(x => x.Cantidad));

                // ===================== RANGO DE FECHAS / MESES =====================
                var rangoFechas = Enumerable.Range(0, diasPeriodo)
                    .Select(i => desde.AddDays(i))
                    .ToList();

                var rangoMeses = Enumerable.Range(0, mesesPeriodo)
                    .Select(i => new DateTime(desde.Year, desde.Month, 1).AddMonths(i))
                    .ToList();

                Func<DateTime, string> keySelector = f =>
                    agruparPorMes ? f.ToString("MM/yyyy") : f.ToString("dd/MM");

                // ===================== PRODUCCIÓN / VENTAS DIARIAS =====================
                var prodEnvRaw = produccionPeriodo
                    .Where(x => x.FechaProduccion >= desde && x.FechaProduccion <= hasta)
                    .GroupBy(x => keySelector(x.FechaProduccion))
                    .ToDictionary(g => g.Key, g => g.Sum(x => x.CantidadProducida));

                var produccionDiariaEnvases = agruparPorMes
                    ? rangoMeses.ToDictionary(f => f.ToString("MM/yyyy"), f => prodEnvRaw.ContainsKey(f.ToString("MM/yyyy")) ? prodEnvRaw[f.ToString("MM/yyyy")] : 0)
                    : rangoFechas.ToDictionary(f => f.ToString("dd/MM"), f => prodEnvRaw.ContainsKey(f.ToString("dd/MM")) ? prodEnvRaw[f.ToString("dd/MM")] : 0);

                var prodKgRaw = produccionPeriodo
                    .Where(x => x.FechaProduccion >= desde && x.FechaProduccion <= hasta)
                    .GroupBy(x => keySelector(x.FechaProduccion))
                    .ToDictionary(g => g.Key, g => g.Sum(x => x.CantidadProducida * x.Contenido));

                var produccionDiariaKg = agruparPorMes
                    ? rangoMeses.ToDictionary(f => f.ToString("MM/yyyy"), f => prodKgRaw.ContainsKey(f.ToString("MM/yyyy")) ? prodKgRaw[f.ToString("MM/yyyy")] : 0)
                    : rangoFechas.ToDictionary(f => f.ToString("dd/MM"), f => prodKgRaw.ContainsKey(f.ToString("dd/MM")) ? prodKgRaw[f.ToString("dd/MM")] : 0);

                var ventasRaw = ventasPeriodo
                    .Where(x => x.Fecha >= desde && x.Fecha <= hasta)
                    .GroupBy(x => keySelector(x.Fecha))
                    .ToDictionary(g => g.Key, g => g.Sum(x => x.Cantidad));

                var ventasDiaria = agruparPorMes
                    ? rangoMeses.ToDictionary(f => f.ToString("MM/yyyy"), f => ventasRaw.ContainsKey(f.ToString("MM/yyyy")) ? ventasRaw[f.ToString("MM/yyyy")] : 0)
                    : rangoFechas.ToDictionary(f => f.ToString("dd/MM"), f => ventasRaw.ContainsKey(f.ToString("dd/MM")) ? ventasRaw[f.ToString("dd/MM")] : 0);

                var recaudacionRaw = ventasPeriodo
                    .Where(x => x.Fecha >= desde && x.Fecha <= hasta)
                    .GroupBy(x => keySelector(x.Fecha))
                    .ToDictionary(g => g.Key, g => g.Sum(x => x.Cantidad * x.CostoUnitario));

                var recaudacionDiaria = agruparPorMes
                    ? rangoMeses.ToDictionary(f => f.ToString("MM/yyyy"), f => recaudacionRaw.ContainsKey(f.ToString("MM/yyyy")) ? recaudacionRaw[f.ToString("MM/yyyy")] : 0)
                    : rangoFechas.ToDictionary(f => f.ToString("dd/MM"), f => recaudacionRaw.ContainsKey(f.ToString("dd/MM")) ? recaudacionRaw[f.ToString("dd/MM")] : 0);

                // ===================== RESULTADO FINAL =====================
                return new DashboardResumen
                {
                    CantidadProducida = cantidadProducidaActual,
                    VentasTotales = ventasTotalesActual,
                    ProduccionKg = produccionKgActual,
                    TicketPromedio = ticketPromedioActual,

                    VariacionCantidadProducida = variacionCantidadProducida,
                    VariacionVentasTotales = variacionVentasTotales,
                    VariacionProduccionKg = variacionProduccionKg,
                    VariacionTicketPromedio = variacionTicketPromedio,

                    ProduccionPorTipo = produccionPorTipo,
                    VentasPorTipo = ventasPorTipo,
                    StockPorTipo = stockActualPorTipo,

                    ProduccionDiariaEnvases = produccionDiariaEnvases,
                    ProduccionDiariaKg = produccionDiariaKg,
                    VentasDiaria = ventasDiaria,
                    RecaudacionDiaria = recaudacionDiaria
                };
            }
            catch (Exception ex)
            {
                throw new Exception("Error al obtener el dashboard: " + ex.Message, ex);
            }
        }

    }
}
