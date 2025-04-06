using EmailClient.Core.DTOs;
using EmailClient.Core.Interfaces;
using EmailClient.Core.Options;
using EmailClient.Core.Requests;
using Microsoft.Extensions.Options;

namespace EmailClient.Infrastructure
{
    public class ImapClient : IImapClient
    {
        private readonly IOptionsMonitor<ImapSettings> _settings;

        public ImapClient(IOptionsMonitor<ImapSettings> settings)
        {
            _settings = settings;
        }

        public async Task<List<EmailDto>> ReadEmailsAsync(GetEmailsRequest request)
        {
            throw new NotImplementedException();
        }
    }
}
