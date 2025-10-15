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

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer(
    "Server=DESKTOP-5NQHMNN;Database=FidelandiaDB;Trusted_Connection=True;TrustServerCertificate=True;");

        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Definimos explícitamente las claves primarias
            modelBuilder.Entity<ProveedorModel>().HasKey(p => p.ProveedorID);
            modelBuilder.Entity<TransaccionModel>().HasKey(t => t.TransaccionID);

            // Relación 1 a muchos
            modelBuilder.Entity<TransaccionModel>()
                        .HasOne(t => t.Proveedor)
                        .WithMany(p => p.Transacciones)
                        .HasForeignKey(t => t.ProveedorID);
        }
    }
}
