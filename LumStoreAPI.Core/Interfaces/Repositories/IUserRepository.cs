using LumStoreAPI.Core.Entities.Systems;
using LumStoreAPI.Core.Interfaces.ContentEngine;
using LumStoreAPI.Core.Models.Riches;
using System.Linq.Expressions;

namespace LumStoreAPI.Core.Interfaces.Repositories
{
    public interface IUserRepository
    {
        Task<User?> GetUserAsync(int userId);
        Task<IEnumerable<User>> GetUsersAsync(Func<IQueryable<User>, IQueryable<User>>? func = null);
        Task<IPagedEnumerable<User>> GetUsersAsync(int page, int pageSize, Func<IQueryable<User>, IQueryable<User>>? func = null);
        Task<User> UpdateUserAsync(int userID, Action<User> update);
        Task<int> UpdateUsersAsync(Expression<Func<User, bool>> condition, Action<SetterBuilder<User>> action);
        Task<User> InsertUserAsync(User user);
        Task<IEnumerable<User>> InsertUsersAsync(params User[] users);
        Task<bool> CheckUserAsync(Expression<Func<User, bool>> condition);
    }
}
