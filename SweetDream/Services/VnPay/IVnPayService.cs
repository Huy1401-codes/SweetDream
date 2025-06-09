using SweetDream.Models.VnPay;

namespace SweetDream.Services.VnPay
{
    public interface IVnPayService
    {

        string CreatePaymentUrl(PaymentInformationModel model, HttpContext context);
        PaymentResponseModel PaymentExecute(IQueryCollection collections);

    }
}
