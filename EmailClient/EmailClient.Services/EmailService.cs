using EmailClient.Core.DTOs;
using EmailClient.Core.Interfaces;
using EmailClient.Core.Requests;
using System.Net.Mail;

namespace EmailClient.Services
{
    public class EmailService : IEmailService
    {
        public readonly ISmtpClient _smtpClient;
        public readonly IImapClient _imapClient;

        public EmailService(ISmtpClient smtpClient, IImapClient imapClient)
        {
            _smtpClient = smtpClient;
            _imapClient = imapClient;
        }

        public Task GetEmailByIdAsync()
        {
            throw new NotImplementedException();
        }

        public async Task GetEmailsAsync(GetEmailsRequest request)
        {
            var result = await _imapClient.ReadEmailsAsync(request);
        }

        public async Task SendEmailAsync(SendEmailRequest request)
        {
            if (!IsValidEmailAddress(request.From) || !IsValidEmailAddress(request.To))
            {
                // Could be done best with Result pattern -> return EmailClientResults.InvalidEmailAddressFormat (w status code 400 Bad Request)
                throw new InvalidDataException("Invalid email address format");
            }

            var emailMessage = new EmailMessageDto(request.From, request.To, request.Subject, request.Body);

            await _smtpClient.SendEmailAsync(emailMessage);
        }

        private bool IsValidEmailAddress(string emailAddress)
        {
            try
            {
                var email = new MailAddress(emailAddress);

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
