using FluentValidation;

namespace EmailClient.Presentation.API.Validators
{
    public abstract class BaseValidator<T> : AbstractValidator<T>
        where T : class
    {
    }
}
