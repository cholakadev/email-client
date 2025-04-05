using EmailClient.Core.Requests;

namespace EmailClient.Core.Interfaces
{
    public interface IEmailService
    {
        Task GetEmailSubjectsAsync();

        Task GetEmailByIdAsync();

        Task SendEmailAsync(EmailMessageRequest request);
    }
}
