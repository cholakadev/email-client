namespace EmailClient.Core.Errors
{
    public class EmailClientValidationException : Exception
    {
        public ValidationError Error { get; }

        public EmailClientValidationException(ValidationError error)
            : base("Validation failed.")
        {
            Error = error;
        }
    }
}
