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
        }

    }
}
