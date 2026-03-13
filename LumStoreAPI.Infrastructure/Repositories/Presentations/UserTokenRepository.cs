using LumStoreAPI.Core.Entities.Systems;
using LumStoreAPI.Core.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace LumStoreAPI.Infrastructure.Repositories.Presentations
{
    internal class UserTokenRepository : IUserTokenRepository
    {
        private readonly LumStoreContext _lumStoreContext;
        public UserTokenRepository(LumStoreContext lumStoreContext)
        {
            _lumStoreContext = lumStoreContext;
        }
        public Task<int> DeleteToken(Guid token)
        {
            return DeleteTokens(x => x.TokenID == token);
        }

        public Task<int> DeleteTokens(Expression<Func<UserToken, bool>> expression)
        {
            return _lumStoreContext.UserTokens.Where(expression).ExecuteDeleteAsync();
        }

        public async Task<UserToken> InsertToken(UserToken userToken)
        {
            _lumStoreContext.UserTokens.Add(userToken);
            await _lumStoreContext.SaveChangesAsync();
            return userToken;
        }

        public Task<bool> IsValidRefreshToken(Guid token, string refreshToken)
        {
            return _lumStoreContext.UserTokens.AnyAsync(x => x.TokenID == token
            && !string.IsNullOrEmpty(x.RefreshToken) &&
            x.RefreshToken.Equals(refreshToken) &&
             x.ValidTo > DateTime.UtcNow);
        }

        public Task<bool> IsValidToken(Guid token)
        {
            return _lumStoreContext.UserTokens.AnyAsync(x => x.TokenID == token);
        }
    }
}
