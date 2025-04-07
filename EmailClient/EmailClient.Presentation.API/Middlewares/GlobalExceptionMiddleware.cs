using EmailClient.Core.Errors;
using System.Net;
using System.Text.Json;

namespace EmailClient.Presentation.API.Middlewares
{
    public class GlobalExceptionMiddleware
    {
        private readonly RequestDelegate _next;

        private readonly ILogger<GlobalExceptionMiddleware> _logger;

        public GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception occurred");

                var response = context.Response;
                context.Response.ContentType = "application/json";
                var errorStatusCode = (ex as HttpRequestException)?.StatusCode;

                response.StatusCode = (int?)errorStatusCode ?? ex switch
                {
                    KeyNotFoundException => (int)HttpStatusCode.NotFound,
                    EmailClientValidationException => (int)HttpStatusCode.BadRequest,
                    _ => (int)HttpStatusCode.InternalServerError, // unhandled
                };

                _logger.LogError(ex, "Unhandled exception occurred.");

                if (ex is EmailClientValidationException e)
                {
                    _logger.LogError(e.Message, e.Error);
                    var errorResponse = new Error(e.Error.Code, e.Error.Message, HttpStatusCode.BadRequest);
                    await response.WriteAsync(JsonSerializer.Serialize(errorResponse));
                }
                else if (response.StatusCode == (int)HttpStatusCode.InternalServerError)
                    await response.WriteAsync(JsonSerializer.Serialize(EmailClientErrors.InternalServerError));
            }
        }
    }
}
