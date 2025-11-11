using FIDELANDIA.Data;
using FIDELANDIA.Models;
using FIDELANDIA.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;

namespace FIDELANDIA.Views.Proveedores
{
    public partial class ResumenProveedores : UserControl
    {
        private readonly ProveedorService _service;

        public ResumenProveedores()
        {
            InitializeComponent();
            var dbContext = new FidelandiaDbContext();
            _service = new ProveedorService(dbContext);

            CargarResumen();
        }

        private void CargarResumen()
        {
            var proveedores = _service.ObtenerTodos();

            var resumen = proveedores.Select(p => new
            {
                p.Nombre,
                p.Cuit,
                p.SaldoActual,
                p.LimiteCredito,
                TotalTransacciones = p.Transacciones.Count,
                UltimaTransaccionInfo = p.Transacciones
                    .OrderByDescending(t => t.Fecha)
                    .Select(t => $"{t.Fecha:dd/MM/yyyy} {t.TipoTransaccion} {t.Monto:C2}")
                    .FirstOrDefault() ?? "Sin movimientos",
                p.IsActivo
            }).ToList();

            ResumenDataGrid.ItemsSource = resumen;
        }
    }
}
