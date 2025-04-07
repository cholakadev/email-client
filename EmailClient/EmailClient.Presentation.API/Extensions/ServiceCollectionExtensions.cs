using EmailClient.Presentation.API.Validators;
using FluentValidation;
using FluentValidation.AspNetCore;

namespace EmailClient.Presentation.API.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static void AddValidatorsConfiguration(this IServiceCollection services)
        {
            ValidatorOptions.Global.DefaultRuleLevelCascadeMode = CascadeMode.Stop;

            ValidatorOptions.Global.DisplayNameResolver = (_, member, _)
                => member != null ? member.Name.Replace(" ", string.Empty) : null;

            services.AddFluentValidationAutoValidation(options => { options.DisableDataAnnotationsValidation = true; });

            services.AddValidatorsFromAssemblyContaining<BaseValidator<object>>();
        }
    }
}
