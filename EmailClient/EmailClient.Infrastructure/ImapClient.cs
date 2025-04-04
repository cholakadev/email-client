using EmailClient.Core.Interfaces;
using EmailClient.Core.Options;
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

        public Task ReadEmails()
        {
            throw new NotImplementedException();
        }
    }
}
