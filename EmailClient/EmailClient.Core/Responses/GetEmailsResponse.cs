using EmailClient.Core.DTOs;

namespace EmailClient.Core.Responses
{
    public class GetEmailsResponse
    {
        public List<EmailDto> Emails { get; set; } = new();
    }
}
