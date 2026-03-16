namespace LumStoreAPI.Application.DTOs.Responses
{
    public class APIResponse<T> : APIResponseBase
    {
        public T? Data { get; set; }
        public static APIResponse<T> Success(T data, string[]? messages = null) => new APIResponse<T> { IsSuccess = true, Data = data, Messages = messages ?? [] };
        public static new APIResponse<T> Failure(string error, string[]? messages = null)
            => new APIResponse<T> { Error = error, IsSuccess = false, Messages = messages ?? [] };
    }
}
