using ApiForAngular.DTO;

namespace ApiForAngular.ServiceContracts
{
    public interface IEmailService
    {
        // void SendEmail(EmailDto request);
        void SendEmail(Message message);

    }
}
