using EmailClient.Core.DTOs;
using EmailClient.Core.Interfaces;
using EmailClient.Core.Requests;
using System.Net.Mail;

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
            if (!IsValidEmailAddress(request.From) || !IsValidEmailAddress(request.To))
            {
                // Could be done best with Result pattern -> return EmailClientResults.InvalidEmailAddressFormat (w status code 400 Bad Request)
                throw new InvalidDataException("Invalid email address format");
            }

            var emailMessage = new EmailMessage(request.From, request.To, request.Subject, request.Body);

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
