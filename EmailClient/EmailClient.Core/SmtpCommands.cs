namespace EmailClient.Core
{
    public static class SmtpCommands
    {
        public const string Ehlo = "EHLO localhost";
        public const string StartTls = "STARTTLS";
        public const string AuthLogin = "AUTH LOGIN";
        public const string Data = "DATA";
        public const string Quit = "QUIT";
        public const string EndOfLineIndicator = ".";
        public const string ContentTypePlainText = "Content-Type: text/plain; charset=utf-8";
        public const string MailFrom = "MAIL FROM:<%%email%%>"; // Replace %%email%% with the actual email address when used
        public const string RcptTo = "RCPT TO:<%%email%%>"; // Replace %%email%% with the actual email address when used
    }
}
