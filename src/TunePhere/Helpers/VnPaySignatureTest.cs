using System;
using System.Security.Cryptography;
using System.Text;

namespace TunePhere.Helpers
{
    public class VnPaySignatureTest
    {
        public static void Main()
        {
            string key = "U7JFCR81AZ9FHGL45B2D7QGANI3XNOWH"; // HashSecret
            string signData = "vnp_Amount=48000000&vnp_Command=pay&vnp_CreateDate=20250624005147&vnp_CurrCode=VND&vnp_IpAddr=::1&vnp_Locale=vn&vnp_OrderInfo=Thanh toan dang ky tai khoan nghe si - 20250624005147&vnp_OrderType=other&vnp_ReturnUrl=http://localhost:5222/Payment/PaymentCallback&vnp_TmnCode=7FW5TV6R&vnp_TxnRef=638863231070138722&vnp_Version=2.1.0";

            var keyBytes = Encoding.UTF8.GetBytes(key);
            var inputBytes = Encoding.UTF8.GetBytes(signData);
            using (var hmac = new HMACSHA512(keyBytes))
            {
                var hashValue = hmac.ComputeHash(inputBytes);
                var hex = BitConverter.ToString(hashValue).Replace("-", "").ToLower();
                Console.WriteLine($"Signature: {hex}");
            }
        }
    }
}
