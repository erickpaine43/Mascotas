using Mascotas.Models;
using Stripe;

namespace Mascotas.Services
{
    public interface IStripeService
    {
        Task<Customer> CreateCustomerAsync(Cliente cliente);
        Task<PaymentIntent> CreatePaymentIntentAsync(Orden orden);
        Task<string> CreateCheckoutSessionAsync(Orden orden, string successUrl, string cancelUrl);
        Task<bool> VerifyPaymentAsync(string paymentIntentId);
        Task<bool> IsCheckoutSessionCompletedAsync(string sessionId);
    }
}
