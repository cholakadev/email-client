using EmailClient.Core.Errors;
using EmailClient.Core.Requests;
using FluentValidation;

namespace EmailClient.Presentation.API.Validators
{
    public class SendEmailRequestValidator : BaseValidator<SendEmailRequest>
    {
        public SendEmailRequestValidator()
        {
            RuleFor(x => x.From)
                .NotEmpty()
                .WithMessage(EmailClientErrors.RequiredFromField.Description)
                .WithErrorCode(EmailClientErrors.RequiredFromField.Code);

            RuleFor(x => x.To)
                .NotEmpty()
                .WithMessage(EmailClientErrors.RequiredToField.Description)
                .WithErrorCode(EmailClientErrors.RequiredToField.Code);

            RuleFor(x => x.Subject)
                .NotEmpty()
                .WithMessage(EmailClientErrors.RequiredEmailSubjectField.Description)
                .WithErrorCode(EmailClientErrors.RequiredEmailSubjectField.Code);

            RuleFor(x => x.Body)
                .NotEmpty()
                .WithMessage(EmailClientErrors.RequiredEmailBodyField.Description)
                .WithErrorCode(EmailClientErrors.RequiredEmailBodyField.Code);
        }
    }
}
