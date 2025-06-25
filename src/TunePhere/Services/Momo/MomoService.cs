using System.Security.Cryptography;
using System.Net.Http;
using System.Net.Http.Json;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System.Text;
using TunePhere.Models.Momo;

namespace TunePhere.Services.Momo
{
    public class MomoService : IMomoService
    {
        private readonly IOptions<MomoOptionModel> _options;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<MomoService> _logger;

        public MomoService(
            IOptions<MomoOptionModel> options, 
            IHttpClientFactory httpClientFactory,
            ILogger<MomoService> logger)
        {
            _options = options;
            _httpClientFactory = httpClientFactory;
            _logger = logger;
        }
        public async Task<MomoCreatePaymentResponseModel> CreatePaymentAsync(OrderInfoModel model)
        {
            try
            {
                model.OrderId = "MOMO" + DateTime.UtcNow.Ticks.ToString();
                model.OrderInfo = "Thanh toán đăng ký tài khoản nghệ sĩ - " + model.FullName;
                
                var rawData =
                    $"accessKey={_options.Value.AccessKey}" +
                    $"&amount={model.Amount}" +
                    $"&extraData=" +
                    $"&ipnUrl={_options.Value.NotifyUrl}" +
                    $"&orderId={model.OrderId}" +
                    $"&orderInfo={model.OrderInfo}" +
                    $"&partnerCode={_options.Value.PartnerCode}" +
                    $"&redirectUrl=" +
                    $"&requestId={model.OrderId}" +
                    $"&requestType={_options.Value.RequestType}";

            var signature = ComputeHmacSha256(rawData, _options.Value.SecretKey);

            // build request payload
            var requestData = new
            {
                accessKey = _options.Value.AccessKey,
                partnerCode = _options.Value.PartnerCode,
                requestType = _options.Value.RequestType,
                ipnUrl = _options.Value.NotifyUrl,
                redirectUrl = "",
                returnUrl = _options.Value.ReturnUrl,
                orderId = model.OrderId,
                amount = model.Amount.ToString(),
                orderInfo = model.OrderInfo,
                requestId = model.OrderId,
                extraData = "",
                signature = signature
            };

            var httpClient = _httpClientFactory.CreateClient();
            _logger.LogInformation("MoMo request: {@requestData}", requestData);
            var httpResponse = await httpClient.PostAsJsonAsync(_options.Value.MomoApiUrl, requestData);
            var responseContent = await httpResponse.Content.ReadAsStringAsync();
            _logger.LogError("MoMo response: {response}", responseContent);
            httpResponse.EnsureSuccessStatusCode();
            var momoResponse = JsonConvert.DeserializeObject<MomoCreatePaymentResponseModel>(responseContent);
            return momoResponse;

        }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating MoMo payment");
                throw;
            }
        }
        public MomoExecuteResponseModel PaymentExecuteAsync(IFormCollection collection)
        {
            var amount = collection.First(s => s.Key == "amount").Value;
            var orderInfo = collection.First(s => s.Key == "orderInfo").Value;
            var orderId = collection.First(s => s.Key == "orderId").Value;

            return new MomoExecuteResponseModel()
            {
                Amount = amount,
                OrderId = orderId,
                OrderInfo = orderInfo

            };
        }
       private string ComputeHmacSha256(string message, string secretKey)
        {
            try
            {
                var keyBytes = Encoding.UTF8.GetBytes(secretKey);
                var messageBytes = Encoding.UTF8.GetBytes(message);

                using var hmac = new HMACSHA256(keyBytes);
                var hashBytes = hmac.ComputeHash(messageBytes);
                return BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error computing HMAC SHA256");
                throw;
            }
        }

        public async Task<MomoExecuteResponseModel> ProcessPaymentCallback(IQueryCollection collection)
        {
            try
            {
                var amount = collection.First(s => s.Key == "amount").ToString();
                var orderInfo = collection.First(s => s.Key == "orderInfo").ToString();
                var orderId = collection.First(s => s.Key == "orderId").ToString();
                var responseCode = collection.First(s => s.Key == "responseCode").ToString();
                var transId = collection.First(s => s.Key == "transId").ToString();

                // Verify the signature
                var rawHash = $"partnerCode={collection["partnerCode"]}" +
                    $"&accessKey={collection["accessKey"]}" +
                    $"&requestId={collection["requestId"]}" +
                    $"&amount={collection["amount"]}" +
                    $"&orderId={collection["orderId"]}" +
                    $"&orderInfo={collection["orderInfo"]}" +
                    $"&orderType={collection["orderType"]}" +
                    $"&transId={collection["transId"]}" +
                    $"&message={collection["message"]}" +
                    $"&localMessage={collection["localMessage"]}" +
                    $"&responseTime={collection["responseTime"]}" +
                    $"&errorCode={collection["errorCode"]}" +
                    $"&payType={collection["payType"]}" +
                    $"&extraData={collection["extraData"]}";

                var signature = ComputeHmacSha256(rawHash, _options.Value.SecretKey);

                if (signature != collection["signature"].ToString())
                {
                    _logger.LogWarning("Invalid MoMo signature");
                    throw new Exception("Invalid MoMo signature");
                }

                return new MomoExecuteResponseModel
                {
                    Amount = amount,
                    OrderId = orderId,
                    TransId = transId,
                    OrderInfo = orderInfo,
                    ResponseCode = responseCode
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing MoMo callback");
                throw;
            }
        }

    }
}
