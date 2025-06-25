using System;

namespace TunePhere.Models.Momo
{
    public class OrderInfoModel
    {
        public string OrderId { get; set; }
        public string OrderInfo { get; set; }
        public string FullName { get; set; }
        public int Amount { get; set; }
        public string ReturnUrl { get; set; }
        public string NotifyUrl { get; set; }
        public string ExtraData { get; set; }
        public string OrderType { get; set; }
        public string PartnerCode { get; set; }
        public string AccessKey { get; set; }
        public string RequestId { get; set; }
        public string RequestType { get; set; }
        public string Lang { get; set; }
        public string IpAddr { get; set; }
        public string ExpiredDate { get; set; }

        public OrderInfoModel()
        {
            OrderId = DateTime.UtcNow.Ticks.ToString();
            RequestId = DateTime.UtcNow.Ticks.ToString();
            Lang = "vi";
            OrderType = "other";
            ExpiredDate = DateTime.Now.AddMinutes(15).ToString("yyyyMMddHHmmss");
        }
    }
}
