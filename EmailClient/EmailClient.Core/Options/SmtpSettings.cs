namespace EmailClient.Core.Options
{
    public class SmtpSettings
    {
        private readonly int _sslPort = 465;

        public const string SectionKey = nameof(SmtpSettings);

        public string Host { get; set; } = string.Empty;

        public string Port { get; set; } = string.Empty;

        public string Username { get; set; } = string.Empty;

        public string Password { get; set; } = string.Empty;

        public bool RequiresStartTls => int.Parse(Port) != _sslPort;
    }
}
