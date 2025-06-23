using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using TunePhere.Models;
using Microsoft.AspNetCore.Identity;
using System.Net.Http;
using Newtonsoft.Json;
using System.Text;
using TunePhere.Helpers;

namespace TunePhere.Controllers
{
    public class PaymentController : Controller
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly IConfiguration _configuration;
        private readonly AppDbContext _context;

        public PaymentController(
            UserManager<AppUser> userManager,
            IConfiguration configuration,
            AppDbContext context)
        {
            _userManager = userManager;
            _configuration = configuration;
            _context = context;
        }

        [HttpGet]
        public IActionResult ArtistRegistrationPayment()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ProcessVnPay()
        {
            var amount = 480000; // 480,000 VND
            var vnpayUrl = CreateVnPayUrl(amount);
            return Redirect(vnpayUrl);
        }

        [HttpPost]
        public async Task<IActionResult> ProcessMono()
        {
            var amount = 480000;
            var monoUrl = await CreateMonoPaymentUrl(amount);
            return Redirect(monoUrl);
        }

        private string CreateVnPayUrl(int amount)
        {
            var vnpayTmnCode = _configuration["VnPay:TmnCode"];
            var vnpayHashSecret = _configuration["VnPay:HashSecret"];
            var vnpayUrl = _configuration["VnPay:BaseUrl"];

            var returnUrl = Url.Action("PaymentCallback", "Payment", null, Request.Scheme);
            
            var orderInfo = $"Thanh toan dang ky tai khoan nghe si - {DateTime.Now:yyyyMMddHHmmss}";
            var ticketId = DateTime.Now.Ticks.ToString();
            
            var vnpay = new VnPayLibrary();
            
            vnpay.AddRequestData("vnp_Version", VnPayLibrary.VERSION);
            vnpay.AddRequestData("vnp_Command", "pay");
            vnpay.AddRequestData("vnp_TmnCode", vnpayTmnCode);
            vnpay.AddRequestData("vnp_Amount", (amount * 100).ToString());
            vnpay.AddRequestData("vnp_CreateDate", DateTime.Now.ToString("yyyyMMddHHmmss"));
            vnpay.AddRequestData("vnp_CurrCode", "VND");
            vnpay.AddRequestData("vnp_IpAddr", HttpContext.Connection.RemoteIpAddress?.ToString());
            vnpay.AddRequestData("vnp_Locale", "vn");
            vnpay.AddRequestData("vnp_OrderInfo", orderInfo);
            vnpay.AddRequestData("vnp_OrderType", "other");
            vnpay.AddRequestData("vnp_ReturnUrl", returnUrl);
            vnpay.AddRequestData("vnp_TxnRef", ticketId);

            var paymentUrl = vnpay.CreateRequestUrl(vnpayUrl, vnpayHashSecret);
            
            return paymentUrl;
        }

        private async Task<string> CreateMonoPaymentUrl(int amount)
        {
            var monoApiKey = _configuration["Mono:ApiKey"];
            var monoBaseUrl = _configuration["Mono:BaseUrl"];

            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("Authorization", $"Bearer {monoApiKey}");

                var payload = new
                {
                    amount = amount,
                    description = "Thanh toan dang ky tai khoan nghe si",
                    redirectUrl = Url.Action("PaymentCallback", "Payment", null, Request.Scheme),
                    orderInfo = new
                    {
                        orderId = DateTime.Now.Ticks.ToString(),
                        orderType = "ARTIST_REGISTRATION"
                    }
                };

                var content = new StringContent(
                    JsonConvert.SerializeObject(payload),
                    Encoding.UTF8,
                    "application/json"
                );

                var response = await client.PostAsync($"{monoBaseUrl}/v1/payment/init", content);
                var result = await response.Content.ReadFromJsonAsync<MonoPaymentResponse>();
                
                return result.PaymentUrl;
            }
        }

        [HttpGet]
        public async Task<IActionResult> PaymentCallback([FromQuery] Dictionary<string, string> parameters)
        {
            var userId = _userManager.GetUserId(User);
            var user = await _userManager.FindByIdAsync(userId);

            if (parameters.ContainsKey("vnp_ResponseCode") && parameters["vnp_ResponseCode"] == "00" ||
                parameters.ContainsKey("status") && parameters["status"] == "success")
            {
                // Cập nhật trạng thái người dùng thành nghệ sĩ
                user.IsArtist = true;
                await _userManager.UpdateAsync(user);

                // Thêm vào bảng Artists
                var artist = new Artists
                {
                    userId = userId,
                    ArtistName = user.UserName,
                  
                };
                _context.Artists.Add(artist);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Thanh toán thành công! Bạn đã trở thành nghệ sĩ.";
                return RedirectToAction("ArtistDashboard", "Artists");
            }

            TempData["ErrorMessage"] = "Thanh toán thất bại! Vui lòng thử lại.";
            return RedirectToAction("ArtistRegistrationPayment");
        }
    }

    public class MonoPaymentResponse
    {
        public string PaymentUrl { get; set; }
    }
}
