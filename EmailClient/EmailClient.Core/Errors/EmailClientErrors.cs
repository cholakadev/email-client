using System.Net;

namespace EmailClient.Core.Errors
{
    public class EmailClientErrors
    {
        public static readonly Error InternalServerError = new Error("10000", "Internal Server Error.", HttpStatusCode.InternalServerError);
        public static readonly Error RequiredFromField = new Error("10001", "'From' field is required.", HttpStatusCode.BadRequest);
        public static readonly Error RequiredToField = new Error("10002", "'To' field is required.", HttpStatusCode.BadRequest);
        public static readonly Error RequiredEmailSubjectField = new Error("10002", "'Subject' field is required.", HttpStatusCode.BadRequest);
        public static readonly Error RequiredEmailBodyField = new Error("10003", "'Body' field is required.", HttpStatusCode.BadRequest);
        public static readonly Error InvalidEmailAddressFormat = new Error("10004", "Invalid email address format.", HttpStatusCode.BadRequest);
    }
}
