namespace LumStoreAPI.Application.DTOs.Responses
{
    public class APIResponseBase
    {
        public string? Error { get; set; }
        public bool IsSuccess { get; set; }
        public string[] Messages { get; set; } = [];
        public static APIResponseBase Success(string[]? messages = null)
            => new APIResponseBase { IsSuccess = true, Messages = messages ?? [] };
        public static APIResponseBase Failure(string error, string[]? messages = null)
            => new APIResponseBase { IsSuccess = false, Error = error, Messages = messages ?? [] };

    }
}
