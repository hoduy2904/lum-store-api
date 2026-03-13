using System.IdentityModel.Tokens.Jwt;

namespace LumStoreAPI.Libraries.Helpers
{
    public class JwtTokenHelper
    {
        public static JwtSecurityToken GetJwtSecurityToken(string accessToken)
        {
            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadJwtToken(accessToken);
            return jwtToken;
        }
    }
}
