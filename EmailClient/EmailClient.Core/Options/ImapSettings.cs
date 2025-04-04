namespace EmailClient.Core.Options
{
    public class ImapSettings
    {
        public const string SectionKey = nameof(ImapSettings);

        public string Host { get; set; } = string.Empty;

        public string Port { get; set; } = string.Empty;

        public string Username { get; set; } = string.Empty;

        public string Password { get; set; } = string.Empty;
    }
}
