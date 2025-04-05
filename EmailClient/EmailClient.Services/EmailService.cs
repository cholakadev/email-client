using EmailClient.Core.DTOs;
using EmailClient.Core.Interfaces;
using EmailClient.Core.Requests;

namespace EmailClient.Services
{
    public class EmailService : IEmailService
    {
        public readonly ISmtpClient _smtpClient;

        public EmailService(ISmtpClient smtpClient)
        {
            _smtpClient = smtpClient;
        }

        public Task GetEmailByIdAsync()
        {
            throw new NotImplementedException();
        }

        public Task GetEmailSubjectsAsync()
        {
            throw new NotImplementedException();
        }

        public async Task SendEmailAsync(EmailMessageRequest request)
        {
            // Validate from to emails
            var emailMessage = new EmailMessage(request.From, request.To, request.Subject, request.Body);

            await _smtpClient.SendEmailAsync(emailMessage);
        }
    }
}
