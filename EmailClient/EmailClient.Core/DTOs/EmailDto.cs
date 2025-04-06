namespace EmailClient.Core.DTOs
{
    public record EmailDto(
        string Subject,
        string From,
        string To,
        string Date,
        string Body
    );
}
