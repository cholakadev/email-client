using EmailClient.Core.Interfaces;
using EmailClient.Core.Requests;
using Microsoft.AspNetCore.Mvc;

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
            await _emailService.GetEmailsAsync(request);
            return Results.Ok();
        }

        [HttpPost]
        public async Task<IResult> SendEmailAsync(SendEmailRequest request)
        {
            await _emailService.SendEmailAsync(request);
            return Results.Ok();
        }
    }
}
