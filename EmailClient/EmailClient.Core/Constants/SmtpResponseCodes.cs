namespace EmailClient.Core.Constants
{
    public static class SmtpResponseCodes
    {
        public const string Ok = "250";
        public const string StartMailInput = "354";
        public const string AuthSuccess = "235";
        public const string CredentialsPrompt = "334";
        public const string Bye = "221";
        public const string ServiceReady = "220"; // Initial greeing or STARTTLS accepted
    }
}
