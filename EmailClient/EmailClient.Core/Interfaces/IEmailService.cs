using EmailClient.Core.Requests;
using EmailClient.Core.Responses;
using EmailClient.Core.Results;

namespace EmailClient.Core.Interfaces
{
    public interface IEmailService
    {
        Task<ResultT<GetEmailsResponse>> GetEmailsAsync(GetEmailsRequest request);

        Task<Result> SendEmailAsync(SendEmailRequest request);
    }
}
