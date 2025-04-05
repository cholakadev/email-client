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
        public object GetEmailSubjects()
        {
            throw new NotImplementedException();
        }

        [HttpGet("{id}")]
        public object GetEmailById(Guid id)
        {
            throw new NotImplementedException();
        }

        [HttpPost]
        public object SendEmailAsync(EmailMessageRequest request)
        {
            try
            {
                _emailService.SendEmailAsync(request);

                return Ok();
            }
            catch (Exception ex)
            {

                throw;
            }
        }
    }
}
