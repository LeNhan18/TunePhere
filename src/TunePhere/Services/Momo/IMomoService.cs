namespace TunePhere.Services.Momo
{
    using Microsoft.AspNetCore.Http;
    using System.Threading.Tasks;
    using TunePhere.Models.Momo;

    public interface IMomoService 
    {
        Task<MomoCreatePaymentResponseModel> CreatePaymentAsync(OrderInfoModel model);
        Task<MomoExecuteResponseModel> ProcessPaymentCallback(IQueryCollection collection);
        MomoExecuteResponseModel PaymentExecuteAsync(IFormCollection collection);
    }
}
