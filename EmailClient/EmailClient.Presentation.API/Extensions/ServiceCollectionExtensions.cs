using EmailClient.Core.Interfaces;
using EmailClient.Core.Options;
using EmailClient.Infrastructure;
using EmailClient.Presentation.API.Validators;
using EmailClient.Services;
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

        public static void AddOptions(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<ImapSettings>(configuration.GetSection(ImapSettings.SectionKey));
            services.Configure<SmtpSettings>(configuration.GetSection(SmtpSettings.SectionKey));
        }

        public static void RegisterInfrastructure(this IServiceCollection services)
        {
            services.AddScoped<IImapClient, ImapClient>();
            services.AddScoped<ISmtpClient, SmtpClient>();
        }

        public static void RegisterServices(this IServiceCollection services)
            => services.AddScoped<IEmailService, EmailService>();
    }
}
