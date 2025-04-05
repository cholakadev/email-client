namespace EmailClient.Core.DTOs
{
    public record EmailMessage(
        string From,
        string To,
        string Subject,
        string Body);
}
