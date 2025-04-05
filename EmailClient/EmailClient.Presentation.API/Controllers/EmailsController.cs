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
        public async Task<IResult> GetEmailSubjects()
        {
            throw new NotImplementedException();
        }

        [HttpGet("{id}")]
        public async Task<IResult> GetEmailById(Guid id)
        {
            throw new NotImplementedException();
        }

        [HttpPost]
        public async Task<IResult> SendEmailAsync(EmailMessageRequest request)
        {
            await _emailService.SendEmailAsync(request);
            return Results.Ok();
        }
    }
}
