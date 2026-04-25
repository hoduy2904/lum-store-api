using LumStoreAPI.Core.Entities.Systems;
using LumStoreAPI.Core.Interfaces.ContentEngine;
using LumStoreAPI.Core.Interfaces.Repositories;
using LumStoreAPI.Core.Interfaces.Sytems;
using LumStoreAPI.Core.Models.Riches;
using LumStoreAPI.Infrastructure.Types;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace LumStoreAPI.Infrastructure.Repositories.Presentations
{
    internal class UserRepository : IUserRepository
    {
        private readonly LumStoreContext _lumStoreContext;
        private readonly ICacheService _cacheService;
        public UserRepository(LumStoreContext lumStoreContext, ICacheService cacheService)
        {
            _lumStoreContext = lumStoreContext;
            _cacheService = cacheService;
        }

        public Task<bool> CheckUserAsync(Expression<Func<User, bool>> condition)
        {
            return _lumStoreContext.Users.AnyAsync(condition);
        }

        public Task<User?> GetUserAsync(int userId)
        {
            return _lumStoreContext.Users.FirstOrDefaultAsync(x => x.ItemID == userId);
        }

        public async Task<IEnumerable<User>> GetUsersAsync(Func<IQueryable<User>, IQueryable<User>>? func = null)
        {
            IQueryable<User> query = _lumStoreContext.Users;
            if (func != null)
            {
                query = func.Invoke(query);
            }
            return await query.ToArrayAsync();
        }

        public async Task<IPagedEnumerable<User>> GetUsersAsync(int page, int pageSize, Func<IQueryable<User>, IQueryable<User>>? func = null)
        {
            IQueryable<User> query = _lumStoreContext.Users;
            if (func != null)
            {
                query = func.Invoke(query);
            }
            query = query.Skip((page - 1) * pageSize).Take(pageSize);

            int totalRecords = await query.CountAsync();

            var data = await query.ToArrayAsync();
            return new PagedEnumerable<User>(data, totalRecords);
        }

        public async Task<User> InsertUserAsync(User user)
        {
            _lumStoreContext.Users.Add(user);
            await _lumStoreContext.SaveChangesAsync();
            _cacheService.TouchKey(new CacheDependency().Users().GetDependencies().ToArray());
            return user;
        }

        public async Task<IEnumerable<User>> InsertUsersAsync(params User[] users)
        {
            _lumStoreContext.AddRange(users);
            await _lumStoreContext.SaveChangesAsync();
            _cacheService.TouchKey(new CacheDependency().Users().GetDependencies().ToArray());
            return users;
        }

        public async Task<User> UpdateUserAsync(int userID, Action<User> update)
        {
            var user = await _lumStoreContext.Users.FindAsync(userID);
            if (user == null)
                throw new NullReferenceException("Cannot found user with id: " + userID);

            update(user);

            await _lumStoreContext.SaveChangesAsync();
            _cacheService.TouchKey(new CacheDependency().Users().User(userID).GetDependencies().ToArray());
            return user;
        }

        public async Task<int> UpdateUsersAsync(Expression<Func<User, bool>> condition, Action<SetterBuilder<User>> action)
        {
            var builder = new SetterBuilder<User>();
            action.Invoke(builder);

            var result = await _lumStoreContext.Set<User>()
                  .Where(condition)
                  .ExecuteUpdateAsync(x =>
                  {
                      foreach (var bd in builder.GetValues())
                      {
#pragma warning disable EF1001 // Internal EF Core API usage.
                          x.SetProperty(bd.property, bd.value);
#pragma warning restore EF1001 // Internal EF Core API usage.
                      }
                  });
            if (result > 0)
            {
                _cacheService.TouchKey(new CacheDependency().Users().GetDependencies().ToArray());
            }
            return result;
        }
    }
}
