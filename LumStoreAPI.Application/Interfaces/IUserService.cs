using LumStoreAPI.Core.Entities.Systems;

namespace LumStoreAPI.Application.Interfaces
{
    public interface IUserService
    {
        Task<User?> GetCurrentUserAsync();
    }
}
