using System.Net;
using System.Text.Json.Serialization;

namespace EmailClient.Core.Errors
{
    public record Error(string Code, string Description, [property: JsonIgnore] HttpStatusCode statusCode)
    {
        public static readonly Error None = new(string.Empty, string.Empty, HttpStatusCode.Conflict);
    }
}
