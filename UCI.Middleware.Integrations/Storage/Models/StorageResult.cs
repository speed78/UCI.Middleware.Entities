namespace UCI.Middleware.Integrations.Storage.Models
{
    public class StorageResult<T>
    {
        public bool Success { get; set; }
        public T? Data { get; set; }
        public string? ErrorMessage { get; set; }
        public Exception? Exception { get; set; }

        public static StorageResult<T> Ok(T data) => new() { Success = true, Data = data };
        public static StorageResult<T> Fail(string error, Exception? ex = null) => new()
        {
            Success = false,
            ErrorMessage = error,
            Exception = ex
        };
    }
}
