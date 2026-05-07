using LumStoreAPI.Application.DTOs.ContactDTO;
using LumStoreAPI.Application.DTOs.Responses;

namespace LumStoreAPI.Application.Interfaces;

public interface IContactService
{
    Task<APIResponseBase> SendMessageAsync(SendMessageRequest request);
}
