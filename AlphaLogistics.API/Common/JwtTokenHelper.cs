using System.IdentityModel.Tokens.Jwt;

namespace WALMS.API.Common
{
    public class JwtTokenHelper
    {
        public static IDictionary<string, string> GetUserInfoFromToken(HttpContext httpContext)
        {
            var token = httpContext.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
            var handler = new JwtSecurityTokenHandler();
            var jsonToken = handler.ReadToken(token) as JwtSecurityToken;

            var userInfo = new Dictionary<string, string>();

            if (jsonToken != null)
            {
                foreach (var claim in jsonToken.Claims)
                {
                    userInfo.Add(claim.Type, claim.Value);
                }
            }

            return userInfo;
        }
    }
}
