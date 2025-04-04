using Microsoft.AspNetCore.Mvc;

namespace EmailClient.Presentation.API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class EmailsController : ControllerBase
    {
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
        public object SendEmailAsync(object emailInfo)
        {
            throw new NotImplementedException();
        }
    }
}
