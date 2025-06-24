using Stripe;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace TunePhere.Helpers
{
    public class StripeHelper
    {
        public static void Initialize(string apiKey)
        {
            StripeConfiguration.ApiKey = apiKey;
        }

        public static async Task<string> CreatePaymentIntent(decimal amount, string currency = "vnd")
        {
            var options = new PaymentIntentCreateOptions
            {
                Amount = Convert.ToInt64(amount * 100), // Stripe uses cents
                Currency = currency,
                PaymentMethodTypes = new List<string> { "card" }, // Fixed the type
                Metadata = new Dictionary<string, string>
        {
            { "order_type", "ARTIST_REGISTRATION" },
            { "created_at", DateTime.Now.ToString("yyyyMMddHHmmss") }
        }
            };

            var service = new PaymentIntentService();
            var paymentIntent = await service.CreateAsync(options);
            return paymentIntent.ClientSecret;
        }

        public static async Task<PaymentIntent> ConfirmPayment(string paymentIntentId, string paymentMethodId)
        {
            var options = new PaymentIntentConfirmOptions
            {
                PaymentMethod = paymentMethodId
            };

            var service = new PaymentIntentService();
            return await service.ConfirmAsync(paymentIntentId, options);
        }

        public static async Task<PaymentIntent> RetrievePaymentIntent(string paymentIntentId)
        {
            var service = new PaymentIntentService();
            return await service.GetAsync(paymentIntentId);
        }
    }
}
