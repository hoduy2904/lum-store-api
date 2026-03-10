namespace LumStoreAPI.Application.DTOs.Responses
{
    public class APIResponse<T> : APIResponseBase
    {
        public T? Data { get; set; }
        public static APIResponse<T> Success(T data, string[]? messages = null) => new APIResponse<T> { Data = data, Messages = messages ?? [] };
    }
}
