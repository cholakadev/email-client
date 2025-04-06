using EmailClient.Core.DTOs;
using EmailClient.Core.Requests;

namespace EmailClient.Core.Interfaces
{
    public interface IImapClient
    {
        Task<List<EmailDto>> ReadEmailsAsync(GetEmailsRequest request);
    }
}
