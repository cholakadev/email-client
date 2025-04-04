﻿namespace EmailClient.Core.Options
{
    public class SmtpSettings
    {
        public const string SectionKey = nameof(SmtpSettings);

        public string Host { get; set; } = string.Empty;

        public string Port { get; set; } = string.Empty;

        public string Username { get; set; } = string.Empty;

        public string Password { get; set; } = string.Empty;
    }
}
