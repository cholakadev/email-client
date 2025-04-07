using EmailClient.Core.DTOs;
using EmailClient.Core.Errors;
using EmailClient.Core.Interfaces;
using EmailClient.Core.Requests;
using EmailClient.Core.Responses;
using EmailClient.Core.Results;
using System.Net.Mail;

namespace EmailClient.Services
{
    public class EmailService : IEmailService
    {
        public readonly ISmtpClient _smtpClient;
        public readonly IImapClient _imapClient;

        public EmailService(ISmtpClient smtpClient, IImapClient imapClient)
        {
            _smtpClient = smtpClient;
            _imapClient = imapClient;
        }

        public async Task<ResultT<GetEmailsResponse>> GetEmailsAsync(GetEmailsRequest request)
        {
            var emailsResult = await _imapClient.ReadEmailsAsync(request);

            var response = new GetEmailsResponse();
            response.Emails = emailsResult;

            return ResultT<GetEmailsResponse>.Success(response);
        }

        public async Task<Result> SendEmailAsync(SendEmailRequest request)
        {
            // TODO: Could return Result.Failure(EmailClientErrors.InvalidEmailAddressFormat) and map to appropriate response
            // in the controller action by by checking IsFailure & IsSuccess
            if (!IsValidEmailAddress(request.From) || !IsValidEmailAddress(request.To))
                throw new EmailClientValidationException(
                    new ValidationError(
                        EmailClientErrors.InvalidEmailAddressFormat.Code,
                        EmailClientErrors.InvalidEmailAddressFormat.Description));

            var emailMessage = new EmailMessageDto(request.From, request.To, request.Subject, request.Body);

            await _smtpClient.SendEmailAsync(emailMessage);

            return Result.Success();
        }

        private bool IsValidEmailAddress(string emailAddress)
        {
            try
            {
                var email = new MailAddress(emailAddress);

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
