using EmailClient.Core.DTOs;

namespace EmailClient.Core.Interfaces
{
    public interface ISmtpClient
    {
        Task SendEmailAsync(EmailMessage message);
    }
}
