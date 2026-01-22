namespace Services.Helpers
{
    public class ResultHandler<T>
    {
        public bool IsSuccess { get; set; }
        public T? Data { get; set; }
        public string? Message { get; set; }
        public int StatusCode { get; set; }
        public List<string>? Errors { get; set; }

        public static ResultHandler<T> Success(string message, int statusCode, T? data = default)
            => new ResultHandler<T>
            {
                Message = message,
                StatusCode = statusCode,
                IsSuccess = true,
                Data = data
            };

        public static ResultHandler<T> Failure(string message, int statusCode, List<string>? errors = null, T? data = default)
            => new ResultHandler<T>
            {
                Message = message,
                StatusCode = statusCode,
                IsSuccess = false,
                Errors = errors ?? new List<string>(),
                Data = data
            };
    }
}
