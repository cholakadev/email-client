using System.Net;
using System.Text.Json.Serialization;

namespace EmailClient.Core.Errors
{
    public record Error([property: JsonPropertyName("code")] string Code,
        [property: JsonPropertyName("description")] string Description,
        [property: JsonIgnore] HttpStatusCode StatusCode)
    {
        public static readonly Error None = new(string.Empty, string.Empty, HttpStatusCode.Conflict);
    }
}
