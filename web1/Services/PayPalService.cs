using PayPal.Api;
using Microsoft.Extensions.Configuration;

namespace web1.Services
{
    public class PayPalService
    {
        private readonly IConfiguration _configuration;
        private readonly string _clientId;
        private readonly string _clientSecret;

        public PayPalService(IConfiguration configuration)
        {
            _configuration = configuration;
            _clientId = _configuration["PayPal:ClientId"];
            _clientSecret = _configuration["PayPal:Secret"];
        }

        private APIContext GetAPIContext()
        {
            var config = new Dictionary<string, string>
            {
                {"mode", _configuration["PayPal:Mode"]}
            };

            var accessToken = new OAuthTokenCredential(_clientId, _clientSecret, config)
                .GetAccessToken();

            var apiContext = new APIContext(accessToken)
            {
                Config = config
            };

            return apiContext;
        }

        public Payment CreatePayment(decimal amount, string returnUrl, string cancelUrl)
        {
            var apiContext = GetAPIContext();

            var payment = new Payment
            {
                intent = "sale",
                payer = new Payer { payment_method = "paypal" },
                transactions = new List<Transaction>
                {
                    new Transaction
                    {
                        amount = new Amount
                        {
                            currency = "USD",
                            total = amount.ToString("0.00")
                        },
                        description = "Payment for order"
                    }
                },
                redirect_urls = new RedirectUrls
                {
                    return_url = returnUrl,
                    cancel_url = cancelUrl
                }
            };

            return payment.Create(apiContext);
        }

        public Payment ExecutePayment(string paymentId, string payerId)
        {
            var apiContext = GetAPIContext();
            var paymentExecution = new PaymentExecution { payer_id = payerId };
            var payment = new Payment { id = paymentId };
            return payment.Execute(apiContext, paymentExecution);
        }
    }
} 