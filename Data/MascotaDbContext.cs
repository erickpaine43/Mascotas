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
        public DbSet<Review> Reviews { get; set; }
        public DbSet<ReviewReminder> ReviewReminders { get; set; }
        public DbSet<AlertaPrecio> AlertaPrecios { get; set; }
        public DbSet<BusquedaGuardada> BusquedaGuardadas { get; set; }
        public DbSet<FiltroGuardado> FiltroGuardados { get; set; }
        public DbSet<ResultadoCambio> ResultadoCambios { get; set; }

        public DbSet<Notificacion> Notificaciones { get; set; }
        public DbSet<OrderTracking> OrderTrackings { get; set; }
        public DbSet<PerfilUsuario> PerfilesUsuarios { get; set; }
        public DbSet<Direccion> Direcciones { get; set; }
        public DbSet<MascotaCliente> MascotasClientes { get; set; }
        public DbSet<PreferenciasUsuario> PreferenciasUsuarios { get; set; }


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

            modelBuilder.Entity<Review>()
                .HasOne(r => r.Producto)
                .WithMany()
                .HasForeignKey(r => r.ProductoId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Review>()
                .HasOne(r => r.Cliente)
                .WithMany()
                .HasForeignKey(r => r.ClienteId)
                .OnDelete(DeleteBehavior.Cascade);


            modelBuilder.Entity<ReviewReminder>(entity =>
            {
                entity.HasOne(r => r.Orden)
                      .WithMany()
                      .HasForeignKey(r => r.OrdenId)
                      .OnDelete(DeleteBehavior.NoAction); // ← EVITA CICLOS

                entity.HasOne(r => r.Cliente)
                      .WithMany()
                      .HasForeignKey(r => r.ClienteId)
                      .OnDelete(DeleteBehavior.NoAction); // ← EVITA CICLOS

                entity.HasOne(r => r.Producto)
                      .WithMany()
                      .HasForeignKey(r => r.ProductoId)
                      .OnDelete(DeleteBehavior.NoAction); // ← EVITA CICLOS

                entity.HasOne(r => r.Animal)
                      .WithMany()
                      .HasForeignKey(r => r.AnimalId)
                      .OnDelete(DeleteBehavior.NoAction); // ← EVITA CICLOS
            });
            // Configurar AlertaPrecio
            modelBuilder.Entity<AlertaPrecio>()
                .Property(a => a.PrecioObjetivo)
                .HasPrecision(18, 2); // 18 dígitos totales, 2 decimales

            // Configurar FiltroGuardado
            modelBuilder.Entity<FiltroGuardado>()
                .Property(f => f.PorcentajeBajaMinima)
                .HasPrecision(5, 2); // 5 dígitos totales, 2 decimales (ej: 100.00%)

            // Configurar ResultadoCambio
            modelBuilder.Entity<ResultadoCambio>()
                .Property(r => r.PrecioAnterior)
                .HasPrecision(18, 2);

            modelBuilder.Entity<ResultadoCambio>()
                .Property(r => r.PrecioNuevo)
                .HasPrecision(18, 2);

            // ✅ CONFIGURACIONES ADICIONALES RECOMENDADAS

            // Configurar Producto (si tienes propiedades decimales)
            modelBuilder.Entity<Producto>()
                .Property(p => p.Precio)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Producto>()
                .Property(p => p.PrecioOriginal)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Producto>()
                .Property(p => p.Descuento)
                .HasPrecision(5, 2);

            modelBuilder.Entity<Producto>()
                .Property(p => p.Rating)
                .HasPrecision(3, 2); // Para ratings como 4.75

            // Configurar relaciones opcionales si es necesario
            modelBuilder.Entity<Notificacion>()
                .HasOne(n => n.Producto)
                .WithMany()
                .HasForeignKey(n => n.ProductoId)
                .OnDelete(DeleteBehavior.SetNull); // Opcional: si eliminas producto, notificación queda

            modelBuilder.Entity<Notificacion>()
                .HasOne(n => n.FiltroGuardado)
                .WithMany()
                .HasForeignKey(n => n.FiltroGuardadoId)
                .OnDelete(DeleteBehavior.SetNull);

            // Configurar índices para mejor performance
            modelBuilder.Entity<FiltroGuardado>()
                .HasIndex(f => f.UsuarioId);

            modelBuilder.Entity<FiltroGuardado>()
                .HasIndex(f => new { f.UsuarioId, f.EsFavorito });

            modelBuilder.Entity<Notificacion>()
                .HasIndex(n => n.UsuarioId);

            modelBuilder.Entity<Notificacion>()
                .HasIndex(n => new { n.UsuarioId, n.Leida });

            modelBuilder.Entity<AlertaPrecio>()
                .HasIndex(a => new { a.UsuarioId, a.Activa });

            modelBuilder.Entity<ResultadoCambio>()
                .HasIndex(r => r.FiltroGuardadoId);

            modelBuilder.Entity<ResultadoCambio>()
                .HasIndex(r => r.FechaDetectado);

            modelBuilder.Entity<OrderTracking>(entity =>
            {
                entity.HasKey(ot => ot.Id);

                entity.HasOne(ot => ot.Orden)
                      .WithMany(o => o.TrackingHistory)
                      .HasForeignKey(ot => ot.OrdenId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<Orden>(entity =>
            {
                entity.HasKey(o => o.Id);

                entity.HasMany(o => o.TrackingHistory)
                      .WithOne(ot => ot.Orden)
                      .HasForeignKey(ot => ot.OrdenId)
                      .OnDelete(DeleteBehavior.Cascade);

                
                entity.HasOne(o => o.Cliente)
                      .WithMany()
                      .HasForeignKey(o => o.ClienteId);
            });
            
            modelBuilder.Entity<PerfilUsuario>(entity =>
            {
                entity.HasKey(p => p.Id);

                
                entity.HasOne(p => p.Usuario)
                      .WithOne()
                      .HasForeignKey<PerfilUsuario>(p => p.UsuarioId)
                      .OnDelete(DeleteBehavior.Cascade); 

                entity.HasIndex(p => p.UsuarioId).IsUnique(); 
            });

            modelBuilder.Entity<Direccion>(entity =>
            {
                entity.HasKey(d => d.Id);

                
                entity.HasOne(d => d.PerfilUsuario)
                      .WithMany(p => p.Direcciones)
                      .HasForeignKey(d => d.PerfilUsuarioId)
                      .OnDelete(DeleteBehavior.Cascade); 

                entity.HasIndex(d => d.PerfilUsuarioId);
                entity.HasIndex(d => new { d.PerfilUsuarioId, d.EsPrincipal });
            });

            modelBuilder.Entity<MascotaCliente>(entity =>
            {
                entity.HasKey(m => m.Id);

                
                entity.HasOne(m => m.PerfilUsuario)
                      .WithMany(p => p.Mascotas)
                      .HasForeignKey(m => m.PerfilUsuarioId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.Property(m => m.Peso)
                      .HasPrecision(8, 3);

                entity.HasIndex(m => m.PerfilUsuarioId);
                entity.HasIndex(m => m.Especie);
            });

            modelBuilder.Entity<PreferenciasUsuario>(entity =>
            {
                entity.HasKey(p => p.Id);

                
                entity.HasOne(p => p.PerfilUsuario)
                      .WithOne(perfil => perfil.Preferencias)
                      .HasForeignKey<PreferenciasUsuario>(p => p.PerfilUsuarioId)
                      .OnDelete(DeleteBehavior.Cascade); 

                entity.HasIndex(p => p.PerfilUsuarioId).IsUnique();
            });
        }

    }
        
    }
