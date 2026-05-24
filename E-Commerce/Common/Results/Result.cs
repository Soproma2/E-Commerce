namespace E_Commerce.Common.Results
{
    public record Result<T>(int Status, T? Value, string? Message, List<string>? Errors)
    {
        public static Result<T> Ok(T value) => new(200, value, default, default);
        public static Result<T> Success(int status, T value) => new(status, value, default, default);

        public static Result<T> Error(int status, string msg, List<string>? errors) => new(status, default, msg, errors);
        public static Result<T> BadRequest(string msg) => new(400, default, msg, default);
        public static Result<T> Unauthorized() => new(401, default, "User is not authorized.", default);
        public static Result<T> NotFound(string msg) => new(404, default, msg, default);
        public static Result<T> ValidationError(List<string> errors) => new(400, default, "Validation error", errors);
    }
}
