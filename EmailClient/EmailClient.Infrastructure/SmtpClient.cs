using EmailClient.Core.Interfaces;
using EmailClient.Core.Options;
using Microsoft.Extensions.Options;

namespace EmailClient.Infrastructure
{
    public class SmtpClient : ISmtpClient
    {
        private readonly IOptionsMonitor<SmtpSettings> _settings;

        public SmtpClient(IOptionsMonitor<SmtpSettings> settings)
        {
            _settings = settings;
        }

        public Task SendEmail()
        {
            throw new NotImplementedException();
        }
    }
}
