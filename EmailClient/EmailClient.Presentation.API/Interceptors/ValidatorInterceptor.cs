using EmailClient.Core.Errors;
using FluentValidation;
using FluentValidation.AspNetCore;
using FluentValidation.Results;
using Microsoft.AspNetCore.Mvc;

namespace EmailClient.Presentation.API.Interceptors
{
    public class ValidatorInterceptor : IValidatorInterceptor
    {
        public ValidationResult AfterAspNetValidation(ActionContext actionContext, IValidationContext validationContext, ValidationResult result)
        {
            if (result == null)
                throw new ArgumentNullException(nameof(result));

            if (!result.IsValid)
            {
                var validationFailure = result.Errors.First();
                var validationError = new ValidationError(validationFailure.ErrorCode, validationFailure.ErrorMessage);
                throw new EmailClientValidationException(validationError);
            }

            return result;
        }

        public IValidationContext BeforeAspNetValidation(ActionContext actionContext, IValidationContext commonContext)
            => commonContext;
    }
}
