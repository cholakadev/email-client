using EmailClient.Core.Requests;
using EmailClient.Core.Responses;

namespace EmailClient.Core.Interfaces
{
    public interface IEmailService
    {
        Task<GetEmailsResponse> GetEmailsAsync(GetEmailsRequest request);

        Task SendEmailAsync(SendEmailRequest request);
    }
}
