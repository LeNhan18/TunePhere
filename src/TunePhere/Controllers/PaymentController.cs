using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using TunePhere.Models;
using Microsoft.AspNetCore.Identity;
using Stripe;
using TunePhere.Helpers;
using Newtonsoft.Json;
using Microsoft.Extensions.Options;

namespace TunePhere.Controllers
{
    public class PaymentController : Controller
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly IConfiguration _configuration;
        private readonly AppDbContext _context;

        private readonly IOptions<StripeSettings> _stripeSettings;

        public PaymentController(
            UserManager<AppUser> userManager,
            IConfiguration configuration,
            AppDbContext context,
            IOptions<StripeSettings> stripeSettings)
        {
            _userManager = userManager;
            _configuration = configuration;
            _context = context;
            _stripeSettings = stripeSettings;
        }

        [HttpGet]
        public IActionResult ArtistRegistrationPayment()
        {
            ViewBag.StripePublicKey = _configuration["Stripe:PublicKey"];
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> CreatePaymentIntent(decimal amount)
        {
            StripeHelper.Initialize(_stripeSettings.Value.SecretKey);
            try
            {
                var clientSecret = await StripeHelper.CreatePaymentIntent(amount);
                return Json(new { client_secret = clientSecret });
            }
            catch (Exception ex)
            {
                return Json(new { error = new { message = ex.Message } });
            }
        }

        [HttpPost]
        public async Task<IActionResult> ProcessVnPay()
        {
            var amount = 480000; // 480,000 VND
            var vnpayUrl = CreateVnPayUrl(amount);
            return Redirect(vnpayUrl);
        }

   
        private string CreateVnPayUrl(int amount)
        {
            var vnpayTmnCode = "your_vnpay_tmncode";
            var vnpayHashSecret = "your_vnpay_hashsecret";
            var vnpayUrl = "your_vnpay_url";

            // Sử dụng action PaymentCallback làm returnUrl
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
            vnpay.AddRequestData("vnp_IpAddr", HttpContext.Connection.RemoteIpAddress?.ToString() ?? "127.0.0.1");
            vnpay.AddRequestData("vnp_Locale", "vn");
            vnpay.AddRequestData("vnp_OrderInfo", orderInfo);
            vnpay.AddRequestData("vnp_OrderType", "other");
            vnpay.AddRequestData("vnp_ReturnUrl", returnUrl);
            vnpay.AddRequestData("vnp_TxnRef", ticketId);

            return vnpay.CreateRequestUrl(vnpayUrl, vnpayHashSecret);
        }

        
        [HttpGet]
        public async Task<IActionResult> PaymentCallback([FromQuery] Dictionary<string, string> parameters)
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId))
            {
                TempData["ErrorMessage"] = "Không tìm thấy thông tin người dùng!";
                return RedirectToAction("ArtistRegistrationPayment");
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                TempData["ErrorMessage"] = "Người dùng không tồn tại!";
                return RedirectToAction("ArtistRegistrationPayment");
            }

            // Kiểm tra thanh toán Stripe
            if (parameters.ContainsKey("status") && parameters["status"] == "success")
            {
                // Cập nhật trạng thái người dùng thành nghệ sĩ
                user.IsArtist = true;
                await _userManager.UpdateAsync(user);

                // Thêm vào bảng Artists
                var artist = new Artists
                {
                    userId = userId,
                    ArtistName = user.UserName
                };
                _context.Artists.Add(artist);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Thanh toán thành công! Bạn đã trở thành nghệ sĩ.";
                return RedirectToAction("Index", "Home");
            }

            // Kiểm tra thanh toán VNPay
            if (parameters.ContainsKey("vnp_ResponseCode") && parameters["vnp_ResponseCode"] == "00")
            {
                // Cập nhật trạng thái người dùng thành nghệ sĩ
                user.IsArtist = true;
                await _userManager.UpdateAsync(user);

                // Thêm vào bảng Artists
                var artist = new Artists
                {
                    userId = userId,
                    ArtistName = user.UserName
                };
                _context.Artists.Add(artist);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Thanh toán thành công! Bạn đã trở thành nghệ sĩ.";
                return RedirectToAction("Index", "Home");
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