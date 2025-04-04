namespace EmailClient.Core.Interfaces
{
    public interface IEmailService
    {
        Task GetEmailSubjectsAsync();

        Task GetEmailByIdAsync();

        Task SendEmailAsync();
    }
}
