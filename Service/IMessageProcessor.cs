using Service.Model;

namespace Service
{
    public interface IMessageProcessor
    {
        Invoice ProcessMessage(string inputMessage);

        bool SetGstPrice(decimal gstAmount);

        decimal GetGstPrice();

        ValidateResponse ValidateMessage(string inputMessage);
    }
}
