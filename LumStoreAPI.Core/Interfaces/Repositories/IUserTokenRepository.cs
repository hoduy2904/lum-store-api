using LumStoreAPI.Core.Entities.Systems;
using System.Linq.Expressions;

namespace LumStoreAPI.Core.Interfaces.Repositories
{
    public interface IUserTokenRepository
    {
        Task<bool> IsValidToken(Guid token);
        Task<bool> IsValidRefreshToken(Guid token, string refreshToken);
        Task<UserToken> InsertToken(UserToken userToken);
        Task<int> DeleteToken(Guid token);
        Task<int> DeleteTokens(Expression<Func<UserToken, bool>> expression);
    }
}
