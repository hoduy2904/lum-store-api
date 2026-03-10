namespace LumStoreAPI.Application.DTOs.Responses
{
    public class APIResponseBase
    {
        public bool IsSuccess { get; set; }
        public string[] Messages { get; set; } = [];
        public static APIResponseBase Success(string[]? messages = null)
            => new APIResponseBase { IsSuccess = true, Messages = messages ?? [] };
        public static APIResponseBase Failure(string[]? messages = null)
            => new APIResponseBase { IsSuccess = false, Messages = messages ?? [] };

    }
}
