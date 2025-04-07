using EmailClient.Core.Errors;
using EmailClient.Core.Results;

namespace EmailClient.Core.Extensions
{
    public static class ResultExtensions
    {
        public static T Match<T>(
            this Result result,
            Func<T> onSuccess,
            Func<Error, T> onFailure)
            => result.IsSuccess ? onSuccess() : onFailure(result.Error);
    }
}
