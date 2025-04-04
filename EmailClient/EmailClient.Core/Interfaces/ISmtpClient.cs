namespace EmailClient.Core.Interfaces
{
    public interface ISmtpClient
    {
        Task SendEmail();
    }
}
