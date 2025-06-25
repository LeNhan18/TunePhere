using TunePhere.Models.VNPAY;
using TunePhere.Helpers;
namespace TunePhere.Services.VNPAY
{
    public class VnPayService :IVnPayService
    {
        private readonly IConfiguration _configuration;

        public VnPayService(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        public string CreatePaymentUrl(PaymentInformationModel model, HttpContext context)
        {
            var timeZoneId = _configuration["TimeZoneId"];
            TimeZoneInfo timeZoneById;
            try
            {
                timeZoneById = string.IsNullOrEmpty(timeZoneId)
                    ? TimeZoneInfo.Local
                    : TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
            }
            catch (TimeZoneNotFoundException)
            {
                timeZoneById = TimeZoneInfo.Local;
            }
            var timeNow = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, timeZoneById);
            var tick = DateTime.Now.Ticks.ToString();
            var pay = new VnPayLibrary();
            var urlCallBack = _configuration["Vnpay:PaymentBackReturnUrl"];
            if (string.IsNullOrWhiteSpace(urlCallBack))
            {
                // fallback to current host
                urlCallBack = $"{context.Request.Scheme}://{context.Request.Host}/Payment/PaymentCallback";
            }

            pay.AddRequestData("vnp_Version", _configuration["Vnpay:Version"]);
            pay.AddRequestData("vnp_Command", _configuration["Vnpay:Command"]);
            pay.AddRequestData("vnp_TmnCode", _configuration["Vnpay:TmnCode"]);
            // VNPay requires amount in VND * 100 and as integer
            var amountVnd = (long)Math.Round(model.Amount * 100);
            pay.AddRequestData("vnp_Amount", amountVnd.ToString());
            pay.AddRequestData("vnp_CreateDate", timeNow.ToString("yyyyMMddHHmmss"));
            pay.AddRequestData("vnp_CurrCode", _configuration["Vnpay:CurrCode"]);
            pay.AddRequestData("vnp_IpAddr", pay.GetIpAddress(context));
            var locale = _configuration["Vnpay:Locale"] ?? "vn";
            pay.AddRequestData("vnp_Locale", locale);
            pay.AddRequestData("vnp_OrderInfo", $"{model.Name} {model.OrderDescription} {model.Amount}");
            pay.AddRequestData("vnp_OrderType", model.OrderType);
            pay.AddRequestData("vnp_ReturnUrl", urlCallBack);
            pay.AddRequestData("vnp_TxnRef", tick);

            var paymentUrl =
                pay.CreateRequestUrl(_configuration["Vnpay:BaseUrl"], _configuration["Vnpay:HashSecret"]);

            return paymentUrl;
        }
        public PaymentResponseModel PaymentExecute(IQueryCollection collections)
        {
            var pay = new VnPayLibrary();
            var response = pay.GetFullResponseData(collections, _configuration["Vnpay:HashSecret"]);

            return response;
        }
    }
}
