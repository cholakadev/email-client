using EmailClient.Core.Errors;

namespace EmailClient.Core.Results
{
    public class ResultT<T> : Result
    {
        private ResultT(bool isSuccess, T data, Error error)
            : base(isSuccess, error)
            => Data = data;

        public T Data { get; }

        public static ResultT<T> Success(T data) => new(true, data, Error.None);

        public static ResultT<T> Failure(Error error) => new(false, default!, error);

        public static ResultT<T> Failure() => new(false, default!, Error.None);
    }
}
