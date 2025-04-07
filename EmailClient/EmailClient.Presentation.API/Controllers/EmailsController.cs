using EmailClient.Core.Interfaces;
using EmailClient.Core.Requests;
using Microsoft.AspNetCore.Mvc;
using EmailClient.Core.Extensions;

namespace EmailClient.Presentation.API.Controllers
{
    [ApiController]
    [Route("/api/[controller]")]
    public class EmailsController : ControllerBase
    {
        private readonly IEmailService _emailService;

        public EmailsController(IEmailService emailService)
        {
            _emailService = emailService;
        }

        [HttpGet]
        public async Task<IResult> GetEmailsAsync([FromQuery] GetEmailsRequest request)
        {
            var response = await _emailService.GetEmailsAsync(request);

            return response.Match(
                onSuccess: () => Results.Ok(response.Data),
                onFailure: error => Results.Json(error, statusCode: (int)error.StatusCode));
        }

        [HttpPost]
        public async Task<IResult> SendEmailAsync(SendEmailRequest request)
        {
            var response = await _emailService.SendEmailAsync(request);

            return response.Match(
                onSuccess: () => Results.Ok(),
                onFailure: error => Results.Json(error, statusCode: (int)error.StatusCode));
        }
    }
}
