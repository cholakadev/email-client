using EmailClient.Core.Requests;

namespace EmailClient.Core.Interfaces
{
    public interface IEmailService
    {
        Task GetEmailsAsync(GetEmailsRequest request);

        Task SendEmailAsync(SendEmailRequest request);
    }
}
