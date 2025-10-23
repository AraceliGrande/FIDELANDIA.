using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using FIDELANDIA.Models;


namespace FIDELANDIA.Data
{
    public class FidelandiaDbContext : DbContext
    {
        public DbSet<ProveedorModel> Proveedores { get; set; }
        public DbSet<TransaccionModel> Transacciones { get; set; }
        public DbSet<CategoriaProveedorModel> CategoriaProveedor { get; set; }
        public DbSet<TipoPastaModel> TiposPasta { get; set; }
        public DbSet<LoteProduccionModel> LoteProduccion { get; set; }
        public DbSet<StockActualModel> StockActual { get; set; }
        public DbSet<VentaModel> Venta { get; set; }
        public DbSet<DetalleVentaModel> DetalleVenta { get; set; }



        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer(
    "Server=DESKTOP-5NQHMNN;Database=FidelandiaDB;Trusted_Connection=True;TrustServerCertificate=True;");

        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)

        {

            modelBuilder.Entity<CategoriaProveedorModel>().ToTable("CategoriaProveedor");

            // Claves primarias
            modelBuilder.Entity<ProveedorModel>().HasKey(p => p.ProveedorID);
            modelBuilder.Entity<TransaccionModel>().HasKey(t => t.TransaccionID);
            modelBuilder.Entity<CategoriaProveedorModel>().HasKey(c => c.CategoriaProveedorID);
            modelBuilder.Entity<TipoPastaModel>().HasKey(tp => tp.IdTipoPasta);
            modelBuilder.Entity<LoteProduccionModel>().HasKey(lp => lp.IdLote);
            modelBuilder.Entity<StockActualModel>().HasKey(s => s.IdStock);
            modelBuilder.Entity<VentaModel>().HasKey(v => v.IdVenta);
            modelBuilder.Entity<DetalleVentaModel>().HasKey(d => d.IdDetalle);

            // Relación Transaccion → Proveedor
            modelBuilder.Entity<TransaccionModel>()
                .HasOne(t => t.Proveedor)
                .WithMany(p => p.Transacciones)
                .HasForeignKey(t => t.ProveedorID);

            // Relación Proveedor → Categoria
            modelBuilder.Entity<ProveedorModel>()
                .HasOne(p => p.Categoria)
                .WithMany(c => c.Proveedores)
                .HasForeignKey(p => p.CategoriaProveedorID)
                .OnDelete(DeleteBehavior.Restrict);

            // Relación Produccion → TipoPasta
            modelBuilder.Entity<LoteProduccionModel>()
                 .HasOne(lp => lp.TipoPasta)
                 .WithMany(tp => tp.Lotes)
                 .HasForeignKey(lp => lp.IdTipoPasta)
                 .OnDelete(DeleteBehavior.Restrict);

            // TipoPasta → Stock (1:1)
            modelBuilder.Entity<TipoPastaModel>()
                .HasOne(tp => tp.Stock)
                .WithOne(s => s.TipoPasta)
                .HasForeignKey<StockActualModel>(s => s.IdTipoPasta)
                .OnDelete(DeleteBehavior.Restrict);

            // Relación StockActual -> LoteProduccion
            modelBuilder.Entity<StockActualModel>()
                .HasMany(s => s.LotesDisponibles)
                .WithOne() 
                .HasForeignKey(l => l.IdTipoPasta) 
                .OnDelete(DeleteBehavior.Restrict);

            // TipoPasta → Lotes (1:N) (opcional)
            modelBuilder.Entity<TipoPastaModel>()
                .HasMany(tp => tp.Lotes)
                .WithOne(l => l.TipoPasta)
                .HasForeignKey(l => l.IdTipoPasta)
                .OnDelete(DeleteBehavior.Restrict);

            // Relación DetalleVenta → Venta
            modelBuilder.Entity<DetalleVentaModel>()
                .HasOne(d => d.Venta)
                .WithMany(v => v.DetalleVenta)
                .HasForeignKey(d => d.IdVenta)
                .OnDelete(DeleteBehavior.Cascade);

            // Relación DetalleVenta → LoteProduccion
            modelBuilder.Entity<DetalleVentaModel>()
                .HasOne(d => d.Lote)
                .WithMany() // sin navegación inversa
                .HasForeignKey(d => d.IdLote)
                .OnDelete(DeleteBehavior.Restrict);

        }

    }
}
