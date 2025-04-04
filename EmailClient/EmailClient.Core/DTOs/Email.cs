namespace EmailClient.Core.DTOs
{
    public record Email(
        string From,
        string To,
        string Subject,
        string Body);
}
