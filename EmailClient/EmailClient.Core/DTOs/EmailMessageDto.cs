namespace EmailClient.Core.DTOs
{
    public record EmailMessageDto(
        string From,
        string To,
        string Subject,
        string Body);
}
