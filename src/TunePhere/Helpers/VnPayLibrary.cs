using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using System.Web;

namespace TunePhere.Helpers
{
    public class VnPayLibrary
    {
        public const string VERSION = "2.1.0";
        private const string DEFAULT_RETURN_URL = "http://localhost:5222/Payment/ArtistRegistrationPayment";
        private readonly SortedList<string, string> _requestData = new SortedList<string, string>();
        private string _returnUrl;

        public VnPayLibrary()
        {
            AddRequestData("vnp_ReturnUrl", DEFAULT_RETURN_URL);
        }

        public VnPayLibrary(string returnUrl)
        {
            AddRequestData("vnp_ReturnUrl", returnUrl);
        }

        public void AddRequestData(string key, string value)
        {
            if (!string.IsNullOrEmpty(value) && !_requestData.ContainsKey(key))
            {
                _requestData.Add(key, value);
            }
        }

        public string CreateRequestUrl(string baseUrl, string hashSecret)
        {
            // Sắp xếp các tham số theo thứ tự bảng chữ cái
            var sortedParams = new SortedList<string, string>(_requestData);

            // Tạo chuỗi signData với giá trị thô (không mã hóa)
            string signData = string.Join("&", sortedParams.Select(kv => $"{kv.Key}={kv.Value}"));

            // Tạo chữ ký
            string vnp_SecureHash = HmacSHA512(hashSecret, signData);

            // Log thông tin để debug
            Console.WriteLine($"[VNPAY] signData: {signData}");
            Console.WriteLine($"[VNPAY] vnp_SecureHash: {vnp_SecureHash}");

            // Tạo URL hoàn chỉnh với các giá trị được mã hóa
            var data = new StringBuilder();
            foreach (var kv in sortedParams)
            {
                if (data.Length > 0) data.Append('&');
                string encodedValue = HttpUtility.UrlEncode(kv.Value);
                data.Append($"{kv.Key}={encodedValue}");
            }
            data.Append($"&vnp_SecureHash={vnp_SecureHash}");

            return baseUrl + "?" + data.ToString();
        }

        public static string HmacSHA512(string key, string inputData)
        {
            var keyBytes = Encoding.UTF8.GetBytes(key);
            var dataBytes = Encoding.UTF8.GetBytes(inputData);

            using (var hmac = new HMACSHA512(keyBytes))
            {
                var hashBytes = hmac.ComputeHash(dataBytes);
                return BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
            }
        }

        public static bool VerifySignature(string hashSecret, string signData, string secureHash)
        {
            var expectedHash = HmacSHA512(hashSecret, signData);
            return secureHash.Equals(expectedHash, StringComparison.OrdinalIgnoreCase);
        }
    }
}