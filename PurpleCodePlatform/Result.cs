namespace PurpleCodePlatform
{
    public class Result
    {
        public bool Success { get; }
        public string Error { get; private set; }
        public bool IsNotFound { get; private set; }

        protected Result(bool success, string error)
        {
            Success = success;
            Error = error;
        }

        public static Result Fail(string message) => new Result(false, message);
        public static Result<T> Fail<T>(string message) => new Result<T>(default, false, message);
        public static Result NotFound() => new Result(false, string.Empty) { IsNotFound  = true };
        public static Result<T> NotFound<T>() => new Result<T>(default, false, string.Empty) { IsNotFound  = true };
        public static Result Ok() => new Result(true, string.Empty);
        public static Result<T> Ok<T>(T value) => new Result<T>(value, true, string.Empty);
    }

    public class Result<T> : Result
    {
        public T Value { get; set; }

        protected internal Result(T value, bool success, string error) : base(success, error)
        {
            Value = value;
        }
    }
}
