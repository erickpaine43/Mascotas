using Mascotas.Models;
using Mascotas.Services;
using Mascotas.Data;
using Stripe;
using Stripe.Checkout;
using Microsoft.Extensions.Logging;

namespace Mascotas.Services
{
    public class StripeService : IStripeService
    {
        private readonly MascotaDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly ILogger<StripeService> _logger;

        public StripeService(MascotaDbContext context, IConfiguration configuration, ILogger<StripeService> logger)
        {
            _context = context;
            _configuration = configuration;
            _logger = logger;
            StripeConfiguration.ApiKey = _configuration["Stripe:SecretKey"];
        }

        public async Task<string> CreateCheckoutSessionAsync(Orden orden, string successUrl, string cancelUrl)
        {
            try
            {
                // 1. Verificar que la orden tiene cliente
                if (orden.Cliente == null)
                {
                    throw new Exception("Cliente no encontrado en la orden");
                }

                // 2. Verificar email del cliente
                if (string.IsNullOrEmpty(orden.Cliente.Email))
                {
                    throw new Exception("Cliente no tiene email configurado");
                }

                // 3. Cargar items si es necesario
                if (!orden.Items.Any())
                {
                    throw new Exception("La orden no contiene items");
                }

                var lineItems = new List<SessionLineItemOptions>();

                foreach (var item in orden.Items)
                {
                    // Cargar datos del animal si existe
                    if (item.AnimalId.HasValue && item.Animal == null)
                    {
                        await _context.Entry(item)
                            .Reference(i => i.Animal)
                            .LoadAsync();
                    }

                    // Cargar datos del producto si existe
                    if (item.ProductoId.HasValue && item.Producto == null)
                    {
                        await _context.Entry(item)
                            .Reference(i => i.Producto)
                            .LoadAsync();
                    }

                    string productName = item.Animal?.Nombre ?? item.Producto?.Nombre ?? "Producto";

                    // ✅ SOLUCIÓN: Si la descripción está vacía, usar un valor por defecto
                    string description = item.Animal?.Descripcion ?? item.Producto?.DescripcionCorta ?? "Producto de calidad";

                    // ✅ Si aún está vacía, usar el nombre como descripción
                    if (string.IsNullOrEmpty(description))
                    {
                        description = productName;
                    }

                    var productData = new SessionLineItemPriceDataProductDataOptions
                    {
                        Name = productName,
                        Description = description, // ✅ Ahora nunca estará vacío
                        Metadata = new Dictionary<string, string>
                        {
                            { "tipo", item.AnimalId.HasValue ? "animal" : "producto" },
                            { "id", (item.AnimalId ?? item.ProductoId)?.ToString() ?? "" }
                        }
                    };

                    lineItems.Add(new SessionLineItemOptions
                    {
                        PriceData = new SessionLineItemPriceDataOptions
                        {
                            Currency = "mxn",
                            ProductData = productData,
                            UnitAmount = (long)(item.PrecioUnitario * 100),
                        },
                        Quantity = item.Cantidad,
                    });
                }

                // ✅ CONFIGURACIÓN SIMPLIFICADA - Solo CustomerEmail
                var options = new SessionCreateOptions
                {
                    CustomerEmail = orden.Cliente.Email,
                    PaymentMethodTypes = new List<string> { "card" },
                    LineItems = lineItems,
                    Mode = "payment",
                    SuccessUrl = successUrl,
                    CancelUrl = cancelUrl,
                    Metadata = new Dictionary<string, string>
                    {
                        { "orden_id", orden.Id.ToString() },
                        { "numero_orden", orden.NumeroOrden }
                    }
                };

                var service = new SessionService();
                var session = await service.CreateAsync(options);

                // Guardar session ID en la orden
                orden.StripeSessionId = session.Id;
                await _context.SaveChangesAsync();

                return session.Url;
            }
            catch (StripeException ex)
            {
                throw new Exception($"Error procesando pago con Stripe: {ex.Message}");
            }
            catch (Exception ex)
            {
                throw new Exception($"Error creando sesión de pago: {ex.Message}");
            }
        }

        // ✅ AGREGA ESTOS MÉTODOS QUE FALTAN
        public async Task<Customer> CreateCustomerAsync(Cliente cliente)
        {
            var options = new CustomerCreateOptions
            {
                Name = cliente.Nombre,
                Email = cliente.Email,
                Phone = cliente.Telefono,
                Metadata = new Dictionary<string, string>
                {
                    { "cliente_id", cliente.Id.ToString() }
                }
            };

            var service = new CustomerService();
            return await service.CreateAsync(options);
        }

        public async Task<PaymentIntent> CreatePaymentIntentAsync(Orden orden)
        {
            var options = new PaymentIntentCreateOptions
            {
                Amount = (long)(orden.Total * 100),
                Currency = "mxn",
                Metadata = new Dictionary<string, string>
                {
                    { "orden_id", orden.Id.ToString() },
                    { "numero_orden", orden.NumeroOrden }
                },
                AutomaticPaymentMethods = new PaymentIntentAutomaticPaymentMethodsOptions
                {
                    Enabled = true,
                },
            };

            var service = new PaymentIntentService();
            return await service.CreateAsync(options);
        }

        public async Task<bool> VerifyPaymentAsync(string paymentIntentId)
        {
            var service = new PaymentIntentService();
            var paymentIntent = await service.GetAsync(paymentIntentId);
            return paymentIntent.Status == "succeeded";
        }

        public async Task<bool> IsCheckoutSessionCompletedAsync(string sessionId)
        {
            var service = new SessionService();
            var session = await service.GetAsync(sessionId);
            return session.PaymentStatus == "paid";
        }
    }
}