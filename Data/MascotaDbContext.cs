using Mascotas.Models;
using Microsoft.EntityFrameworkCore;

namespace Mascotas.Data
{
    public class MascotaDbContext : DbContext
    {
        public MascotaDbContext(DbContextOptions<MascotaDbContext> options) : base(options)
        {
        }
        public DbSet<Usuario> Usuarios { get; set; }
        public DbSet<Animal> Animales { get; set; }
        public DbSet<Producto> Productos { get; set; }
        public DbSet<Cliente> Clientes { get; set; }
        public DbSet<Carrito> Carritos { get; set; }
        public DbSet<CarritoItem> CarritoItems { get; set; }
        public DbSet<Orden> Ordenes { get; set; }
        public DbSet<OrdenItem> OrdenItems { get; set; }
        public DbSet<Categoria> Categorias { get; set; }
        public DbSet<PasswordResetToken> PasswordResetTokens { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<Usuario>(entity =>
            {
                entity.HasIndex(u => u.Email).IsUnique();
                entity.Property(u => u.Rol).HasConversion<string>();
            });
            modelBuilder.Entity<Animal>(entity =>
            {
                entity.HasIndex(m => m.Nombre);
                entity.HasIndex(m => m.Especie);
                entity.HasIndex(m => m.Disponible);
                entity.Property(m => m.Precio).HasPrecision(18, 2);
            });

            modelBuilder.Entity<Producto>(entity =>
            {
                entity.HasIndex(p => p.Nombre);
                entity.HasIndex(p => p.Activo);
                entity.Property(p => p.Precio).HasPrecision(18, 2);
                entity.Property(p => p.Descuento).HasPrecision(5, 2);

                entity.HasOne(p => p.Categoria)
             .WithMany(c => c.Productos)
             .HasForeignKey(p => p.CategoriaId)
             .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<Cliente>(entity =>
            {
                entity.HasIndex(c => c.Email).IsUnique();
                entity.HasIndex(c => c.Telefono);
                entity.HasIndex(c => c.StripeCustomerId);
            });

            modelBuilder.Entity<Carrito>(entity =>
            {
                entity.HasOne(c => c.Cliente)
                      .WithMany()
                      .HasForeignKey(c => c.ClienteId);
            });

            modelBuilder.Entity<CarritoItem>(entity =>
            {
                entity.Property(ci => ci.PrecioUnitario).HasPrecision(18, 2);
                entity.HasOne(ci => ci.Carrito)
                      .WithMany(c => c.Items)
                      .HasForeignKey(ci => ci.CarritoId);
                entity.HasOne(ci => ci.Mascota)
                      .WithMany()
                      .HasForeignKey(ci => ci.MascotaId);
                entity.HasOne(ci => ci.Producto)
                      .WithMany()
                      .HasForeignKey(ci => ci.ProductoId);
            });

            modelBuilder.Entity<Orden>(entity =>
            {
                entity.HasIndex(o => o.NumeroOrden).IsUnique();
                entity.HasIndex(o => o.StripePaymentIntentId);
                entity.HasIndex(o => o.StripeSessionId);
                entity.Property(o => o.Subtotal).HasPrecision(18, 2);
                entity.Property(o => o.Impuesto).HasPrecision(18, 2);
                entity.Property(o => o.Descuento).HasPrecision(18, 2);
                entity.Property(o => o.Total).HasPrecision(18, 2);
                entity.Property(o => o.Estado).HasConversion<string>();
                entity.Property(o => o.MetodoPago).HasConversion<string>();

                entity.HasOne(o => o.Cliente)
                      .WithMany()
                      .HasForeignKey(o => o.ClienteId);
            });

            modelBuilder.Entity<OrdenItem>(entity =>
            {
                entity.Property(oi => oi.PrecioUnitario).HasPrecision(18, 2);
                entity.Property(oi => oi.Subtotal).HasPrecision(18, 2);

                entity.HasOne(oi => oi.Orden)
                      .WithMany(o => o.Items)
                      .HasForeignKey(oi => oi.OrdenId);
                entity.HasOne(oi => oi.Animal)
                      .WithMany()
                      .HasForeignKey(oi => oi.AnimalId);
                entity.HasOne(oi => oi.Producto)
                      .WithMany()
                      .HasForeignKey(oi => oi.ProductoId);
            });
            modelBuilder.Entity<Categoria>(entity =>
            {
                entity.HasIndex(c => c.Nombre).IsUnique();
                entity.HasIndex(c => c.Orden);
            });

            modelBuilder.Entity<Producto>(entity =>
            {
                entity.Property(p => p.PrecioOriginal)
                .HasPrecision(18, 6)
                .HasColumnType("decimal(18,6)");

                entity.Property(p => p.Rating)
                .HasPrecision(3, 2)
                .HasColumnType("decimal(3, 2)");

            });

            
        }
        
    }
}
