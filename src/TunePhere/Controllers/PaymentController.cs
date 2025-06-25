using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;

using Microsoft.EntityFrameworkCore;
using System;


using System.Threading.Tasks;
using TunePhere.Models;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;
using TunePhere.Models.Momo;
using TunePhere.Models.VNPAY;
using TunePhere.Services.Momo;
using TunePhere.Services.VNPAY;

namespace TunePhere.Controllers
{
    [Authorize]
    public class PaymentController : Controller
    {
        private const double ArtistRegistrationFee = 480000d; // 480.000 VNĐ

        private readonly UserManager<AppUser> _userManager;
        private readonly AppDbContext _db;
        private readonly IMomoService _momoService;
        private readonly IVnPayService _vnPayService;
        private readonly ILogger<PaymentController> _logger;

        public PaymentController(
            UserManager<AppUser> userManager,
            AppDbContext db,
            IMomoService momoService,
            IVnPayService vnPayService,
            ILogger<PaymentController> logger)
        {
            _userManager = userManager;
            _db = db;
            _momoService = momoService;
            _vnPayService = vnPayService;
            _logger = logger;
        }

        [HttpGet]
        public IActionResult ArtistRegistrationPayment()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ProcessVnPay()
        {
            var amount = ArtistRegistrationFee;
            var user = await _userManager.GetUserAsync(User);
            var paymentInfo = new PaymentInformationModel
            {
                Amount = amount,
                Name = user.FullName ?? user.UserName,
                OrderDescription = "Đăng ký tài khoản nghệ sĩ",
                OrderType = "other"
            };
            var vnpayUrl = _vnPayService.CreatePaymentUrl(paymentInfo, HttpContext);
            return Redirect(vnpayUrl);
        }

        [HttpPost]
        public async Task<IActionResult> ProcessMomo()
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                {
                    return Unauthorized();
                }

                var amount = 480000; // 480,000 VND
                 var orderInfo = new OrderInfoModel
                {
                    FullName = user.FullName ?? user.UserName,
                    OrderInfo = "Dang ky tai khoan nghe si", // Không dấu để tránh lỗi MoMo
                    Amount = amount,
                    ReturnUrl = Url.Action("PaymentCallback", "Payment", null, Request.Scheme),
                    NotifyUrl = Url.Action("PaymentCallback", "Payment", null, Request.Scheme),
                    Lang = "vi",
                    IpAddr = HttpContext.Connection.RemoteIpAddress?.ToString()
                };

                var response = await _momoService.CreatePaymentAsync(orderInfo);
                if (response != null && !string.IsNullOrEmpty(response.PayUrl))
                {
                    return Redirect(response.PayUrl);
                }

                TempData["ErrorMessage"] = "Không thể khởi tạo thanh toán. Vui lòng thử lại sau.";
                return RedirectToAction("ArtistRegistrationPayment");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing MoMo payment");
                TempData["ErrorMessage"] = "Có lỗi xảy ra khi xử lý thanh toán. Vui lòng thử lại.";
                return RedirectToAction("ArtistRegistrationPayment");
            }
        }

        [HttpGet]
        public async Task<IActionResult> PaymentCallback()
        {
            try
            {
                var userId = _userManager.GetUserId(User);
                var user = await _userManager.FindByIdAsync(userId);
                
                if (user == null)
                {
                    return Unauthorized();
                }

                // Xác thực callback từ VNPAY
                if (Request.Query.ContainsKey("vnp_ResponseCode"))
                {
                    var response = _vnPayService.PaymentExecute(Request.Query);
                    if (response.Success && response.VnPayResponseCode == "00")
                    {
                        await ProcessSuccessfulPayment(user);
                        return RedirectToAction("Index", "Home");
                    }
                    else
                    {
                        TempData["ErrorMessage"] = $"Thanh toán không thành công: {response.VnPayResponseCode}";
                        return RedirectToAction("ArtistRegistrationPayment");
                    }
                }
                // Check for MoMo response
                else if (Request.Query.ContainsKey("resultCode") && 
                        Request.Query["resultCode"] == "0")
                {
                    await ProcessSuccessfulPayment(user);
                    return RedirectToAction("Index", "Home");
                }
                // Process MoMo IPN (server-to-server)
                else if (Request.Method == "POST" && 
                        Request.HasFormContentType && 
                        Request.Form.ContainsKey("partnerCode"))
                {
                    var result = _momoService.PaymentExecuteAsync(Request.Form);
                    if (result.Success)
                    {
                        await ProcessSuccessfulPayment(user);
                        return Ok(new { Status = 0, Message = "Success" });
                    }
                    return BadRequest(new { Status = 1, Message = "Invalid payment" });
                }

                TempData["ErrorMessage"] = "Thanh toán không thành công. Vui lòng thử lại.";
                return RedirectToAction("ArtistRegistrationPayment");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in payment callback");
                TempData["ErrorMessage"] = "Có lỗi xảy ra khi xử lý thanh toán. Vui lòng liên hệ hỗ trợ.";
                return RedirectToAction("ArtistRegistrationPayment");
            }
        }

        private async Task ProcessSuccessfulPayment(AppUser user)
        {
            // Update user to artist
            if (!user.IsArtist)
            {
                user.IsArtist = true;
                await _userManager.UpdateAsync(user);
            }

            // Đảm bảo role 'Artist' cho user nếu có sử dụng role
            if (_userManager != null && !(await _userManager.IsInRoleAsync(user, "Artist")))
            {
                await _userManager.AddToRoleAsync(user, "Artist");
            }

            // Add to Artists table if not exists
            var existingArtist = await _db.Artists.FirstOrDefaultAsync(a => a.userId == user.Id);
            if (existingArtist == null)
            {
                var artist = new Artists
                {
                    userId = user.Id,
                    ArtistName = user.UserName
                };
                _db.Artists.Add(artist);
                await _db.SaveChangesAsync();
            }

            TempData["SuccessMessage"] = "Thanh toán thành công! Bạn đã trở thành nghệ sĩ.";
        }
    }

    public class MonoPaymentResponse
    {
        public string PaymentUrl { get; set; }
    }
}
