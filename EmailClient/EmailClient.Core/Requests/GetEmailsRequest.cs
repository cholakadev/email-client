namespace EmailClient.Core.Requests
{
    public class GetEmailsRequest
    {
        public int Skip { get; set; } = 0;

        public int Take { get; set; } = 5;
    }
}
